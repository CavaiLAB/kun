# NuGet 包迁移指南

首个合并版发布后，从平台 0.x 或内核 2026.10.25 升级的项目可按以下步骤选择版本。内核档从 nuget.org 还原，平台档从 CavaiLAB 的 GitHub Packages 还原。beta 版本的两档包均从 GitHub Packages 还原。

1. 在该版本的 GitHub Release 下载 `NuGet.config` 与 `release-packages.json`。前者列出准确的包源映射，后者列出该版本的全部包和档位。已有包源配置的项目合并相应条目。
2. 使用有读取包权限的账号与令牌设置 `KUN_PLATFORM_FEED_USER` 和 `KUN_PLATFORM_FEED_TOKEN`，然后更新项目的 `PackageReference`。采用中央包管理的项目还需更新 `Directory.Packages.props`。
3. 将使用的 Kun 包固定到同一个日历版本。使用内部接口的包之间声明精确版本依赖。运行 `dotnet restore --configfile NuGet.config`，再构建并执行项目的测试。

## 平台 0.x 的包

以下对照覆盖 GitHub Packages 上已发布的 54 个包。这些包从 0.3.0 起停更，已发布的所有版本均保留，固定版本的项目可以继续还原。一行有多个目标时，可按使用的功能选取。

| 已发布包 | 合并版包 | 引用说明 |
|---|---|---|
| `Qian.Audit` | `Kun.Audit.EntityFrameworkCore` | EF 与命令审计。 |
| `Qian.Audit.Abstractions` | `Kun.Audit.Abstractions` | 更新包引用与对应的命名空间。 |
| `Qian.Audit.Native` | `Kun.Audit.Postgres` | 原生 PostgreSQL 审计。 |
| `Qian.Authz` | `Kun.Authorization.Rbac` | RBAC 权限计算。 |
| `Qian.Authz.Abstractions` | `Kun.Authorization.Rbac.Abstractions`、`Kun.Authorization` | RBAC 契约与内核授权端口按使用方引用。 |
| `Qian.Authz.AspNetCore` | `Kun.Authorization.AspNetCore` | HTTP 与 gRPC 的授权入口。 |
| `Qian.Capabilities` | `Kun.Capabilities` | 更新包引用与对应的命名空间。 |
| `Qian.Capabilities.Abstractions` | `Kun.Capabilities.Abstractions` | 更新包引用与对应的命名空间。 |
| `Qian.Capabilities.AspNetCore` | `Kun.Capabilities.AspNetCore` | 更新包引用与对应的命名空间。 |
| `Qian.Config.Abstractions` | `Kun.Configuration.Abstractions` | 配置契约进入内核档。 |
| `Qian.Config.Apollo` | `Kun.Configuration.Apollo`、`Kun.Configuration.Platform` | 前者接 Apollo 配置源，后者提供组件登记、开关与平台配置管理。 |
| `Qian.Config.AspNetCore` | `Kun.Configuration.AspNetCore` | 平台配置端点。 |
| `Qian.Config.Secrets` | `Kun.Configuration.Secrets` | 密钥解析提供者。 |
| `Qian.Data` | `Kun.Data`、`Kun.EntityFrameworkCore.PostgreSQL` | 平台装配使用前者，自定义 EF 宿主按需引入 PostgreSQL 租户迁移生成器。 |
| `Qian.Data.Abstractions` | `Kun.Data.Abstractions`、`Kun.EntityFrameworkCore` | 数据服务契约在前者，严格上下文与 EF 数据会话契约在后者。 |
| `Qian.Data.AspNetCore` | `Kun.Data.AspNetCore` | 更新包引用与对应的命名空间。 |
| `Qian.Data.Dialect.Pg` | `Kun.Data.Dialect.Pg` | 更新包引用与对应的命名空间。 |
| `Qian.Data.Native.Abstractions` | `Kun.Data.Native.Abstractions` | 更新包引用与对应的命名空间。 |
| `Qian.Data.Native.Postgres` | `Kun.Data.Native.Postgres` | 更新包引用与对应的命名空间。 |
| `Qian.Docs` | `Kun.Docs` | 更新包引用与对应的命名空间。 |
| `Qian.Docs.Abstractions` | `Kun.Docs.Abstractions` | 更新包引用与对应的命名空间。 |
| `Qian.Events` | `Kun.EventBus`、`Kun.EventBus.EntityFrameworkCore`、`Kun.EventBus.RabbitMQ` | 按写入信封、投递 Outbox、接入消息代理三项职责选包。 |
| `Qian.Events.Abstractions` | `Kun.EventBus.Abstractions` | 事件信封、来源作用域及拓扑契约。 |
| `Qian.Files` | `Kun.Files`、`Kun.Storage.Local` | 文件服务使用前者，本地传输与签名实现使用后者。 |
| `Qian.Files.Abstractions` | `Kun.Files.Abstractions`、`Kun.Storage.Abstractions` | 文件领域契约与底层存储端口分别引用。 |
| `Qian.Files.AspNetCore` | `Kun.Files.AspNetCore` | 更新包引用与对应的命名空间。 |
| `Qian.Gateway` | `Kun.Gateway`、`Kun.Gateway.Shell`、`Kun.RateLimit` | 代理与网络配置、平台外壳、限流实现分别引用。 |
| `Qian.Gateway.Abstractions` | `Kun.Gateway.Abstractions` | 更新包引用与对应的命名空间。 |
| `Qian.Identity.Abstractions` | `Kun.Identity.Abstractions`、`Kun.Security.Abstractions` | 人员、成员与连接器契约在前者，请求主体与 claim 契约在后者。 |
| `Qian.Identity.Client` | `Kun.Identity.Client`、`Kun.Identity.AspNetCore` | 平台客户端与内核 ASP.NET Core 身份验证分别引用。 |
| `Qian.Identity.Connector.Oidc` | `Kun.Identity.Connector.Oidc`、`Kun.Identity` | 连接器使用前者，共享 OIDC 协议与会话实现来自后者。 |
| `Qian.Identity.Server` | `Kun.Identity.Server` | 更新包引用与对应的命名空间。 |
| `Qian.Lifecycle` | `Kun.Lifecycle` | 更新包引用与对应的命名空间。 |
| `Qian.Lifecycle.Abstractions` | `Kun.Lifecycle.Abstractions` | 更新包引用与对应的命名空间。 |
| `Qian.Lifecycle.AspNetCore` | `Kun.Lifecycle.AspNetCore` | 更新包引用与对应的命名空间。 |
| `Qian.Plugin.Sdk` | `Kun.Plugin.Sdk`、`Kun.Plugin.Sdk.Commands` | 组件声明使用内核 SDK，使用事务命令管道时另加平台包。 |
| `Qian.Plugin.Sdk.AspNetCore` | `Kun.Plugin.Sdk.AspNetCore` | 更新包引用与对应的命名空间。 |
| `Qian.Plugin.Sdk.Generators` | `Kun.Plugin.Sdk.Generators` | 更新包引用与对应的命名空间。 |
| `Qian.Profiles` | `Kun.Profiles` | 更新包引用与对应的命名空间。 |
| `Qian.Profiles.Abstractions` | `Kun.Profiles.Abstractions` | 更新包引用与对应的命名空间。 |
| `Qian.Registry` | `Kun.Registry` | 更新包引用与对应的命名空间。 |
| `Qian.Registry.Abstractions` | `Kun.Registry.Abstractions` | 更新包引用与对应的命名空间。 |
| `Qian.Registry.AspNetCore` | `Kun.Registry.AspNetCore` | 更新包引用与对应的命名空间。 |
| `Qian.Runtime` | `Kun.Runtime` | 更新包引用与对应的命名空间。 |
| `Qian.Runtime.Errors` | `Kun.Errors` | 进程内错误码与错误契约。 |
| `Qian.Runtime.Errors.AspNetCore` | `Kun.Errors.AspNetCore` | 将错误映射为 HTTP 响应。 |
| `Qian.Scheduler` | `Kun.Scheduler` | 更新包引用与对应的命名空间。 |
| `Qian.Scheduler.Abstractions` | `Kun.Scheduler.Abstractions` | 更新包引用与对应的命名空间。 |
| `Qian.Surfaces` | `Kun.Surfaces` | 更新包引用与对应的命名空间。 |
| `Qian.Surfaces.Abstractions` | `Kun.Surfaces.Abstractions` | 更新包引用与对应的命名空间。 |
| `Qian.Surfaces.Hosting` | `Kun.Surfaces.Hosting` | 更新包引用与对应的命名空间。 |
| `Qian.Tenancy` | `Kun.Tenancy`、`Kun.MultiTenancy` | 租户开通使用前者，基础租户作用域与消息传播使用后者。 |
| `Qian.Tenancy.Abstractions` | `Kun.Tenancy.Abstractions` | 更新包引用与对应的命名空间。 |
| `Qian.Tenancy.AspNetCore` | `Kun.Tenancy.AspNetCore` | 更新包引用与对应的命名空间。 |

