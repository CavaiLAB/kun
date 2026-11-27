> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class GatewayRoute

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Gateway.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Gateway.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Gateway.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Gateway](/sdk/api/kun/kun-gateway/index.md)\
Assembly: Kun.Gateway.Abstractions.dll

受保护路由；不开放匿名策略覆盖。

```csharp
public sealed record GatewayRoute : IEquatable<GatewayRoute>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[GatewayRoute](/sdk/api/kun/kun-gateway/gatewayroute.md)

#### Implements

[IEquatable\<GatewayRoute>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### GatewayRoute(string, string, string) \{#Kun_Gateway_GatewayRoute__ctor_System_String_System_String_System_String\_}

受保护路由；不开放匿名策略覆盖。

```csharp
public GatewayRoute(string Id, string Path, string Cluster)
```

#### Parameters

`Id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Path` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Cluster` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Cluster \{#Kun_Gateway_GatewayRoute_Cluster}

```csharp
public string Cluster { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Id \{#Kun_Gateway_GatewayRoute_Id}

```csharp
public string Id { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Path \{#Kun_Gateway_GatewayRoute_Path}

```csharp
public string Path { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### RateLimiterPolicy \{#Kun_Gateway_GatewayRoute_RateLimiterPolicy}

必填：kun-default（可信客户端 IP）、tenant 或 user；缺失时拒绝构建网关配置。

```csharp
public required string RateLimiterPolicy { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### \<Clone>$() \{#Kun_Gateway_GatewayRoute__Clone\_\_}

```csharp
public GatewayRoute <Clone>$()
```

#### Returns

[GatewayRoute](/sdk/api/kun/kun-gateway/gatewayroute.md)

### Deconstruct(out string, out string, out string) \{#Kun_Gateway_GatewayRoute_Deconstruct_System_String__System_String__System_String\_\_}

```csharp
public void Deconstruct(out string Id, out string Path, out string Cluster)
```

#### Parameters

`Id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Path` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Cluster` [string](https://learn.microsoft.com/dotnet/api/system.string)

### Equals(object?) \{#Kun_Gateway_GatewayRoute_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(GatewayRoute?) \{#Kun_Gateway_GatewayRoute_Equals_Kun_Gateway_GatewayRoute\_}

```csharp
public bool Equals(GatewayRoute? other)
```

#### Parameters

`other` [GatewayRoute](/sdk/api/kun/kun-gateway/gatewayroute.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Gateway_GatewayRoute_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Gateway_GatewayRoute_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(GatewayRoute?, GatewayRoute?) \{#Kun_Gateway_GatewayRoute_op_Equality_Kun_Gateway_GatewayRoute_Kun_Gateway_GatewayRoute\_}

```csharp
public static bool operator ==(GatewayRoute? left, GatewayRoute? right)
```

#### Parameters

`left` [GatewayRoute](/sdk/api/kun/kun-gateway/gatewayroute.md)?

`right` [GatewayRoute](/sdk/api/kun/kun-gateway/gatewayroute.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(GatewayRoute?, GatewayRoute?) \{#Kun_Gateway_GatewayRoute_op_Inequality_Kun_Gateway_GatewayRoute_Kun_Gateway_GatewayRoute\_}

```csharp
public static bool operator !=(GatewayRoute? left, GatewayRoute? right)
```

#### Parameters

`left` [GatewayRoute](/sdk/api/kun/kun-gateway/gatewayroute.md)?

`right` [GatewayRoute](/sdk/api/kun/kun-gateway/gatewayroute.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
