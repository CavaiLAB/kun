# Kun.EntityFrameworkCore

## Purpose

Write-side persistence for Kun Framework: `KunDbContext` base with automatic `IMultiTenant` tenant filters and `ISoftDelete` soft-delete filters, atomic Outbox integration, and a unit-of-work pipeline behavior that commits aggregate changes + integration events + domain events in a single transaction.

## Key Files

| File | Description |
|------|-------------|
| `KunDbContext.cs` | Base DbContext with `IMultiTenant` + `ISoftDelete` global query filters and Outbox DbSet |
| `OutboxMessageConfiguration.cs` | EF mapping for `OutboxMessage` (no ORM attrs on the entity) |
| `IUnitOfWork.cs` / `EfUnitOfWork.cs` | Atomic SaveChanges: aggregates + outbox + domain-event dispatch |
| `IntegrationEventOutboxAccumulator.cs` | `IIntegrationEventBus` impl — serializes events, captures TraceParent, appends to buffer |
| `Behaviors/UnitOfWorkPipelineBehavior.cs` | Mediator behavior: calls `SaveChangesAsync` after handler returns |
| `NoOpDomainEventDispatcher.cs` | Default no-op dispatcher (users replace with their own implementation) |
| `Interceptors/TenantRlsConnectionInterceptor.cs` | Defence-in-depth: pushes `ITenantContext.TenantId` into `SET app.current_tenant_id` on PG connection open; RESET on close. Opt-in via `opts.AddInterceptors(...)` on the caller's `DbContextOptionsBuilder`. |
| `DependencyInjection/KunEntityFrameworkCoreServiceCollectionExtensions.cs` | `AddKunEntityFrameworkCore` DI entry point |
| `KunStrictDbContext.cs` | 严格上下文基类：构造期装配检查、schema 规则、模型补全（租户列与索引、DeletedBy、与业务过滤取交集） |
| `KunDataModelExtensions.cs` | `UseKunDataModel`（严格模型定制器）与 `RequireKunSchemaPrefix` |
| `DataContracts.cs` | 数据会话契约：`DataDefinition`、`IDataService`、`IDataSession<T>`、`IOutboxBufferBinding`、`IDataContextCatalog` |
| `KunApplicationRoleInterceptor.cs` / `KunSoftDeleteInterceptor.cs` | 应用角色检查；直接删除转软删除 |
| `KunDataMisconfiguredException.cs` | 装配错误，错误码 DATA_MISCONFIGURED |
| `MultiTenancy/` | 租户过滤 `ApplyKunMultiTenantFilters`、租户字段拦截器、模型缓存键、SchemaPerTenant / DatabasePerTenant 与 RLS 的登记 |
| `DependencyInjection/KunMultiTenancyEntityFrameworkCoreServiceCollectionExtensions.cs` | `AddKunMultiTenancyEntityFrameworkCore`：登记租户字段拦截器 |

## For AI Agents

### Working In This Directory
- **Not AOT-compatible** (the csproj does not set `IsAotCompatible`) — EF Core uses runtime expression trees. Do not add AOT annotations.
- All persistence mapping lives in `IEntityTypeConfiguration<T>` — never add `[Table]`/`[Column]`/`[Key]` attributes to domain types in `Kun.Core`.
- `EfUnitOfWork.SaveChangesAsync` is the single transaction boundary. Domain event collection, outbox flush, and `DbContext.SaveChangesAsync` must stay in one call — do not split.
- `UnitOfWorkPipelineBehavior` should be registered after validation behaviors, before logging behaviors.
- `IMultiTenant` and `ISoftDelete` are both in the `Kun.Core.Domain` namespace (not `Kun.Core.MultiTenancy`).
- 领域事件在 `DbContext.SaveChangesAsync` 前、同一 scope 内派发；派发生成的集成事件与业务一起落库。默认 Outbox 存储用 PostgreSQL SKIP LOCKED 持有批次行锁。
- `IntegrationEventOutboxAccumulator` uses `JsonSerializer.Serialize<TEvent>` (generic overload) — NOT the non-generic overload, to satisfy CA2263.
- `TenantRlsConnectionInterceptor` is **opt-in**. The EF global query filter in `KunDbContext` is the first isolation layer; the interceptor is a *second* layer that pairs with RLS policies defined on the database schema. Wiring is explicit — `AddKunEntityFrameworkCore` registers the interceptor as Scoped but does not attach it to any `DbContextOptionsBuilder`. Callers do that per context: `opts.AddInterceptors(sp.GetRequiredService<TenantRlsConnectionInterceptor>())`. See `docs/security/multi-tenancy-rls.md` for the policy template.

`AddKunTenantRls<TContext>()` 自动装配 RLS 双拦截器与 `KunMultiTenantModelCacheKeyFactory`；上文手动接线说明仅适用于无泛型旧入口。`KunDbContext` 已实现 `IKunMultiTenantDbContext`。

租户查询过滤器通过 DbContext 自身的 `TenantIdForModelCacheKey` 读取当前访问器；不能把 `ITenantContext` 实例常量存进共享模型，后者会在多宿主或 scoped 访问器场景复用已经失效的租户上下文。

- 修改严格基类的隔离检查必须同步 `_import/qian/tests/Kun.Data.Tests` 与本项目的测试，禁止为了通过测试关闭校验；启动安全范围见本目录 README。
- DeletedBy 沿用 CreatedBy 的可空用户键类型；直接 Remove 必须转软删除，不能绕过删除审计。

### 多租户

`ApplyKunMultiTenantFilters<TContext>(ModelBuilder, TContext)` 遍历模型，给每个实现 `Kun.Core.Domain.IMultiTenant` 的实体加租户过滤，条件绑定到当前执行查询的 DbContext，不保留第一次构建模型时的租户访问器。

```csharp
services.AddKunMultiTenancy();

// DbContext.OnModelCreating：
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyKunMultiTenantFilters(this); // DbContext 实现 IKunMultiTenantDbContext
}
```

HTTP 入口从已认证主身份的 `tenant_id` 建立作用域。后台通过 `ITenantContext.BeginScope` 回灌，详情见 `docs/security/multi-tenancy-rls.md`。

### Testing Requirements
Integration tests require Docker + Testcontainers and are tracked separately (M2 2.4).

## Dependencies

### Internal
- `Kun.Core` — `IAggregateRoot`, `IDomainEventDispatcher`, `ITenantContext`, `IMultiTenant`, `ISoftDelete`
- `Kun.EventBus` — `OutboxMessage`, `IntegrationEventBuffer`, `IEventTypeRegistry`, `IIntegrationEventBus`
- `Kun.Mediator` — `IPipelineBehavior`
- `Kun.Runtime` — 数据服务键与 `ctx.Data()` 所在的组件运行时
- `Kun.Errors` — `KunDataMisconfiguredException` 的错误码

### External
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Logging.Abstractions`
