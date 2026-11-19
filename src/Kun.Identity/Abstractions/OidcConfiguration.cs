namespace Kun.Identity;

/// <summary>Subset of an OpenID Connect discovery document surfaced by <see cref="IOidcClient.DiscoverAsync"/>.</summary>
/// <param name="Issuer">Canonical issuer URL as published by the provider.</param>
/// <param name="AuthorizationEndpoint">URL for starting the authorization-code flow.</param>
/// <param name="TokenEndpoint">URL for token exchange.</param>
/// <param name="UserInfoEndpoint">URL for fetching user claims; <see langword="null"/> when the provider does not publish one.</param>
/// <param name="JwksUri">URL for the JSON Web Key Set used to validate issued tokens.</param>
public sealed record OidcConfiguration(
    string Issuer,
    string AuthorizationEndpoint,
    string TokenEndpoint,
    string? UserInfoEndpoint,
    string JwksUri)
{
    internal bool UsesPostAuthentication { get; init; }
    /// <summary>Whether the provider includes the issuer in authorization responses (RFC 9207).</summary>
    public bool RequiresIssuerParameter { get; internal init; }
}
