# Kun.EntityFrameworkCore

EF Core 10 write-side persistence for the Kun Framework.

## What's in the package

- `KunDbContext` — abstract base with a tenant global filter, a soft-delete
  filter, the Outbox `DbSet<OutboxMessage>`, and the
  `SchemaConsistencyInterceptor` safety net (auto-registered).
- `EfUnitOfWork` — commits aggregate changes, dispatches domain events, and
  flushes the Outbox accumulator in a single transaction.
- `IntegrationEventOutboxAccumulator` — scoped bridge that implements both
  `IIntegrationEventBus` and `IOutboxWriter`, so aggregates and non-aggregate
  enqueuers (e.g. audit interceptors) share a single in-request buffer.
- `TenantRlsTransactionInterceptor` — opt-in (`AddKunTenantRls()`) defence-in-depth
  for Postgres Row-Level Security (P1-4). Emits
  `SELECT set_config('app.current_tenant_id', @tenant, true)` at each
  `TransactionStartedAsync` so RLS policies can enforce isolation even when a
  caller bypasses the EF global query filter.
- `UseKunSchema` extension — wires `MigrationsHistoryTableSchema` through the
  relational provider, for schema-per-service deployments. See
  [docs/guides/schema-per-service.md](../../docs/guides/schema-per-service.md) in the repo.

## 严格上下文与数据会话契约

`KunStrictDbContext` 继承 `KunDbContext`，保留租户/软删过滤和 Outbox 模型。构造期检查 RLS 双拦截器、软删/审计/租户字段拦截器、应用角色检查、`KunMultiTenantModelCacheKeyFactory`、模型定制器（`UseKunDataModel`）及 schema 规则；拒绝外部 EF 服务容器和预编译模型绕过。配置统一从宿主的 options 回调传入（平台是 `AddKunData` 的回调），派生上下文不覆盖 `OnConfiguring`。

schema 规则：`UseKunSchema` 指定的 schema 默认只须是合法的小写标识符（小写字母开头，只含小写字母、数字和下划线，最长 63 个字符）。宿主用 `RequireKunSchemaPrefix(prefix)` 再要求一个前缀，设定后不能改成别的；平台的 `AddKunData` 设 `plugin_`。

`UseKunDataModel` 在业务映射之后强制应用默认过滤，与业务已有条件取交集。命名、Guid v7、审计字段和 xmin 用 Kun 的数据约定；租户列、索引和 DeletedBy 由严格基类补充。DeletedBy 使用与 CreatedBy 一致的可空类型，映射为 deleted_by；没有 CLR 属性时自动创建 shadow property。

`KunApplicationRoleInterceptor` 校验同步/异步新连接的登录角色和当前角色，仅允许非超级用户且无 BYPASSRLS 的应用角色；平台的启动检查还显式检查预打开连接。`KunSoftDeleteInterceptor` 强制在 Kun 审计拦截器之前运行，将直接删除转换为软删除并填充删除审计。装配不满足时抛 `KunDataMisconfiguredException`，错误码 DATA_MISCONFIGURED 由它的 `Code` 公开。

`DataDefinition.Key` 拥有 Data 服务键，`ctx.Data()` 解析当前 scope 可见的 `IDataService`。`OpenAsync<TContext>()` 返回 `IDataSession`，暴露该工作单元的 Context 和 `IOutboxWriter`，继承 `IUnitOfWork` 的 `SaveChangesAsync`；会话通过 `IAsyncDisposable` 释放。实现方负责把业务上下文、通知处理器和事件缓冲绑定在同一 scope，防止多上下文串写。

替换默认累加器的 Outbox 写入端（如事件组件的信封写入端）实现 `IOutboxBufferBinding`，声明自己写入的缓冲器；数据会话只接受写入当前 scope 缓冲器的实现。`IDataContextCatalog` 列出宿主登记的全部业务上下文，供 Outbox 投递等宿主级组件逐个处理。

平台的装配、启动检查与数据服务实现在 [Kun.Data](../Kun.Data/README.md)（`AddKunData<T>`）；建租户表时打开行级安全的迁移 SQL 生成器在 [Kun.EntityFrameworkCore.PostgreSQL](../Kun.EntityFrameworkCore.PostgreSQL/README.md)。

## 多租户（命名空间 `Kun.EntityFrameworkCore.MultiTenancy`）

- `ApplyKunMultiTenantFilters<TContext>(ModelBuilder, TContext)`：给实现 `Kun.Core.Domain.IMultiTenant` 的实体加租户过滤，条件绑定到当前执行查询的 DbContext 的租户号（`IKunMultiTenantDbContext.TenantIdForModelCacheKey`），不把第一次建模时的租户固定进共享模型。`KunDbContext` 已实现 `IKunMultiTenantDbContext` 并自动应用。
- `KunTenantSaveChangesInterceptor`：新增实体没写租户号时按当前租户补上，显式写了的保留。`AddKunMultiTenancyEntityFrameworkCore()`（命名空间 `Kun.EntityFrameworkCore.DependencyInjection`）把它登记为 scoped 服务，由上下文自己 `AddInterceptors`。
- `KunMultiTenantModelCacheKeyFactory`：模型缓存键由上下文类型、租户号与设计时标记组成，共享表与 SchemaPerTenant 都用它。

