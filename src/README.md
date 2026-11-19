# src

Kun Framework 的核心源码。所有项目都继承 `/Directory.Build.props` 与 `/Directory.Packages.props` 的配置。

## 模块速查

| 模块 | 职责 | AOT Ready |
| :--- | :--- | :---: |
| [Kun.Core](Kun.Core/) | 领域抽象、CQRS 接口、Result、`ITenantContext`（零第三方依赖） | ✅ |
| [Kun.Errors](Kun.Errors/) | 错误码、错误码目录、`Error` 与线上错误模型 `ErrorDetail`（零依赖） | ✅ |
| [Kun.Errors.AspNetCore](Kun.Errors.AspNetCore/) | 错误转 HTTP 响应（problem+json 的 `ErrorDetail`） | ✅ |
| [Kun.Mediator](Kun.Mediator/) | `FrozenDictionary` 无反射调度、Pipeline Behaviors | ✅ |
| [Kun.Runtime](Kun.Runtime/) | 组件运行时：插件装载、服务键与注入、类型化事件、扁平 scope | — |
| [Kun.Plugin.Sdk](Kun.Plugin.Sdk/) | 插件开发包：组件的代码声明、生成契约、命令策略协议 | — |
| [Kun.Plugin.Sdk.Commands](Kun.Plugin.Sdk.Commands/) | 插件开发包的命令管道（平台档）：权限、事务、数据会话、钩子能力 | — |
| [Kun.Plugin.Sdk.Generators](Kun.Plugin.Sdk.Generators/) | 插件开发包的源生成器与 KUNSDK 诊断（只以分析器引用） | — |
| [Kun.Plugin.Sdk.AspNetCore](Kun.Plugin.Sdk.AspNetCore/) | 插件开发包的 HTTP 适配：插件可见性中间件与端点 | — |
| [Kun.MultiTenancy](Kun.MultiTenancy/) | HTTP、消息与后台任务的租户范围与传播 | ✅ |
| [Kun.EntityFrameworkCore](Kun.EntityFrameworkCore/) | 写侧 EF Core 实现、Outbox 落库、租户过滤器 | ⚠️ trim-safe |
| [Kun.EntityFrameworkCore.PostgreSQL](Kun.EntityFrameworkCore.PostgreSQL/) | EF 迁移的 PostgreSQL 租户 RLS 生成器 | — |
| [Kun.Data.Abstractions](Kun.Data.Abstractions/) | 平台数据与方言契约 | — |
| [Kun.Data](Kun.Data/) | 平台的 EF 数据组件 | — |
| [Kun.Data.AspNetCore](Kun.Data.AspNetCore/) | 数据目录 HTTP 端点 | — |
| [Kun.Data.Dialect.Pg](Kun.Data.Dialect.Pg/) | PostgreSQL 方言 | — |
| [Kun.Data.Native.Abstractions](Kun.Data.Native.Abstractions/) | 原生数据会话和事务的契约 | ✅ |
| [Kun.Data.Native.Postgres](Kun.Data.Native.Postgres/) | 原生数据会话的 PostgreSQL 实现 | ✅ |
| [Kun.Tenancy.Abstractions](Kun.Tenancy.Abstractions/) | 租户目录契约 | — |
| [Kun.Tenancy](Kun.Tenancy/) | 租户目录与生命周期 | — |
| [Kun.Tenancy.AspNetCore](Kun.Tenancy.AspNetCore/) | 租户目录 HTTP 端点 | — |
| [Kun.Tenancy.Signup.Abstractions](Kun.Tenancy.Signup.Abstractions/) | 自助注册契约 | — |
| [Kun.Tenancy.Signup](Kun.Tenancy.Signup/) | 自助注册流程 | — |
| [Kun.EventBus.Abstractions](Kun.EventBus.Abstractions/) | 稳定事件标记与 JSON 注册端口、Outbox 写入端口、集成事件信封（依赖 Core 与 Runtime） | ✅ |
| [Kun.EventBus](Kun.EventBus/) | 领域/集成事件抽象、Outbox 发布器 | ✅ |
| [Kun.EventBus.EntityFrameworkCore](Kun.EventBus.EntityFrameworkCore/) | EF Core 事务 Outbox 投递与事件组件 | — |
| [Kun.EventBus.RabbitMQ](Kun.EventBus.RabbitMQ/) | RabbitMQ 传输、消费端幂等 | ✅ |
| [Kun.Identity.Abstractions](Kun.Identity.Abstractions/) | 身份服务、身份连接器与账号目录的契约 | — |
| [Kun.Identity.Client](Kun.Identity.Client/) | OIDC 登录、Cookie 会话与服务间身份验证 | ✅ |
| [Kun.Identity.Server](Kun.Identity.Server/) | 可嵌入的 OpenIddict 服务、账号目录与会话管理 | — |
| [Kun.Identity.Connector.Oidc](Kun.Identity.Connector.Oidc/) | 身份服务的上游 OIDC 连接器 | — |
| [Kun.Security.Abstractions](Kun.Security.Abstractions/) | 服务间令牌的主体模型与声明词汇表（零依赖） | ✅ |
| [Kun.Security](Kun.Security/) | S2S JWT 校验与当前用户 | ✅ |
| [Kun.Validation](Kun.Validation/) | SourceGen 验证器 | ✅ |
| [Kun.AspNetCore](Kun.AspNetCore/) | Minimal API 扩展、Pipeline Behaviors | ✅ |
| [Kun.Grpc](Kun.Grpc/) | gRPC 拦截器、Result → RpcException 映射 | ✅ |
| [Kun.Gateway](Kun.Gateway/) | YARP 扩展、S2S 签发、限流 | ✅ |

## 开发规范

所有开发必须遵循根 [AGENTS.md 的「Kun 框架的构建、测试与代码约定」](../AGENTS.md#kun-框架的构建测试与代码约定)。核心约束：

- **Kun.Core 零第三方依赖**
- **领域层不得携带 ORM / JSON / 验证属性**
- **AOT 候选模块禁止运行时反射**
- **CI 强制 AOT 零警告发布**

详见 [CONTRIBUTING.md](../CONTRIBUTING.md) 与 [`docs/design/module-map.md`](../docs/design/module-map.md)。
