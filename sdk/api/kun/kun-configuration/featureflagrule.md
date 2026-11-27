> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class FeatureFlagRule

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

从快照读取的一份开关规则。

```csharp
public sealed class FeatureFlagRule
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[FeatureFlagRule](/sdk/api/kun/kun-configuration/featureflagrule.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Properties

### Default \{#Kun_Configuration_FeatureFlagRule_Default}

未命中定向条件时的值。

```csharp
public bool Default { get; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Percentage \{#Kun_Configuration_FeatureFlagRule_Percentage}

百分比。

```csharp
public int Percentage { get; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### Tenants \{#Kun_Configuration_FeatureFlagRule_Tenants}

指定的租户集合。

```csharp
public IReadOnlySet<Guid> Tenants { get; }
```

#### Property Value

[IReadOnlySet](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlyset-1)\<[Guid](https://learn.microsoft.com/dotnet/api/system.guid)>
