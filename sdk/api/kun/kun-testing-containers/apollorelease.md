> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ApolloRelease

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Testing.Containers` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Testing.Containers`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Testing.Containers)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Testing.Containers](/sdk/api/kun/kun-testing-containers/index.md)\
Assembly: Kun.Testing.Containers.dll

一次发布的结果。

```csharp
public sealed record ApolloRelease : IEquatable<ApolloRelease>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ApolloRelease](/sdk/api/kun/kun-testing-containers/apollorelease.md)

#### Implements

[IEquatable\<ApolloRelease>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### ApolloRelease(string, long) \{#Kun_Testing_Containers_ApolloRelease__ctor_System_String_System_Int64\_}

一次发布的结果。

```csharp
public ApolloRelease(string ReleaseKey, long NotificationId)
```

#### Parameters

`ReleaseKey` [string](https://learn.microsoft.com/dotnet/api/system.string)

本次发布的 releaseKey；客户端带上它拉取时，未变化返回 304。

`NotificationId` [long](https://learn.microsoft.com/dotnet/api/system.int64)

本次发布后该 namespace 的通知号，长轮询据此判断有无更新。

## Properties

### NotificationId \{#Kun_Testing_Containers_ApolloRelease_NotificationId}

本次发布后该 namespace 的通知号，长轮询据此判断有无更新。

```csharp
public long NotificationId { get; init; }
```

#### Property Value

[long](https://learn.microsoft.com/dotnet/api/system.int64)

### ReleaseKey \{#Kun_Testing_Containers_ApolloRelease_ReleaseKey}

本次发布的 releaseKey；客户端带上它拉取时，未变化返回 304。

```csharp
public string ReleaseKey { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### \<Clone>$() \{#Kun_Testing_Containers_ApolloRelease__Clone\_\_}

```csharp
public ApolloRelease <Clone>$()
```

#### Returns

[ApolloRelease](/sdk/api/kun/kun-testing-containers/apollorelease.md)

### Deconstruct(out string, out long) \{#Kun_Testing_Containers_ApolloRelease_Deconstruct_System_String__System_Int64\_\_}

```csharp
public void Deconstruct(out string ReleaseKey, out long NotificationId)
```

#### Parameters

`ReleaseKey` [string](https://learn.microsoft.com/dotnet/api/system.string)

`NotificationId` [long](https://learn.microsoft.com/dotnet/api/system.int64)

### Equals(object?) \{#Kun_Testing_Containers_ApolloRelease_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(ApolloRelease?) \{#Kun_Testing_Containers_ApolloRelease_Equals_Kun_Testing_Containers_ApolloRelease\_}

```csharp
public bool Equals(ApolloRelease? other)
```

#### Parameters

`other` [ApolloRelease](/sdk/api/kun/kun-testing-containers/apollorelease.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Testing_Containers_ApolloRelease_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Testing_Containers_ApolloRelease_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(ApolloRelease?, ApolloRelease?) \{#Kun_Testing_Containers_ApolloRelease_op_Equality_Kun_Testing_Containers_ApolloRelease_Kun_Testing_Containers_ApolloRelease\_}

```csharp
public static bool operator ==(ApolloRelease? left, ApolloRelease? right)
```

#### Parameters

`left` [ApolloRelease](/sdk/api/kun/kun-testing-containers/apollorelease.md)?

`right` [ApolloRelease](/sdk/api/kun/kun-testing-containers/apollorelease.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(ApolloRelease?, ApolloRelease?) \{#Kun_Testing_Containers_ApolloRelease_op_Inequality_Kun_Testing_Containers_ApolloRelease_Kun_Testing_Containers_ApolloRelease\_}

```csharp
public static bool operator !=(ApolloRelease? left, ApolloRelease? right)
```

#### Parameters

`left` [ApolloRelease](/sdk/api/kun/kun-testing-containers/apollorelease.md)?

`right` [ApolloRelease](/sdk/api/kun/kun-testing-containers/apollorelease.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
