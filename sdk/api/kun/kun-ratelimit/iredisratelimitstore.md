> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IRedisRateLimitStore

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.RateLimit` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.RateLimit`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.RateLimit)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.RateLimit](/sdk/api/kun/kun-ratelimit/index.md)\
Assembly: Kun.RateLimit.dll

Low-level Redis-backed counter used for distributed rate limiting across multiple
Gateway pods. Primitive surface for a future `PartitionedRateLimiter<HttpContext>`
integration; directly usable today as a building block for custom rate-limit middleware.

```csharp
public interface IRedisRateLimitStore
```

## Methods

### HitAsync(string, TimeSpan, CancellationToken) \{#Kun_RateLimit_IRedisRateLimitStore_HitAsync_System_String_System_TimeSpan_System_Threading_CancellationToken\_}

Atomically increments the count for `partitionKey` within a
rolling window of `window`, applying TTL on the first write so
stale partitions self-expire. Returns the post-increment count and the time until
the window resets.

```csharp
Task<RateLimitHitResult> HitAsync(string partitionKey, TimeSpan window, CancellationToken cancellationToken = default)
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
