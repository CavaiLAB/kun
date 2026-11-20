using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kun.Configuration;
using Kun.Errors;
using Kun.Identity.Abstractions;
using Kun.Identity.Configuration;
using Kun.Identity.Implementations;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Kun.Identity.Connector.Oidc;

/// <summary>
/// 通用 OIDC 上游连接器：授权码 + PKCE（S256）。state、nonce 与 code_verifier 由身份服务生成并核对关联，本类只做协议：
/// 发现文档（issuer 逐字一致）、JWKS（遇到未知 kid 刷新一次）、换码（client_secret_basic 或 client_secret_post），
/// 并校验 id_token 的签名（只收 RS / PS / ES 族）、iss、aud、azp、有效期与 nonce；授权响应带 iss（RFC 9207）时一并核对。
/// 客户端密钥只经 secretRef 在换码时现取现用，本类不保存明文。实例可并发使用，缓存的只有发现文档与公钥。
/// </summary>
public sealed class OidcIdentityConnector : IIdentityConnector, IDisposable
{
    private readonly OidcConnectorOptions _options;
    private readonly KunOidcClient _protocol;

    /// <summary>
    /// 以已校验的配置构造。<paramref name="secrets"/> 解析 <see cref="OidcConnectorOptions.ClientSecret"/>；
    /// <paramref name="handler"/> 承担全部出站请求，由调用方持有与释放，应当关闭自动跟随重定向。
    /// </summary>
    public OidcIdentityConnector(OidcConnectorOptions options, ISecretResolver secrets, HttpMessageHandler handler, TimeProvider? clock = null)
    {
        ArgumentNullException.ThrowIfNull(options); ArgumentNullException.ThrowIfNull(secrets); ArgumentNullException.ThrowIfNull(handler);
        options.Validate();
        _options = options;
        var protocolOptions = new KunOidcClientOptions
        {
            Name = options.Name,
            Authority = options.Issuer,
            ClientId = options.ClientId,
            RequireHttpsMetadata = options.RequireHttps,
            DiscoveryRefreshInterval = TimeSpan.FromHours(1),
            AllowedSigningAlgorithms = options.AllowedSigningAlgorithms,
            AllowAdditionalAudiences = options.AllowAdditionalAudiences,
            ClockSkew = options.ClockSkew
        };
        _protocol = new KunOidcClient(protocolOptions, () => new HttpClient(handler, disposeHandler: false), ResolveSecretAsync, clock ?? TimeProvider.System);

        async ValueTask<string?> ResolveSecretAsync(CancellationToken ct)
        {
            using var secret = await secrets.ResolveAsync(options.ClientSecret, ct).ConfigureAwait(false);
            return secret.RevealUtf8();
        }
        Descriptor = new ConnectorDescriptor(options.Name, options.DisplayName, "oidc", [.. options.Terminals], ConfigSchema.Value);
    }

    /// <summary>程序化适配器与稳定分发入口共用配置声明；适配器允许宿主选择自己的注册名。</summary>
    internal static readonly Lazy<JsonElement> ConfigSchema = new(() =>
    {
        var schema = System.Text.Json.Nodes.JsonNode.Parse(ComponentConfiguration.Schema)!.AsObject();
        schema["properties"]!["name"]!.AsObject().Remove("const");
        using var document = JsonDocument.Parse(schema.ToJsonString());
        return document.RootElement.Clone();
    });

    /// <inheritdoc />
    public ConnectorDescriptor Descriptor { get; }

    /// <inheritdoc />
    public override string ToString() => $"OidcIdentityConnector({_options.Name})";

    /// <summary>释放内部的 HTTP 客户端与刷新锁；出站处理器归构造方，不在这里释放。</summary>
    public void Dispose() => _protocol.Dispose();

    /// <inheritdoc />
    public async Task<ChallengeResult> BeginSignInAsync(ConnectorContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        KunOidcClient.ValidateVerifier(context.CodeVerifier!);
        var metadata = await WithProviderErrorsAsync(_protocol.DiscoverAsync, cancellationToken).ConfigureAwait(false);
        var query = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = context.CallbackUri.AbsoluteUri,
            ["scope"] = string.Join(' ', _options.Scopes),
            ["state"] = context.State,
            ["nonce"] = context.Nonce,
            ["code_challenge"] = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(context.CodeVerifier!))),
            ["code_challenge_method"] = "S256"
        };
        var target = metadata.AuthorizationEndpoint;
        var separator = new Uri(target).Query.Length > 0 ? '&' : '?';
        return new ChallengeResult(new Uri(target + separator + string.Join('&', query.Select(p => Uri.EscapeDataString(p.Key) + "=" + Uri.EscapeDataString(p.Value)))), null);
    }

    /// <inheritdoc />
    public async Task<ExternalAssertion> CompleteSignInAsync(ConnectorCallback callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);
        var context = callback.Context;
        KunOidcClient.ValidateVerifier(context.CodeVerifier!);
        if (!callback.Parameters.TryGetValue("code", out var code) || code.Length is 0 or > 2048) throw Fail(PlatformErrors.BadGateway);
        var request = new OidcCodeExchange(code, context.CallbackUri.AbsoluteUri, context.CodeVerifier!, context.Nonce)
        {
            ResponseIssuer = callback.Parameters.TryGetValue("iss", out var issuer) ? issuer : null
        };
        var token = await WithProviderErrorsAsync(ct => _protocol.ExchangeIdentityAsync(request, ct), cancellationToken).ConfigureAwait(false);
        return new ExternalAssertion(_options.Issuer, token.Subject, null, Profile(token), Amr(token));
    }

    private static async Task<T> WithProviderErrorsAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct)
    {
        try { return await operation(ct).ConfigureAwait(false); }
        catch (OidcCorrelationException) { throw Fail(IdentityErrors.ConnectorStateInvalid); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { throw Fail(PlatformErrors.GatewayTimeout); }
        catch (Exception error) when (error is HttpRequestException or IOException or JsonException or SecurityTokenException or InvalidOperationException or ArgumentException)
        { throw Fail(PlatformErrors.BadGateway); }
    }

    private static JsonElement Profile(JsonWebToken token)
    {
        var profile = new JsonObject();
        foreach (var name in (string[])["name", "preferred_username", "email"])
            if (token.TryGetPayloadValue<string>(name, out var value) && value is { Length: > 0 and <= 320 } && !value.Any(char.IsControl)) profile[name] = value;
        if (token.TryGetPayloadValue<bool>("email_verified", out var verified)) profile["email_verified"] = verified;
        return JsonSerializer.SerializeToElement(profile);
    }

    private static string[] Amr(JsonWebToken token)
    {
        if (!token.TryGetPayloadValue<string[]>("amr", out var values) || values is null) return [];
        return [.. values.Where(value => value is { Length: > 0 and <= 32 } && value.All(char.IsAsciiLetterOrDigit)).Distinct(StringComparer.Ordinal).Take(8)];
    }

    private static ErrorException Fail(ErrorCode code) => new(new Error(code));
}
