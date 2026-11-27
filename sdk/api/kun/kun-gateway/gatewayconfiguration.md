> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class GatewayConfiguration

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Gateway.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Gateway.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Gateway.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Gateway](/sdk/api/kun/kun-gateway/index.md)\
Assembly: Kun.Gateway.Abstractions.dll

网关声明式配置；只允许一个身份发行方，密钥值从不进入配置，PEM 路径指向宿主的 Secret 投射。

```csharp
public sealed record GatewayConfiguration : IEquatable<GatewayConfiguration>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[GatewayConfiguration](/sdk/api/kun/kun-gateway/gatewayconfiguration.md)

#### Implements

[IEquatable\<GatewayConfiguration>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### GatewayConfiguration(string, string, string, string, string, IReadOnlyList\<string>, IReadOnlyList\<GatewayRoute>, IReadOnlyList\<GatewayCluster>) \{#Kun_Gateway_GatewayConfiguration__ctor_System_String_System_String_System_String_System_String_System_String_System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyList_Kun_Gateway_GatewayRoute__System_Collections_Generic_IReadOnlyList_Kun_Gateway_GatewayCluster\_\_}

网关声明式配置；只允许一个身份发行方，密钥值从不进入配置，PEM 路径指向宿主的 Secret 投射。

```csharp
[JsonConstructor]
public GatewayConfiguration(string IdentityIssuer, string IdentityAudience, string Issuer, string SigningKeyPath, string PublicKeyPath, IReadOnlyList<string> TrustedProxyNetworks, IReadOnlyList<GatewayRoute> Routes, IReadOnlyList<GatewayCluster> Clusters)
```

#### Parameters

`IdentityIssuer` [string](https://learn.microsoft.com/dotnet/api/system.string)

`IdentityAudience` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Issuer` [string](https://learn.microsoft.com/dotnet/api/system.string)

`SigningKeyPath` [string](https://learn.microsoft.com/dotnet/api/system.string)

`PublicKeyPath` [string](https://learn.microsoft.com/dotnet/api/system.string)

`TrustedProxyNetworks` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

`Routes` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[GatewayRoute](/sdk/api/kun/kun-gateway/gatewayroute.md)>

`Clusters` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[GatewayCluster](/sdk/api/kun/kun-gateway/gatewaycluster.md)>

### GatewayConfiguration() \{#Kun_Gateway_GatewayConfiguration__ctor}

Creates configuration for a host supplying its own YARP configuration provider.

```csharp
public GatewayConfiguration()
```

## Properties

### Clusters \{#Kun_Gateway_GatewayConfiguration_Clusters}

```csharp
public IReadOnlyList<GatewayCluster> Clusters { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[GatewayCluster](/sdk/api/kun/kun-gateway/gatewaycluster.md)>

### EmitTraceIdClaim \{#Kun_Gateway_GatewayConfiguration_EmitTraceIdClaim}

Retained for configuration compatibility; no longer changes the closed JWT payload.
Trace context is propagated through standard HTTP headers.

```csharp
[JsonIgnore]
public bool EmitTraceIdClaim { get; init; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### ExposeInternalJwks \{#Kun_Gateway_GatewayConfiguration_ExposeInternalJwks}

When [true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool), the gateway exposes the internal JWKS endpoint
`/internal/.well-known/jwks.json` in non-Development environments. Defaults to
[false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) so the internal endpoint returns 404 in production unless the
operator deliberately opts in. The public endpoint `/.well-known/jwks.json`
remains always-on; this option only gates the internal-network variant.

```csharp
[JsonIgnore]
public bool ExposeInternalJwks { get; init; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Remarks

Keep [false](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) in internet-facing Kubernetes deployments and rely on
NetworkPolicy / service-mesh egress rules to isolate the internal route. Flip to
[true](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/bool) only when JWKS fetching from internal pods must bypass the
public path (e.g. rate-limiting sidecars that reshape external traffic).

### ExternalJwksUri \{#Kun_Gateway_GatewayConfiguration_ExternalJwksUri}

The JWKS endpoint URI of the external IdP, used to fetch public keys for validating incoming JWTs.

```csharp
[JsonIgnore]
public string ExternalJwksUri { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ForwardLimit \{#Kun_Gateway_GatewayConfiguration_ForwardLimit}

最多接受的可信代理跳数，默认一跳。

```csharp
[JsonIgnore]
public int ForwardLimit { get; init; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ForwardedClaimTypes \{#Kun_Gateway_GatewayConfiguration_ForwardedClaimTypes}

Compatibility allowlist option. Only canonical scope/session/revision/project/plugin field names are accepted;
the canonical field set is fixed and does not require an allowlist. Arbitrary application claims are rejected.

```csharp
[JsonIgnore]
public string[] ForwardedClaimTypes { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)\[]

### IdentityAudience \{#Kun_Gateway_GatewayConfiguration_IdentityAudience}

```csharp
public string IdentityAudience { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### IdentityIssuer \{#Kun_Gateway_GatewayConfiguration_IdentityIssuer}

```csharp
public string IdentityIssuer { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### InternalAudience \{#Kun_Gateway_GatewayConfiguration_InternalAudience}

The audience claim placed in the minted internal S2S JWT. Defaults to `kun-internal`.

```csharp
[JsonIgnore]
public string InternalAudience { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### InternalTokenLifetime \{#Kun_Gateway_GatewayConfiguration_InternalTokenLifetime}

Lifetime of the minted internal S2S JWT. Defaults to 60 seconds per zero-trust TTL policy.

```csharp
[JsonIgnore]
public TimeSpan InternalTokenLifetime { get; init; }
```

#### Property Value

[TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

### Issuer \{#Kun_Gateway_GatewayConfiguration_Issuer}

```csharp
public string Issuer { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### KeyOverlapPeriod \{#Kun_Gateway_GatewayConfiguration_KeyOverlapPeriod}

轮换后旧公钥的重叠发布时长，默认五分钟；消费者仍须执行令牌寿命验证。

```csharp
[JsonIgnore]
public TimeSpan KeyOverlapPeriod { get; init; }
```

#### Property Value

[TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

### KeyReloadInterval \{#Kun_Gateway_GatewayConfiguration_KeyReloadInterval}

检查私钥文件变更的最短间隔，默认一秒；零表示每次签发或获取 JWKS 都检查。

```csharp
[JsonIgnore]
public TimeSpan KeyReloadInterval { get; init; }
```

#### Property Value

[TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

### PublicKeyPath \{#Kun_Gateway_GatewayConfiguration_PublicKeyPath}

```csharp
public string PublicKeyPath { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### RateLimits \{#Kun_Gateway_GatewayConfiguration_RateLimits}

每条代理路由独立计数，副本间共享 Redis；省略时每种策略均为 100 次/分钟。

```csharp
public GatewayRateLimits RateLimits { get; init; }
```

#### Property Value

[GatewayRateLimits](/sdk/api/kun/kun-gateway/gatewayratelimits.md)

### Routes \{#Kun_Gateway_GatewayConfiguration_Routes}

```csharp
public IReadOnlyList<GatewayRoute> Routes { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[GatewayRoute](/sdk/api/kun/kun-gateway/gatewayroute.md)>

### SigningKeyPath \{#Kun_Gateway_GatewayConfiguration_SigningKeyPath}

```csharp
public string SigningKeyPath { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TrustedProxyAddresses \{#Kun_Gateway_GatewayConfiguration_TrustedProxyAddresses}

允许转发客户端地址与协议的代理 IP；缺省不信任任何转发头。

```csharp
[JsonIgnore]
public string[] TrustedProxyAddresses { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)\[]

### TrustedProxyNetworks \{#Kun_Gateway_GatewayConfiguration_TrustedProxyNetworks}

```csharp
public IReadOnlyList<string> TrustedProxyNetworks { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

## Methods

### \<Clone>$() \{#Kun_Gateway_GatewayConfiguration__Clone\_\_}

```csharp
public GatewayConfiguration <Clone>$()
```

#### Returns

[GatewayConfiguration](/sdk/api/kun/kun-gateway/gatewayconfiguration.md)

### Deconstruct(out string, out string, out string, out string, out string, out IReadOnlyList\<string>, out IReadOnlyList\<GatewayRoute>, out IReadOnlyList\<GatewayCluster>) \{#Kun_Gateway_GatewayConfiguration_Deconstruct_System_String__System_String__System_String__System_String__System_String__System_Collections_Generic_IReadOnlyList_System_String___System_Collections_Generic_IReadOnlyList_Kun_Gateway_GatewayRoute___System_Collections_Generic_IReadOnlyList_Kun_Gateway_GatewayCluster\_\_\_}

```csharp
public void Deconstruct(out string IdentityIssuer, out string IdentityAudience, out string Issuer, out string SigningKeyPath, out string PublicKeyPath, out IReadOnlyList<string> TrustedProxyNetworks, out IReadOnlyList<GatewayRoute> Routes, out IReadOnlyList<GatewayCluster> Clusters)
```

#### Parameters

`IdentityIssuer` [string](https://learn.microsoft.com/dotnet/api/system.string)

`IdentityAudience` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Issuer` [string](https://learn.microsoft.com/dotnet/api/system.string)

`SigningKeyPath` [string](https://learn.microsoft.com/dotnet/api/system.string)

`PublicKeyPath` [string](https://learn.microsoft.com/dotnet/api/system.string)

`TrustedProxyNetworks` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

`Routes` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[GatewayRoute](/sdk/api/kun/kun-gateway/gatewayroute.md)>

`Clusters` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[GatewayCluster](/sdk/api/kun/kun-gateway/gatewaycluster.md)>

### Equals(object?) \{#Kun_Gateway_GatewayConfiguration_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(GatewayConfiguration?) \{#Kun_Gateway_GatewayConfiguration_Equals_Kun_Gateway_GatewayConfiguration\_}

```csharp
public bool Equals(GatewayConfiguration? other)
```

#### Parameters

`other` [GatewayConfiguration](/sdk/api/kun/kun-gateway/gatewayconfiguration.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Gateway_GatewayConfiguration_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Gateway_GatewayConfiguration_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(GatewayConfiguration?, GatewayConfiguration?) \{#Kun_Gateway_GatewayConfiguration_op_Equality_Kun_Gateway_GatewayConfiguration_Kun_Gateway_GatewayConfiguration\_}

```csharp
public static bool operator ==(GatewayConfiguration? left, GatewayConfiguration? right)
```

#### Parameters

`left` [GatewayConfiguration](/sdk/api/kun/kun-gateway/gatewayconfiguration.md)?

`right` [GatewayConfiguration](/sdk/api/kun/kun-gateway/gatewayconfiguration.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(GatewayConfiguration?, GatewayConfiguration?) \{#Kun_Gateway_GatewayConfiguration_op_Inequality_Kun_Gateway_GatewayConfiguration_Kun_Gateway_GatewayConfiguration\_}

```csharp
public static bool operator !=(GatewayConfiguration? left, GatewayConfiguration? right)
```

#### Parameters

`left` [GatewayConfiguration](/sdk/api/kun/kun-gateway/gatewayconfiguration.md)?

`right` [GatewayConfiguration](/sdk/api/kun/kun-gateway/gatewayconfiguration.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
