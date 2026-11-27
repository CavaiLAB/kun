> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.MultiTenancy

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.MultiTenancy` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.MultiTenancy`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.MultiTenancy)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.MultiTenancy.Abstractions](/sdk/api/kun/kun-multitenancy-abstractions/index.md)

[Kun.MultiTenancy.DependencyInjection](/sdk/api/kun/kun-multitenancy-dependencyinjection/index.md)

[Kun.MultiTenancy.Middleware](/sdk/api/kun/kun-multitenancy-middleware/index.md)

### Classes

[AsyncLocalTenantContext](/sdk/api/kun/kun-multitenancy/asynclocaltenantcontext.md)

[ITenantContext](/sdk/api/kun/kun-core-multitenancy/itenantcontext.md) implementation backed by an
[`AsyncLocal`](https://learn.microsoft.com/dotnet/api/system.threading.asynclocal-1) so the tenant id flows with the async
call graph across thread hops, task schedulers, and concurrent
parallel tasks. Pair with `TenantContextMiddleware` (or a
background-worker scope that opens a tenant scope explicitly) to
populate the value.

[TenantContextPropagator](/sdk/api/kun/kun-multitenancy/tenantcontextpropagator.md)

复用唯一 ITenantContext；传播字段只携带租户，不授予调用权限。

[TenantRequestBoundary](/sdk/api/kun/kun-multitenancy/tenantrequestboundary.md)

按已认证主身份的 tenant_id 建立本次请求的租户作用域。

### Enums

[TenantRequestMode](/sdk/api/kun/kun-multitenancy/tenantrequestmode.md)

请求入口允许的主体范围。
