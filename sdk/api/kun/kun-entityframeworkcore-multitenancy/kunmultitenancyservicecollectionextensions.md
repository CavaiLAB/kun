> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunMultiTenancyServiceCollectionExtensions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore.MultiTenancy](/sdk/api/kun/kun-entityframeworkcore-multitenancy/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

DI entry point for Kun's SaaS multi-tenancy strategy selection. Dispatches based on
[KunMultiTenancyOptions.Strategy](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions_Strategy).

```csharp
public static class KunMultiTenancyServiceCollectionExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunMultiTenancyServiceCollectionExtensions](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyservicecollectionextensions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### AddKunMultiTenancy\<TContext>(IServiceCollection, Action\<KunMultiTenancyOptions>) \{#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyServiceCollectionExtensions_AddKunMultiTenancy__1_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions\_\_}

Registers the multi-tenancy services for a specific DbContext type. Must be called
BEFORE the matching `AddDbContext<TContext>`, and the consumer is
responsible for registering their own [ITenantDbContextFactory\<TContext>](/sdk/api/kun/kun-entityframeworkcore-multitenancy/itenantdbcontextfactory-1.md)
when [KunMultiTenancyOptions.Strategy](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions_Strategy) is
[TenantIsolation.Schema](/sdk/api/kun/kun-core-multitenancy/tenantisolation.md#Kun_Core_MultiTenancy_TenantIsolation_Schema).

```csharp
public static IServiceCollection AddKunMultiTenancy<TContext>(this IServiceCollection services, Action<KunMultiTenancyOptions> configure) where TContext : DbContext
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

`configure` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[KunMultiTenancyOptions](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md)>

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

#### Type Parameters

`TContext`

#### Remarks

**Shared**: delegates to `AddKunTenantRls()` — no additional services
beyond the RLS interceptor and [NullTenantProvisioner](/sdk/api/kun/kun-core-multitenancy/nulltenantprovisioner.md) default.

**SchemaPerTenant**: registers [KunMultiTenantModelCacheKeyFactory](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenantmodelcachekeyfactory.md),
the [TenantRlsTransactionInterceptor](/sdk/api/kun/kun-entityframeworkcore-interceptors/tenantrlstransactioninterceptor.md) (defence-in-depth), a
Singleton [SchemaPerTenantProvisioner\<TContext>](/sdk/api/kun/kun-entityframeworkcore-multitenancy/schemapertenantprovisioner-1.md) as the
[ITenantProvisioner](/sdk/api/kun/kun-core-multitenancy/itenantprovisioner.md) implementation, and binds
[KunMultiTenancyOptions](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md). Consumers must also add the schema selection
and the model-cache factory to their DbContext options, e.g.:

```csharp
services.AddDbContext<CatalogDbContext>((sp, opts) =>
    opts.UseNpgsql(cs)
        .UseKunSchema(sp.GetRequiredService<IOptions<KunMultiTenancyOptions>>().Value.GetSchemaName(
            sp.GetRequiredService<ITenantContext>().TenantId ?? throw new InvalidOperationException("Tenant required.")))
        .ReplaceService<IModelCacheKeyFactory, KunMultiTenantModelCacheKeyFactory>()
        .AddInterceptors(sp.GetRequiredService<TenantRlsTransactionInterceptor>()));
```

**DatabasePerTenant**: registers a default
[OptionsBasedTenantConnectionResolver](/sdk/api/kun/kun-entityframeworkcore-multitenancy/optionsbasedtenantconnectionresolver.md) reading from
[TenantsConnectionOptions](/sdk/api/kun/kun-entityframeworkcore-multitenancy/tenantsconnectionoptions.md) plus a Singleton
[DatabasePerTenantProvisioner\<TContext>](/sdk/api/kun/kun-entityframeworkcore-multitenancy/databasepertenantprovisioner-1.md). Consumers are still responsible
for registering their own [ITenantDbContextFactory\<TContext>](/sdk/api/kun/kun-entityframeworkcore-multitenancy/itenantdbcontextfactory-1.md) that
resolves the tenant's connection string via [IDbConnectionResolver](/sdk/api/kun/kun-entityframeworkcore-multitenancy/idbconnectionresolver.md) and
constructs a DbContext bound to that connection. Entries bound from configuration
([TenantsConnectionOptions.ConfiguredConnectionStrings](/sdk/api/kun/kun-entityframeworkcore-multitenancy/tenantsconnectionoptions.md#Kun_EntityFrameworkCore_MultiTenancy_TenantsConnectionOptions_ConfiguredConnectionStrings)) are merged into
[TenantsConnectionOptions.ConnectionStrings](/sdk/api/kun/kun-entityframeworkcore-multitenancy/tenantsconnectionoptions.md#Kun_EntityFrameworkCore_MultiTenancy_TenantsConnectionOptions_ConnectionStrings). Host start fails when a key is not
a non-empty UUID in lowercase D format, or when code and configuration disagree for a tenant.

**Strategy snapshotting**: the [KunMultiTenancyOptions.Strategy](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions_Strategy)
value set by `configure` is captured at registration time to decide
which services to register. A subsequent
`services.Configure<KunMultiTenancyOptions>` that mutates
[KunMultiTenancyOptions.Strategy](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions_Strategy) will NOT retro-actively change the DI
wiring — the options-read path would disagree with the registered graph. Set the
strategy exactly once, inside this call's `configure` delegate.
