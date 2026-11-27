> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.RateLimit

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.RateLimit` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.RateLimit`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.RateLimit)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.RateLimit.Abstractions](/sdk/api/kun/kun-ratelimit-abstractions/index.md)

[Kun.RateLimit.Extensions](/sdk/api/kun/kun-ratelimit-extensions/index.md)

### Classes

[RedisFixedWindowRateLimitStore](/sdk/api/kun/kun-ratelimit/redisfixedwindowratelimitstore.md)

Counts a hit and establishes the fixed window atomically in Redis. A preexisting counter without a TTL starts a fresh expiry window.

[RedisRateLimitStoreOptions](/sdk/api/kun/kun-ratelimit/redisratelimitstoreoptions.md)

Configuration for [RedisFixedWindowRateLimitStore](/sdk/api/kun/kun-ratelimit/redisfixedwindowratelimitstore.md).

### Structs

[RateLimitHitResult](/sdk/api/kun/kun-ratelimit/ratelimithitresult.md)

Result of [IRedisRateLimitStore.HitAsync](/sdk/api/kun/kun-ratelimit/iredisratelimitstore.md#Kun_RateLimit_IRedisRateLimitStore_HitAsync_System_String_System_TimeSpan_System_Threading_CancellationToken_).

### Interfaces

[IRedisRateLimitStore](/sdk/api/kun/kun-ratelimit/iredisratelimitstore.md)

Low-level Redis-backed counter used for distributed rate limiting across multiple
Gateway pods. Primitive surface for a future `PartitionedRateLimiter<HttpContext>`
integration; directly usable today as a building block for custom rate-limit middleware.
