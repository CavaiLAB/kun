> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Security

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Security.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Security.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Security.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.Security.Authentication](/sdk/api/kun/kun-security-authentication/index.md)

[Kun.Security.DependencyInjection](/sdk/api/kun/kun-security-dependencyinjection/index.md)

[Kun.Security.Extensions](/sdk/api/kun/kun-security-extensions/index.md)

[Kun.Security.Http](/sdk/api/kun/kun-security-http/index.md)

[Kun.Security.Jwks](/sdk/api/kun/kun-security-jwks/index.md)

[Kun.Security.Security](/sdk/api/kun/kun-security-security/index.md)

### Classes

[DelegationActor](/sdk/api/kun/kun-security/delegationactor.md)

RFC 8693 委托链，保持与 v0 Actor 定义同形。

[IdentityClaimNames](/sdk/api/kun/kun-security/identityclaimnames.md)

身份 claim 的唯一词汇表；OIDC 用户令牌与冻结 S2S 令牌的字段集合分别列出。

[Principal](/sdk/api/kun/kun-security/principal.md)

冻结 S2S payload 的 DTO，不承担 JWT 验签、发行或权限判定。

[PrincipalKindConverter](/sdk/api/kun/kun-security/principalkindconverter.md)

主体类型的 JSON 转换器：只收 user、service、agent 三个小写字符串，数字与其它值一律报错。

[S2sTokenOptions](/sdk/api/kun/kun-security/s2stokenoptions.md)

Configuration for S2S JWT issuance and validation.
Algorithm is always asymmetric (RS256/ES256) — HS256 is rejected per zero-trust policy.

### Enums

[PrincipalKind](/sdk/api/kun/kun-security/principalkind.md)

统一主体模型；Agent 在 S1 只预留，不因此开放签发。