## 内核包的替代项

| 停发的包 | 使用方要做的调整 |
|---|---|
| `Kun.Audit` | 命令审计契约使用 `Kun.Audit.Abstractions`，按宿主选择 `Kun.Audit.EntityFrameworkCore` 或 `Kun.Audit.Postgres`。 |
| `Kun.Auditing` | 实体变更审计迁至 `Kun.Audit.EntityFrameworkCore`。 |
| `Kun.Dapper` | 原生 PostgreSQL 通道使用 `Kun.Data.Native.Postgres` 与 `Kun.Data.Native.Abstractions`。逐条迁移自定义 SQL。 |
| `Kun.Infrastructure` | 写侧直接使用业务 DbContext 与工作单元，读侧直接投影为查询 DTO。 |
| `Kun.MultiTenancy.EntityFrameworkCore` | 引用 `Kun.EntityFrameworkCore`，更新注册扩展所在的命名空间。 |
| `Kun.Saga` | 补偿编排器归档。依赖它的业务需要保留当前版本，或在应用中迁移编排与补偿逻辑后升级。 |
| `Kun.Sandbox` | 沙箱已归档。先迁移宿主中的隔离实现，再移除包引用。 |
| `Kun.Storage` | 引用 `Kun.Storage.Abstractions`，并按后端选择 `Kun.Storage.Local`、`Kun.Storage.S3` 或 `Kun.Storage.Oss`。 |

