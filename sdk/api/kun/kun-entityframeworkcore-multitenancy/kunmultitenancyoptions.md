> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunMultiTenancyOptions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore.MultiTenancy](/sdk/api/kun/kun-entityframeworkcore-multitenancy/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

Configuration for `AddKunMultiTenancy`. Selects the isolation strategy and tunes
the few knobs the framework exposes across all three strategies.

```csharp
public sealed class KunMultiTenancyOptions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunMultiTenancyOptions](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### KunMultiTenancyOptions() \{#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions__ctor}

```csharp
public KunMultiTenancyOptions()
```

## Properties

### AllowDestructiveDeprovision \{#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions_AllowDestructiveDeprovision}

When [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool), `SchemaPerTenantProvisioner.DeprovisionAsync` is
permitted to run `DROP SCHEMA IF EXISTS "{schema}" CASCADE`. Default is
[false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) — deprovisioning is a data-loss operation and must be an
explicit opt-in. When left at the default, `DeprovisionAsync` throws
[`NotSupportedException`](https://learn.microsoft.com/dotnet/api/system.notsupportedexception) with a remediation message.

```csharp
public bool AllowDestructiveDeprovision { get; set; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### SchemaPrefix \{#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions_SchemaPrefix}

租户数据库架构名称的前缀，后接 32 位小写 UUID。

```csharp
public string SchemaPrefix { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Strategy \{#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions_Strategy}

The active isolation strategy. Defaults to [TenantIsolation.Row](/sdk/api/kun/kun-core-multitenancy/tenantisolation.md#Kun_Core_MultiTenancy_TenantIsolation_Row).

```csharp
public TenantIsolation Strategy { get; set; }
```

#### Property Value

TenantIsolation

## Methods

### GetSchemaName(Guid) \{#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions_GetSchemaName_System_Guid\_}

返回指定租户的 PostgreSQL 架构名，可用于上下文工厂和迁移配置。

```csharp
public string GetSchemaName(Guid tenantId)
```

#### Parameters

`tenantId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
