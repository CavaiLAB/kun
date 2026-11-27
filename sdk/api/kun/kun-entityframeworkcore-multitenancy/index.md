> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.EntityFrameworkCore.MultiTenancy

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[DatabasePerTenantProvisioner\<TContext>](/sdk/api/kun/kun-entityframeworkcore-multitenancy/databasepertenantprovisioner-1.md)

[ITenantProvisioner](/sdk/api/kun/kun-core-multitenancy/itenantprovisioner.md) for the
[TenantIsolation.Database](/sdk/api/kun/kun-core-multitenancy/tenantisolation.md#Kun_Core_MultiTenancy_TenantIsolation_Database) strategy. Runs EF Core
migrations against the tenant's dedicated database on [DatabasePerTenantProvisioner\<TContext>.ProvisionAsync](/sdk/api/kun/kun-entityframeworkcore-multitenancy/databasepertenantprovisioner-1.md#Kun_EntityFrameworkCore_MultiTenancy_DatabasePerTenantProvisioner_1_ProvisionAsync_Kun_Core_MultiTenancy_TenantInfo_System_Threading_CancellationToken_).
[DatabasePerTenantProvisioner\<TContext>.DeprovisionAsync](/sdk/api/kun/kun-entityframeworkcore-multitenancy/databasepertenantprovisioner-1.md#Kun_EntityFrameworkCore_MultiTenancy_DatabasePerTenantProvisioner_1_DeprovisionAsync_System_Guid_System_Threading_CancellationToken_) is a no-op by default because dropping a tenant database
is an operations concern (needs admin credentials and often a separate control plane) —
the framework does not take that responsibility.

[KunMultiTenancyModelBuilderExtensions](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancymodelbuilderextensions.md)

[`ModelBuilder`](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.modelbuilder) extensions that attach the Kun multi-tenant
global query filter to every [IMultiTenant](/sdk/api/kun/kun-core-domain/imultitenant.md) entity.

[KunMultiTenancyOptions](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md)

Configuration for `AddKunMultiTenancy`. Selects the isolation strategy and tunes
the few knobs the framework exposes across all three strategies.

[KunMultiTenancyServiceCollectionExtensions](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyservicecollectionextensions.md)

DI entry point for Kun's SaaS multi-tenancy strategy selection. Dispatches based on
[KunMultiTenancyOptions.Strategy](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions_Strategy).

[KunMultiTenantModelCacheKeyFactory](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenantmodelcachekeyfactory.md)

[`IModelCacheKeyFactory`](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.infrastructure.imodelcachekeyfactory) that emits a distinct compiled
model per tenant when the [`DbContext`](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext) implements
[IKunMultiTenantDbContext](/sdk/api/kun/kun-entityframeworkcore-multitenancy/ikunmultitenantdbcontext.md). Falls back to EF Core's
default key shape (context type + design-time flag) otherwise, so
consumers who forget to opt in get the same behaviour they had
before. The [TenantIsolation.Schema](/sdk/api/kun/kun-core-multitenancy/tenantisolation.md#Kun_Core_MultiTenancy_TenantIsolation_Schema)
strategy relies on the same key: each tenant's schema-specific model is compiled separately,
and a [null](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null) tenant (no scoped tenant) shares one model.

[KunTenantSaveChangesInterceptor](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kuntenantsavechangesinterceptor.md)

EF Core [`SaveChangesInterceptor`](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.savechangesinterceptor) that walks newly added
[IMultiTenant](/sdk/api/kun/kun-core-domain/imultitenant.md) entities and, when their
[IMultiTenant.TenantId](/sdk/api/kun/kun-core-domain/imultitenant.md#Kun_Core_Domain_IMultiTenant_TenantId) is [null](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null) or
empty, injects the ambient [ITenantContext.TenantId](/sdk/api/kun/kun-core-multitenancy/itenantcontext.md#Kun_Core_MultiTenancy_ITenantContext_TenantId).
Explicit values must be non-empty UUIDs in lowercase D format.
Valid explicit values take precedence over the ambient tenant.

[OptionsBasedTenantConnectionResolver](/sdk/api/kun/kun-entityframeworkcore-multitenancy/optionsbasedtenantconnectionresolver.md)

Default [IDbConnectionResolver](/sdk/api/kun/kun-entityframeworkcore-multitenancy/idbconnectionresolver.md) backed by [TenantsConnectionOptions](/sdk/api/kun/kun-entityframeworkcore-multitenancy/tenantsconnectionoptions.md).
Reads the current snapshot on every call via [`IOptionsMonitor`](https://learn.microsoft.com/dotnet/api/microsoft.extensions.options.ioptionsmonitor-1)
so on-the-fly config reloads reach new tenants without restarting the host.

[SchemaPerTenantProvisioner\<TContext>](/sdk/api/kun/kun-entityframeworkcore-multitenancy/schemapertenantprovisioner-1.md)

[ITenantProvisioner](/sdk/api/kun/kun-core-multitenancy/itenantprovisioner.md) implementation for the
[TenantIsolation.Schema](/sdk/api/kun/kun-core-multitenancy/tenantisolation.md#Kun_Core_MultiTenancy_TenantIsolation_Schema) strategy. On
[SchemaPerTenantProvisioner\<TContext>.ProvisionAsync](/sdk/api/kun/kun-entityframeworkcore-multitenancy/schemapertenantprovisioner-1.md#Kun_EntityFrameworkCore_MultiTenancy_SchemaPerTenantProvisioner_1_ProvisionAsync_Kun_Core_MultiTenancy_TenantInfo_System_Threading_CancellationToken_) executes `CREATE SCHEMA IF NOT EXISTS "{schema}"`
and runs EF Core migrations under that schema; on [SchemaPerTenantProvisioner\<TContext>.DeprovisionAsync](/sdk/api/kun/kun-entityframeworkcore-multitenancy/schemapertenantprovisioner-1.md#Kun_EntityFrameworkCore_MultiTenancy_SchemaPerTenantProvisioner_1_DeprovisionAsync_System_Guid_System_Threading_CancellationToken_)
drops the schema when [KunMultiTenancyOptions.AllowDestructiveDeprovision](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions_AllowDestructiveDeprovision)
is enabled, else throws.

[TenantsConnectionOptions](/sdk/api/kun/kun-entityframeworkcore-multitenancy/tenantsconnectionoptions.md)

Static tenant → connection-string mapping consumed by
[OptionsBasedTenantConnectionResolver](/sdk/api/kun/kun-entityframeworkcore-multitenancy/optionsbasedtenantconnectionresolver.md). Suitable for small deployments
where the full tenant list is known at configuration time (appsettings.json or
Kubernetes ConfigMap).

### Interfaces

[IDbConnectionResolver](/sdk/api/kun/kun-entityframeworkcore-multitenancy/idbconnectionresolver.md)

Resolves the connection string for a given tenant. Used by the
[TenantIsolation.Database](/sdk/api/kun/kun-core-multitenancy/tenantisolation.md#Kun_Core_MultiTenancy_TenantIsolation_Database) strategy
to dispatch DbContext construction to the tenant's dedicated database.

[IKunMultiTenantDbContext](/sdk/api/kun/kun-entityframeworkcore-multitenancy/ikunmultitenantdbcontext.md)

Marker interface for `DbContext`s that want the
[KunMultiTenantModelCacheKeyFactory](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenantmodelcachekeyfactory.md) to partition EF
Core's compiled-model cache per tenant. Implementers expose the
ambient tenant id so the factory can emit a distinct cache key for
each tenant, preventing EF from folding the query-filter constant
across tenants on the first model build.

[ITenantDbContextFactory\<TContext>](/sdk/api/kun/kun-entityframeworkcore-multitenancy/itenantdbcontextfactory-1.md)

Builds a `TContext` instance bound to a specific tenant's schema
or database. Used by [SchemaPerTenantProvisioner\<TContext>](/sdk/api/kun/kun-entityframeworkcore-multitenancy/schemapertenantprovisioner-1.md) at tenant
provisioning time, when the ambient [ITenantContext](/sdk/api/kun/kun-core-multitenancy/itenantcontext.md)
may not yet reference the new tenant.
