> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class GatewayRateLimits

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Gateway.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Gateway.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Gateway.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Gateway](/sdk/api/kun/kun-gateway/index.md)\
Assembly: Kun.Gateway.Abstractions.dll

代理路由的 Redis 固定窗口限额；同一逻辑网关的副本必须使用相同配置。

```csharp
public sealed record GatewayRateLimits : IEquatable<GatewayRateLimits>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[GatewayRateLimits](/sdk/api/kun/kun-gateway/gatewayratelimits.md)

#### Implements

[IEquatable\<GatewayRateLimits>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### GatewayRateLimits() \{#Kun_Gateway_GatewayRateLimits__ctor}

```csharp
public GatewayRateLimits()
```

## Properties

### IpPermitLimit \{#Kun_Gateway_GatewayRateLimits_IpPermitLimit}

每个 IP 的窗口请求数，1–1000000，默认 100；缺少规范身份时也使用此限额。

```csharp
public int IpPermitLimit { get; init; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### KeyPrefix \{#Kun_Gateway_GatewayRateLimits_KeyPrefix}

Redis 键前缀；环境或独立网关可显式隔离，键还包含规范 issuer、路由与主体的摘要。

```csharp
public string KeyPrefix { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TenantPermitLimit \{#Kun_Gateway_GatewayRateLimits_TenantPermitLimit}

每个租户的窗口请求数，1–1000000，默认 100。

```csharp
public int TenantPermitLimit { get; init; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### UserPermitLimit \{#Kun_Gateway_GatewayRateLimits_UserPermitLimit}

每个租户内用户或平台主体的窗口请求数，1–1000000，默认 100。

```csharp
public int UserPermitLimit { get; init; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### WindowSeconds \{#Kun_Gateway_GatewayRateLimits_WindowSeconds}

窗口秒数，1–86400，默认 60。

```csharp
public int WindowSeconds { get; init; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

## Methods

### \<Clone>$() \{#Kun_Gateway_GatewayRateLimits__Clone\_\_}

```csharp
public GatewayRateLimits <Clone>$()
```

#### Returns

[GatewayRateLimits](/sdk/api/kun/kun-gateway/gatewayratelimits.md)

### Equals(object?) \{#Kun_Gateway_GatewayRateLimits_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(GatewayRateLimits?) \{#Kun_Gateway_GatewayRateLimits_Equals_Kun_Gateway_GatewayRateLimits\_}

```csharp
public bool Equals(GatewayRateLimits? other)
```

#### Parameters

`other` [GatewayRateLimits](/sdk/api/kun/kun-gateway/gatewayratelimits.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Gateway_GatewayRateLimits_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Gateway_GatewayRateLimits_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(GatewayRateLimits?, GatewayRateLimits?) \{#Kun_Gateway_GatewayRateLimits_op_Equality_Kun_Gateway_GatewayRateLimits_Kun_Gateway_GatewayRateLimits\_}

```csharp
public static bool operator ==(GatewayRateLimits? left, GatewayRateLimits? right)
```

#### Parameters

`left` [GatewayRateLimits](/sdk/api/kun/kun-gateway/gatewayratelimits.md)?

`right` [GatewayRateLimits](/sdk/api/kun/kun-gateway/gatewayratelimits.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(GatewayRateLimits?, GatewayRateLimits?) \{#Kun_Gateway_GatewayRateLimits_op_Inequality_Kun_Gateway_GatewayRateLimits_Kun_Gateway_GatewayRateLimits\_}

```csharp
public static bool operator !=(GatewayRateLimits? left, GatewayRateLimits? right)
```

#### Parameters

`left` [GatewayRateLimits](/sdk/api/kun/kun-gateway/gatewayratelimits.md)?

`right` [GatewayRateLimits](/sdk/api/kun/kun-gateway/gatewayratelimits.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
