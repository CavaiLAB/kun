using System.Text.Json;
using Kun.Errors;
using Kun.Identity.Abstractions;
using Kun.Identity.Implementations;
using Qian.Config;

namespace Kun.Identity.Connector.Oidc;

/// <summary>
/// 通用 OIDC 连接器的配置，与组件文件夹的 <c>schemas/config.schema.json</c> 一一对应。
/// <see cref="ClientSecret"/> 只接受 secretRef（<c>secretRef://platform/…</c> 或 <c>secretRef://tenant/…</c>），每次换码时现取现用、不缓存。
/// </summary>
public sealed class OidcConnectorOptions
{
    /// <summary>注册名：连接器标识与回调路径段，缺省 <c>oidc</c>。</summary>
    public string Name { get; init; } = "oidc";
    /// <summary>登录页按钮上的身份源名称。</summary>
    public string DisplayName { get; init; } = "企业身份源";
    /// <summary>上游 issuer，须与其发现文档的 issuer 逐字一致。</summary>
    public string Issuer { get; init; } = "";
    /// <summary>在上游登记的客户端标识。</summary>
    public string ClientId { get; init; } = "";
    /// <summary>客户端密钥的 secretRef；不接受明文。</summary>
    public string ClientSecret { get; init; } = "";
    /// <summary>请求的 scope，必须含 openid。</summary>
    public IReadOnlyList<string> Scopes { get; init; } = ["openid", "profile", "email"];
    /// <summary>支持的终端（<see cref="ConnectorTerminals"/>）。</summary>
    public IReadOnlyList<string> Terminals { get; init; } = [ConnectorTerminals.Pc, ConnectorTerminals.Mobile];
    /// <summary>是否要求 https；只允许对回环地址的 issuer 关闭（本机开发与测试）。</summary>
    public bool RequireHttps { get; init; } = true;

    /// <summary>可接受的签名算法，为 RS、PS、ES 三族九种算法的非空子集。</summary>
    public IReadOnlyList<string> AllowedSigningAlgorithms { get; init; } = KunOidcClient.SigningAlgorithms;
    /// <summary>是否允许 azp 为本客户端的多受众令牌。</summary>
    public bool AllowAdditionalAudiences { get; init; } = true;
    /// <summary>校验令牌签发与有效期的时钟容差，范围 0 到 5 分钟。</summary>
    public TimeSpan ClockSkew { get; init; } = TimeSpan.FromMinutes(2);

    /// <inheritdoc />
    public override string ToString() => "OidcConnectorOptions(redacted)";

    /// <summary>逐项检查，返回字段错误（pointer 指向配置成员）；空列表表示通过。明文密钥在这里被拒绝。</summary>
    public IReadOnlyList<FieldError> Check()
    {
        var errors = new List<FieldError>();
        if (!ConnectorManifest.IsValidId(Name)) errors.Add(new FieldError(PlatformErrors.PatternMismatch, "#/name"));
        if (string.IsNullOrWhiteSpace(DisplayName)) errors.Add(new FieldError(PlatformErrors.Required, "#/displayName"));
        else if (DisplayName.Length > 64 || DisplayName.Any(char.IsControl)) errors.Add(new FieldError(PlatformErrors.OutOfRange, "#/displayName"));
        if (string.IsNullOrEmpty(Issuer)) errors.Add(new FieldError(PlatformErrors.Required, "#/issuer"));
        else if (!KunOidcClient.IsAllowedEndpoint(Issuer, RequireHttps)) errors.Add(new FieldError(PlatformErrors.FormatInvalid, "#/issuer"));
        if (string.IsNullOrWhiteSpace(ClientId)) errors.Add(new FieldError(PlatformErrors.Required, "#/clientId"));
        else if (ClientId.Length > 256 || ClientId.Any(char.IsControl)) errors.Add(new FieldError(PlatformErrors.OutOfRange, "#/clientId"));
        if (string.IsNullOrEmpty(ClientSecret)) errors.Add(new FieldError(PlatformErrors.Required, "#/clientSecret"));
        else if (!SecretReference.IsValid(ClientSecret)) errors.Add(new FieldError(PlatformErrors.FormatInvalid, "#/clientSecret"));
        if (Scopes is null || !Scopes.Contains("openid", StringComparer.Ordinal)) errors.Add(new FieldError(PlatformErrors.ValueNotAllowed, "#/scopes"));
        else if (Scopes.Count > 32) errors.Add(new FieldError(PlatformErrors.OutOfRange, "#/scopes"));
        else
            for (var i = 0; i < Scopes.Count; i++)
                if (Scopes[i] is not { Length: > 0 and <= 128 } scope || !scope.All(c => c is '!' or >= '#' and <= '[' or >= ']' and <= '~'))
                    errors.Add(new FieldError(PlatformErrors.PatternMismatch, ErrorPointer.Input("scopes", i)));
        if (Terminals is null || Terminals.Count == 0) errors.Add(new FieldError(PlatformErrors.Required, "#/terminals"));
        else
            for (var i = 0; i < Terminals.Count; i++)
                if (!ConnectorTerminals.All.Contains(Terminals[i]) || Terminals.Take(i).Contains(Terminals[i]))
                    errors.Add(new FieldError(PlatformErrors.ValueNotAllowed, ErrorPointer.Input("terminals", i)));
        if (!KunOidcClient.AreValidAlgorithms(AllowedSigningAlgorithms)) errors.Add(new FieldError(PlatformErrors.ValueNotAllowed, "#/allowedSigningAlgorithms"));
        if (ClockSkew < TimeSpan.Zero || ClockSkew > TimeSpan.FromMinutes(5)) errors.Add(new FieldError(PlatformErrors.OutOfRange, "#/clockSkewSeconds"));
        return errors;
    }

