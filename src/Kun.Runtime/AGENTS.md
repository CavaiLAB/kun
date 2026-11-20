# Kun.Runtime 开发约定

- 行为以组件模型规格 §5 为准；改公共行为同时更新 `tck/runtime/` 与 .NET harness（`tests/Kun.Runtime.Tests`）。
- 只依赖 Kun.Errors 与 .NET 的 DI、日志抽象；不引用 Kun.Core、Kun.Mediator、Kun.Validation。
- 不带产品语义：scope 种类只查语法，取值由平台档定；表面目录键在平台档。
- 服务、监听器和 restriction 都归属某次 Apply 的效果栈；Scoped 视图共享该栈，不能创建独立资源生命周期。
- 生命周期操作由装载器串行化；不要在 Apply 或 effect 内等待该装载器的 Mount / Unmount / WhenIdle。
- 不加入业务状态、数据库、租户层级或运行时代码热装卸；清单与 patch 解析由对应组件负责。
- 公共 API 写中文 XML 注释并登记 PublicAPI 文件。
