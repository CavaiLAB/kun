> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunMultiTenancyEntityFrameworkCoreServiceCollectionExtensions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore.DependencyInjection](/sdk/api/kun/kun-entityframeworkcore-dependencyinjection/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

Service-collection extensions for the EF Core side of Kun.MultiTenancy.

```csharp
public static class KunMultiTenancyEntityFrameworkCoreServiceCollectionExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunMultiTenancyEntityFrameworkCoreServiceCollectionExtensions](/sdk/api/kun/kun-entityframeworkcore-dependencyinjection/kunmultitenancyentityframeworkcoreservicecollectionextensions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### AddKunMultiTenancyEntityFrameworkCore(IServiceCollection) \{#Kun_EntityFrameworkCore_DependencyInjection_KunMultiTenancyEntityFrameworkCoreServiceCollectionExtensions_AddKunMultiTenancyEntityFrameworkCore_Microsoft_Extensions_DependencyInjection_IServiceCollection\_}

Registers [KunTenantSaveChangesInterceptor](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kuntenantsavechangesinterceptor.md) as a
scoped service so consumers can attach it to their
`DbContextOptionsBuilder.AddInterceptors(...)` call.
Pair with `modelBuilder.ApplyKunMultiTenantFilters(this)` on an IKunMultiTenantDbContext
in `OnModelCreating` for full read + write coverage.

```csharp
public static IServiceCollection AddKunMultiTenancyEntityFrameworkCore(this IServiceCollection services)
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)
