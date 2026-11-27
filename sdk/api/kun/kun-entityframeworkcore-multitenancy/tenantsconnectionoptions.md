> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class TenantsConnectionOptions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore.MultiTenancy](/sdk/api/kun/kun-entityframeworkcore-multitenancy/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

Static tenant → connection-string mapping consumed by
[OptionsBasedTenantConnectionResolver](/sdk/api/kun/kun-entityframeworkcore-multitenancy/optionsbasedtenantconnectionresolver.md). Suitable for small deployments
where the full tenant list is known at configuration time (appsettings.json or
Kubernetes ConfigMap).

```csharp
public sealed class TenantsConnectionOptions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[TenantsConnectionOptions](/sdk/api/kun/kun-entityframeworkcore-multitenancy/tenantsconnectionoptions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Remarks

Bind the section with `Configure<TenantsConnectionOptions>(configuration.GetSection(TenantsConnectionOptions.SectionName))`.
Under `ConnectionStrings`, each key is a tenant ID as a non-empty UUID in lowercase D format.
The configuration binder cannot populate [`Guid`](https://learn.microsoft.com/dotnet/api/system.guid)-keyed dictionaries, so the section lands in
[TenantsConnectionOptions.ConfiguredConnectionStrings](/sdk/api/kun/kun-entityframeworkcore-multitenancy/tenantsconnectionoptions.md#Kun_EntityFrameworkCore_MultiTenancy_TenantsConnectionOptions_ConfiguredConnectionStrings). `AddKunMultiTenancy<TContext>` with
`TenantIsolation.Database` merges those entries into [TenantsConnectionOptions.ConnectionStrings](/sdk/api/kun/kun-entityframeworkcore-multitenancy/tenantsconnectionoptions.md#Kun_EntityFrameworkCore_MultiTenancy_TenantsConnectionOptions_ConnectionStrings). The host fails to start when a key is not
a canonical UUID or when code and configuration give one tenant different connection strings.

For scale, swap in a custom [IDbConnectionResolver](/sdk/api/kun/kun-entityframeworkcore-multitenancy/idbconnectionresolver.md) implementation backed
by a secret manager + tenant-metadata service. See the P2-D Part 3 notes in
`docs/architecture/adr-p2d-multitenancy.md`.

## Constructors

### TenantsConnectionOptions() \{#Kun_EntityFrameworkCore_MultiTenancy_TenantsConnectionOptions__ctor}

```csharp
public TenantsConnectionOptions()
```

## Fields

### SectionName \{#Kun_EntityFrameworkCore_MultiTenancy_TenantsConnectionOptions_SectionName}

Config section name bound via `Configure<TenantsConnectionOptions>`.

```csharp
public const string SectionName = "Kun:MultiTenancy:Tenants"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### ConfiguredConnectionStrings \{#Kun_EntityFrameworkCore_MultiTenancy_TenantsConnectionOptions_ConfiguredConnectionStrings}

Receives the `ConnectionStrings` configuration section. Keys are tenant IDs as
non-empty UUIDs in lowercase D format. Code should write [TenantsConnectionOptions.ConnectionStrings](/sdk/api/kun/kun-entityframeworkcore-multitenancy/tenantsconnectionoptions.md#Kun_EntityFrameworkCore_MultiTenancy_TenantsConnectionOptions_ConnectionStrings) instead.

```csharp
[ConfigurationKeyName("ConnectionStrings")]
public IDictionary<string, string> ConfiguredConnectionStrings { get; set; }
```

#### Property Value

[IDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

### ConnectionStrings \{#Kun_EntityFrameworkCore_MultiTenancy_TenantsConnectionOptions_ConnectionStrings}

Maps tenant IDs to ADO.NET connection strings. Fill it from code; entries from
configuration are merged in from [TenantsConnectionOptions.ConfiguredConnectionStrings](/sdk/api/kun/kun-entityframeworkcore-multitenancy/tenantsconnectionoptions.md#Kun_EntityFrameworkCore_MultiTenancy_TenantsConnectionOptions_ConfiguredConnectionStrings).

```csharp
public IDictionary<Guid, string> ConnectionStrings { get; set; }
```

#### Property Value

[IDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary-2)\<[Guid](https://learn.microsoft.com/dotnet/api/system.guid), [string](https://learn.microsoft.com/dotnet/api/system.string)>
