> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Kun 框架

:::info 发布版本来源
本页由本仓库的指南与模块目录生成，源码入口为 [`README.md`](https://github.com/CavaiLAB/kun/blob/v2026.11.24/README.md)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

> 匠心智能体平台的企业级后端框架：为 Agent 时代构建的多租户平台框架，.NET 10。

Kun 提供 .NET 内核与平台组件。[产品族](/start/family.md)列出这两档的仓库与版本来源。本节说明本仓库的 **v2026.11.24** 正式版本。版本取根目录 `CHANGELOG.md` 里最新的正式发布段。[Kun 源码与文档](https://github.com/CavaiLAB/kun)由同一仓库维护。

## 本节内容

- [内核与平台组件](/sdk/kun/with-qian.md)列出平台组件使用的模块、公共 API、宿主装配和历史版本
- 上手：[Getting Started with Kun Framework](/sdk/kun/getting-started.md)
- 模块地图：[模块地图](/sdk/kun/modules.md)
- 指南：[Gateway 限流策略指南](/sdk/kun/guides/gateway-rate-limit.md)、[Identity 客户端、浏览器会话与迁移](/sdk/kun/guides/identity-client.md)、[Outbox + Integration Events 指南](/sdk/kun/guides/outbox-and-integration-events.md)、[NuGet 包迁移指南](/sdk/kun/guides/package-migration.md)、[Read-Side Patterns: Writing Query Handlers](/sdk/kun/guides/read-side-patterns.md)、[SaaS 多租户策略选型指南](/sdk/kun/guides/saas-multitenancy.md)、[Schema-per-service with KunDbContext](/sdk/kun/guides/schema-per-service.md)、[Tracing Tour: Reading Distributed Traces in the Aspire Dashboard](/sdk/kun/guides/tracing-tour.md)、[Command 校验指南（Kun.Validation）](/sdk/kun/guides/validating-commands.md)
- 安全：[命令审计与实体审计](/sdk/kun/security/audit-log.md)、[Multi-Tenancy: Row-Level Security as Defence in Depth](/sdk/kun/security/multi-tenancy-rls.md)、[Service-to-Service JWT Authentication](/sdk/kun/security/s2s-jwt.md)
- 设计说明与 ADR：[ADR-001 Do Not Introduce a New Kun.Identity Module](/sdk/kun/architecture/adr-001-kun-identity-module-decision.md)、[ADR-003 Introduce Kun.Audit and Kun.Audit.EntityFrameworkCore Modules](/sdk/kun/architecture/adr-003-kun-audit-module.md)、[ADR-004 Introduce Kun.Observability Module](/sdk/kun/architecture/adr-004-kun-observability-module.md)、[ADR-005 Introduce Kun.Caching Module](/sdk/kun/architecture/adr-005-kun-caching-module.md)、[ADR-006 Introduce Kun.RateLimit Module](/sdk/kun/architecture/adr-006-kun-ratelimit-module.md)、[ADR-007 Introduce Kun.MultiTenancy and Kun.MultiTenancy.EntityFrameworkCore Modules](/sdk/kun/architecture/adr-007-kun-multitenancy-module.md)、[ADR-008 Introduce Kun.Identity Module — Reversing ADR-001](/sdk/kun/architecture/adr-008-kun-identity-module.md)、[ADR-009 Introduce Kun.Configuration Module](/sdk/kun/architecture/adr-009-kun-configuration-module.md)、[ADR-010 Add Cookie Session + IAuthenticatedUser to Kun.Identity](/sdk/kun/architecture/adr-010-kun-identity-cookie-session.md)、[ADR-011 Result<T> composition extensions (sync + async)](/sdk/kun/architecture/adr-011-result-async-extensions.md)、[ADR-012 Introduce Kun.Testing module](/sdk/kun/architecture/adr-012-kun-testing-module.md)、[ADR-013 Baggage-driven activity enricher for Kun.Observability](/sdk/kun/architecture/adr-013-kun-observability-enricher.md)、[ADR-014 Result-returning validation pipeline behavior](/sdk/kun/architecture/adr-014-validation-result-behavior.md)、[ADR-015 Outbox pattern design for Kun.EventBus](/sdk/kun/architecture/adr-015-outbox-pattern.md)、[ADR-016 配置可靠性、权限端口与能力目录](/sdk/kun/architecture/adr-016-platform-ports.md)、[ADR-S02 Introduce Kun.Sandbox Module — E2B-compatible sandbox abstraction for Kun consumers](/sdk/kun/architecture/adr-s02-kun-sandbox-module.md)、[ADR P2-D SaaS 多租户 3-策略架构](/sdk/kun/architecture/adr-p2d-multitenancy.md)、[When to use ICurrentUser vs IAuthenticatedUser](/sdk/kun/architecture/when-to-use-currentuser.md)、[Why Kun Splits Writes from Reads](/sdk/kun/architecture/why-cqrs.md)、[Why Kun Uses the Outbox Pattern](/sdk/kun/architecture/why-outbox.md)
- 白皮书：[Kun Framework 架构设计与产品白皮书](/sdk/kun/whitepaper.md)
- [变更日志](/sdk/kun/releases/index.md)；发布说明：[Kun 0.1.0 历史发布说明](/sdk/kun/releases/v0-1-0-phase1.md)
- [API 参考](/sdk/api/kun/index.md)：DocFX 从 42 个 `Kun.*` 包生成
