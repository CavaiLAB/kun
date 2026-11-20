# Gateway 限流策略指南

> `Kun.Gateway.RateLimiting` 把 ASP.NET Core 内置的 RateLimiter 包装成**策略 × 分区**的两维组合：
> 三种分区解析（partition resolver）× 三种限流策略（limiter policy）= 九种可选配置，配合 Redis 后端做跨 Pod 计数共享。
>
> 参考：`docs/design/why-cqrs.md` · 白皮书 §12.2

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
