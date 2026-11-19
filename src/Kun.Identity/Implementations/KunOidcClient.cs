#pragma warning disable RS0026 // Preserve the legacy string signature beside the typed PKCE request.
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Kun.Identity.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Kun.Identity.Implementations;

/// <summary>OIDC protocol client with bounded metadata caching and signed, issuer/audience/nonce-bound ID Tokens.</summary>
public sealed partial class KunOidcClient : IOidcClient, IDisposable
{
    private readonly KunOidcClientOptions _options;
    private readonly Func<HttpClient> _clientFactory;
    private readonly Func<CancellationToken, ValueTask<string?>> _secret;
    private readonly string[] _algorithms;
    private readonly TimeSpan _skew;
    private readonly bool _allowAdditionalAudiences;
    private readonly TimeProvider _clock;
    private readonly SemaphoreSlim _discoveryGate = new(1, 1);
    private DiscoverySnapshot? _cached;
    private int _disposed;

    /// <summary>Creates a client using the system clock.</summary>
    public KunOidcClient(KunOidcClientOptions options, IHttpClientFactory httpClientFactory) : this(options, httpClientFactory, TimeProvider.System) { }

    /// <summary>Creates a client with an explicit clock for metadata freshness and token lifetimes.</summary>
    public KunOidcClient(KunOidcClientOptions options, IHttpClientFactory httpClientFactory, TimeProvider clock)
        : this(options, MakeClientFactory(options, httpClientFactory), _ => ValueTask.FromResult<string?>(options.ClientSecret), clock) { }

    private static Func<HttpClient> MakeClientFactory(KunOidcClientOptions options, IHttpClientFactory factory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(factory);
        return () => factory.CreateClient(KunIdentityBuilder.OidcHttpClientName(options.Name));
    }

