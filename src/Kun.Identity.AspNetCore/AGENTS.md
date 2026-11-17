# Kun.Identity.AspNetCore

## Security-sensitive defaults

- ChallengeHelper protects the PKCE/state/nonce/redirect/return transaction with IDataProtector, five-minute expiry and an atomic replay registry. ConsumeAsync validates state and rejects replay. Share protection keys and Redis namespace across replicas.
- AddKunIdentityAspNetCore wires helpers, current-user aliases, antiforgery and an opaque-cookie authentication scheme without replacing host defaults. UseKunCookieAuth runs before authentication/authorization; failed renewal stays anonymous and never recreates a revoked session.
- Cookie-authenticated unsafe methods require antiforgery by default. The signed back-channel endpoint explicitly uses token authentication, not browser CSRF credentials. Log only SHA256 sid hashes and error types, never backend exception content.
- Current complete wiring and migration are in [identity-client.md](../../_import/kun/docs/guides/identity-client.md).

## Wiring

Use the complete flow in [the identity client guide](../../_import/kun/docs/guides/identity-client.md): AddKunIdentity + AddKunIdentityAspNetCore, protected ChallengeHelper, OidcLoginService, and MapKunBackChannelLogout. Do not manually parse challenge cookies or turn raw token responses into browser sessions. Additional IJwtValidator policies on the compatibility SignInHelper constructor still execute after nonce-bound validation.
