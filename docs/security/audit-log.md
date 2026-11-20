# 命令审计与实体审计

成功命令的哈希链由 `qian.audit` 组件维护。需要记录业务实体变化的宿主可以另外启用实体审计。

## 命令审计哈希链

`Kun.Audit.Abstractions` 定义 `Kun.Audit.IAuditWriter` 和 `Kun.Audit.IAuditReader`。平台的命令运行时把成功命令的审计记录与业务写入放在同一事务。命令返回失败或抛出异常时，事务回滚，命令链只保留已提交的记录。

`Kun.Audit.EntityFrameworkCore` 提供 EF 宿主的实现，`Kun.Audit.Postgres` 提供原生 PostgreSQL 实现。两者保留相同的链协议、数据库 schema 和租户边界。普通读取与验链使用当前租户；平台级方法要求宿主显式提供有权限的插件上下文。组件装配和查询方式见 [EF 提供者](../../src/Kun.Audit.EntityFrameworkCore/README.md)与[原生提供者](../../src/Kun.Audit.Postgres/README.md)。

## 实体审计与读取

非严格的 `KunDbContext` 可从 `Kun.Audit.EntityFrameworkCore` 登记实体拦截器。启动登记会拒绝严格上下文。宿主照常提供租户与已认证主体，按需登记数据库、数据约定、事件目录和要审计的实体。

```csharp
using Kun.Auditing.DependencyInjection;

services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
services.AddKunDataConventions<AppDbContext>();
services.AddKunAuditing<AppDbContext>();
services.AddAuditable<Order>();
services.AddKunEventBus(registry => registry.RegisterAuditing());
```

每个已登记实体的变化生成本库的 `AuditLog` 与一个 `kun.audit.event.v2` Outbox 信封。日志、Outbox 行与信封使用相同 ID。业务实体、日志和 Outbox 在同一个数据库事务中提交。已有 v1 宿主通过 `AddAuditingInterceptor` 使用当前正在保存的上下文暂存 `kun.audit.event.v1`。默认写入端在保存失败或取消时撤回本次添加的审计行，保留同一上下文里其他业务消息以供重试。实体应使用保存前即可确定的主键。

信封的 producer 为 `qian.audit`。命令内保留现有 actor、相关 ID 与 trace；没有命令时使用审计主体或已认证用户。后台保存要用 `EventEnvelopeScope.Begin` 指定实际服务主体。平台级事件的租户为 null。原有 v1、v2 业务字段均在信封的 `payload` 中。

网关的内部令牌用 `sub` 标识当前主体，`principal_kind` 标识主体类型。业务审计可通过 `ICurrentUser` 取得已验证身份，需要记录委托关系时使用验证后的 `Principal.Actor`。链路关联使用当前 Activity 的 TraceId。

应用可在当前租户的业务上下文中，通过 `db.Set<AuditLog>()` 与 EF 查询读取实体日志，继续使用本业务的访问控制和租户过滤。命令链的分页读取及验链分别使用 `IAuditReader.ReadAsync` 和 `VerifyAsync`。

## 数据约定

模型配置最后调用 `modelBuilder.ApplyKunDataConventions()`，保存拦截器通过 `AddKunDataConventions<TContext>()` 自动装配：

- 表、列、键、索引使用 snake_case；未赋值的 Guid 主键生成 v7，显式主键保持不变。
- `IAuditedObject` 新增时写 `created_at/created_by`，修改时仅刷新 `updated_at/updated_by`，禁止改写创建字段。
- 主体支持 `string`、`Guid?`、`long?`、`int?`。匿名或系统操作写 null，不用空 Guid 或 0 冒充主体；无法转换的已认证主体拒绝保存。
- 主体取自上下文创建时解析到的 `ICurrentUser`；宿主没有登记身份适配器时按匿名写 null。数据约定不登记 `ICurrentUser`，身份适配器与它的登记先后不影响结果。
- C# 无约束泛型的 `T?` 不会把值类型自动变成可空值类型。已有 `AggregateRoot<TKey>`（Guid 主体）需显式迁移为 `AggregateRoot<TKey, Guid?>`，并生成相应数据库迁移；旧公共基类保持兼容。
- 审计实体映射 shadow `KunRowVersion` 至 PostgreSQL `xmin`。冲突保留 `DbUpdateConcurrencyException`；需要 Result 边界时调用 `SaveChangesResultAsync()`，映射为平台错误码 `CONFLICT`（409），不自动覆盖获胜写入。

## 从旧包迁移

- 原 `Kun.Audit` 命令管道和本地链存储已归档。依赖命令审计的宿主接入平台命令审计组件；原先由旧管道记录的失败命令不会进入替代链。
- 原实体读端口 `IAuditReader.QueryAsync`、`AuditQuery` 和 `EfAuditReader` 已归档；实体日志继续保存在业务数据库。
- 实体审计代码改为引用 `Kun.Audit.EntityFrameworkCore`。原生链提供者改为引用 `Kun.Audit.Postgres`。组件 ID、链数据与既有 Postgres schema 保持兼容。
