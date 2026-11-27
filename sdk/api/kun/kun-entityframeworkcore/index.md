> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.EntityFrameworkCore

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.EntityFrameworkCore.Behaviors](/sdk/api/kun/kun-entityframeworkcore-behaviors/index.md)

[Kun.EntityFrameworkCore.Conventions](/sdk/api/kun/kun-entityframeworkcore-conventions/index.md)

[Kun.EntityFrameworkCore.DependencyInjection](/sdk/api/kun/kun-entityframeworkcore-dependencyinjection/index.md)

[Kun.EntityFrameworkCore.Extensions](/sdk/api/kun/kun-entityframeworkcore-extensions/index.md)

[Kun.EntityFrameworkCore.Interceptors](/sdk/api/kun/kun-entityframeworkcore-interceptors/index.md)

[Kun.EntityFrameworkCore.MultiTenancy](/sdk/api/kun/kun-entityframeworkcore-multitenancy/index.md)

[Kun.EntityFrameworkCore.PostgreSQL](/sdk/api/kun/kun-entityframeworkcore-postgresql/index.md)

### Classes

[DataDefinition](/sdk/api/kun/kun-entityframeworkcore/datadefinition.md)

数据 seam 的稳定服务键；业务组件只依赖 Definition。

[EfUnitOfWork](/sdk/api/kun/kun-entityframeworkcore/efunitofwork.md)

EF Core-backed [IUnitOfWork](/sdk/api/kun/kun-entityframeworkcore/iunitofwork.md) that atomically persists aggregate changes,
dispatches domain events in-process, and writes integration events to the Outbox.

[InProcessDomainEventDispatcher](/sdk/api/kun/kun-entityframeworkcore/inprocessdomaineventdispatcher.md)

Dispatches domain events to in-process Mediator notification handlers.
Domain events that also implement [INotification](/sdk/api/kun/kun-mediator/inotification.md) are routed to all
registered [INotificationHandler\<TNotification>](/sdk/api/kun/kun-mediator/inotificationhandler-1.md)s via [IPublisher](/sdk/api/kun/kun-mediator/ipublisher.md).
Events that do not implement [INotification](/sdk/api/kun/kun-mediator/inotification.md) are silently skipped.

[KunApplicationRoleInterceptor](/sdk/api/kun/kun-entityframeworkcore/kunapplicationroleinterceptor.md)

在线应用连接只能使用受 RLS 约束的 PostgreSQL 角色；迁移管理员通过独立连接执行 DDL。

[KunDataMisconfiguredException](/sdk/api/kun/kun-entityframeworkcore/kundatamisconfiguredexception.md)

在构造上下文或启动宿主时拒绝不安全的数据层配置。

[KunDataModelExtensions](/sdk/api/kun/kun-entityframeworkcore/kundatamodelextensions.md)

安装不可由派生上下文遗漏的 Kun 模型约定。

[KunDbContext](/sdk/api/kun/kun-entityframeworkcore/kundbcontext.md)

Base EF Core DbContext for Kun Framework services.

[KunSoftDeleteInterceptor](/sdk/api/kun/kun-entityframeworkcore/kunsoftdeleteinterceptor.md)

将直接删除改为软删除，填充删除主体与时间；必须在 Kun 审计字段拦截器之前运行。

[KunStrictDbContext](/sdk/api/kun/kun-entityframeworkcore/kunstrictdbcontext.md)

带安全装配门禁的数据上下文。租户过滤与 Outbox 继承 Kun 的行为。

[NoOpDomainEventDispatcher](/sdk/api/kun/kun-entityframeworkcore/noopdomaineventdispatcher.md)

Default no-op domain event dispatcher. Users replace this with a real implementation
(e.g. one backed by `Kun.Mediator.IPublisher`) by registering their own
[IDomainEventDispatcher](/sdk/api/kun/kun-core-domain/idomaineventdispatcher.md) before calling `AddKunEntityFrameworkCore`.

[OutboxMessageConfiguration](/sdk/api/kun/kun-entityframeworkcore/outboxmessageconfiguration.md)

EF Core mapping for [OutboxMessage](/sdk/api/kun/kun-eventbus-outbox/outboxmessage.md).

### Interfaces

[IDataContextCatalog](/sdk/api/kun/kun-entityframeworkcore/idatacontextcatalog.md)

宿主经 AddKunData 登记的全部业务上下文；Outbox 投递等宿主级组件据此逐个处理各上下文的表。

[IDataService](/sdk/api/kun/kun-entityframeworkcore/idataservice.md)

创建独立工作单元；不把不同业务上下文绑定到一个全局 KunDbContext。

[IDataSession\<TContext>](/sdk/api/kun/kun-entityframeworkcore/idatasession-1.md)

同一上下文中的业务变更、领域通知与 Outbox 保存边界。

[IUnitOfWork](/sdk/api/kun/kun-entityframeworkcore/iunitofwork.md)

Atomic unit-of-work boundary for command handlers.
