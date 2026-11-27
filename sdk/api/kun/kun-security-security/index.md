> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Security.Security

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Security` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Security`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Security)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[JwtCurrentUser](/sdk/api/kun/kun-security-security/jwtcurrentuser.md)

[ICurrentUser](/sdk/api/kun/kun-core-security/icurrentuser.md) implementation that reads `sub`, `name`,
and `roles` claims from the authenticated [`ClaimsPrincipal`](https://learn.microsoft.com/dotnet/api/system.security.claims.claimsprincipal)
on the current HTTP request.
Also implements [ITracedCurrentUser](/sdk/api/kun/kun-core-security/itracedcurrentuser.md) to expose the `trace_id` claim
propagated from the gateway S2S JWT for OpenTelemetry trace continuity.
