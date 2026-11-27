> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class RedisFixedWindowRateLimitStore

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.RateLimit` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.RateLimit`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.RateLimit)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.RateLimit](/sdk/api/kun/kun-ratelimit/index.md)\
Assembly: Kun.RateLimit.dll

Counts a hit and establishes the fixed window atomically in Redis. A preexisting counter without a TTL starts a fresh expiry window.

```csharp
public sealed class RedisFixedWindowRateLimitStore : IRedisRateLimitStore
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[RedisFixedWindowRateLimitStore](/sdk/api/kun/kun-ratelimit/redisfixedwindowratelimitstore.md)

#### Implements

[IRedisRateLimitStore](/sdk/api/kun/kun-ratelimit/iredisratelimitstore.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### RedisFixedWindowRateLimitStore(IConnectionMultiplexer, IOptions\<RedisRateLimitStoreOptions>) \{#Kun_RateLimit_RedisFixedWindowRateLimitStore__ctor_StackExchange_Redis_IConnectionMultiplexer_Microsoft_Extensions_Options_IOptions_Kun_RateLimit_RedisRateLimitStoreOptions\_\_}

Creates a new store.

```csharp
public RedisFixedWindowRateLimitStore(IConnectionMultiplexer redis, IOptions<RedisRateLimitStoreOptions> options)
```

#### Parameters

`redis` IConnectionMultiplexer

`options` [IOptions](https://learn.microsoft.com/dotnet/api/microsoft.extensions.options.ioptions-1)\<[RedisRateLimitStoreOptions](/sdk/api/kun/kun-ratelimit/redisratelimitstoreoptions.md)>

## Methods

### HitAsync(string, TimeSpan, CancellationToken) \{#Kun_RateLimit_RedisFixedWindowRateLimitStore_HitAsync_System_String_System_TimeSpan_System_Threading_CancellationToken\_}

Atomically increments the count for `partitionKey` within a
rolling window of `window`, applying TTL on the first write so
stale partitions self-expire. Returns the post-increment count and the time until
the window resets.

```csharp
public Task<RateLimitHitResult> HitAsync(string partitionKey, TimeSpan window, CancellationToken cancellationToken = default)
```

#### Parameters

`partitionKey` [string](https://learn.microsoft.com/dotnet/api/system.string)

Caller's partition key (tenant, IP, sub, etc.).

`window` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

Window length; controls Redis EXPIRE TTL on first hit.

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

Cancellation token.

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[RateLimitHitResult](/sdk/api/kun/kun-ratelimit/ratelimithitresult.md)>

`Count`: current number of hits within the active window (>= 1).
`ResetsIn`: time until the window resets. Never negative.
