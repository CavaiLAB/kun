namespace Kun.Identity;

/// <summary>Authorization code and the original browser transaction's PKCE verifier and nonce.</summary>
public sealed record OidcCodeExchange(string Code, string RedirectUri, string CodeVerifier, string ExpectedNonce)
{
    /// <summary>Issuer supplied by the authorization response (RFC 9207).</summary>
    public string? ResponseIssuer { get; init; }
}

/// <summary>Validated back-channel logout instruction. Subject or SessionId (or both) identifies sessions within Issuer.</summary>
public sealed record OidcLogoutToken(string Issuer, string? Subject, string? SessionId, string TokenId, DateTimeOffset ExpiresAt);
