> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Identity.Client

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Identity.AspNetCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Identity.AspNetCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Identity.AspNetCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[IdentitySessionTokens](/sdk/api/kun/kun-identity-client/identitysessiontokens.md)

当前浏览器会话的访问令牌。到期前用 refresh token 换新：同一进程内同一会话只发一次刷新请求，跨副本用 Kun 会话注册表的一次性标记互斥
（发行方开启了 refresh 轮换与复用检测，并发复用会吊销整族）；刷新结果只续写仍然存活的会话，已登出的会话不会因刷新复活；刷新失败即结束会话。