    /// <summary>不合法时抛 INPUT_INVALID，<c>errors[]</c> 列出每个出错的配置成员。</summary>
    public void Validate()
    {
        var errors = Check();
        if (errors.Count > 0) throw new ErrorException(new Error(PlatformErrors.InputInvalid) { Errors = [.. errors.Take(100)] });
    }

    /// <summary>从组件配置（camelCase，与配置 Schema 同形）解析并校验；未知成员按 UNKNOWN_PROPERTY 拒绝。</summary>
    public static OidcConnectorOptions FromConfig(JsonElement config)
    {
        if (config.ValueKind != JsonValueKind.Object)
            throw new ErrorException(new Error(PlatformErrors.InputInvalid) { Errors = [new FieldError(PlatformErrors.TypeMismatch, "#")] });
        var errors = new List<FieldError>();
        string? Text(string name)
        {
            if (!config.TryGetProperty(name, out var value)) return null;
            if (value.ValueKind == JsonValueKind.String) return value.GetString();
            errors.Add(new FieldError(PlatformErrors.TypeMismatch, "#/" + name));
            return null;
        }
        string[]? List(string name)
        {
            if (!config.TryGetProperty(name, out var value)) return null;
            if (value.ValueKind == JsonValueKind.Array && value.EnumerateArray().All(item => item.ValueKind == JsonValueKind.String))
                return [.. value.EnumerateArray().Select(item => item.GetString()!)];
            errors.Add(new FieldError(PlatformErrors.TypeMismatch, "#/" + name));
            return null;
        }
        foreach (var property in config.EnumerateObject())
            if (property.Name is not ("name" or "displayName" or "issuer" or "clientId" or "clientSecret" or "scopes" or "terminals" or "requireHttps" or "allowedSigningAlgorithms" or "allowAdditionalAudiences" or "clockSkewSeconds"))
                errors.Add(new FieldError(PlatformErrors.UnknownProperty, ErrorPointer.Input(property.Name)));
        var defaults = new OidcConnectorOptions();
        bool Flag(string name, bool fallback)
        {
            if (!config.TryGetProperty(name, out var value)) return fallback;
            if (value.ValueKind is JsonValueKind.True or JsonValueKind.False) return value.GetBoolean();
            errors.Add(new FieldError(PlatformErrors.TypeMismatch, "#/" + name));
            return fallback;
        }
        var skew = defaults.ClockSkew;
        if (config.TryGetProperty("clockSkewSeconds", out var seconds))
        {
            if (seconds.ValueKind == JsonValueKind.Number && seconds.TryGetInt32(out var amount)) skew = TimeSpan.FromSeconds(amount);
            else errors.Add(new FieldError(PlatformErrors.TypeMismatch, "#/clockSkewSeconds"));
        }
        var options = new OidcConnectorOptions
        {
            Name = Text("name") ?? defaults.Name,
            DisplayName = Text("displayName") ?? defaults.DisplayName,
            Issuer = Text("issuer") ?? "",
            ClientId = Text("clientId") ?? "",
            ClientSecret = Text("clientSecret") ?? "",
            Scopes = List("scopes") ?? defaults.Scopes,
            Terminals = List("terminals") ?? defaults.Terminals,
            RequireHttps = Flag("requireHttps", defaults.RequireHttps),
            AllowedSigningAlgorithms = List("allowedSigningAlgorithms") ?? defaults.AllowedSigningAlgorithms,
            AllowAdditionalAudiences = Flag("allowAdditionalAudiences", defaults.AllowAdditionalAudiences),
            ClockSkew = skew
        };
        errors.AddRange(options.Check());
        if (errors.Count > 0) throw new ErrorException(new Error(PlatformErrors.InputInvalid) { Errors = [.. errors.Take(100)] });
        return options;
    }
}
