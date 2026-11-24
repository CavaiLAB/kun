# Gateway 限流策略指南

> `Kun.Gateway.RateLimiting` 把 ASP.NET Core 内置的 RateLimiter 包装成**策略 × 分区**的两维组合：
> 三种分区解析（partition resolver）× 三种限流策略（limiter policy）= 九种可选配置，计数在进程内。限流一共有三套入口，对照见下一节。
>
> 参考：`docs/design/why-cqrs.md` · 白皮书 §12.2

## 三套入口

限流有三套入口，都登记在 ASP.NET Core 的限流中间件上，超出额度时返回 429。

| 入口 | 包 | 计数放在哪里 | 多副本之间 |
|---|---|---|---|
| `AddKunRateLimit` | `Kun.RateLimit` | 进程内，固定窗口 | 不共享，每个进程各自计数 |
| `AddKunRateLimiter`、`AddKunSlidingWindowRateLimiter`、`AddKunTokenBucketRateLimiter` | `Kun.Gateway` | 进程内，固定窗口、滑动窗口或令牌桶 | 不共享，每个进程各自计数 |
| 网关代理路由的限流 | `Kun.Gateway` | Redis，固定窗口 | 连同一个 Redis、网关配置相同时共用额度 |

### `AddKunRateLimit`（`Kun.RateLimit`）

- 登记：`services.AddKunRateLimit(o => o.AddWindow("api", w => { w.PermitLimit = 100; w.Period = TimeSpan.FromMinutes(1); }))`，命名空间 `Kun.RateLimit.Extensions`。每个窗口登记为一个同名的策略，端点写 `RequireRateLimiting("api")`。
- 计数：用 `System.Threading.RateLimiting` 的固定窗口限流器，在进程内计数。
- 分区：每个窗口按 `WindowRule.PartitionKey` 分区，取 `Tenant`（默认）、`User` 或 `Ip`。
- 超限：返回 429 和 `Retry-After`，再调用 `KunRateLimitOptions.OnRejected`（设置了的话）。
- 用在：宿主自己的端点，一次登记几个命名窗口，各自设额度、窗口长度与分区键。

### `AddKunRateLimiter` 一组（`Kun.Gateway`）

- 登记：命名空间 `Kun.Gateway.RateLimiting`。`AddKunRateLimiter()` 登记 `kun-default`（固定窗口，每分钟 100 次），`AddKunSlidingWindowRateLimiter()` 登记 `kun-sliding-window`，`AddKunTokenBucketRateLimiter()` 登记 `kun-token-bucket`；端点写 `RequireRateLimiting(策略名)`。
- 计数：用 `System.Threading.RateLimiting` 的固定窗口、滑动窗口或令牌桶限流器，在进程内计数。
- 分区：由 `IRateLimitPartitionResolver` 决定，宿主没有登记时按租户，见「分区解析」。
- 用在：宿主自己的端点，算法可选固定窗口、滑动窗口或令牌桶。`AddKunGateway()` 总会调用 `AddKunRateLimiter()`，并给没写 `RateLimiterPolicy` 的 YARP 路由补上 `kun-default`，见「网关默认边界」。

### 网关代理路由的限流（`Kun.Gateway`）

- 登记：没有单独的扩展方法。`AddKunGateway(configuration, …)` 传入带路由与集群的 `GatewayConfiguration` 时启用，`MapKunGateway()` 把它绑到全部代理端点。每条路由必须写 `RateLimiterPolicy`，取 `kun-default`、`tenant` 或 `user`，否则装配时拒绝。
- 计数：在 Redis 里计数，用 `Kun.RateLimit` 的 `IRedisRateLimitStore`（固定窗口）；宿主要登记 `IConnectionMultiplexer`。
- 多副本：副本连同一个 Redis，且键前缀、`Issuer` 与路由配置相同时，共用额度。
- 分区与额度：`kun-default` 按可信客户端 IP；`tenant` 按租户，`user` 按租户内主体，匿名或身份不完整时都按 IP。额度、窗口与键前缀取 `Gateway:RateLimits`，见「声明式网关路由」。
- 用在：声明式配置路由的网关（如组件网关），代理路由的额度在多个副本之间共用。

## 分区解析 (IRateLimitPartitionResolver)

决定"同一个限流桶由谁共享"。从 HttpContext 里取一个字符串 key。

| 实现 | 分区依据 | 适用 |
|---|---|---|
| `IpRateLimitPartitionResolver` | `HttpContext.Connection.RemoteIpAddress` | 匿名 API、防 abuse |
| `TenantRateLimitPartitionResolver` | 已验证外部 JWT 的 `tenant_id` claim | 多租户共享库场景（默认） |
| `UserPrincipalRateLimitPartitionResolver` | 已验证外部 JWT 的 `sub` claim | Free-tier API，per-account 限额 |

**注册**：任意一种用 `services.AddSingleton<IRateLimitPartitionResolver, XxxResolver>()`。Kun 的 `AddKunXxxRateLimiter()` 系列扩展会用 `TryAddSingleton<...,TenantRateLimitPartitionResolver>()` 做默认，你预先注册就覆盖它。

