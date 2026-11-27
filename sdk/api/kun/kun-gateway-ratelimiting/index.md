> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Gateway.RateLimiting

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Gateway` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Gateway`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Gateway)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.Gateway.RateLimiting.Resolvers](/sdk/api/kun/kun-gateway-ratelimiting-resolvers/index.md)

### Classes

[KunRateLimiterExtensions](/sdk/api/kun/kun-gateway-ratelimiting/kunratelimiterextensions.md)

Extension methods for registering Kun rate limiting services.

[KunSlidingWindowOptions](/sdk/api/kun/kun-gateway-ratelimiting/kunslidingwindowoptions.md)

Tunable parameters for `AddKunSlidingWindowRateLimiter`. Sliding-window divides
each window into segments and shifts one segment at a time, yielding smoother behavior
than a fixed window at the cost of proportionally higher CPU/memory.

[KunTokenBucketOptions](/sdk/api/kun/kun-gateway-ratelimiting/kuntokenbucketoptions.md)

Tunable parameters for `AddKunTokenBucketRateLimiter`. Token-bucket smooths bursty
traffic by accumulating tokens at a steady rate up to a cap; each request consumes one
token and is rejected (or queued) when the bucket is empty.

### Interfaces

[IRateLimitPartitionResolver](/sdk/api/kun/kun-gateway-ratelimiting/iratelimitpartitionresolver.md)

Resolves the rate-limit partition key for an incoming HTTP request.
Implement this interface to plug in a custom partitioning strategy (by tenant, IP, API key, etc.).
