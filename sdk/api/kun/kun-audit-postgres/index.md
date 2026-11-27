> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Audit.Postgres

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Postgres` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Postgres`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Postgres)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[NativeAuditComponent](/sdk/api/kun/kun-audit-postgres/nativeauditcomponent.md)

原生 PostgreSQL 审计的唯一组件入口。

[NativeAuditPlugin](/sdk/api/kun/kun-audit-postgres/nativeauditplugin.md)

服务与审计策略同属 Apply 效果栈，卸载取消并排空在途调用。

[NativeAuditRoles](/sdk/api/kun/kun-audit-postgres/nativeauditroles.md)

离线迁移和在线核验共用的专用角色声明；角色名不是凭据。

[NativeAuditStorage](/sdk/api/kun/kun-audit-postgres/nativeauditstorage.md)

原生审计的离线迁移和固定安全声明。在线 Provider 只校验，不调用迁移。
