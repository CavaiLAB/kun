> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SchemaPerTenantProvisioner\<TContext>

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore.MultiTenancy](/sdk/api/kun/kun-entityframeworkcore-multitenancy/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

[ITenantProvisioner](/sdk/api/kun/kun-core-multitenancy/itenantprovisioner.md) implementation for the
[TenantIsolation.Schema](/sdk/api/kun/kun-core-multitenancy/tenantisolation.md#Kun_Core_MultiTenancy_TenantIsolation_Schema) strategy. On
[SchemaPerTenantProvisioner\<TContext>.ProvisionAsync](/sdk/api/kun/kun-entityframeworkcore-multitenancy/schemapertenantprovisioner-1.md#Kun_EntityFrameworkCore_MultiTenancy_SchemaPerTenantProvisioner_1_ProvisionAsync_Kun_Core_MultiTenancy_TenantInfo_System_Threading_CancellationToken_) executes `CREATE SCHEMA IF NOT EXISTS "{schema}"`
and runs EF Core migrations under that schema; on [SchemaPerTenantProvisioner\<TContext>.DeprovisionAsync](/sdk/api/kun/kun-entityframeworkcore-multitenancy/schemapertenantprovisioner-1.md#Kun_EntityFrameworkCore_MultiTenancy_SchemaPerTenantProvisioner_1_DeprovisionAsync_System_Guid_System_Threading_CancellationToken_)
drops the schema when [KunMultiTenancyOptions.AllowDestructiveDeprovision](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions_AllowDestructiveDeprovision)
is enabled, else throws.

```csharp
public sealed class SchemaPerTenantProvisioner<TContext> : ITenantProvisioner where TContext : DbContext
```

#### Type Parameters

`TContext`

The KunDbContext-derived context whose migrations to apply.

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SchemaPerTenantProvisioner\<TContext>](/sdk/api/kun/kun-entityframeworkcore-multitenancy/schemapertenantprovisioner-1.md)

#### Implements

ITenantProvisioner

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Remarks

The provisioner and the context factory use [KunMultiTenancyOptions.GetSchemaName](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md#Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions_GetSchemaName_System_Guid_)
to derive the same schema from the tenant UUID and configured prefix.

## Constructors

### SchemaPerTenantProvisioner(ITenantDbContextFactory\<TContext>, IOptions\<KunMultiTenancyOptions>, ILogger\<SchemaPerTenantProvisioner\<TContext>>?) \{#Kun_EntityFrameworkCore_MultiTenancy_SchemaPerTenantProvisioner_1__ctor_Kun_EntityFrameworkCore_MultiTenancy_ITenantDbContextFactory__0__Microsoft_Extensions_Options_IOptions_Kun_EntityFrameworkCore_MultiTenancy_KunMultiTenancyOptions__Microsoft_Extensions_Logging_ILogger_Kun_EntityFrameworkCore_MultiTenancy_SchemaPerTenantProvisioner__0\_\_\_}

Creates a new provisioner.

```csharp
public SchemaPerTenantProvisioner(ITenantDbContextFactory<TContext> factory, IOptions<KunMultiTenancyOptions> options, ILogger<SchemaPerTenantProvisioner<TContext>>? logger = null)
```

#### Parameters

`factory` [ITenantDbContextFactory](/sdk/api/kun/kun-entityframeworkcore-multitenancy/itenantdbcontextfactory-1.md)\<TContext>

`options` [IOptions](https://learn.microsoft.com/dotnet/api/microsoft.extensions.options.ioptions-1)\<[KunMultiTenancyOptions](/sdk/api/kun/kun-entityframeworkcore-multitenancy/kunmultitenancyoptions.md)>

`logger` [ILogger](https://learn.microsoft.com/dotnet/api/microsoft.extensions.logging.ilogger-1)\<[SchemaPerTenantProvisioner](/sdk/api/kun/kun-entityframeworkcore-multitenancy/schemapertenantprovisioner-1.md)\<TContext>>?

## Methods

### DeprovisionAsync(Guid, CancellationToken) \{#Kun_EntityFrameworkCore_MultiTenancy_SchemaPerTenantProvisioner_1_DeprovisionAsync_System_Guid_System_Threading_CancellationToken\_}

清理指定租户的持久化基础件。幂等：对未开户的租户调用不得抛异常。

```csharp
public Task DeprovisionAsync(Guid tenantId, CancellationToken cancellationToken = default)
```

#### Parameters

`tenantId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### ProvisionAsync(TenantInfo, CancellationToken) \{#Kun_EntityFrameworkCore_MultiTenancy_SchemaPerTenantProvisioner_1_ProvisionAsync_Kun_Core_MultiTenancy_TenantInfo_System_Threading_CancellationToken\_}

为指定租户准备持久化基础件（schema / database / seed 数据）。幂等。

```csharp
public Task ProvisionAsync(TenantInfo tenant, CancellationToken cancellationToken = default)
```

#### Parameters

`tenant` TenantInfo

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)
