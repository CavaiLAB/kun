# Kun.MultiTenancy

## Canonical tenant string

JWT `tenant_id`
claims, `X-Tenant-Id` header values, and downstream `TenantId`
columns are all carried as strings so that Guid / integer / slug
tenants coexist without framework changes.

## Wiring

```csharp
services.AddKunMultiTenancy(opts =>
{
    opts.UseJwtClaim("tenant_id");              // primary — S2S JWT
    opts.UseHttpHeader("X-Tenant-Id");    // fallback for admin tooling
    opts.FallbackTenantId = "default";    // last resort
});
```

## For AI Agents

生产装配使用 `options.Production = true`；请求头与默认租户仅供显式开发兼容模式使用。后台通过 `ITenantContext.BeginScope` 回灌，详情见 `docs/security/multi-tenancy-rls.md`。