## 限流策略 (IRateLimiterPolicy)

决定"桶里怎么发放许可"。

| 策略 | 行为 | 特点 | 默认值 |
|---|---|---|---|
| **FixedWindow**（`AddKunRateLimiter`） | 每 N 秒重置计数 | 最简单、内存最低，边界突刺 | 100 permits / 1 min |
| **SlidingWindow**（`AddKunSlidingWindowRateLimiter`） | 分段滑动 | 平滑，无突刺；内存与 `SegmentsPerWindow` 线性 | 100 / 1 min / 6 segments |
| **TokenBucket**（`AddKunTokenBucketRateLimiter`） | 令牌累积 + 匀速补充 | 吸收突发流量；适合 webhook fanout、批量导入 | 100 tokens burst，20 / 10s 补充 |

三者均调用 `AddRateLimiter()` 注册一个具名策略：`"kun-default"`、`"kun-sliding-window"`、`"kun-token-bucket"`。端点开启时写：

```csharp
app.MapGet("/orders", handler).RequireRateLimiting(KunRateLimiterExtensions.SlidingWindowPolicyName);
```

## 网关默认边界

`AddKunGateway()` 已注册 `kun-default`，并为未显式设置 `RateLimiterPolicy` 的 YARP 路由绑定它。业务路由同时默认要求认证；只有配置 `AuthorizationPolicy: "Anonymous"` 的路由可匿名访问。普通 ASP.NET 端点仍需调用上面的 `RequireRateLimiting`。三种内置策略超限均返回 429；限流器提供重试时间时，响应附整数秒 `Retry-After`。具名自定义策略也沿用默认拒绝回调，除非宿主显式覆盖。

IP 分区仅读取 `Connection.RemoteIpAddress`。宿主须在认证和限流之前调用 `UseForwardedHeaders()`；独立网关已接好。`Gateway:TrustedProxyAddresses`（IP 列表）和 `Gateway:TrustedProxyNetworks`（CIDR 列表）只填写实际反向代理出口，`ForwardLimit` 默认 1。两张名单均为空时关闭转发头处理，非信任来源伪造 X-Forwarded-For 不会切换限流桶。不要单独清空框架的 KnownProxies/KnownIPNetworks：框架会将其理解为信任全部。

## 典型组合

### 1. 多租户 SaaS，边界精度要求一般
```csharp
builder.Services.AddKunRateLimiter();   // Tenant partition + FixedWindow
```

### 2. 按用户限流，平滑曲线
```csharp
builder.Services.AddSingleton<IRateLimitPartitionResolver, UserPrincipalRateLimitPartitionResolver>();
builder.Services.AddKunSlidingWindowRateLimiter(o => { o.PermitLimit = 60; o.Window = TimeSpan.FromMinutes(1); });
```

### 3. 防 DDoS 按 IP，允许突发
```csharp
builder.Services.AddSingleton<IRateLimitPartitionResolver, IpRateLimitPartitionResolver>();
builder.Services.AddKunTokenBucketRateLimiter(o =>
{
    o.TokenLimit = 500;
    o.TokensPerPeriod = 50;
    o.ReplenishmentPeriod = TimeSpan.FromSeconds(1);   // 50 req/s 稳态
});
```

## Redis 后端：跨 Pod 分布式计数

内置的 FixedWindow / SlidingWindow / TokenBucket 策略都是**进程内**（每个 Gateway pod 各自独立计数）。在多 pod 部署时 100 permits 会变成 `100 × podCount` 的实际吞吐。

`Kun.RateLimit.IRedisRateLimitStore` 提供基础 primitive：

```csharp
using Kun.RateLimit.Extensions;

services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis:6379"));
services.AddKunRedisRateLimitStore(o => o.KeyPrefix = "kun:prod:rl");
```

调用：
```csharp
var result = await store.HitAsync("tenant-a", TimeSpan.FromMinutes(1));
// result.Count 现在有多少个命中、result.ResetsIn 距离重置还有多久
if (result.Count > 100) { /* reject */ }
```

Redis 用一条 Lua 脚本完成计数、查询剩余 TTL 和设置有效期。首次命中与已有计数但缺 TTL 的桶会得到完整窗口。正常后续命中沿用已有到期时间。服务侧与网关可以共用这个存储实现。

## 声明式网关路由

在 `Gateway:Routes` 中为每条路由指定 `RateLimiterPolicy`。组件网关把 `kun-default` 映射到可信客户端 IP、`tenant` 映射到租户、`user` 映射到租户内主体。网关使用 `Gateway:RateLimits` 中的前缀、窗口和各分区额度。所有副本配置相同的 Redis 连接与前缀即可共享额度。额度用尽时返回 429 和 Retry-After，存储不可用时返回 503。

[固定窗口存储](../../src/Kun.RateLimit/Redis/RedisFixedWindowRateLimitStore.cs) · [网关分区与响应](../../src/Kun.Gateway/GatewayRateLimiting.cs) · [平台外壳与配置](../../src/Kun.Gateway.Shell/README.md)
