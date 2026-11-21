> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# ADR-005: Introduce `Kun.Caching` Module

:::info 发布版本来源
本页适用 [v2026.10.25](https://github.com/CavaiLAB/kun/releases/tag/v2026.10.25)；正文取自同仓 [Kun #17 审核的文档](https://github.com/CavaiLAB/kun/pull/17)。
本页由审核后的源文件生成 [`docs/adr/kun/ADR-005-Kun.Caching-module.md`](https://github.com/CavaiLAB/kun/blob/97503ce39963fa7f34d95e332e093d6564ebe4b6/docs/adr/kun/ADR-005-Kun.Caching-module.md)（`v2026.10.25`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

> 本文是原 Kun 的决策记录，引用时写「Kun ADR-005」。2026-11-18 搬入本仓库时清理了引擎与下游产品的名字：正文照原文，方括号里是整理时替换的说法。

**Status**: Accepted
**Date**: 2026-04-22
**Author**: claude-kun (Kun framework specialist)
**Context**: ［下游的 C# 服务］ has been calling `services.AddStackExchangeRedisCache(...)` directly from each service's `Program.cs` and hand-rolling `IDistributedCache`-based cache-aside logic (P1-CS-REDIS). Each service repeats the same serialization, key-prefixing, TTL-normalization, and single-flight boilerplate. Kun Framework has no unified cache abstraction — consumers choose between raw `IMemoryCache`, raw `IDistributedCache`, or the OSS `HybridCache` preview. This ADR introduces `Kun.Caching` to consolidate the pattern.

***

## Decision

Introduce a new framework module `Kun.Caching` with a single public
contract:

```csharp
public interface IKunCache
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? ttl = null, CancellationToken ct = default);
}
```

Two implementations ship in the module:

- `MemoryKunCache` — wraps `IMemoryCache`. Stores `T` directly (no
  serialization hop). Intended for in-process caches and tests.
- `RedisKunCache` — wraps `IDistributedCache` (the
  `Microsoft.Extensions.Caching.StackExchangeRedis` registration).
  Serializes values via a pluggable `IKunCacheSerializer`; default is
  `SystemTextJsonCacheSerializer`.

DI entry point is a fluent builder, mirroring the pattern established
by `Kun.Audit` and `Kun.Storage`:

```csharp
services.AddKunCaching(o =>
{
    o.KeyPrefix = "platform:";
    o.DefaultTtl = TimeSpan.FromMinutes(10);
    o.FallbackToMemoryOnRedisFailure = true;
}).UseRedis(connectionString);
```

***

## Module Shape and AOT

| Property           | Value                                                                                                                                                                                                                                                                                              |
| ------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Layer              | **Infrastructure** — references `Microsoft.Extensions.Caching.*` + `StackExchange.Redis`                                                                                                                                                                                                           |
| `<IsAotCandidate>` | `true` — local `dotnet publish -p:PublishAot=true /warnaserror` passes the IL + trim analyzer stage with zero warnings                                                                                                                                                                             |
| AOT escape hatch   | `IKunCache` generic methods + `IKunCacheSerializer` are annotated with `[RequiresUnreferencedCode]` + `[RequiresDynamicCode]`. Callers either inherit the requirement through the same attributes or replace the default serializer with a source-generated `JsonSerializerContext` implementation |

***

## Alternatives Considered

| Alternative                                               | Why Rejected                                                                                                                                                                                                                                    |
| --------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A. Recommend `Microsoft.Extensions.Caching.Hybrid`        | `HybridCache` is preview, its API is generic over `HybridCacheEntryOptions`, and its L2 backend is only `IDistributedCache`. The same boilerplate returns — consumers still build their own service wrapper.                                    |
| B. Keep consumers on raw `IDistributedCache`              | Each service re-implements serialization and single-flight. That is the status quo being replaced.                                                                                                                                              |
| C. New `Kun.Caching.Hybrid` that wraps `HybridCache`      | Possible follow-up once `HybridCache` GAs, but doesn't solve the API drift today.                                                                                                                                                               |
| D. **New `Kun.Caching` module with two impls (selected)** | Matches the idiomatic Kun module pattern (see `Kun.EventBus` vs. `Kun.EventBus.RabbitMQ`), handles both in-process and cross-process cases behind one surface, and lets consumers migrate from `AddStackExchangeRedisCache` with a single call. |

***

## Design Notes

- **Key prefix**: `KunCachingOptions.KeyPrefix` is prepended to every
  key before hitting the backend. Ideal for namespacing per-service
  caches on a shared Redis instance (e.g. `"platform:"` vs `"edge:"`).
- **Default TTL**: when a caller omits the `ttl` argument,
  `KunCachingOptions.DefaultTtl` applies. When both are null the
  entry never expires — Redis default. Memory cache never-expires
  entries still obey `MemoryCacheOptions.SizeLimit`-driven eviction
  if the consumer set one.
- **Single-flight `GetOrSetAsync`**: a per-key `SemaphoreSlim` pool
  guarantees the value factory runs at most once under concurrent
  access on the same process. The pool is reference-counted —
  semaphores are removed when the last waiter releases, so the memory
  footprint is bounded by the in-flight working set, not the lifetime
  key universe. Cross-process single-flight is out of scope; callers
  that need it compose a Redis distributed lock separately.
- **Redis fail-open**: when
  `KunCachingOptions.FallbackToMemoryOnRedisFailure = true`,
  `RedisKunCache` catches `RedisConnectionException`,
  `RedisTimeoutException`, and `SocketException`, logs at Warning,
  and routes the operation to a sidecar `MemoryKunCache` for the
  duration of the outage. When `false` (default), exceptions
  propagate. The fallback is per-process — replicas will not see
  writes made while Redis is unreachable, which is the documented
  tradeoff.

***

## Consequences

### Positive

- Consumer services stop copy-pasting `AddStackExchangeRedisCache` +
  JSON wrapper + TTL math.
- A single upgrade point for cache-related features (SlidingExpiration
  support, compression, multi-tier cache, metrics) — future work
  lives in one module.
- `Kun.Caching.Tests` exercises both backends under the same
  contract, preventing silent drift between Memory and Redis.

### Negative / Trade-offs

- Any consumer host now carries a transitive dependency on
  `Microsoft.Extensions.Caching.StackExchangeRedis` even when
  `UseMemory()` is selected — the Redis package brings in
  `StackExchange.Redis` (\~1 MB trim-safe).
- `IKunCache.GetAsync<T>` with a reference-type `T` returns `null`
  for both "key absent" and "value was null when cached". Consumers
  that need to distinguish should cache `Option<T>` / a tagged
  envelope.

### Risks

| Risk                                                                                | Mitigation                                                                                                                                                             |
| ----------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| StackExchange.Redis AOT regression                                                  | CI AOT matrix includes `Kun.Caching`; Stop-gate if the analyzer warns.                                                                                                 |
| Serializer drift between MemoryKunCache (no serialization) and RedisKunCache (JSON) | Contract tests in `KunCacheBackendTests` cover both backends with the same assertions — any divergence fails the matrix.                                               |
| Per-key semaphore pool leaks under pathological unique-key usage                    | The pool is reference-counted; entries are removed on final release. Documented in `AGENTS.md` — callers with unbounded ephemeral keys should not use `GetOrSetAsync`. |

***

## Follow-ups

1. **Migrate ［下游的 C# 服务］** off hand-rolled
   `AddStackExchangeRedisCache` to
   `services.AddKunCaching(...).UseRedis(...)`.
2. **Source-generated JSON serializer** example — demonstrate a
   caller-supplied `IKunCacheSerializer` backed by a
   `JsonSerializerContext` so AOT-strict consumers have a copy-paste
   starting point.
3. **Hybrid cache follow-on**: once `HybridCache` GAs (expected .NET
   10 GA), evaluate adding a `UseHybrid(...)` variant that keeps L1 in
   process and writes L2 through to Redis — different single-flight
   semantics.
4. **Cache stampede metrics** — surface `cache.hit`, `cache.miss`,
   `cache.stampede.single_flight_wait` counters via the
   `ActivitySource` pattern used by `Kun.Mediator`.

***

## References

- Handoff: task brief P1-KN-Caching
- ［下游 C# 服务各自的 `Program.cs`］ — the current
  `AddStackExchangeRedisCache` registrations being abstracted
- `Kun.Audit` / `Kun.Storage` / `Kun.Observability` — fluent-builder DI
  pattern reused here
