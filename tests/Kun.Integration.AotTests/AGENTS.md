# Kun.Integration.AotTests

## Key Files

| File | Description |
|------|-------------|
| `GatewayPackageAotPublishTests.cs` | 普通打包五个 Kun 依赖包，独立 Linux NativeAOT 宿主通过 PackageReference 消费并验证 JWKS。 |
| `Fixtures/GatewayPackageConsumer/` | 复制到仓库外构建的消费者；不参加测试程序集编译，不允许引用 Kun 项目源码。 |
| `README.md` | 普通包消费回归的执行命令、隔离方式与验收边界。 |

## For AI Agents

### Working In This Directory

- Gateway 回归的库打包必须保持 `PublishAot=false` 与 `EnableRequestDelegateGenerator=false`，宿主只能使用新生成的 NuGet 包；不能让宿主的发布属性传入库并掩盖普通制品的运行时绑定问题。
