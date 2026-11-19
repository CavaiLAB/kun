# Kun.Gateway.Standalone

Kun 网关的独立宿主：校验外部身份提供方签发的 JWT，换成内部短期 S2S JWT 转发给下游；YARP 反向代理、限流、JWKS、健康检查与 gRPC-Web。它组合的是 `src/Kun.Gateway` 这个库（nuget.org 上的 `Kun.Gateway` 包）；自己写宿主的，照这里的 `Program.cs` 组合库即可。本项目不打包。

## 运行

```bash
dotnet run --project hosts/Kun.Gateway.Standalone
```

原生发布（按本机的 RID；交叉发布另给 `-r`）：

```bash
dotnet publish hosts/Kun.Gateway.Standalone -c Release
```

## 配置

`appsettings.json` 是样板，部署时用环境变量或别的配置源覆盖：

- `KunGateway`：外部签发方、受众与 JWKS 地址，内部签发方与受众，令牌有效期，密钥轮换的重叠期与检查间隔，可信代理，签名密钥路径 `SigningKeyPath` 与 `PublicKeyPath`（缺省在本目录的 `certs/` 下，已在 `.gitignore` 里）。
- `ReverseProxy`：YARP 的路由与集群。路由缺省要认证并走 `kun-default` 限流，公开路由显式写 `AuthorizationPolicy: "Anonymous"`。
- `KunObservability`：遥测；服务名是 `kun-gateway`，EF 埋点关闭。

各项的含义与安全要求见库的说明 `src/Kun.Gateway/GATEWAY.md` 与仓库的 `docs/security/s2s-jwt.md`。

## 验证

`tests/Kun.IntegrationTests` 的 `GatewayBoundaryTests` 起本宿主核对路由与限流边界、密钥轮换与 S2S 载荷；`tests/Kun.Integration.AotTests` 的 `GatewayStandaloneAotPublishTests` 原生发布本宿主并请求它的端点。
