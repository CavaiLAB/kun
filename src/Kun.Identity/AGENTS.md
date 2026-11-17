# Kun.Identity

## Purpose

| Abstraction | Purpose |
|-------------|---------|
| `IOidcClient` | Discover an OIDC provider's metadata and exchange an authorization code for tokens. Validates PKCE/nonce-bound code flow, refresh, machine credentials and signed logout tokens. |

## Relationship to ADR-001 / Kun.Security

S1-KUN-004 retires both legacy issuer surfaces with KUNS2S001. Use Kun.Gateway.S2sTokenIssuer and its canonical payload/JWKS ring. The Identity module retains OIDC validation and session responsibilities, not a competing issuer. Current usage and migration: [identity client guide](../../_import/kun/docs/guides/identity-client.md).

## For AI Agents

- Code exchange must use OidcCodeExchange with original PKCE verifier/nonce. The legacy signature fails closed; tokens are signed, issuer/audience/lifetime checked. Refresh requires expected subject. Device flow remains outside this task.
- Browser sessions use ISessionRegistry atomic renewal/revocation/replay. Redis uses a direct connection and application prefix; never use a memory fallback for session security. The byte-cache constructor is compatibility-only (KUNSESSION001).
