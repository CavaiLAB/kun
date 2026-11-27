> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class FeatureFlagPolicy

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

固定灰度规则；没有表达式、外部地址或任意配置键。

```csharp
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class FeatureFlagPolicy
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[FeatureFlagPolicy](/sdk/api/kun/kun-configuration/featureflagpolicy.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### FeatureFlagPolicy() \{#Kun_Configuration_FeatureFlagPolicy__ctor}

```csharp
public FeatureFlagPolicy()
```

## Properties

### Default \{#Kun_Configuration_FeatureFlagPolicy_Default}

未命中白名单或百分比时的结果。

```csharp
public required bool Default { get; init; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Percentage \{#Kun_Configuration_FeatureFlagPolicy_Percentage}

0 至 100 的整数；同名开关使用固定哈希桶。

```csharp
public int Percentage { get; set; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### Tenants \{#Kun_Configuration_FeatureFlagPolicy_Tenants}

非空且不重复的租户 UUID，最多 256 项。

```csharp
public IReadOnlyList<Guid> Tenants { get; set; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[Guid](https://learn.microsoft.com/dotnet/api/system.guid)>
