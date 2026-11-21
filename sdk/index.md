> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# SDK

本部分只写**编程模型与 API**：怎样写一个组件、怎样让 Agent 用上它。某个组件怎么装、怎么配，写在[组件目录](/components/index.md)，两边互相链接、不重复。每节按「编程模型 → 示例 → API 参考」组织。

| 部分                                   | 包                                                                         | 内容                                                                                       |
| ------------------------------------ | ------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| [Kun 框架](/sdk/kun/index.md)          | `Kun.*` 内核包                                                               | 按发布标签生成的上手、模块、指南、安全与版本说明。装配对照见[内核与平台组件](/sdk/kun/with-qian.md)                           |
| [后端 SDK（.NET）](/sdk/dotnet/index.md) | `Kun.Runtime`、`Kun.Plugin.Sdk`、平台组件包                                      | 插件上下文、服务注册、类型化事件、可逆副作用、[能力目录](/sdk/dotnet/capabilities.md)与[事件分发](/sdk/dotnet/events.md) |
| [前端 SDK](/sdk/frontend/index.md)     | `@cavailab/qian-runtime`、`@cavailab/qian-plugin-sdk`、`@cavailab/qian-sdk` | 外壳插件树、MF2 远程模块、插槽                                                                        |
| [AI SDK](/sdk/ai/index.md)           | —                                                                         | 在插件与前端里调模型与 Embedding、写 Agent 原生表面、MCP、Agent 运行时对接                                       |

[Kun 内核 API 参考](/sdk/api/kun/index.md)由 DocFX 从正式版本的 NuGet 包生成。平台 HTTP API 和 TypeScript 参考的交付状态见 [API 参考](/sdk/api/index.md)。

「Kun 框架」使用仓库根 `CHANGELOG.md` 中已发布的最新版本和对应 Git 标签。仓库内的内核与平台项目使用源码引用，外部项目按发布清单选择具体的包。