## Requirements

- .NET 10 SDK `10.0.100` or newer.
- `Npgsql.EntityFrameworkCore.PostgreSQL` as the provider (the framework is
  relational-provider-agnostic, but `UseKunSchema` and the Outbox poller rely
  on Postgres-specific features in practice).
- Central Package Management is on at the solution level; do NOT add a
  `Version="..."` attribute to your `<PackageReference>` — version pins live
  in `Directory.Packages.props`.

## PgBouncer transaction-pooling compatibility

When you front Postgres with PgBouncer in `POOL_MODE=transaction` (the
production default for most Kun deployments), the backend connection is
recycled between transactions. Two Npgsql defaults break under this regime
and MUST be turned off in the connection string:

| Parameter | Required value | Why |
|---|---|---|
| `No Reset On Close` | `true` | PgBouncer itself resets session state; letting Npgsql send `DISCARD ALL` on close wastes a round-trip and can conflict with the pooler. |
| `Max Auto Prepare` | `0` | Auto-prepared statements reference backend-side names that do not survive the connection swap — you'll see `prepared statement does not exist` under load. |

Minimum working PgBouncer connection string:

```
Host=pgbouncer;Port=6432;Database=kun;Username=kun;Password=***;
No Reset On Close=true;Max Auto Prepare=0
```

`tests/Kun.IntegrationTests/Fixtures/PgBouncerPostgresFixture.cs` applies
these automatically; consult it if you stand up your own local PgBouncer
container.

## Row-Level Security + explicit transactions

`TenantRlsTransactionInterceptor` sets the tenant GUC with
`set_config(..., is_local=true)` — the function-form equivalent of
`SET LOCAL`, so the value is scoped to the current transaction. This works
correctly **only** inside an explicit transaction. Under PgBouncer
transaction-pool, the interceptor guarantees each transaction starts fresh
and emits the GUC before business statements run.

Practical consequences:

1. **Always run RLS-protected writes inside a transaction.** The Mediator
   `UnitOfWorkPipelineBehavior` wraps every Command handler in an EF
   `SaveChangesAsync` call, which opens its own transaction, so this is the
   default for Command flows.
2. **Auto-commit SELECTs under PgBouncer do NOT get the GUC.** If you query
   via Dapper against the same Postgres instance without an explicit
   transaction, the RLS policy sees no `app.current_tenant_id` setting. EF
   policies in this repo are written to evaluate
   `current_setting('app.current_tenant_id', true)` with `missing_ok=true`,
   which returns NULL in that case and filters out every row — fail-closed.
3. **Admin paths that genuinely need cross-tenant access** must either not
   opt into `AddKunTenantRls()` or run against a connection that bypasses
   the policy (`ALTER TABLE ... NO FORCE ROW LEVEL SECURITY` on an admin
   role, for example).

The fixture and the acceptance test are in
`tests/Kun.IntegrationTests/Persistence/`:

- `RlsEnforcementTests` — direct-to-Postgres proof that policies block
  cross-tenant reads even with `IgnoreQueryFilters()`.
- `TenantRlsInterceptorPgBouncerTests` (M2) — same proof through PgBouncer
  transaction-pool.
- `PgBouncerCompatTests` — 100-parallel-query stress loop that would fail
  with `prepared statement does not exist` if the compatibility flags were
  ever dropped.

## Dev checklist

- Override `KunDbContext.DefaultSchema` in your service's DbContext AND call
  `.UseKunSchema("your-schema")` on the options builder. Forgetting either
  side trips `SchemaConsistencyInterceptor` at the first save.
- Register services via `AddKunEntityFrameworkCore()` — wires the UoW,
  Outbox accumulator, and the default `InProcessDomainEventDispatcher`.
- Opt into RLS with `AddKunTenantRls()` when the database side has policies
  defined. Not a default because it adds one `set_config` round-trip per
  transaction.
- Never introduce new `PackageReference` to `Kun.Core` — the framework
  enforces this in CI via the dependency gate `scripts/check_deps.py`
  (rule C for packages that declare `KunZeroDependency`).

## See also

- [docs/guides/schema-per-service.md](../../docs/guides/schema-per-service.md) — schema
  override + migrations regeneration walkthrough.
- [docs/guides/read-side-patterns.md](../../docs/guides/read-side-patterns.md) —
  the Dapper read-side pairs with this package via CQRS.
- [AGENTS.md](../../AGENTS.md#运行时约定) §运行时约定 (the Outbox rule) and
  [Kun ADR-015](../../docs/adr/kun/ADR-015-Outbox-pattern.md) — the
  architectural invariants this package enforces.
