> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.MultiTenancy.Abstractions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.MultiTenancy` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.MultiTenancy`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.MultiTenancy)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Interfaces

[ITenantContextPropagator](/sdk/api/kun/kun-multitenancy-abstractions/itenantcontextpropagator.md)

事件/作业 JSON 与 gRPC 文本元数据共用的租户传播端口；不替代传输层认证。
