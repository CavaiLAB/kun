# Kun.Integration.AotTests

## Key Files

| File | Description |
|------|-------------|
| `GatewayPackageAotPublishTests.cs` | 普通打包 Kun.Gateway 与它依赖的四个 Kun 包，独立 Linux NativeAOT 宿主通过 PackageReference 消费并验证 JWKS。 |
| `GatewayStandaloneAotPublishTests.cs` | 网关的独立宿主 `hosts/Kun.Gateway.Standalone` 按本机 RID 原生发布并运行，请求健康检查、JWKS 与要认证的代理路由。 |
| `Fixtures/GatewayPackageConsumer/` | 复制到仓库外构建的消费者；不参加测试程序集编译，不允许引用 Kun 项目源码。 |
| `README.md` | 普通包消费回归的执行命令、隔离方式与验收边界。 |

## For AI Agents

### Working In This Directory

- Gateway 包消费回归里，库按普通类库打包（`Kun.Gateway` 是类库，不开 `PublishAot`，也就不开 Request Delegate Generator），宿主只能使用新生成的 NuGet 包；不能让宿主的发布属性传入库并掩盖普通制品的运行时绑定问题。
- 独立宿主的回归不写死端口：`ASPNETCORE_URLS` 给 `http://127.0.0.1:0`，从启动日志读实际地址；就绪用有上限的轮询；结束时停进程、删临时目录；断言消息带上进程的标准错误与标准输出。
