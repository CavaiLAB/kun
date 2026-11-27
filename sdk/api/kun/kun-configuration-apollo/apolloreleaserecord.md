> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ApolloReleaseRecord

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Apollo` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Apollo`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Apollo)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration.Apollo](/sdk/api/kun/kun-configuration-apollo/index.md)\
Assembly: Kun.Configuration.Apollo.dll

一份快照的发布结果，仅含资源标识、变更数量、原因码与脱敏摘要。

```csharp
public sealed class ApolloReleaseRecord
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ApolloReleaseRecord](/sdk/api/kun/kun-configuration-apollo/apolloreleaserecord.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Properties

### ChangedKeys \{#Kun_Configuration_Apollo_ApolloReleaseRecord_ChangedKeys}

与上次生效快照比较的键变更数。

```csharp
public int ChangedKeys { get; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### InputHash \{#Kun_Configuration_Apollo_ApolloReleaseRecord_InputHash}

脱敏快照的 SHA-256 摘要。

```csharp
public string InputHash { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Namespace \{#Kun_Configuration_Apollo_ApolloReleaseRecord_Namespace}

配置命名空间。

```csharp
public string Namespace { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### OccurredAt \{#Kun_Configuration_Apollo_ApolloReleaseRecord_OccurredAt}

实例记录此次发布的时间。

```csharp
public DateTimeOffset OccurredAt { get; }
```

#### Property Value

[DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

### Reasons \{#Kun_Configuration_Apollo_ApolloReleaseRecord_Reasons}

拒绝原因码。

```csharp
public IReadOnlyList<string> Reasons { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

### Rejected \{#Kun_Configuration_Apollo_ApolloReleaseRecord_Rejected}

该快照是否被拒绝。

```csharp
public bool Rejected { get; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Resource \{#Kun_Configuration_Apollo_ApolloReleaseRecord_Resource}

本次发布的资源标识。

```csharp
public string Resource { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
