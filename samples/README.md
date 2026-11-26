# Kun Samples

本目录将包含完整的示例项目，演示如何使用 Kun Framework 构建生产级微服务。

## 计划中的示例

- **Kun.Sample.Catalog**: 商品目录服务，演示 DDD + CQRS + Outbox + gRPC
- **Kun.Sample.Order**: 订单服务，演示命令流程（下单 → 经 gRPC 预留库存 → 经 Outbox 发集成事件）
- **Kun.Sample.AppHost**: .NET Aspire 编排入口，一键拉起全部服务

## 使用文档的示例

`docs/` 收着网站「开始」里三篇使用文档（上手、写一个组件、在 .NET 里接权限）让读者整段写进文件的代码。它们不进 `Kun.slnx`，由 CI 的「使用文档示例」照页面建项目、装包、运行并核对输出，说明见 [docs/README.md](docs/README.md)。

## 运行示例（待实现）

```bash
cd samples/Kun.Sample.Catalog
dotnet run --project Kun.Sample.AppHost
```

访问 Aspire Dashboard（默认 `http://localhost:15888`）查看所有服务状态与分布式追踪。

## 参与

欢迎提交 PR 补充示例！请遵循根 [AGENTS.md 的「Kun 框架的构建、测试与代码约定」](../AGENTS.md#kun-框架的构建测试与代码约定)。
