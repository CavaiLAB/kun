> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Gateway

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Gateway.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Gateway.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Gateway.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.Gateway.DependencyInjection](/sdk/api/kun/kun-gateway-dependencyinjection/index.md)

[Kun.Gateway.Endpoints](/sdk/api/kun/kun-gateway-endpoints/index.md)

[Kun.Gateway.Identity](/sdk/api/kun/kun-gateway-identity/index.md)

[Kun.Gateway.Jwks](/sdk/api/kun/kun-gateway-jwks/index.md)

[Kun.Gateway.RateLimiting](/sdk/api/kun/kun-gateway-ratelimiting/index.md)

### Classes

[GatewayCluster](/sdk/api/kun/kun-gateway/gatewaycluster.md)

静态下游；Id 同时是 S2S 的唯一受众。

[GatewayConfiguration](/sdk/api/kun/kun-gateway/gatewayconfiguration.md)

网关声明式配置；只允许一个身份发行方，密钥值从不进入配置，PEM 路径指向宿主的 Secret 投射。

[GatewayDefinition](/sdk/api/kun/kun-gateway/gatewaydefinition.md)

网关 seam；Provider 安装到 gateway profile，业务组件只消费本契约。

[GatewayHosting](/sdk/api/kun/kun-gateway/gatewayhosting.md)

网关宿主装配；Identity.Client 的 Provider 由宿主独立组合，组件库不直接依赖其他 Provider。

[GatewayRateLimits](/sdk/api/kun/kun-gateway/gatewayratelimits.md)

代理路由的 Redis 固定窗口限额；同一逻辑网关的副本必须使用相同配置。

[GatewayRoute](/sdk/api/kun/kun-gateway/gatewayroute.md)

受保护路由；不开放匿名策略覆盖。

[S2sTokenIssuer](/sdk/api/kun/kun-gateway/s2stokenissuer.md)

Issues the closed, short-lived S2S claim contract with RS256 and the shared rotating JWKS ring.
An authenticated principal/resolver must provide explicit user/service identity, tenant context and user session/revision fields.
Trace context travels in headers, not additional JWT payload fields.

### Interfaces

[IGatewayService](/sdk/api/kun/kun-gateway/igatewayservice.md)

网关只读状态；路由配置在构建宿主时固定，未挂载时禁止代理请求。
