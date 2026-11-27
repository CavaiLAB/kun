> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class GatewayCluster

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Gateway.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Gateway.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Gateway.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Gateway](/sdk/api/kun/kun-gateway/index.md)\
Assembly: Kun.Gateway.Abstractions.dll

静态下游；Id 同时是 S2S 的唯一受众。

```csharp
public sealed record GatewayCluster : IEquatable<GatewayCluster>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[GatewayCluster](/sdk/api/kun/kun-gateway/gatewaycluster.md)

#### Implements

[IEquatable\<GatewayCluster>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### GatewayCluster(string, string) \{#Kun_Gateway_GatewayCluster__ctor_System_String_System_String\_}

静态下游；Id 同时是 S2S 的唯一受众。

```csharp
public GatewayCluster(string Id, string Address)
```

#### Parameters

`Id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Address` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Address \{#Kun_Gateway_GatewayCluster_Address}

```csharp
public string Address { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Id \{#Kun_Gateway_GatewayCluster_Id}

```csharp
public string Id { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### \<Clone>$() \{#Kun_Gateway_GatewayCluster__Clone\_\_}

```csharp
public GatewayCluster <Clone>$()
```

#### Returns

[GatewayCluster](/sdk/api/kun/kun-gateway/gatewaycluster.md)

### Deconstruct(out string, out string) \{#Kun_Gateway_GatewayCluster_Deconstruct_System_String__System_String\_\_}

```csharp
public void Deconstruct(out string Id, out string Address)
```

#### Parameters

`Id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Address` [string](https://learn.microsoft.com/dotnet/api/system.string)

### Equals(object?) \{#Kun_Gateway_GatewayCluster_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(GatewayCluster?) \{#Kun_Gateway_GatewayCluster_Equals_Kun_Gateway_GatewayCluster\_}

```csharp
public bool Equals(GatewayCluster? other)
```

#### Parameters

`other` [GatewayCluster](/sdk/api/kun/kun-gateway/gatewaycluster.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Gateway_GatewayCluster_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Gateway_GatewayCluster_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(GatewayCluster?, GatewayCluster?) \{#Kun_Gateway_GatewayCluster_op_Equality_Kun_Gateway_GatewayCluster_Kun_Gateway_GatewayCluster\_}

```csharp
public static bool operator ==(GatewayCluster? left, GatewayCluster? right)
```

#### Parameters

`left` [GatewayCluster](/sdk/api/kun/kun-gateway/gatewaycluster.md)?

`right` [GatewayCluster](/sdk/api/kun/kun-gateway/gatewaycluster.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(GatewayCluster?, GatewayCluster?) \{#Kun_Gateway_GatewayCluster_op_Inequality_Kun_Gateway_GatewayCluster_Kun_Gateway_GatewayCluster\_}

```csharp
public static bool operator !=(GatewayCluster? left, GatewayCluster? right)
```

#### Parameters

`left` [GatewayCluster](/sdk/api/kun/kun-gateway/gatewaycluster.md)?

`right` [GatewayCluster](/sdk/api/kun/kun-gateway/gatewaycluster.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