    internal KunOidcClient(KunOidcClientOptions options, Func<HttpClient> clientFactory, Func<CancellationToken, ValueTask<string?>> secret, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ClientId);
        ArgumentOutOfRangeException.ThrowIfNegative(options.DiscoveryRefreshInterval.Ticks);
        if (!AreValidAlgorithms(options.AllowedSigningAlgorithms))
            throw new ArgumentException("Configure a nonempty subset of supported asymmetric signing algorithms.", nameof(options));
        if (options.ClockSkew < TimeSpan.Zero || options.ClockSkew > TimeSpan.FromMinutes(5))
            throw new ArgumentOutOfRangeException(nameof(options), "OIDC clock skew must be between zero and five minutes.");
        _options = options;
        _algorithms = [.. options.AllowedSigningAlgorithms];
        _skew = options.ClockSkew;
        _allowAdditionalAudiences = options.AllowAdditionalAudiences;
        _clientFactory = clientFactory;
        _secret = secret;
        _clock = clock;
        ValidateEndpoint(options.Authority);
    }

    /// <inheritdoc />
    public string Name => _options.Name;

    /// <inheritdoc />
    public async Task<OidcConfiguration> DiscoverAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed != 0, this);
        var snapshot = Volatile.Read(ref _cached);
        if (snapshot is not null && _clock.GetUtcNow() < snapshot.ExpiresAt) return snapshot.Configuration;
        await _discoveryGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            snapshot = Volatile.Read(ref _cached);
            if (snapshot is not null && _clock.GetUtcNow() < snapshot.ExpiresAt) return snapshot.Configuration;
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{_options.Authority.TrimEnd('/')}/.well-known/openid-configuration");
            using var json = await ReadResponseAsync(request, cancellationToken).ConfigureAwait(false);
            var document = json.Deserialize(OidcJsonContext.Default.OidcDiscoveryDocument)
                ?? throw new InvalidOperationException("OIDC discovery returned an empty document.");
            if (!string.Equals(document.Issuer, _options.Authority, StringComparison.Ordinal))
                throw new SecurityTokenInvalidIssuerException("OIDC discovery issuer does not match the configured authority.");
            ValidateEndpoint(document.Issuer); ValidateEndpoint(document.AuthorizationEndpoint); ValidateEndpoint(document.TokenEndpoint); ValidateEndpoint(document.JwksUri);
            if (document.UserInfoEndpoint is not null) ValidateEndpoint(document.UserInfoEndpoint);
            if (document.ChallengeMethods is { } methods && !methods.Contains("S256", StringComparer.Ordinal))
                throw new InvalidOperationException("OIDC provider does not support the S256 challenge method.");
            var auth = document.AuthenticationMethods;
            var configuration = new OidcConfiguration(document.Issuer, document.AuthorizationEndpoint, document.TokenEndpoint, document.UserInfoEndpoint, document.JwksUri)
            {
                UsesPostAuthentication = auth is not null && !auth.Contains("client_secret_basic", StringComparer.Ordinal) && auth.Contains("client_secret_post", StringComparer.Ordinal),
                RequiresIssuerParameter = document.RequiresIssuerParameter
            };
            Volatile.Write(ref _cached, new DiscoverySnapshot(configuration, _clock.GetUtcNow() + _options.DiscoveryRefreshInterval));
            return configuration;
        }
        finally { _discoveryGate.Release(); }
    }

    /// <inheritdoc />
    [Obsolete("Pass an OidcCodeExchange with PKCE and nonce.", DiagnosticId = "KUNOIDC001")]
    public Task<OidcTokenResponse> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Authorization code exchange requires the original PKCE verifier and nonce.");

    /// <inheritdoc />
    public async Task<OidcTokenResponse> ExchangeCodeAsync(OidcCodeExchange request, CancellationToken cancellationToken = default)
    {
        try
        {
            var (document, validated) = await ExchangeAsync(request, cancellationToken).ConfigureAwait(false);
            return Tokens(document) with { ValidatedPrincipal = new ClaimsPrincipal(validated.ClaimsIdentity) };
        }
        catch (OidcCorrelationException error) { throw new SecurityTokenValidationException(error.Message, error); }
    }

    internal async Task<JsonWebToken> ExchangeIdentityAsync(OidcCodeExchange request, CancellationToken cancellationToken)
    {
        var (_, validated) = await ExchangeAsync(request, cancellationToken).ConfigureAwait(false);
        return (JsonWebToken)validated.SecurityToken;
    }

    private async Task<(OidcTokenDocument, TokenValidationResult)> ExchangeAsync(OidcCodeExchange request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Code);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RedirectUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ExpectedNonce);
        ValidateVerifier(request.CodeVerifier);
        var discovery = await DiscoverAsync(cancellationToken).ConfigureAwait(false);
        if (request.ResponseIssuer is { } issuer ? !string.Equals(issuer, discovery.Issuer, StringComparison.Ordinal) : discovery.RequiresIssuerParameter)
            throw new OidcCorrelationException("Authorization response issuer does not match the initiating transaction.");
        var tokens = await GrantDocumentAsync(discovery, new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = request.Code,
            ["redirect_uri"] = request.RedirectUri,
            ["code_verifier"] = request.CodeVerifier
        }, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(tokens.IdToken)) throw new SecurityTokenValidationException("Code exchange did not return an ID Token.");
        var validated = await ValidateIdentityAsync(tokens.IdToken, discovery, cancellationToken).ConfigureAwait(false);
        var nonce = validated.ClaimsIdentity.FindFirst("nonce")?.Value;
        if (nonce is null || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(nonce), Encoding.UTF8.GetBytes(request.ExpectedNonce)))
            throw new OidcCorrelationException("OIDC nonce does not match the initiating transaction.");
        return (tokens, validated);
    }

    /// <inheritdoc />
    public async Task<OidcTokenResponse> RefreshAsync(string refreshToken, string expectedSubject, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken); ArgumentException.ThrowIfNullOrWhiteSpace(expectedSubject);
        var discovery = await DiscoverAsync(cancellationToken).ConfigureAwait(false);
        var tokens = Tokens(await GrantDocumentAsync(discovery, new Dictionary<string, string> { ["grant_type"] = "refresh_token", ["refresh_token"] = refreshToken }, cancellationToken).ConfigureAwait(false));
        if (tokens.IdToken is null) return tokens;
        var principal = new ClaimsPrincipal((await ValidateIdentityAsync(tokens.IdToken, discovery, cancellationToken).ConfigureAwait(false)).ClaimsIdentity);
        if (principal.FindFirst("sub")?.Value != expectedSubject) throw new SecurityTokenValidationException("Refresh changed the session subject.");
        return tokens with { ValidatedPrincipal = principal };
    }

    /// <inheritdoc />
    public async Task<OidcTokenResponse> ClientCredentialsAsync(string scope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if (string.IsNullOrEmpty(_options.ClientSecret)) throw new InvalidOperationException("Client credentials require a configured client secret.");
        return Tokens(await GrantDocumentAsync(await DiscoverAsync(cancellationToken).ConfigureAwait(false), new Dictionary<string, string>
        { ["grant_type"] = "client_credentials", ["scope"] = scope }, cancellationToken).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task<OidcLogoutToken> ValidateLogoutTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var discovery = await DiscoverAsync(cancellationToken).ConfigureAwait(false);
        var principal = (await ValidateSignedAsync(token, discovery, cancellationToken).ConfigureAwait(false)).ClaimsIdentity;
        var subject = principal.FindFirst("sub")?.Value; var sid = principal.FindFirst("sid")?.Value;
        if (string.IsNullOrEmpty(subject) && string.IsNullOrEmpty(sid)) throw new SecurityTokenValidationException("Logout token has neither sub nor sid.");
        if (principal.HasClaim(claim => claim.Type == "nonce")) throw new SecurityTokenValidationException("Logout token must not contain a nonce.");
        var jti = principal.FindFirst("jti")?.Value;
        if (string.IsNullOrWhiteSpace(jti)) throw new SecurityTokenValidationException("Logout token is missing jti.");
        var eventJson = principal.FindFirst("events")?.Value;
        if (eventJson is null) throw new SecurityTokenValidationException("Logout token is missing its event.");
        using var events = JsonDocument.Parse(eventJson);
        if (events.RootElement.ValueKind != JsonValueKind.Object || !events.RootElement.TryGetProperty("http://schemas.openid.net/event/backchannel-logout", out var logout) || logout.ValueKind != JsonValueKind.Object)
            throw new SecurityTokenValidationException("Logout token has an invalid event.");
        return new(discovery.Issuer, subject, sid, jti, DateTimeOffset.FromUnixTimeSeconds(long.Parse(principal.FindFirst("exp")!.Value, CultureInfo.InvariantCulture)));
    }

    private async Task<TokenValidationResult> ValidateIdentityAsync(string token, OidcConfiguration configuration, CancellationToken ct)
    {
        var validated = await ValidateSignedAsync(token, configuration, ct).ConfigureAwait(false);
        var identity = validated.ClaimsIdentity;
        if (identity.FindFirst("sub")?.Value is not { Length: > 0 and <= 255 } sub || string.IsNullOrWhiteSpace(sub) || sub.Any(char.IsControl) || identity.HasClaim(claim => claim.Type == "events"))
            throw new SecurityTokenValidationException("Token is not an ID Token with a subject.");
        return validated;
    }

    private async Task<TokenValidationResult> ValidateSignedAsync(string token, OidcConfiguration configuration, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        var keys = await KeysAsync(configuration.JwksUri, false, ct).ConfigureAwait(false);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuration.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.ClientId,
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = keys.Values,
            ValidAlgorithms = _algorithms,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = _skew,
            LifetimeValidator = (notBefore, expires, _, p) =>
            {
                var now = _clock.GetUtcNow().UtcDateTime;
                return expires is not null && expires > now - p.ClockSkew && (notBefore is null || notBefore <= now + p.ClockSkew);
            }
        };
        var handler = new JsonWebTokenHandler { MapInboundClaims = false };
        var result = await handler.ValidateTokenAsync(token, parameters).ConfigureAwait(false);
        if (!result.IsValid && result.Exception is SecurityTokenSignatureKeyNotFoundException)
        {
            var refreshed = await KeysAsync(configuration.JwksUri, true, ct).ConfigureAwait(false);
            if (!ReferenceEquals(refreshed, keys))
            {
                parameters.IssuerSigningKeys = refreshed.Values;
                result = await handler.ValidateTokenAsync(token, parameters).ConfigureAwait(false);
            }
        }
        if (!result.IsValid) throw new SecurityTokenValidationException("OIDC token signature, issuer, audience or lifetime is invalid.");
        var principal = result.ClaimsIdentity;
        if (!long.TryParse(principal.FindFirst("iat")?.Value, CultureInfo.InvariantCulture, out var issuedAt) || issuedAt > _clock.GetUtcNow().Add(_skew).ToUnixTimeSeconds())
            throw new SecurityTokenValidationException("OIDC token has an invalid issue time.");
        var signed = (JsonWebToken)result.SecurityToken;
        var audiences = signed.Audiences.ToArray();
        var hasParty = signed.TryGetPayloadValue<string>("azp", out var party);
        if (!audiences.Contains(_options.ClientId, StringComparer.Ordinal) ||
            (principal.HasClaim(c => c.Type == "azp") && (!hasParty || party != _options.ClientId)) ||
            (audiences.Length > 1 && (!_allowAdditionalAudiences || !hasParty)))
            throw new SecurityTokenValidationException("OIDC token has an untrusted additional audience or authorized party.");
        return result;
    }

    private async Task<OidcTokenDocument> GrantDocumentAsync(OidcConfiguration discovery, Dictionary<string, string> form, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, discovery.TokenEndpoint);
        var secret = await _secret(ct).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(secret) && !discovery.UsesPostAuthentication)
        {
            var credentials = WebUtility.UrlEncode(_options.ClientId) + ":" + WebUtility.UrlEncode(secret);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));
        }
        else
        {
            form["client_id"] = _options.ClientId;
            if (!string.IsNullOrEmpty(secret)) form["client_secret"] = secret;
        }
        request.Content = new FormUrlEncodedContent(form);
        using var json = await ReadResponseAsync(request, ct).ConfigureAwait(false);
        return json.Deserialize(OidcJsonContext.Default.OidcTokenDocument)
            ?? throw new InvalidOperationException("OIDC token endpoint returned an empty response.");
    }

    private static OidcTokenResponse Tokens(OidcTokenDocument token)
    {
        if (string.IsNullOrWhiteSpace(token.AccessToken) || !string.Equals(token.TokenType, "Bearer", StringComparison.OrdinalIgnoreCase) || token.ExpiresIn <= 0)
            throw new SecurityTokenValidationException("OIDC token response is incomplete or uses an unsupported token type.");
        return new(token.AccessToken, token.IdToken, token.RefreshToken, token.TokenType!, token.ExpiresIn);
    }

    internal static void ValidateVerifier(string verifier)
    {
        if (verifier is null || verifier.Length is < 43 or > 128 || verifier.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '.' and not '_' and not '~'))
            throw new ArgumentException("PKCE verifier must contain 43-128 unreserved ASCII characters.", nameof(verifier));
    }

    private void ValidateEndpoint(string endpoint)
    {
        if (!IsAllowedEndpoint(endpoint, _options.RequireHttpsMetadata))
            throw new InvalidOperationException("OIDC endpoints require HTTPS, or loopback HTTP when enabled, without user-info or fragments.");
    }

    internal static bool IsAllowedEndpoint(string endpoint, bool requireHttps) =>
        Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && uri.UserInfo.Length == 0 && uri.Fragment.Length == 0 &&
        (uri.Scheme == Uri.UriSchemeHttps || !requireHttps && uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback);

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        _discoveryGate.Dispose();
        _keysGate.Dispose();
    }
    private sealed record DiscoverySnapshot(OidcConfiguration Configuration, DateTimeOffset ExpiresAt);
}
