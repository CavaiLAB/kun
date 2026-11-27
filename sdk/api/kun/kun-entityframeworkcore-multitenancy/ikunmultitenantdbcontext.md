> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IKunMultiTenantDbContext

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore.MultiTenancy](/sdk/api/kun/kun-entityframeworkcore-multitenancy/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

Marker interface for `DbContext`s that want the
[KunMultiTenantModelCacheKeyFactory](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenantmodelcachekeyfactory.md) to partition EF
Core's compiled-model cache per tenant. Implementers expose the
ambient tenant id so the factory can emit a distinct cache key for
each tenant, preventing EF from folding the query-filter constant
across tenants on the first model build.

```csharp
public interface IKunMultiTenantDbContext
```

## Remarks

`Kun.EntityFrameworkCore.KunDbContext` already exposes an
equivalent `TenantIdForModelCacheKey` property; custom
contexts that inherit from plain [`DbContext`](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext)
should implement this interface to opt into per-tenant isolation.

## Properties

### TenantIdForModelCacheKey \{#Kun_EntityFrameworkCore_MultiTenancy_IKunMultiTenantDbContext_TenantIdForModelCacheKey}

Ambient tenant id used by the model-cache key and the DbContext-bound query-filter overload.

```csharp
string? TenantIdForModelCacheKey { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?
