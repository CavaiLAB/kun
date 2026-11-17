# Kun.MultiTenancy.EntityFrameworkCore

## Purpose

- `ApplyKunMultiTenantFilters<TContext>(ModelBuilder, TContext)` — a
  ModelBuilder extension that walks the model, finds every entity
  implementing `Kun.Core.Domain.IMultiTenant`, and attaches a
  predicate bound to the executing DbContext's tenant ID so
  cross-tenant reads are impossible through the EF pipeline.

## Wiring

```csharp
services.AddKunMultiTenancy(opts => opts.UseJwtClaim("tenant_id"));

// In your DbContext.OnModelCreating:
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyKunMultiTenantFilters(this); // DbContext implements IKunMultiTenantDbContext
}
```

DbContext 重载把租户条件绑定到当前执行查询的 DbContext，不保留第一次构建模型时的租户访问器。旧 `ITenantContext` 重载标记 KUNTENANT001 并在构建模型时明确拒绝；调用者必须迁移到 DbContext 重载，不能用捕获常量值代替（默认共享模型下仍会错误固定到首个租户）。

## For AI Agents

生产装配使用 `options.Production = true`；请求头与默认租户仅供显式开发兼容模式使用。后台通过 `ITenantContext.BeginScope` 回灌，详情见 `docs/security/multi-tenancy-rls.md`。