## 同名包的行为变化

- `Kun.EventBus.RabbitMQ` 使用组件事件信封与相应拓扑。升级前核对发布端与订阅端的交换机、路由键、消息形状和消费者组，协调切换生产与消费。
- `Kun.Audit.EntityFrameworkCore` 属于平台档，升级时将这个包的源映射指向 GitHub Packages。实体审计与业务事务的接线见该版本的包说明。
- `Kun.Gateway` 提供类库。把宿主的可执行入口切到网关宿主，使用平台外壳时再引用 `Kun.Gateway.Shell`。选项类型 `KunGatewayOptions` 换成 `Kun.Gateway.Abstractions` 里的 `GatewayConfiguration`，`ExternalIssuer`、`ExternalAudience`、`InternalIssuer` 依次改名为 `IdentityIssuer`、`IdentityAudience`、`Issuer`。Redis 固定窗口计数器的类型（`IRedisRateLimitStore`、`RateLimitHitResult`、`RedisRateLimitStoreOptions`、`RedisFixedWindowRateLimitStore`）改在 `Kun.RateLimit` 命名空间，注册扩展 `AddKunRedisRateLimitStore` 改在 `Kun.RateLimit.Extensions`。
- 租户上下文使用 `Guid?` 表示租户或平台作用域。处理消息与网络输入时先验证租户标识，数据库与既有消息的文本按原有格式读写。
- `IKunPlugin<TConfig>` 的实现通过 `ConfigTypeInfo` 提供源生成的 JSON 元数据。使用 `JsonSerializerDefaults.Web` 保持配置属性名匹配规则。
- HTTP 错误与进程内错误使用 `Kun.Errors` 定义的错误码和 `ErrorDetail`。依赖旧错误字面或响应结构的调用方需要同步更新解析逻辑。

各项公共 API、注册方法与部署变更见[版本变更记录](../../CHANGELOG.md)。
