> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Security.Authentication

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Security` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Security`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Security)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[KunServiceAuthenticationExtensions](/sdk/api/kun/kun-security-authentication/kunserviceauthenticationextensions.md)

Extension methods for registering Kun S2S JWT Bearer authentication on downstream services.

[KunServiceAuthenticationOptions](/sdk/api/kun/kun-security-authentication/kunserviceauthenticationoptions.md)

Options for configuring S2S JWT Bearer authentication on a downstream service.
Supports both static key sets and dynamic JWKS endpoint resolution.

[PrincipalClaims](/sdk/api/kun/kun-security-authentication/principalclaims.md)

从已认证的主身份读取授权主体，不拼接不同身份的 claim。

[S2sPrincipalContract](/sdk/api/kun/kun-security-authentication/s2sprincipalcontract.md)

已认证的主体快照在外部身份投影、网关签发与服务端接收之间共用的契约。
