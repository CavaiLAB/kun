# Kun.Security

`AddKunSecurity` 当前将 `ITenantContext` 注册为 `AsyncLocalTenantContext`，`JwtTenantContext` 仅以 `ITenantIdSource` 提供已认证的 `tenant_id`；显式后台作用域优先。

`AddKunSecurity` 以 `TryAdd` 登记 `ITenantContext` 与 `ICurrentUser`。数据约定（`AddKunDataConventions<TContext>`）与实体审计（`AddKunAuditing<TContext>`）不占这两个端口，宿主先登记数据组件再调用 `AddKunSecurity` 同样生效；详见 `docs/architecture/when-to-use-currentuser.md` 的「端口登记与次序」。
