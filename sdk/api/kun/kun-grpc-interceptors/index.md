> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Grpc.Interceptors

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Grpc` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Grpc`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Grpc)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[TenantGrpcClientInterceptor](/sdk/api/kun/kun-grpc-interceptors/tenantgrpcclientinterceptor.md)

从当前可信租户上下文添加 gRPC tenant_id 元数据；认证令牌由宿主的身份客户端另行提供。

[TenantGrpcServerInterceptor](/sdk/api/kun/kun-grpc-interceptors/tenantgrpcserverinterceptor.md)

租户 gRPC 服务入口；宿主先完成 JWT 认证，再核对传播元数据，覆盖全部四种调用形态。
