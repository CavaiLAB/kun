> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Identity

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Identity` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Identity`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Identity)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.Identity.AspNetCore](/sdk/api/kun/kun-identity-aspnetcore/index.md)

[Kun.Identity.Client](/sdk/api/kun/kun-identity-client/index.md)

[Kun.Identity.Configuration](/sdk/api/kun/kun-identity-configuration/index.md)

[Kun.Identity.DependencyInjection](/sdk/api/kun/kun-identity-dependencyinjection/index.md)

[Kun.Identity.Implementations](/sdk/api/kun/kun-identity-implementations/index.md)

### Classes

[OidcCodeExchange](/sdk/api/kun/kun-identity/oidccodeexchange.md)

Authorization code and the original browser transaction's PKCE verifier and nonce.

[OidcConfiguration](/sdk/api/kun/kun-identity/oidcconfiguration.md)

Subset of an OpenID Connect discovery document surfaced by [IOidcClient.DiscoverAsync](/sdk/api/kun/kun-identity/ioidcclient.md#Kun_Identity_IOidcClient_DiscoverAsync_System_Threading_CancellationToken_).

[OidcLogoutToken](/sdk/api/kun/kun-identity/oidclogouttoken.md)

Validated back-channel logout instruction. Subject or SessionId (or both) identifies sessions within Issuer.

[OidcTokenResponse](/sdk/api/kun/kun-identity/oidctokenresponse.md)

Result of an OIDC authorization-code exchange.

[SessionData](/sdk/api/kun/kun-identity/sessiondata.md)

Serialized payload held by [ISessionStore](/sdk/api/kun/kun-identity/isessionstore.md) implementations.
The session cookie carries only an opaque session id; the record
below is what that id resolves to.

### Interfaces

[IAuthenticatedUser](/sdk/api/kun/kun-identity/iauthenticateduser.md)

Browser-flow extension of [ICurrentUser](/sdk/api/kun/kun-core-security/icurrentuser.md). Adds the
multi-tenant identifier, profile claims, and direct access to the
ambient [`ClaimsPrincipal`](https://learn.microsoft.com/dotnet/api/system.security.claims.claimsprincipal) that the
`Kun.Identity.AspNetCore` cookie middleware populates.

[IJwtValidator](/sdk/api/kun/kun-identity/ijwtvalidator.md)

Validates a Bearer JWT against the configured issuer / audience /
signing keys and returns the resulting [`ClaimsPrincipal`](https://learn.microsoft.com/dotnet/api/system.security.claims.claimsprincipal)
on success. Returns [null](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null) when validation fails —
callers decide whether to surface a 401 or fall through.

[IOidcClient](/sdk/api/kun/kun-identity/ioidcclient.md)

OpenID Connect client for discovery, PKCE code exchange, token refresh,
client credentials and signed back-channel logout validation.

[ISessionRegistry](/sdk/api/kun/kun-identity/isessionregistry.md)

Atomic session lifecycle operations. Renewing a revoked session must never recreate it.

[ISessionStore](/sdk/api/kun/kun-identity/isessionstore.md)

Backend for cookie-based session data. Implementations ship in
`Kun.Identity.Implementations.Session` (in-memory for tests /
single-process dev; Redis-backed via `Kun.Caching` for
production). Implementations must be safe for concurrent use from
multiple request threads.
