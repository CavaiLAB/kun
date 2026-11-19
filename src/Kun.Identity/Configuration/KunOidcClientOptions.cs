namespace Kun.Identity.Configuration;

/// <summary>Per-provider configuration for <c>KunOidcClient</c>.</summary>
public sealed class KunOidcClientOptions
{
    /// <summary>Logical name for this client; used as the DI key.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Provider authority — used to resolve the OIDC discovery
    /// document at <c>{Authority}/.well-known/openid-configuration</c>.
    /// The issuer in discovery must match this value exactly.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>OIDC client id registered with the provider.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>OIDC client secret. Leave empty when the provider uses PKCE / public clients.</summary>
    public string ClientSecret { get; set; } = string.Empty;
    /// <summary>Maximum metadata cache age. Defaults to five minutes; zero forces a refresh for every operation.</summary>
    public TimeSpan DiscoveryRefreshInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Require HTTPS for provider metadata and advertised endpoints. Disable only for local test providers.</summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>Accepted ID token signing algorithms. Only a nonempty subset of the RS, PS and ES families at 256, 384 and 512 bits is allowed.</summary>
    public IReadOnlyList<string> AllowedSigningAlgorithms { get; set; } = Implementations.KunOidcClient.SigningAlgorithms;

    /// <summary>Accept additional audiences when the authorized party is this client.</summary>
    public bool AllowAdditionalAudiences { get; set; }

    /// <summary>Clock tolerance for expiry, not-before and issued-at, from zero to five minutes.</summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(2);
}
