# Changelog

> 本文件是 Kun 框架的变更记录。旧条目照原文，只替换了引擎与下游产品的名字（2026-11-18）和旧的仓库地址（2026-11-19），方括号里是整理时替换的说法；旧条目里的 PR 编号换成了本仓库里对应的合并提交，「review #N」是代码审查条目的序号，不是 PR。

本文档记录 Kun Framework 的所有显著变更。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)。版本号采用 **CalVer**（`vYYYY.M.D[-hotfix.N|-beta.N]`，从 `main` 打 tag，见 `CONTRIBUTING.md`）；更早的历史条目遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

## [Unreleased]

### Changed — 异常信息

- 以下异常的消息文字有调整，异常类型、错误码与触发条件不变：`KunDataMisconfiguredException` 的四条装配错误（迁移生成器、软删除拦截器、数据模型约定、模型约定只用于 `KunStrictDbContext`），以及 `AddKunCapability` 在框架当前主体端口解析为匿名兜底时的启动检查（`InvalidOperationException`）。按消息原文匹配的代码请改为按异常类型或错误码判断。

### Fixed — 补记 2026.11.21 网关一节的类型迁移

`2026.11.21`「网关装配与限流」一节列了配置节与键名的变化，代码里用到的类型补记如下：

- `Kun.Gateway.Configuration.KunGatewayOptions` 不再提供，改用 `Kun.Gateway.Abstractions` 包里的 `Kun.Gateway.GatewayConfiguration`。`ExternalIssuer`、`ExternalAudience`、`InternalIssuer` 依次改为 `IdentityIssuer`、`IdentityAudience`、`Issuer`，其余属性同名；新类型是记录，属性在创建时赋值。**迁移**：原来用 `Configure<KunGatewayOptions>(…)` 或绑定 `KunGateway` 节的，把 `GatewayConfiguration` 传给 `AddKunGateway(configuration)`，或不传、由 `AddKunGateway()` 读取 `Gateway` 节；注入 `IOptions<KunGatewayOptions>` 的改为 `IOptions<GatewayConfiguration>`。
- Redis 固定窗口计数器的类型从 `Kun.Gateway` 包的 `Kun.Gateway.RateLimiting` 命名空间挪到 `Kun.RateLimit` 包：`IRedisRateLimitStore`、`RateLimitHitResult`、`RedisRateLimitStoreOptions`、`RedisFixedWindowRateLimitStore` 的命名空间改为 `Kun.RateLimit`，注册扩展 `AddKunRedisRateLimitStore` 改由 `Kun.RateLimit.Extensions.KunRateLimitServiceCollectionExtensions` 提供。**迁移**：加 `using Kun.RateLimit;`，调用注册扩展的再加 `using Kun.RateLimit.Extensions;`。配置节 `Kun:Gateway:RateLimit:Redis` 与缺省键前缀 `kun:rl` 不变，引用 `Kun.Gateway` 的项目经它传递拿到 `Kun.RateLimit`。

### Fixed — 更正服务间令牌各入口的核对说明

`2026.11.21`「身份依赖方端口与服务间令牌」一节关于签发与接收时核对的写法，按入口更正如下：

- 网关的 `S2sTokenIssuer` 按服务间令牌契约核对主体快照后再签发。
- `AddKunServiceAuthentication`（`Kun.Security`）校验签名（只收 RS256、ES256）、签发方、受众与有效期，时钟容差 30 秒；签发方与受众须是配置的 `Issuer`、`AdditionalValidIssuers` 与 `Audience`、`AdditionalValidAudiences` 之一。
- `AddKunIdentityS2s`（`Kun.Identity.Client`）在 `AddKunServiceAuthentication` 的基础上要求令牌类型为 `JWT`，并按契约核对字段闭合、受众恰好是本服务、身份、角色、scope 与有效期（不超过 300 秒），通过后把主体快照交给当前用户适配器。下游要做完整的契约核对时用这个入口；自己配置 JWT 方案的，在 `OnTokenValidated` 里调用 `PrincipalClaims.ReadS2s`，再核对目标签发方与受众。
- 外部访问令牌投影复用其中的身份字段核对。字段形状见 `contracts/v0/s2s-claims.schema.json`。

### Fixed — 补记 2026.11.21 事件与审计的行为变化与删除项

`2026.11.21` 的事件与审计各节没有写全的行为变化与删除项，补记如下：

- 自 2026.11.21 起，普通事件经 RabbitMQ 发布时不要求路由到队列（`mandatory: false`）：没有队列绑定该事件的路由键时，broker 丢弃这条事件并照常确认，Outbox 行标为已发布。此前 `Kun.EventBus.RabbitMQ` 的发布器按 mandatory 发布，路由不到时发布报错、由 Outbox 重试；现在不报错也不重试。需要接收事件的宿主要先部署订阅、让队列完成绑定，再让生产方发送。
- 自 2026.11.21 起，实体审计的保存需要主体：不在 `EventEnvelopeScope` 内（命令管道会打开它）、`AuditContext.ActorId` 为空、当前用户也未认证时，拦截器拒绝这次保存，抛出错误码为 `EVENTS_PUBLISH_REJECTED` 的 `KunEventsException`，业务实体、审计日志与 Outbox 都不写入。后台作业、匿名请求等没有已认证用户的保存，要先用 `EventEnvelopeScope.Begin` 以实际的服务主体打开作用域，打开后可用同一上下文重试。
- 2026.11.21 的两节 Fixed（RabbitMQ 连接已被对端关掉时的释放、订阅端停止时的确认）写的是 `RabbitMqConnectionProvider` 与当时的订阅端，这两样在同一版里已经删除，2026.11.21 的包里没有。现在的订阅端（由 `AddKunEventsSubscriber` 登记）停止时同样不在正在关的通道上确认或拒绝，关通道遇到连接已被对端关掉时照常释放通道。
- 2026.11.21 还删除了下列公共类型，前文没有逐个列出：
  - `Kun.EventBus`：`Kun.EventBus.Background` 下除 `IOutboxLockProvider`、`RedisOutboxLockProvider` 以外的类型，即 `IOutboxMessageStore`、`IOutboxBatchStore`、`NullOutboxLockProvider`、`OutboxPublisherOptions` 与前文列过的 `OutboxPublisherBackgroundService`；以及 `Kun.EventBus.DependencyInjection.KunOutboxPublisherExtensions`。
  - `Kun.EventBus.RabbitMQ`：此前的公共类型全部删除，即 `RabbitMqConnectionProvider`、`Options.RabbitMqOptions`、`Publishing.RabbitMqIntegrationEventPublisher`、`Consuming.RabbitMqIntegrationEventSubscriber`、`DependencyInjection.KunRabbitMqBuilder` 与 `DependencyInjection.KunRabbitMqServiceCollectionExtensions`（命名空间都在 `Kun.EventBus.RabbitMQ` 下）。现在的公共类型是 `KunEventsRabbitMqExtensions`、`KunEventsSubscriberBuilder` 与 `KunEventsSubscriberOptions`。
  - `Kun.Audit.EntityFrameworkCore`：`AuditSaveChangesInterceptor`、`EfAuditWriter<TDbContext>`、`PostgresAuditChain` 与 `DependencyInjection.KunAuditEntityFrameworkCoreServiceCollectionExtensions`（`AddKunAuditEntityFrameworkCore`、`UseEntityFramework<TDbContext>`、`UsePostgresChain`），命名空间都在 `Kun.Audit.EntityFrameworkCore` 下。替代用法见前文「命令链与实体审计」。

## [2026.11.21] - 2026-11-21

本版停发 8 个旧包，包括 `Kun.Infrastructure`、`Kun.Dapper`、`Kun.Saga`、`Kun.Sandbox`、`Kun.Storage`、`Kun.Auditing`、`Kun.Audit` 和 `Kun.MultiTenancy.EntityFrameworkCore`。nuget.org 保留既有版本，各包的替代实现与升级步骤见下文。

平台 0.x 的 54 个 NuGet 包停更并保留全部历史版本。当前升级路径见[包迁移指南](https://github.com/CavaiLAB/kun/blob/main/docs/guides/package-migration.md)。

<!-- package-migration-table -->

### Changed — .NET 构建、测试与打包（S1-KUN-029）

- `Qian.slnf` 已移除。从仓库根运行 `dotnet build Kun.slnx -c Release`。平台测试使用 `tests/kun-platform.slnf`，根单元测试使用 `tests/kun-unit.slnf`。CI 的分片清单可用 `python3 scripts/dotnet_ci.py shard a --out /tmp/kun-tests-a.slnf` 生成。
- 需要每个测试项目分别输出 TRX 的命令和 CI 作业，请将 `-p:QianTrxPerModule=true` 改为 `-p:KunTrxPerModule=true`。打包工具可读取 `scripts/release_packages.py list` 的项目清单，`QianIsPackage` 和 `QianPackageId` 两个 MSBuild 属性已移除。
- `python3 scripts/release_packages.py list` 列出统一解决方案的发包项目。构建和打包使用同一个 `-p:Version` 值，随后用 `python3 scripts/release_packages.py verify-packages --version "$VERSION" --packages "$PACKAGES"` 核验该目录的制品。

### Changed — 平台分析器

- `QIAN0001` 至 `QIAN0006` 的新诊断号依次为 `KUNAPI001`、`KUNLEGACY002`、`KUNDATA001`、`KUNDATA002`、`KUNAUTHZ001`、`KUNEVENT001`。配置本组诊断的项目须同步更新 `.editorconfig`、`NoWarn` 和 `#pragma warning` 中的编号。
- 平台档包与宿主通过项目声明接入禁用符号和平台分析器。门禁样例通过 `KunPlatformAnalyzers=true` 显式接入。

### Changed — 样例、开发工具与测试宿主（S1-KUN-029）

- CRM 样例位于仓库根的 `samples/Kun.Sample.Crm`。开发工具、文档工具和镜像自检分别位于 `tools/Kun.Dev.Shell`、`tools/Kun.Dev.TenantOps`、`tools/Kun.Docs.Tool` 与 `tools/Kun.Image.SelfCheck*`。这些项目均为可执行程序。租户运营开发工具的日志类别为 `Kun.Dev.TenantOps`。

### Changed — 平台组件和平台宿主（S1-KUN-029）

- 能力目录、注册表、表面、配置文件、文档、生命周期和调度共 18 个包使用 `Kun.*` 包名。更新项目的包引用、`using` 及按这些类型名设置的日志类别。能力目录、注册表、生命周期与调度的契约位于相应的 `Kun.<组件>` 命名空间。表面、配置文件和文档的契约位于 `Kun.<组件>.Abstractions`。
- 宿主使用 `hosts/Kun.Platform.Host`。装配入口为 `AddKunPlatform`、`UseKunPlatform`、`MapKunPlatform`。能力目录、注册表、生命周期与表面的扩展方法同样使用 `Kun` 前缀。镜像构建的 `--project` 参数接受相对仓库根的项目路径。
- HTTP 契约的类型键由完整类型名和程序集简单名派生，泛型实参递归使用同一规则。注册表、租户与开户的三份 OpenAPI 和两个界面包已重新生成，端点与 operationId 保持兼容。
- 组件版本为 `qian.capabilities` 1.1.0、`qian.lifecycle` 1.1.0、`qian.registry` 1.2.0、`qian.scheduler` 1.1.0、`qian.tenancy` 2.3.0、`qian.tenancy.signup` 2.3.0。


### Added — 容器测试支持（S1-KUN-028）

- xUnit v3 集成测试可引用内核包 `Kun.Testing.Containers`，使用 PostgreSQL、Redis、RabbitMQ、PgBouncer 与 S3 兼容服务等夹具。镜像可按变量覆盖，PgBouncer 和 RustFS 的默认镜像按摘要钉住。
- 框架的 RabbitMQ 兼容套件继续使用 3.13，通用夹具默认使用 4.3。应用连接使用受行级安全约束的 DML 角色，建表和准备数据使用夹具的管理连接。

### Changed — Apollo 配置源与配置契约（S1-KUN-028）

- 引导参数 `KunConfigurationOptions` 和扩展 `AddKunConfigurationSources` 迁至内核包与命名空间 `Kun.Configuration.Apollo`。使用宿主原有的代码时引入这个命名空间。宿主仍可使用相同的源顺序、AccessKey 引用解析、快照扫描和缓存。
- 内核契约 `Kun.Configuration.Abstractions` 提供配置与密钥读取端口、开关规则的解析及不可变结果。租户命中、百分比放量、管理与审计写入继续由 `Kun.Configuration.Platform` 提供。独立配置源通过 `ApolloConfigurationDescription.From` 暴露本实例的只读引导信息和脱敏发布记录。

### Changed — 强类型配置与重载通知（S1-KUN-028）

- `Kun.Configuration.Extensions` 提供四个 `AddKunOptions<T>` 重载，AOT 宿主使用绑定委托，三个反射重载保留动态代码与裁剪标注。反射重载现在在锁外报告被拒的候选值，释放配置根引发的 `ObjectDisposedException` 继续向调用方传播。
- Schema 统一为 `Kun.Configuration.OptionsSchema`，按节名、选项类型和可选组件 id 枚举。`IKunConfigurationValidator<T>` 迁至 `Kun.Configuration` 命名空间。
- 宿主可注册 `IObserver<OptionsReloadFailure>` 或订阅 `IObservable<OptionsReloadFailure>` 获取仅含元数据的失败通知。使用平台配置时，一次拒绝分别计入内核的 `kun.configuration.reload.rejected` 和平台的 `qian.config.reload.failed`。失败日志使用 `Kun.Configuration.Options` 类别和事件号 2101。

### Changed — 配置、文件与测试支持（S1-KUN-028）

- 配置契约、平台提供者、ASP.NET Core 端点与挂载式密钥包现在分别使用 `Kun.Configuration.Abstractions`、`Kun.Configuration.Platform`、`Kun.Configuration.AspNetCore` 和 `Kun.Configuration.Secrets`。平台配置、选项、特性开关与端点扩展统一使用 `Kun` 前缀。
- 文件的契约、提供者和 HTTP 端点分别迁至 `Kun.Files.Abstractions`、`Kun.Files` 与 `Kun.Files.AspNetCore`。配置和文件组件升至 1.1.0。清单中的组件标识、权限点、路由、数据库 schema 与已发布包钉继续兼容。
- 平台配置计量器由 `Qian.Config` 改为 `Kun.Configuration.Platform`，继续报告 `qian.config.reload.failed`。日志类别 `Qian.Config.Flags`、`Qian.Config.Options` 与 `Qian.Config.Http` 对应改为 `Kun.Configuration.Platform.Flags`、`Kun.Configuration.Platform.Options` 与 `Kun.Configuration.AspNetCore.Http`。
- 配置与密钥命令行工具分别迁至仓库根的 `tools/Kun.Configuration.Tool` 和 `tools/Kun.Configuration.Secrets.Tool`。集成测试共享夹具使用 `Kun.Testing.Containers` 命名空间。

### Removed — Mediator 静态能力目录（S1-KUN-029）

- 移除 `Kun.Mediator.ICapabilityRegistry`、`CapabilityDescriptor` 及带能力 ID 和 Schema 的 `AddRequestHandler` 和 `AddAuthorizedHandler` 重载。处理器使用无参数的 `AddRequestHandler`，受保护请求使用仅传拒绝回调的 `AddAuthorizedHandler`。
- 能力 ID 由请求上的 `[Capability]` 声明。输入和输出 Schema 从 `Kun.Plugin.Sdk.CapabilityContract` 获取，按租户枚举已挂载能力时使用平台能力目录。

### Changed — HTTP 和 gRPC 授权执行点（S1-KUN-027）

- 在 `Kun.Authorization.AspNetCore` 中使用 `AddKunAuthorization`、`RequireKunPermission`、`UseKunAuthorization` 及 `AddKunAuthorizationInterceptor`。普通宿主登记自己的 `IPermissionChecker`，组件宿主提供当前请求的插件上下文。库模式的启动日志说明不写决策审计、不比对身份修订。原来从 Kun.AspNetCore 间接取得 Kun.Authorization 或 Kun.Security 的项目需要显式引用。
- 权限点为 `<plugin-id>:<resource>:<action>`，例如 `sample.crm:item:read`。缺少组件前缀的 `sample:item:read` 在端点声明时抛出「权限点必须是 <plugin-id>:<resource>:<action> 全名」。权限拒绝由 HTTP ErrorDetail 或 gRPC 状态返回，gRPC 对权限执行点和漏装拒绝都写入 `kun-error-code` 和 `qian-error-bin`。
- 调用 `AddKunGrpc()` 的宿主在全局或按服务登记 `AddKunAuthorizationInterceptor()`，带权限声明的调用在漏装时被拒绝。映射数据端点时传入 `(route, permission) => route.RequireKunPermission(permission)`，与文件端点使用相同的回调校验。

### Changed — 文件存储与本地签名传输（S1-KUN-028）

- 文件组件升为 2.0.0。公开服务键为 `Files`，按挂载的 `storage.kind` 使用 `IFileStorage`。`Files.Backends:local`、`IFileService.Backends`、旧后端注册表、工厂和后端上下文已移除。
- `Kun.Storage.Local.LocalSignedTransfers` 可签发限时上传与附件下载地址，按内容类型与声明长度接收上传。`Kun.Files` 的 local 端点现在原子创建上传对象，并发写入同一地址时只有一个请求成功。
- 开发宿主可以使用进程共享的临时签名密钥，其他环境须配置至少 32 字节的 `KunFilesOptions.LocalSigningKey`。多实例使用同一密钥。

### Changed — 权限执行请求与数据过滤（S1-KUN-027）

- HTTP、gRPC、命令和配置入口使用 `Kun.Authorization.AuthorizationSubject`。把 `EnforcementSubject` 的身份修订、委托状态改传给 `EnforcementRequest` 的必填参数 `IdentityRevision` 与 `Delegated`。修订为空的令牌仍需显式传 `null`。
- `IDataPermissionFilter` 的进程内过滤使用 `GetExpressionAsync<T>`。序列化的数据范围采用数据契约的 `filter-ast.schema.json` 和权限的数据范围计划。删除 `DataPermissionPredicate` 和 `GetPredicateAsync`。

### Changed — 权限包、组件入口与计量器（S1-KUN-027）

- 将包引用与命名空间从 `Qian.Authz.Abstractions`、`Qian.Authz`、`Qian.Authz.AspNetCore` 分别改为 `Kun.Authorization.Rbac.Abstractions`、`Kun.Authorization.Rbac`、`Kun.Authorization.AspNetCore`。
- 映射组件端点改用 `MapKunAuthz`，登记授权事件改用 `AddKunAuthzEvents`。组件 `qian.authz` 的版本升至 1.1.0，清单入口为 `Kun.Authorization.Rbac.AuthzComponent, Kun.Authorization.Rbac`。
- 订阅授权指标的宿主将计量器名从 `Qian.Authz` 改为 `Kun.Authorization.Rbac`。仪表继续使用 `qian.authz.*`。

### Changed — 命令链与实体审计

- `Qian.Audit.Abstractions`、`Qian.Audit`、`Qian.Audit.Native` 的代码分别迁入 `Kun.Audit.Abstractions`、`Kun.Audit.EntityFrameworkCore`、`Kun.Audit.Postgres`。命令链契约使用 `Kun.Audit` 命名空间。原生提供者使用 `Kun.Audit.Postgres` 命名空间。实体拦截器沿用 `Kun.Auditing.AuditingSaveChangesInterceptor` 日志类别。组件 `qian.audit` 升至 1.1.0。数据库 schema、链协议、组件 ID 均保持兼容。
- 原 `Kun.Audit` 命令管道、链存储与实体日志读端口 `IAuditReader.QueryAsync`、`AuditQuery`、`EfAuditReader` 归档。新的 `IAuditWriter` 与 `IAuditReader` 用于命令链，成功命令与业务同事务提交，失败命令不入链。
- `Kun.Audit.EntityFrameworkCore` 保留包名，改为平台档的命令链与实体审计实现。原来从 nuget.org 引用该包的项目须将其映射到 CavaiLAB 的 GitHub Packages。迁移原 `Kun.Auditing` 实体审计时，业务宿主通过 `AddKunAuditing<TContext>()` 登记非严格的 `KunDbContext`。宿主通过业务上下文的 `Set<AuditLog>()` 读取实体日志。默认 v1 和 v2 拦截器在正在保存的上下文暂存标准 Outbox 信封，数据库失败后可在原上下文重试。

### Changed — 组件运行时的类型化配置

- `Kun.Runtime` 支持 NativeAOT。实现 `IKunPlugin<TConfig>` 的组件须增加 `JsonTypeInfo<TConfig> ConfigTypeInfo { get; }`，从采用 `JsonSerializerDefaults.Web` 的源生成上下文返回配置契约。配置属性名继续按 Web 约定匹配。

### Changed — 网关装配与限流（S1-KUN-026）

- `Kun.Gateway.Abstractions` 提供 `GatewayConfiguration`、网关服务与路由契约。宿主使用 `AddKunGateway`，YARP 在分发前按目标服务签发一次 S2S。独立宿主读取 `Gateway` 配置节。迁移 `KunGateway` 配置时，将 `ExternalIssuer`、`ExternalAudience`、`InternalIssuer` 依次改为 `IdentityIssuer`、`IdentityAudience`、`Issuer`。其余原选项使用新节中的原键名，独立 YARP 路由继续读取 `ReverseProxy`。
- 平台外壳托管、引导端点与组件登记位于 `Kun.Gateway.Shell`，通过 `AddKunGatewayShell` 注册。Redis 固定窗口计数器现由 `Kun.RateLimit` 提供，路由分区由 `Kun.Gateway` 选择。
- 生产网关宿主在 `hosts/Kun.Gateway.Host`。镜像工具的 `--project` 参数现使用相对仓库根的项目路径。

### Changed — OIDC 客户端与上游连接器（S1-KUN-026）

- 两个入口共用发现、授权码交换与验签。配置的 issuer 必须与发现文档逐字相等，包括末尾斜杠。关闭 HTTPS 要求后仅允许回环 HTTP。回调应把授权响应中的 `iss` 原样传给 `OidcCodeExchange.ResponseIssuer`。声明支持 RFC 9207 的发行方必须返回该参数。
- 客户端根据发现文档选用 Basic 或表单认证。令牌端点须返回 HTTP 200。出站请求限时 30 秒，单份响应上限 1 MiB，默认 HTTP 处理器关闭自动重定向。JWKS 缓存 1 小时，未知签名密钥可每隔 30 秒提前刷新。
- `AllowedSigningAlgorithms` 默认接受 RS、PS、ES 三族各 256、384、512 位签名，可配置为其中的子集。`AllowAdditionalAudiences` 在应用侧默认关闭，在平台连接器默认开启。多个受众需要本客户端作为 `azp`。`ClockSkew` 默认 2 分钟，可设 0 至 5 分钟。需要原应用侧容差的宿主可设 `TimeSpan.FromSeconds(30)`。
- 平台身份客户端保留服务端口、浏览器端点与组件配置。会话、主体和服务认证实现进入 `Kun.Identity.AspNetCore`，原包通过类型转发继续提供 `IdentitySessionTokens`。

### Changed — 身份平台项目（S1-KUN-026）

- 身份包 `Qian.Identity.Abstractions`、`Qian.Identity.Client`、`Qian.Identity.Server`、`Qian.Identity.Connector.Oidc` 迁至仓库根 `src/`，包名与命名空间统一使用 `Kun.Identity.*`。宿主使用的 `AddQianIdentity…`、`MapQianIdentity…`、`UseQianIdentity…` 方法同步更名为 `AddKunIdentity…`、`MapKunIdentity…`、`UseKunIdentity…`。
- 组件 `qian.identity.client` 升至 1.1.0，`qian.identity.server` 升至 1.2.0。入口程序集和身份事件元数据随代码更新。已发布的 NuGet 运行时坐标继续沿用当前包源的版本。

### Changed — 统一事件投递与收发（S1-KUN-025）

- **Outbox 与 RabbitMQ**：发布端与消费端统一使用 v1 信封。升级前先排空旧 Outbox 行与队列，并协调切换收发两端。普通事件使用 `qian.events`、无版本后缀的 subject 和完整信封；竞争队列为 `q.<service>.<subscription>`，广播队列为 `q.<service>.<instance>`，死信按队列路由到 `qian.dlx`。重试头变为 `x-qian-attempts`，幂等键按订阅区分并默认保留 7 天。同一 subject 的不同版本共用路由键，未订阅的版本会转入相应死信队列。
- 宿主用 `AddKunEventsBroker` 和 `AddKunEventsSubscriber` 取代 `AddRabbitMq`；订阅以具体 handler 类型登记。事件类型须登记源生成 JSON 元数据，并在 `EventEnvelopeScope` 内发布。普通 `KunDbContext` 调用 `AddKunOutboxRelay<TContext>()`，严格上下文沿用无泛型重载。原有 `IntegrationEventOutboxAccumulator`、`OutboxPublisherBackgroundService`、`EfOutboxMessageStore`、`AddKunOutboxPublisher` 和 `UseRedisOutboxLock` 归档。自定义发布器从 Outbox 信封读取租户。
- 实体审计的 v1 和 v2 事件以标准信封发布，原字段在 `payload` 中。非命令的后台保存通过 `EventEnvelopeScope` 提供真实服务主体。
- 投递活动源从 `Kun.EventBus.Outbox` 切换到 `Kun.EventBus.EntityFrameworkCore`；保留 `outbox.publish` 生产者 span。冻结的记录变更信封契约只改了描述文字。

### Changed — 事件发布、投递与订阅（S1-KUN-025）

- `Kun.EventBus` 提供 `AddKunEvents()` 与事件目录。`Kun.EventBus.RabbitMQ` 提供 `AddKunEventsBroker` 和 `AddKunEventsSubscriber`。持有业务数据库的宿主另外调用 `Kun.EventBus.EntityFrameworkCore` 的 `AddKunOutboxRelay()` 启动投递，可用 `AddKunEvents(options => …)` 配置领取租约与重试。
- `Qian.Events` 的发布端、投递端与订阅器拆入上述三个包。Outbox 写入端实现的 `IOutboxBufferBinding` 位于 `Kun.EventBus.Outbox` 命名空间和 `Kun.EventBus.Abstractions` 程序集。
- 订阅端活动源和计量器由 `Qian.Events` 改为 `Kun.EventBus.RabbitMQ`。Outbox 投递的计量器改为 `Kun.EventBus.EntityFrameworkCore`。既有仪表名继续使用 `qian.events.*`。日志类别中的 `Qian.Events.QianEventSubscriber` 和 `Qian.Events.QianOutboxRelay` 分别变为 `Kun.EventBus.RabbitMQ.KunEventSubscriber` 和 `Kun.EventBus.EntityFrameworkCore.KunOutboxRelay`。发布连接的日志类别为 `Kun.EventBus.RabbitMQ.RabbitMqBroker`。
- `qian.events` 组件版本为 1.1.0。源码装配使用 `Kun.EventBus.EntityFrameworkCore.EventsPlugin`，私有源安装仍按清单锁定的软件包与摘要还原。

### Changed — 事件契约并进 Kun.EventBus.Abstractions（S1-KUN-025）

- 组件 `qian.events` 从 1.0.1 升至 1.0.2，组件说明中的契约包链接指向 `Kun.EventBus.Abstractions`。
- 包 `Qian.Events.Abstractions`（已发布 0.2.0）并进 `Kun.EventBus.Abstractions`，命名空间 `Qian.Events` → `Kun.EventBus`。带前缀的 4 个类型：`QianEventEnvelope` → `KunEventEnvelope`、`QianEventTopology` → `KunEventTopology`、`QianEventDelivery` → `KunEventDelivery`、`QianEventsException` → `KunEventsException`；`EventsDefinition`、`IEventsService`、`EventEnvelopeScope`、`EventOrigin`、`EventActor`、`IntegrationEventTypes`、`EventsErrors` 等名字不变。**迁移**：项目引用改指 `Kun.EventBus.Abstractions`，`using Qian.Events;` 换成 `using Kun.EventBus;`。原先调用 `AddQianEvents` 的宿主另引用 `Kun.EventBus` 并调用 `AddKunEvents()`。平台 0.x 事件沿用 v1 信封和 `qian.events` 交换机。原 Kun RabbitMQ 使用方按上文「统一事件投递与收发」迁移交换机、路由键与消息体。
- `Kun.EventBus.Abstractions` 新依赖 `Kun.Runtime`（事件服务键与插件上下文）：只声明事件的契约包随之传递引入 `Kun.Runtime`、`Microsoft.Extensions.DependencyInjection` 与 `Microsoft.Extensions.Logging.Abstractions`。
- `IIntegrationEventBus` 与 `Kun.EventBus.Outbox` 的 `IOutboxWriter`、`IIntegrationEventOutbox`、`IntegrationEventBuffer`、`OutboxMessage`、`OutboxStatus` 移进 `Kun.EventBus.Abstractions`，命名空间不变，`Kun.EventBus` 留类型转发：源码与按 `Kun.EventBus` 编译的程序集不用改；按程序集反射找这些类型的（如比对 `typeof(OutboxMessage).Assembly`）拿到的是 `Kun.EventBus.Abstractions`。
- 日志类别、活动源与计量器的名字都不变：挪动的类型不写日志，也不建活动源或计量器。

### Changed — gRPC 主体与错误尾部（S1-KUN-026）

- 纯 Kun gRPC 服务保留 `kun-error-code`，并在 `qian-error-bin` 提供与 HTTP 相同的 ErrorDetail JSON。租户拦截器同样提供两项尾部。
- `AddKunGrpc` 依次执行追踪、身份认证、租户范围、权限和结果映射。有效平台主体的显式空 `tenant_id` 对应平台范围；租户 RPC 继续校验传播元数据与认证结果是否一致。

### Changed — 身份依赖方端口与服务间令牌（S1-KUN-026）

- 依赖方、OIDC 与会话类型统一到 `Kun.Identity` 命名空间。引用 `Kun.Identity.Abstractions` 或 `Kun.Identity.Abstractions.Session` 的源码改用 `using Kun.Identity;`。浏览器宿主可继续通过 `IAuthenticatedUser` 获取租户、邮箱、显示名和当前声明主体。
- 原有 `IJwtIssuer`、`KunJwtIssuer`、`KunJwtIssuerOptions`、`AddS2SIssuer` 及 `Kun.Security.S2SJwt` 签发、验证、JWKS 入口已归档。网关使用 `S2sTokenIssuer` 签发，下游通过 `AddKunServiceAuthentication` 验签。`PropagateS2sTokenHandler` 与 `AddKunServiceAuth` 标为过时，使用这些入口会产生 KUNS2S001。
- 服务间令牌在签发与接收时共同核对每跳受众、身份、角色、scope 和有效期。外部访问令牌投影复用身份字段校验。字段形状见 `contracts/v0/s2s-claims.schema.json`。

### Removed — 归档通用仓储包（S1-KUN-024）

- 停发 `Kun.Infrastructure`。写入使用业务定义的仓储端口或 `KunDbContext`，提交事务使用 `Kun.EntityFrameworkCore.IUnitOfWork`。消息消费幂等使用 `Kun.EventBus.Idempotency.IProcessedMessageStore`。既有包版本可继续还原。

### Changed — 租户上下文使用 UUID（S1-KUN-024）

- `ITenantContext.TenantId` 和 `BeginScope` 使用 `Guid?`。租户范围要求非空 UUID。需要租户的请求缺少有效 `tenant_id` 即拒绝，原 `default` 租户回落已移除。平台范围用 `null`。消息与 HTTP 仍以 D 格式传输租户标识。实体租户列与 RLS 配置项继续使用小写 UUID 文本。
- 宽松和严格的 EF 上下文在插入多租户实体时都会检查显式租户标识。已赋值的标识须为非空 UUID 的小写 D 格式，格式不符时保存失败。未赋值的标识从当前租户范围填入。
- `TenantInfo`、租户开户与注销、数据库连接解析、上下文工厂、原生数据会话的租户端口使用 UUID。配置连接映射时以 UUID 为键。架构隔离的开户器与上下文工厂通过 `KunMultiTenancyOptions.GetSchemaName` 取得同一个架构名。已有按旧标识命名的架构需要在切换前迁移。

### Changed — 租户上下文与隔离策略（S1-KUN-024）

- 需要处理租户请求的宿主登记 `AddKunMultiTenancy()`，在认证后挂载 `UseKunMultiTenancy()`。租户从已认证主身份的唯一 `tenant_id` 读取，使用 D 格式非空 UUID。平台入口可显式选择 `PlatformPrincipalAllowed`。后台任务通过 `BeginScope` 进入租户范围。
- 租户传播接口和实现由 `Kun.MultiTenancy` 提供，gRPC 租户拦截器由 `Kun.Grpc` 提供。

- EF 配置与租户目录公用 `Kun.Core.MultiTenancy.TenantIsolation`。配置中的 `Shared`、`SchemaPerTenant`、`DatabasePerTenant` 依次改为 `Row`、`Schema`、`Database`，数值仍为 0、1、2。HTTP 租户契约继续使用 `row`、`schema`、`database`。

### Changed — 错误模型合一到 Kun.Errors（S1-KUN-023）

- 新增零依赖包 `Kun.Errors`（错误码 `ErrorCode`、错误码目录、`Error`、`ErrorException`、线上模型 `ErrorDetail`），`Kun.Core` 引用它。`Result` / `Result<T>` 的 `Error` 是 `Kun.Errors.Error`：错误码加文案参数、出错位置（JSON Pointer）、允许值与逐项字段错误。**迁移**：错误码先定义成 `ErrorCode`（大写下划线）并登记进错误码目录，通用的取 `PlatformErrors`；失败结果写 `Result.Fail(new Error(code, args))`。
- `Error` 是按引用比较的类，不再是按值比较的 record。**迁移**：比较两个错误改比 `Error.Code`；测试里断言同一个实例用 `BeSameAs` / `Assert.Same`。
- 成功的 `Result` 取 `Error` 抛 `InvalidOperationException`（之前返回 `Error.None`），`default(Result)` 取 `Error` 也抛；`ToString()` 不抛，失败时打印错误码。`Result.Fail(null)` 抛 `ArgumentNullException`。`EnsureSuccess()` 抛 `ErrorException`。
- Kun 内置的错误码改用已登记的平台码：`request.invalid`、`validation.failed` → `INPUT_INVALID`；`authorization.unauthenticated` → `UNAUTHENTICATED`；`authorization.denied` → `AUTHZ_DENIED`；`data.concurrency_conflict` → `CONFLICT`。**迁移**：按错误码字符串判断的调用方改用新码。权限行为拒绝时不再用文案区分原因：缺判定器、主体类型无效、判定不允许都给 `AUTHZ_DENIED`。
- `Kun.Validation`：校验器产出 `Kun.Errors.FieldError`（字段级错误码 + JSON Pointer），不再拼接文案；抛异常式的校验行为抛 `ErrorException`（`INPUT_INVALID`），结果式返回同一个 `Error`；`Errors` 至多 100 项，多出的丢弃。
- HTTP 错误响应由 `Kun.Errors.AspNetCore` 负责：`services.AddKunErrors()`、`app.UseKunErrors()`。响应体是 `ErrorDetail`（`application/problem+json`，`code` 为大写下划线的错误码，带 `trace_id`，文案取错误码目录），不再是 ProblemDetails；宿主登记的 `IProblemDetailsWriter` 不再接管 `ErrorException` 的响应体。
- `Kun.Grpc`：尾部 `kun-error-code` 的值是大写下划线的错误码；状态码按错误码的 HTTP 状态映射，比之前多出 408/504 → `DeadlineExceeded`、412/422 → `FailedPrecondition`、413/429 → `ResourceExhausted`、502 → `Unavailable`；状态说明是按当前界面语言渲染的错误码文案。
- `Kun.Testing`：`ShouldHaveErrorCode(ErrorCode)` 取代 `ShouldHaveErrorType`；`ShouldHaveErrorCode(string)` 比的是大写下划线的错误码。
- 包 `Qian.Runtime.Errors`、`Qian.Runtime.Errors.AspNetCore`（已发布 0.2.0）换成 `Kun.Errors`、`Kun.Errors.AspNetCore`，命名空间跟着换。类型与方法：`QianError` → `Error`；`QianFieldError` → `FieldError`，线上 DTO 的 `FieldError` 换成 `FieldErrorDetail`（JSON 形状不变）；`QianErrorException` → `ErrorException`；`QianResults`、`QianErrorResult` → `KunResults`、`KunErrorResult`；`AddQianErrors`、`UseQianErrors`、`QianErrorsExtensions` → `AddKunErrors`、`UseKunErrors`、`KunErrorsExtensions`。
- 组件 `qian.registry` 1.0.0 → 1.1.0，`qian.tenancy`、`qian.tenancy.signup` 2.0.0 → 2.1.0：三份发布的 HTTP 契约里，错误响应的 Schema 键随 `ErrorDetail` 所在的程序集变了，`$ref` 跟着变；线上形状与 operationId 不变。

### Removed — 第二套错误模型、异常处理器与两边之间的桥（S1-KUN-023）

- `Kun.Core`：`Kun.Core.Results.Error`（连同 `Error.None`、`Error.NotFound(code, message)` 等工厂）、`ErrorType`、`ResultFailureException`、`Kun.Core.Domain.DomainException`。**迁移**：状态码看 `ErrorCode.Status`，类别看 `ErrorCode.Kind`；领域规则不满足时返回或抛出带平台码的 `Error`（如 `BUSINESS_RULE_VIOLATED`）。
- `Kun.Validation`：`ValidationError`、`ValidationException`。**迁移**：读 `ErrorException.Error.Errors` 里的 `FieldError`。
- `Kun.AspNetCore`：`AddKunAspNetCore`、`KunExceptionHandler`、`ToProblemDetails`、`AddKunHealthChecks`，以及对 `Kun.Validation` 的引用；这个包只剩端点映射（`MapCommand` / `MapQuery`）。**迁移**：错误响应改用 `AddKunErrors()` / `UseKunErrors()`，健康检查直接调 `services.AddHealthChecks()`。
- 两座桥：`Qian.Runtime.KunErrorBridge`（`ToQianError`）与 `Kun.Errors.AspNetCore.KunValidationErrorBridge`。两边已是同一套模型，不再需要转换。

### Changed — 中介登记可叠加（S1-KUN-023）

- `AddKunMediator` 可以对同一个服务集合调多次：第一次建 `KunMediatorBuilder` 并登记 `KunMediator`、`ISender`、`IPublisher`，之后的调用往同一个 builder 里加，几个模块的登记叠进同一个中介。之前每次调用都新建 builder，最后一次生效，前面登记的处理器与行为都丢。**迁移**：靠「再调一次就整体替换」的宿主，删掉重复的登记或合成一次。描述符被拷进另一个服务集合时，那个集合另建自己的 builder。
- 同一请求登记第二个处理器时，登记当场抛 `InvalidOperationException`，消息写明请求与两个处理器；之前要到首次解析 `ISender` 才以 `ArgumentException` 失败。

### Added — 权限行为的正式扩展点（S1-KUN-023）

- `PermissionBehaviorOptions` 与 `AddPermissionBehavior<TRequest, TResponse>(denied, configure)` 重载：只改这一个请求取权限判定器（`PermissionChecker`）与当前主体（`CurrentUser`）的方式，比如在特定流程里包一层判定器；判定算法与拒绝映射不变。
- 权限行为因装配不全（没有权限判定器、没有权限声明）拒绝时记一条警告日志；拒绝结果照旧是 `AUTHZ_DENIED`。

### Changed — 组件运行时统一到 Kun.Runtime（S1-KUN-023）

- 包 `Qian.Runtime`（已发布 0.2.0）换成 `Kun.Runtime`，命名空间跟着换。类型：`IQianPlugin`、`IQianPlugin<TConfig>` → `IKunPlugin`、`IKunPlugin<TConfig>`；`QianEvent`、`QianEvent<TPayload, TResult>` → `KunEvent`、`KunEvent<TPayload, TResult>`；`QianEventAttribute` → `KunEventAttribute`。
- `Kun.Runtime` 只引用 `Kun.Errors`，不再传递带来 `Kun.Core`、`Kun.Mediator`、`Kun.Authorization`。**迁移**：用到这些包的项目直接引用它们。
- `ScopeKey` 只查语法：`global`，或小写的种类名（小写字母开头，小写字母、数字与连字符）加非空 id；之前只认 `tenant`、`app`、`agent-session` 三种。平台档（Profiles、Registry）的种类校验不变，种类常量在 `Kun.Profiles.Abstractions.ScopeKinds`。
- `SurfaceKeys` 由 `Kun.Surfaces.Abstractions` 提供，命名空间同包名。

### Changed — 插件开发包拆成四个包（S1-KUN-023）

- 已发布的包 `Qian.Plugin.Sdk` 拆成两个：`Kun.Plugin.Sdk`（内核档：代码声明、生成契约、命令策略协议）与新包 `Kun.Plugin.Sdk.Commands`（平台档：命令管道）。`Qian.Plugin.Sdk.Generators`、`Qian.Plugin.Sdk.AspNetCore` 换成 `Kun.Plugin.Sdk.Generators`、`Kun.Plugin.Sdk.AspNetCore`。命名空间跟着包名换；`Kun.Plugin.Sdk.Commands` 里的类型仍在命名空间 `Kun.Plugin.Sdk`。清单工具搬到仓库根的 `tools/Kun.Plugin.Tool`，不打包。
- 进了 `Kun.Plugin.Sdk.Commands` 的公共类型：`KunCommandRegistration`（`AddKunCapability`）、`CommandDataSession<TContext>`、`HookCapabilityRegistration`（`ExposeCapabilityHook`）。**迁移**：登记能力命令、用数据会话或钩子能力的项目另引用 `Kun.Plugin.Sdk.Commands`，代码不用改。
- 只引用 `Kun.Plugin.Sdk` 的项目如果使用数据、权限或表面契约，须按需直接引用 `Kun.Data.Abstractions`、`Kun.Authorization.Rbac.Abstractions` 或 `Kun.Surfaces.Abstractions`。`Kun.Plugin.Sdk.Commands` 已引用这三个包。
- 类型名里的 `Qian` 换成 `Kun`：`QianPluginAttribute`、`QianConfigSchemaAttribute`、`QianCodeMetadataAttribute`、`QianJobAttribute` → `KunPluginAttribute`、`KunConfigSchemaAttribute`、`KunCodeMetadataAttribute`、`KunJobAttribute`；静态抽象属性 `QianCapability`、`QianJobCode` → `KunCapability`、`KunJobCode`；`QianCommandRegistration`、`AddQianCapability` → `KunCommandRegistration`、`AddKunCapability`；`AddQianPluginEndpoints`、`UseQianPluginEndpoints`、`RequireQianPlugin` → `AddKunPluginEndpoints`、`UseKunPluginEndpoints`、`RequireKunPlugin`。源生成器产出的元数据工厂类名的后缀 `QianCodeMetadata` 换成 `KunCodeMetadata`（如 `GatewayComponentKunCodeMetadata`）。特性里写的组件 id（`qian.events` 等）不变。
- 源生成器的诊断号 `QIANSDK001`–`QIANSDK003` 换成 `KUNSDK001`–`KUNSDK003`，类别 `QianSdk` 换成 `KunSdk`。**迁移**：`.editorconfig`、`NoWarn` 与 `#pragma` 里按诊断号压的警告改写新号。
- 组件 `qian.config`、`qian.data`、`qian.events`、`qian.files`、`qian.scheduler` 1.0.0 → 1.0.1：组件文件夹里的说明随工具与特性的名字改了，只动文档。

### Removed — 归档 Kun.Dapper（S1-KUN-024）

- 包 `Kun.Dapper` 停发、移出解决方案，nuget.org 上已发布的版本不动。随包删掉的公共 API：`IReadDbConnectionFactory`、`ReadDbOptions`、`PageRequest`、`PagedResult<T>`、`Kun.Dapper.Npgsql.NpgsqlReadDbConnectionFactory`、`DapperAotExtensions.QueryPageAsync<T>`、`AddKunDapper` / `AddKunDapperNpgsql`。**迁移**：读侧直接引用 `Npgsql` 与 `Dapper`；登记一个读侧数据源（`services.AddSingleton(_ => NpgsqlDataSource.Create(readConnectionString))`），查询处理器注入 `NpgsqlDataSource`，用 `OpenConnectionAsync` 取连接后以 Dapper 查询；分页的请求与结果类型在应用里自己定义；要原生 AOT 发布的服务，在自己的项目里加 Dapper 的源生成器（`Dapper.AOT`）或直接用 `NpgsqlDataReader` 读行。写法见 `docs/guides/read-side-patterns.md`，样例见 `samples/Kun.Sample.Catalog`。

### Removed — 归档 Kun.Saga（S1-KUN-025）

- 包 `Kun.Saga` 停发、移出解决方案，已经发布的版本不动，没有替代的包。随包删掉的公共 API：`ISaga<TContext>`、`ISagaStep<TContext>`、`ISagaContext`、`ISagaOrchestrator` 与 `SagaOrchestrator`，后台服务 `SagaStepExecutor`、`SagaTimeoutDetector` 与它们的选项，`SagaStepFailedException`、`SagaNotFoundException`，实例存储 `ISagaInstanceStore`、`EfSagaInstanceStore` 与两份 EF 映射，`ApplyKunSagaConfiguration`、`AddKunSaga` / `KunSagaBuilder`。**迁移**：跨服务的流程用集成事件经 Outbox 串起来，失败后的补偿由各服务按业务自己处理；已经建好的 `saga_instances`、`saga_step_queue` 两张表由服务自己的迁移删掉（写法见 Order 样例的 `DropSagaTables`）。
- `Kun.Observability`：`AddKunObservability` 缺省监听的活动源不再含 `Kun.Saga`。
- 样例 `samples/Kun.Sample.Order` 不再演示编排：下单改成「经 gRPC 预留库存 → 经 Outbox 发 `order.completed.v1`」的命令流程，预留被拒返回 422（`BUSINESS_RULE_VIOLATED`）；演示端点换成 `POST /demo/orders`、`GET /demo/orders/{id}`；加一次迁移 `DropSagaTables` 删掉两张编排表；订单去掉只给编排用的 `Paid`、`Shipped` 两个状态。
- 文档 `docs/design/why-saga.md`、`docs/guides/orchestrating-sagas.md` 随包删掉。

### Changed — Kun.Gateway 改成纯库，独立宿主拆到 hosts/（S1-KUN-026）

- `Kun.Gateway` 由可执行项目改成类库：不再有入口，程序集里不再有顶层语句生成的公开类型 `Program`；包里不再带 `appsettings.json`（`content/` 与 `contentFiles/`）与 `Kun.Gateway.runtimeconfig.json`。包名、程序集名与 `Kun.Gateway` 命名空间保留。**迁移**：把网关当程序跑的，改跑仓库里的独立宿主 `hosts/Kun.Gateway.Standalone`（`dotnet run --project hosts/Kun.Gateway.Standalone`，原生发布 `dotnet publish hosts/Kun.Gateway.Standalone -c Release`），原 `KunGateway` 节迁至 `Gateway`，签发方与受众的三个键名按上文「网关装配与限流」调整。`ReverseProxy`、`KunObservability` 配置节继续沿用，签名密钥目录 `certs/` 跟着挪到宿主目录下；自己写宿主的照它的 `Program.cs` 组合库。
- `Kun.Gateway` 包不再依赖 `Kun.Observability` 与 `Grpc.AspNetCore.Web`（只有独立宿主用到），也就不再间接带上预发布的 `OpenTelemetry.Instrumentation.EntityFrameworkCore`。**迁移**：自己的宿主调了 `AddKunObservability` 或 `UseGrpcWeb`、又没有直接引用这两个包的，加上引用。
- 独立宿主不打包，不发布到 nuget.org。
- 样例 `samples/Kun.Sample.AppHost` 编排的网关换成独立宿主，资源名 `gateway` 不变。

### Changed — 数据与租户的包统一到 Kun.Data、Kun.Tenancy（S1-KUN-024）

- 包 `Qian.Data`、`Qian.Data.Abstractions`、`Qian.Data.AspNetCore`、`Qian.Data.Dialect.Pg`、`Qian.Data.Native.Abstractions`、`Qian.Data.Native.Postgres`、`Qian.Tenancy`、`Qian.Tenancy.Abstractions`、`Qian.Tenancy.AspNetCore`、`Qian.Tenancy.Signup`、`Qian.Tenancy.Signup.Abstractions` 换成对应的 `Kun.Data.*`、`Kun.Tenancy.*`，命名空间跟着换；包源上已发布的旧包不动。
- 类型与方法：严格基类 `QianDbContext` → `KunStrictDbContext`；`QianSoftDeleteInterceptor`、`QianApplicationRoleInterceptor`、`QianDataModelExtensions`（`UseQianDataModel`）、`QianDataMisconfiguredException` → `KunSoftDeleteInterceptor`、`KunApplicationRoleInterceptor`、`KunDataModelExtensions`（`UseKunDataModel`）、`KunDataMisconfiguredException`。这几个类型与数据会话契约 `DataDefinition`、`IDataService`、`IDataSession<T>`、`IDataContextCatalog` 都在内核的 `Kun.EntityFrameworkCore` 包里，命名空间 `Kun.EntityFrameworkCore`；`Kun.Data.Abstractions` 引用它，引用数据契约包的组件照旧拿得到。**迁移**：用到它们的文件写 `using Kun.EntityFrameworkCore;`。登记与端点的扩展方法、它们所在的静态类与选项类，名字里的 `Qian` 一律换成 `Kun`：`AddKunData`、`AddKunDataMigrations`、`AddKunSequences`、`AddKunSnowflakeIds`、`AddKunDataConnections`、`AddKunDataTypes`、`AddKunPgDialect`、`AddKunNativeData`、`MapKunData`、`MapKunDataConnections`、`MapKunDataTypes`、`MapKunTenancy`、`AddKunTenancyEvents`、`UseKunTenancySignupPages`、`MapKunTenancySignupApplicant`、`MapKunTenancySignupOperations`，`KunDataMigrationOptions`、`KunSequenceOptions`、`KunSnowflakeOptions`、`KunDataConnectionOptions` 等。
- `QianDataConventions.DeletedByPropertyName` 并进 `Kun.EntityFrameworkCore.Conventions.KunDataConventions`（值仍是 `"DeletedBy"`），数据契约包里不再有约定类。**迁移**：`using Kun.EntityFrameworkCore.Conventions;`，写 `KunDataConventions.DeletedByPropertyName`。
- EF 迁移的命名空间与迁移快照里的实体全名跟着换，迁移生成的 SQL 与库表结构不变。分析器按 `Kun.EntityFrameworkCore.KunStrictDbContext` 找严格基类，原 `QIAN0003` 需改为 `KUNDATA001`。
- 运行期字面不变：组件 id（`qian.data` 等）、库 schema `plugin_qian_*`、模型注解键 `"Qian:MultiTenant"`、行级安全策略名 `qian_tenant_access` / `qian_tenant_guard`、索引名、配置节名与租户 GUC `app.current_tenant_id`。
- 日志类别跟着包名换：代码里显式写的类别名 `Qian.Tenancy.Operations`、`Qian.Tenancy.AuditCompletion`、`Qian.Tenancy.Summary`、`Qian.Tenancy.Signup.Operations` 换成 `Kun.Tenancy.Operations`、`Kun.Tenancy.AuditCompletion`、`Kun.Tenancy.Summary`、`Kun.Tenancy.Signup.Operations`，按类型取的类别随命名空间换。按换名前的类别名配的日志级别（如 `Logging:LogLevel` 下的键）或告警规则要跟着改。这一域没有计量器（Meter）与活动源（ActivitySource）。
- 组件 `qian.data` 1.0.1 → 1.1.0，`qian.tenancy`、`qian.tenancy.signup` 2.1.0 → 2.2.0：清单入口换成新程序集；两份发布的 HTTP 契约按类型全名与程序集简单名重建 Schema 键，同步更新 `$ref`，线上形状与 operationId 不变。`qian.data` 清单的 `runtime.nuget[]` 仍钉包源上已发布的旧包，正式包发布并核对下载摘要后更新清单。在完成改钉前用命令行从包源安装 `qian.data`，装上的旧包里没有清单写的入口，宿主挂载失败（从源码引用这些项目的宿主不受影响）。

### Changed — EF 的三处实现并进 Kun.EntityFrameworkCore（S1-KUN-024）

- 包 `Kun.MultiTenancy.EntityFrameworkCore` 停发、移出解决方案，nuget.org 上已发布的版本不动。它的类型进 `Kun.EntityFrameworkCore`，命名空间 `Kun.EntityFrameworkCore.MultiTenancy`（`ApplyKunMultiTenantFilters`、`IKunMultiTenantDbContext`、`KunMultiTenantModelCacheKeyFactory`、`KunTenantSaveChangesInterceptor`）；登记扩展 `AddKunMultiTenancyEntityFrameworkCore` 进 `Kun.EntityFrameworkCore.DependencyInjection`。**迁移**：去掉这个包的引用，引用 `Kun.EntityFrameworkCore`，`using` 换成上面两个命名空间。
- 删除 `ApplyKunMultiTenantFilters` 只收 `ITenantContext` 的那个重载（KUNTENANT001）。**迁移**：改用 DbContext 重载 `modelBuilder.ApplyKunMultiTenantFilters(this)`，上下文实现 `IKunMultiTenantDbContext`（`KunDbContext` 已实现）。
- 删除 `TenantAwareModelCacheKeyFactory`：SchemaPerTenant 也用 `KunMultiTenantModelCacheKeyFactory`，它对没有实现 `IKunMultiTenantDbContext` 的上下文按类型、空租户与设计时标记分键。**迁移**：自己替换了模型缓存键工厂的，改指 `KunMultiTenantModelCacheKeyFactory`。
- 删除 `EfRepository<T>` 与 `SpecificationEvaluator`，`Kun.EntityFrameworkCore` 不再依赖 `Kun.Infrastructure`。**迁移**：写侧直接用业务上下文与 `IUnitOfWork`（白皮书 §5.1 的示例）。
- 新包 `Kun.EntityFrameworkCore.PostgreSQL`（内核档、非 AOT）：建租户表时紧跟 `CREATE TABLE` 打开行级安全并建租户策略的迁移 SQL 生成器搬到这里，`UseKunTenantRlsMigrations()` 装上它，`UsesKunTenantRlsMigrations()` 判断上下文实际用的是不是它，生成的 SQL 不变。`AddKunData` 已经装上，平台组件不用改。
- 严格上下文的 schema 前缀改由宿主设定：严格基类自己只要求合法的小写标识符（小写字母开头，只含小写字母、数字和下划线，最长 63 个字符）；宿主用 `RequireKunSchemaPrefix(prefix)` 加前缀，设定后不能改成别的。`AddKunData` 设 `plugin_`，平台接受与拒绝的 schema 都不变；报错文字去掉了「插件」二字。
- 装配错误码 `DATA_MISCONFIGURED` 改由 `KunDataMisconfiguredException.Code` 公开（字面、HTTP 500、归属 `qian.data` 与文案不变），`DataErrors` 里不再有 `Misconfigured`。**迁移**：`DataErrors.Misconfigured` 换成 `KunDataMisconfiguredException.Code`。
- `Kun.EntityFrameworkCore` 属于内核契约档。自定义 Outbox 写入端需要的 `IOutboxBufferBinding` 在 `Kun.EventBus.Abstractions` 包的 `Kun.EventBus.Outbox` 命名空间。
- 按类型取的日志类别随命名空间换：租户字段拦截器的类别 `Kun.MultiTenancy.EntityFrameworkCore.KunTenantSaveChangesInterceptor` 变成 `Kun.EntityFrameworkCore.MultiTenancy.KunTenantSaveChangesInterceptor`，按它配的日志级别或告警规则要跟着改。别的搬动的类型不写日志。
- 迁移生成的 SQL、库表结构与查询过滤器都不变。

### Removed — 归档 Kun.Sandbox（S1-KUN-028）

- 包 `Kun.Sandbox` 停发、移出解决方案与 AOT 清单，nuget.org 上已发布的版本不动。随包删掉的公共 API：`Kun.Sandbox.Abstractions` 下的沙箱抽象（`ISandboxProvider`、`ISandboxHandle`、`IProcesses`、`IFiles`，以及创建沙箱、执行进程与读写文件用到的选项和结果类型），`Kun.Sandbox.Implementations` 下的三个后端（`InMemorySandboxProvider`、`SupervisorSandboxProvider`、`E2BCloudSandboxProvider`），登记入口 `AddKunSandbox` 与 `KunSandboxOptions`。**迁移**：没有替代的包；仓库里也没有使用方，Catalog 样例原来只演示登记，这一行已去掉。仍要用的服务钉住 nuget.org 上已发布的版本，或直接调用沙箱服务自己的接口。

### Removed — 归档旧线 Kun.Storage（S1-KUN-028）

- 旧线的包 `Kun.Storage` 停发、移出解决方案与 AOT 清单，nuget.org 上已发布的版本不动。随包删掉的公共 API：命名空间 `Kun.Storage.Abstractions` 下的 `IFileStorage`、`ObjectMetadata`、`ObjectDownload`、`ObjectSummary`、`PutObjectOptions`、`StorageOptions`、`StorageProvider`、`ContentDispositionPolicy` 与四个预签名请求和地址类型，`Kun.Storage.Implementations` 下的 `LocalFileStorage`、`S3FileStorage`，登记入口 `AddKunStorage`、`AddLocalStorage`、`AddS3Storage`。仓库根 `BannedSymbols.txt` 里禁用旧线接口的那一条随之删掉。
- **迁移**：引用新线的 `Kun.Storage.Abstractions`（接口是命名空间 `Kun.Storage` 下的 `IFileStorage`），再按后端引用 `Kun.Storage.Local`、`Kun.Storage.S3` 或 `Kun.Storage.Oss`，登记用 `AddKunLocalFileStorage`、`AddKunS3FileStorage` 或 `AddKunOssFileStorage`。方法对照：`PutObjectAsync` → `UploadAsync`（内容类型直接传，大小与类型限制放进 `UploadConstraints`），`GetObjectAsync` → `OpenReadAsync`（直接返回流），`HeadObjectAsync` → `GetMetadataAsync`，`DeleteObjectAsync` → `DeleteAsync`（返回是否删到），`ListObjectsAsync` → `ListAsync`（按续传令牌分页），`CreatePresignedUploadUrlAsync` / `CreatePresignedDownloadUrlAsync` → `GetPresignedUploadAsync` / `GetPresignedDownloadAsync`。两线都只对活动网页类型强制 `Content-Disposition: attachment`（旧线是 `ContentDispositionPolicy.EnforceAttachment`）：旧线上传时就把它写在对象上，签发下载地址时也带上；新线不改对象，只在签发下载地址时强制（`PresignedDownloadUrl.ContentDispositionForced`），不经预签名地址、直接对外提供对象的服务要自己设这个响应头。

### Added — 文档生成可指定源码根（S1-KUN-029）

- `Kun.Docs.Abstractions` 的 `DocumentOptions` 加 `SourceRoot`：源码目录相对它解析、不能越出它，事件映射与事件分析诊断里的位置相对它书写（装配诊断与配置目录仍相对工作区）。相对路径按工作区解析，只能是工作区本身或它的上级目录，到工作区的路径上不能有符号链接；不设时等于工作区，输出与之前相同。
- `qian docs generate` 加对应的 `--source-root <目录>`。本仓库的派生文档改以仓库根为源码根，仓库根 `src/` 下的包也计入事件图。

### Changed — Principal 一组进新的内核契约包 Kun.Security.Abstractions（S1-KUN-026）

- 新增零依赖包 `Kun.Security.Abstractions`（内核档，AOT 兼容），命名空间 `Kun.Security`。服务间令牌的主体模型 `Principal`、`DelegationActor`、`PrincipalKind` 与声明名 `IdentityClaimNames` 从 `Qian.Identity.Abstractions` 挪进来，成员不变；JSON 属性名、先后与令牌里的声明名都不变。**迁移**：`using Kun.Security;`；写全名的把 `Qian.Identity.Abstractions.Principal` 等改成 `Kun.Security.Principal` 等。引用身份契约的项目经它传递拿到新包，不用另加引用。
- `PrincipalKind` 的 JSON 转换器 `PrincipalKindConverter` 改为公开类型：别的程序集里的 JSON 源生成上下文要引用它。读写不变：只收 `user`、`service`、`agent`，数字、大小写不对与其它值都报错。
- `ClientType` 由 `Kun.Identity.Abstractions` 提供，命名空间同包名。
- `Kun.Plugin.Sdk` 不再引用 `Qian.Identity.Abstractions`，改引用 `Kun.Security.Abstractions`（`CommandInvocation.Principal` 的类型是 `Kun.Security.Principal`），也就不再传递带来身份契约与 `Kun.EventBus.Abstractions`。**迁移**：用到身份契约里其余类型（如 `ICurrentUser`、`IIdentityService`）的项目直接引用 `Kun.Identity.Abstractions`。`Kun.Plugin.Sdk.Commands` 改为直接引用身份契约，经它拿到身份契约的项目不受影响。
- 组件 `qian.identity.server` 1.0.0 → 1.1.0：派生的事件元数据里 `identity/signed-in` 的载荷类型全名换成 `Kun.Security.Principal`，对外行为不变。

### Fixed — 样例 AppHost 的 Aspire 安全补丁（S1-KUN-019）

- 样例编排宿主 `samples/Kun.Sample.AppHost` 的 Aspire 从 9.0.0 升到 13.6.5（`Aspire.AppHost.Sdk` 与中央版本里的 `Aspire.Hosting.AppHost` / `PostgreSQL` / `RabbitMQ` / `Redis`），修复 NuGet 审计报出的 [GHSA-4g86-54fm-h9h8](https://github.com/advisories/GHSA-4g86-54fm-h9h8)（`Aspire.Dashboard.Sdk.*`，严重）与 [GHSA-f7vh-h5g3-7jrh](https://github.com/advisories/GHSA-f7vh-h5g3-7jrh)（`Aspire.Hosting`、`Aspire.Hosting.AppHost`，高）。这两条公告发布后，`dotnet restore` 在警告即错误下失败，CI 的构建、测试与格式检查随之失败。编排代码不用改，资源、依赖与启动顺序不变。
- 只对该项目关闭中央传递钉版（`CentralPackageTransitivePinningEnabled=false`）：它的依赖闭包只有 Aspire 自己的包，仓库给库包定的传递钉版会把 Aspire 13 要求的依赖版本往下压（NU1109）。中央钉版本身不变，其余项目解析出的依赖不变。
- 升级后 `KubernetesClient` 解析到不再触发 NU1902 的版本，移除该项目原有的 NU1902 忽略。保持 NuGet 安全审计和警告即错误，不新增忽略，不升级 SDK 或其他依赖族。

### Fixed — 集成测试的对象存储改用有公开来源的 S3 兼容服务（S1-KUN-019）

- `Kun.IntegrationTests` 的存储测试原来用 `minio/minio` 镜像，它已从 Docker Hub 删除，Quay 上的同名镜像要登录才能拉，没有本地缓存的执行环境拉不到：5 个测试失败，另外 6 个把拉取失败当成没有 Docker 而跳过。改用 RustFS 1.0.6（Apache-2.0），镜像 `ghcr.io/rustfs/rustfs:1.0.6` 钉到摘要，匿名可拉；三个存储测试类共用 `Fixtures/S3CompatibleServer.cs`，`MinioFixture` 改名为 `S3StorageFixture`，断言不变。测试包由 `Testcontainers.Minio` 换成同版本的通用 `Testcontainers`，中央版本删去 `Testcontainers.Minio`。
- 拉不到镜像、容器起不来不再算作跳过。只有本机确实没有容器环境时才跳过，环境变量 `CI` 为真时这种情况也算失败（现位于 `src/Kun.Testing.Containers/DockerRequirement.cs`）。

### Fixed — RabbitMQ 连接已被对端关掉时，释放不再抛异常（S1-QA-023）

- `Kun.EventBus.RabbitMQ` 的 `RabbitMqConnectionProvider.DisposeAsync` 在连接已被 RabbitMQ（或网络）关掉时，不再把 `AlreadyClosedException` 抛给宿主，照常释放连接。
  - 原因：客户端只在关闭前查一次连接是否打开；对端的关闭落在查与关之间时，关闭就抛这个异常。宿主停止时偶发失败。
- 关闭时的其他异常照常抛出，但连接与内部锁会先释放。

### Fixed — RabbitMQ 订阅端停止时，不再在正在关的通道上确认消息（S1-QA-023）

- `Kun.EventBus.RabbitMQ` 的订阅端停止时，通道的关闭一旦发出，就不再在这条通道上确认（ack）或拒绝（nack）。上一条修的是释放这一端，这一条修原因。
  - 原因：停止时订阅端先关通道；客户端关通道要等正在执行的处理器返回，处理器返回后，订阅端随即在同一条通道上发确认。这个确认落在关通道之后，RabbitMQ 在已经清掉的通道号上收到帧，按连接级错误（504 `CHANNEL_ERROR - expected 'channel.open'`）关掉整条连接，同一连接上的发布通道一起断；随后释放连接时又撞上对端的这次关闭。
  - 停止时还在处理的消息不再确认：通道关闭时 RabbitMQ 把它重新入队；处理器已经成功的，重新投递时按幂等记录跳过，不重复处理。
  - 每条投递在它到达的那条通道上确认，不再经过一个会被下一次消费会话改写的字段。
- 关通道失败（包括连接已被对端关掉）时照常释放通道，也不再盖掉会话本来的异常：连接已关记 Debug，其余失败记 Warning。
- `RabbitMqConnectionProvider`：开始释放之后，`GetConnectionAsync` 抛 `ObjectDisposedException`，不再新建一条没人关的连接；释放时有连接正在建，等它建完一并关掉；排在内部锁上的调用方不再一直挂着。关闭时只容忍 `AlreadyClosedException`，父类 `OperationInterruptedException` 的其他情形照常抛出。

### Fixed — 调度器 worker 正常停止时不再记错误，已跑完的作业照常提交（S1-QA-023）

- `Kun.Scheduler` 的 worker 正常停止时，不再记「调度领取失败」「续租失败」「收尾失败」这几条 Error，错误输出里也不再有 `ObjectDisposedException`。
  - 原因：停止时 Kun.Runtime 先关闭插件上下文、再执行卸载效果，worker 在效果里才停；这段时间里领取、续租与收尾都经插件上下文取数据服务，抛 `ObjectDisposedException`。
- 这段时间里的行为：
  - 在途领取提交前再次检查上下文。如果调度器在等待数据库时已关闭，整批触发器的租约和代次均回滚，其他 worker 可以立即领取。
  - 刚领到、还没开始准备的执行直接放下，由租约到期后的 worker 接管；
  - 处理器已经正常返回的，照常提交完成；之前完成事务失败，同一 run 在接管后会再执行一次；
  - 解析或处理器因上下文已关闭而抛出的，不写失败，留给接管。
- 在途执行的续租与收尾改用 worker 创建时取得的数据服务，领取与开始执行仍经插件上下文。上下文开着而数据服务缺失（组件回到等待依赖）时照旧按错误处理。生产缺省值不变。

### Changed — 包依赖版本

- 使用其他包内部成员的程序集精确依赖该包的当前版本。消费者升级时，将这对包一起升级。

### Changed — 官方组件许可（S1-KUN-022）

- 39 个官方组件的清单改用 MIT，并各自递增补丁版本。CRM 样例和命令行生成的技能清单也采用 MIT。13 个系统组件的目录摘要随之改变，部署前需按新镜像重新签名。包含清单的 NuGet 包将在下次发布时带上新版声明。

## [2026.10.25] - 2026-10-25

### Fixed — 数据约定与实体审计不再占用当前主体 / 租户端口（S1-KUN-017）

- `AddKunDataConventions<TContext>()` 不再以 `TryAdd` 把 `AnonymousCurrentUser` 登记进 `ICurrentUser`；`AuditFieldsSaveChangesInterceptor` 改为工厂登记，在上下文创建时按需解析 `ICurrentUser`，宿主没有身份适配器时仍按匿名写 null。此前宿主先登记数据约定、后调用 `AddKunSecurity()` 时，后者的 `TryAdd` 落空，`ICurrentUser` 恒为匿名：审计字段写 null，比对框架主体的下游（如命令管道）对一切请求判拒。
- `AddKunAuditing<TContext>()` 同样不再登记 `NullTenantContext` / `AnonymousCurrentUser` 兜底，`TransactionalAuditingSaveChangesInterceptor` 按需解析两个端口。此前先登记实体审计、后调用 `AddKunSecurity()` 时，租户端口落成无租户，审计与 RLS 拿不到请求租户。
- 公开 API 不变。新增用例覆盖身份适配器在数据约定之前 / 之后登记、以及真实 `AddKunSecurity()` 晚于数据组件登记时的同事务审计（审计字段与 `AuditLog` 记令牌主体与租户）；登记次序约定写进 `docs/architecture/when-to-use-currentuser.md`。
- **行为变更**：框架不再顺带登记这两个端口。没有身份适配器、又在自己的服务里构造注入 `ICurrentUser` 的宿主（后台作业、迁移任务等），升级后首次解析即报无法解析，须显式登记 `AnonymousCurrentUser.Instance`；只用 `AddKunAuditing<TContext>()`、没有任何租户登记的单租户宿主，构造 `KunDbContext` 时报无法解析 `ITenantContext`，须显式登记 `NullTenantContext.Instance`。

## [2026.10.23] - 2026-10-23

### Fixed — 普通网关包的 JWKS NativeAOT 消费（S1-KUN-016）

- 公开与内部 JWKS 映射改用显式 RequestDelegate，消除普通 NuGet 制品被独立 NativeAOT 宿主引用时的运行时端点绑定。保持公开签名、匿名元数据、additionalGate 的 404、内部端点的生产 opt-in、no-store、源生成 JSON 与共享密钥轮换行为。
- 普通网关包也生成既有宿主配置绑定，修复同一消费链路内网关／观测配置的反射绑定诊断；配置字段与 EF 观测的运行时守卫不变。
- 新增普通打包后再独立发布的 Linux 原生回归，实际请求两个端点并验证轮换重叠及旧钥退役。库打包和消费者均不依赖 RDG，不以 ProjectReference 重新编译库来代替制品验证。

### Fixed — 工具依赖的安全补丁

- Catalog／Order 的 EF Design／Roslyn 工具依赖通过 Microsoft.Build.Tasks.Core 17.14.28 引入 Pkcs 9.0.0。中央传递钉定 `System.Security.Cryptography.Pkcs` 9.0.21，修复 [GHSA-2fmv-rv2c-59x7](https://github.com/advisories/GHSA-2fmv-rv2c-59x7) 与 [GHSA-846f-f764-76mx](https://github.com/advisories/GHSA-846f-f764-76mx)；保持 NuGet 安全审计和警告即错误，不升级 SDK 或业务依赖族。

## [2026.10.14] - 2026-10-14

### Added — 事件契约独立包（S1-KUN-010）

- 新增 `Kun.EventBus.Abstractions`，提供稳定事件标记、事件类型查询及含 `JsonTypeInfo<T>` 的注册端口，只依赖 `Kun.Core`；事件契约包不再传递引入 Redis 或宿主实现。
- `IIntegrationEvent` 复用 Core 中既有类型并通过新包导出；原 `Kun.EventBus` 对迁移类型保留转发，命名空间、公开签名和持久化事件 ID 均兼容。宿主继续引用 `Kun.EventBus`。

## [2026.10.13] - 2026-10-13

### Fixed — 受保护请求的运行期兜底（S1-KUN-014）

- `KunMediatorBuilder` 构建 Mediator 时核对：实现 `IPermissionProtectedRequest` 的请求必须同时经 `AddPermissionBehavior<TRequest, TResponse>(denied)` 装配权限行为，否则首次解析 `ISender` / `IPublisher` 即抛出 `InvalidOperationException` 并列出漏装的请求类型，处理器不会执行。直接或经反射调用 `AddRequestHandler` 绕过下游编译期门禁的注册同样被拒绝。**行为变更**：此前漏装权限行为的受保护请求会被直接执行，升级后须补齐 `AddPermissionBehavior`。

### Changed — 同日修复版之后的无后缀正式版

- 首个包含 `2026.10.12-hotfix.2`（共享 EF 模型的租户上下文绑定）与 `2026.10.12-hotfix.3`（授权主体类型透传）修复的无后缀版本。`-hotfix.N` 在 NuGet 中按预发布排序，此前 nuget.org 的最新稳定版仍是不含这两处修复的 `2026.10.12`。

## [2026.10.12-hotfix.3] - 2026-10-12

### Fixed — 授权主体类型透传

- HTTP、四种 gRPC 与 Mediator PEP 保留已认证主体的 user/service/agent 类型，服务身份不再默认按同名 user 查权限。未知或歧义类型在进入处理器前拒绝；不改变 S2S 签发端对 Agent 的保留限制。
- ICurrentUser 增加来自 principal_kind 的只读 PrincipalKind，旧适配器通过默认接口实现保持 user；JWT 和浏览器适配器读取已认证主身份，拒绝冲突标识与重复关键 claim，不从次身份拼接权限主体。没有类型声明的旧浏览器身份继续按 user 处理，机器身份适配器必须提供真实类型。
- 宿主复用 Kun.Security 的离线 claim 读取器；不增加身份接口族，不从普通请求头推断身份。回归覆盖真实 RSA 签名 HTTP、gRPC 四形态及 Mediator 的同名主体隔离。
- 补记：该标签发布时本节仍留在 [Unreleased]，2026-10-13 随 2026.10.13 归入本版本。

## [2026.10.12-hotfix.2] - 2026-10-12

### Fixed — 同日修复版发布校验

- 原尝试标签 `v2026.10.12-2` 因纯数字后缀被 nuget.org 的包清单校验拒绝，保留该失败标签。改用字母加点分段的 `hotfix.N`，脚本与 CI 复用版本/日期校验；推送前核对实际包、符号包和 Kun 内部依赖的版本及发行物完整性。

### Fixed — 共享 EF 模型的租户上下文绑定

- `KunDbContext` 查询过滤器改从当前执行查询的 DbContext 读取租户，修复缓存模型继续引用旧宿主或旧请求访问器的问题。旧访问器退出或换租户时，新请求不再查询空集或错误租户。
- `ApplyKunMultiTenantFilters` 增加 DbContext 重载。**迁移要求**：原访问器重载标记 KUNTENANT001 并在模型构建时明确拒绝；非 KunDbContext 消费者须实现 IKunMultiTenantDbContext 并改传 `this`。仅捕获常量值无法兼容默认共享模型，因此不保留会固定到旧租户的降级路径。KunDbContext 消费者自动获得修复，RLS 双层装配保留。
- 真实 PostgreSQL 回归验证同一模型在独立租户访问器之间复用的两种失效场景；NuGet 消费者应精确钉定 `2026.10.12-hotfix.2`，避免版本排序选回同日基础版。

## [2026.10.12] - 2026-10-12

### Fixed — Apollo 原始 namespace 校验

- 增加可选 `ValidateNamespace`，在合并、缓存写入和离线缓存读取前逐份检查原始配置。宿主的敏感数据规则因此可以拦截被后续 namespace 覆盖的明文；任何一份被拒绝都不替换最后有效快照。

## [2026.10.11] - 2026-10-11

### Fixed — Mediator 权限行为启动校验

- `AddPermissionBehavior` 移除通用管道登记附带的构造器注册，只保留能处理可选权限／身份／租户端口及拒绝回调的工厂。开启 `ValidateOnBuild` 时不再因重复登记报缺少服务；重复登记仍能通过依赖校验，缺少身份或引擎继续在 handler 前拒绝。

### Fixed — Identity 客户端与唯一 S2S 签发入口（S1-KUN-004）

- 授权码交换必须发送原始 PKCE verifier 并验证签名/issuer/audience/nonce；补 refresh、client credentials 与有期限的 discovery 缓存。浏览器事务受保护并防重放，完成辅助服务与认证方案可直接解析，保留宿主显式方案。
- 内存与 Redis 会话增加原子续期、按用户吊销及带重放保护的后通道登出；吊销后续期不得复活会话。默认验证 cookie 写请求的防跨站令牌；sid 日志只记 SHA256 与错误类型，并加入静态门禁。
- 网关统一封闭 S2S 形状：aud/roles 数组、规范 UUID、用户/服务主体、会话/修订/成员上下文、1–300 秒 TTL 与每跳受众。拒绝缺失身份，不再转发任意 permission/email/trace 字段；两套旧签发入口标 KUNS2S001。
- Sandbox 默认模板改为 kun-default，增加调用方命名的 JSON 扩展并防止覆盖核心字段；保留显式旧扩展的兼容序列化，标记废弃。
- 此版本有迁移要求：规范身份字段、旧 cookie 重新登录、Redis 新键空间和原子接口、Data Protection 共享密钥。详见 [Identity 客户端指南](docs/guides/identity-client.md)。框架协议夹具不能替代尚未交付的真实 Qian 身份服务联调。

### Fixed — 网关与流式 gRPC 边界（S1-KUN-003）

- 网关业务路由默认认证并绑定限流，显式匿名白名单保留；超限返回 429 和可用的 Retry-After，匿名路由不透传未验证凭据。
- 转发头只接受配置的代理 IP/CIDR，空名单关闭处理；IP 分区只读归一化地址，阻止伪造 X-Forwarded-For 切桶。
- 签发与 JWKS 共用 RSA 热轮换状态，无重启发布新 kid，重叠窗口保留旧公钥、到期移除；坏替换保留最后有效密钥。跨副本轮换与消费者缓存边界见安全文档。
- gRPC 认证、Result 映射与追踪覆盖 unary、客户端流、服务端流和双向流，缺少租户在 handler/应用首帧前拒绝；独立网关启用观测并保持 AOT 门禁。EF 观测路径由运行时特性守卫从原生二进制裁掉；AOT 宿主须显式关闭该选项。
- 新增真实 Kestrel/HTTP2 网关和 gRPC 回归，覆盖匿名 401、默认/自定义限流、代理信任、密钥轮换及四种 RPC 的认证/映射/trace。

### Fixed — Saga 作用域与步骤可靠性（S1-KUN-007）

- Saga 改为实际作用域工厂，移除注册期临时容器，持久化稳定 `ISaga.Name` 并兼容旧类名实例。
- PostgreSQL 实例行锁覆盖步骤与状态/队列提交，超时检测共用行锁并复核状态，阻止双副本重复执行与陈旧条目倒退状态。
- 成功、部分失败及补偿均回写上下文；失败本地写入回滚至保存点。步骤遵守单步取消期限，宿主关闭回滚待恢复，不触发业务补偿。
- 增加可配置、有持久化退避的步骤重试；默认一次保持兼容，附 Order 示例的 NextAttemptAt 迁移。远程副作用仍需幂等键。

## [2026.10.9] - 2026-10-10

### Added / Fixed — 平台基础端口（S1-KUN-006）

- 配置重载失败保留最后有效值并告警，支持关键／可选配置与源生成 JSON Schema；新增 Apollo HTTP provider（长轮询、AccessKey、通知修订、快照校验与缓存）。
- 新增 `Kun.Authorization` 权限／数据／字段端口及 HTTP、Mediator、gRPC 四种调用形态集成，缺失引擎时拒绝；不内置 RBAC/ABAC 实现。
- handler 注册支持 `JsonTypeInfo` 与稳定能力 ID，通过 `ICapabilityRegistry` 枚举请求和响应 Schema。
- `KunExceptionHandler` 改经 `IProblemDetailsService`，允许宿主接管错误体；Dapper 新增不可变 options 工厂重载；慢调用阈值可配置。
- 旧基础设施接口与旧存储接口标记 `KUNLEGACY001`，保留兼容实现。下游门禁应区分旧接口 `Kun.Storage.Abstractions.IFileStorage` 与新包中的 `Kun.Storage.IFileStorage`。

### Fixed — 审计原子性与数据约定（S1-KUN-005）

- `AddKunAuditing<TContext>()` 将实体 `AuditLog` 与 `kun.audit.event.v2` Outbox 消息直接登记到业务上下文；保存失败会撤销暂存记录，避免重试生成重复审计。租户、用户来自当前上下文，v2 支持字符串标识；旧 v1 类型保留读取兼容。
- 事后写入的 `AuditSaveChangesInterceptor`、旧 `UseEventBus()` 与 `kun.audit.record.v1` 路径标记废弃，迁移指引见 [审计文档](docs/security/audit-log.md)。
- `PostgresAuditChain` 在事务里按租户行锁串行化读取链头、写记录并推进链头，保存原始时间 ticks，支持多副本与哈希重放。`AuditEntry.ActorType` 支持 `agent` 并纳入新哈希，历史 unknown 条目维持旧算法。
- `EfAuditReader<TContext>` 在数据库端按当前租户、主体、资源、时间窗口过滤并限量。
- 显式数据约定提供 snake_case、Guid v7 默认主键、可空审计主体及 PostgreSQL `xmin`；`SaveChangesResultAsync()` 将并发冲突映射为 `Result` 的 Conflict。已有非空主键保持不变，旧非空数值主体需要显式迁移。
### Fixed — Outbox 与消费可靠性（S1-KUN-002）

- Catalog 真库测试确认旧领域通知路径会丢失集成事件；领域通知改为提交前同 scope 派发，失败时禁止提交半个通知链。
- 默认 EF Outbox 存储使用 PostgreSQL `FOR UPDATE SKIP LOCKED` 领取与提交批次；新增持久化 `NextAttemptAt` 指数退避，附两份示例迁移。自定义非事务存储不再默认无锁。
- RabbitMQ 开启发布确认与 mandatory 路由，消费者成功后才记录幂等并 ack，失败经持久化 quorum 队列退避后进 DLX；保留共享连接自动恢复。业务 handler 仍需幂等。
- 恢复原先跳过的幂等测试，补齐双副本发布、handler 失败重试与 broker 重启验收；见 [消息指南](docs/guides/outbox-and-integration-events.md) 的迁移边界。

## [2026.10.8] - 2026-10-08

### Fixed — 多租户后台链路（S1-KUN-001）

- `ITenantContext.BeginScope` 使消息消费、Outbox 发布和 Saga 执行／补偿显式恢复租户；租户校验复用同一白名单，缺少租户的多租户消息直接进入死信队列。
- `AddKunSecurity` 与 `AddKunMultiTenancy` 共用 `AsyncLocalTenantContext`，JWT 仅提供已认证的解析来源；显式后台作用域优先且结束后恢复原值。
- `Production` 模式只接受合法令牌的唯一 `tenant_id`，忽略请求头与默认租户回退，失败返回 401；开发兼容模式仍可显式配置旧 claim 和请求头。
- 新增 `AddKunTenantRls<TContext>()` 自动装配连接版、事务版拦截器和 `KunMultiTenantModelCacheKeyFactory`；`KunDbContext` 实现统一缓存键接口。无泛型旧重载保留注册语义，已有手动装配继续兼容。
- RabbitMQ 新增 `tenant_id` 信封头，兼容读取与发送旧 `tenantId` 头（至少两个小版本）；本次不变更 ack／重试／幂等语义。


### Changed — 仓库地址

- 仓库于 2026-10-08 从 CavaiLAB 组织迁回［个人账号名下］，与［下游的智能体产品］、［部署仓库］同一所有者；旧地址自动重定向。GitHub Packages 镜像自下一个版本起发布在［该账号］名下，nuget.org 上的 `Kun.*` 不受影响。本文件的版本比较链接已改为新地址。

### Fixed — 发布配置隔离

- GitHub Packages 发布使用当前步骤专属的临时 NuGet 配置，并在步骤退出时删除，避免把短期凭据和额外包源遗留在共享执行器，导致后续还原触发 NU1507。

### Fixed — 文档

- 修正本文件两处越出仓库根目录的相对链接；`docs/architecture/when-to-use-currentuser.md` 的决策表补齐两行缺失的列（合并提交 [`53d3613`](https://github.com/CavaiLAB/kun/commit/53d36131153003950f9c5252416a88a559f04f5c)）。

## [2026.9.13] - 2026-09-13

### Fixed — 测试依赖漏洞门禁

- 将 Testcontainers 引入的 SSH.NET 在四个测试项目中明确锁定到修复版本 2026.0.0，保留 NU1903 检测；不向生产包添加该依赖。参见 [GHSA-q939-rpr3-3284](https://github.com/sshnet/SSH.NET/security/advisories/GHSA-q939-rpr3-3284)。

### Fixed — 网关与服务的身份上下文一致性

- 网关保留可执行宿主，同时支持 NuGet 打包，便于消费方复用既有 AddKunGateway、发行器和公钥端点，而不复制网关实现。
- 统一网关和下游对 role / roles / ClaimTypes.Role 的处理并去重，避免内部令牌携带的角色在 ICurrentUser 中丢失。
- 新增显式 ForwardedClaimTypes 白名单，支持会话标识、授权版本和权限字段；保留声明、数量和有效期边界，默认不转发额外字段。
- 公钥端点明确使用已有源生成序列化上下文，修复 Native AOT 程序实际调用 JWKS 返回 500。
- 内部令牌增加与 JWKS 一致的 kid 及独立 jti，拒绝超过五分钟或非正的 TTL；YARP 转换改为等待异步解析并传播请求取消。
- 增加实际 JWT 中间件的网关签发到下游身份读取回归，验证角色、租户、会话版本和选定权限字段。

### Added — OSS Live Smoke 按需 CI 工作流

- 新增 `.github/workflows/oss-live-smoke.yml`：把既有的 `OssFileStorageLiveSmokeTests`（实弹 OSS 预签名 PUT/GET 验证）接入 `workflow_dispatch` + 每日定时（03:27 UTC）的可选 job。此前该测试只能在本地或下游消费者手动跑，从不被任何 CI 触发（见合并提交 [`3affc05`](https://github.com/CavaiLAB/kun/commit/3affc05568f1c9f749418a282cae502d10d4a5be) 那个 PR 遗留的"实弹 OSS e2e"项）。
- 仅当配置了 `OSS_SMOKE_*` 四个仓库 secrets 时真正验证；缺失时测试自跳过，job 仍绿，并由 "Check credentials" 步骤打印警告，避免把"跳过"误读为"已验证"。定时任务以 `github.repository` 等于［当时的仓库］为条件，fork 不触发。

## [2026.6.12] - 2026-06-12

首个 CalVer 版本，也是全部可打包模块首次推送 NuGet.org 的正式版。除下列当日变更外，本版还包含此前积压在 Unreleased 的全部 2026-04-22 ～ 04-24 交付（见本节后半部分）。

### Fixed — Kun.sln 完整性：20 个 src/tests 项目纳入 CI 构建与测试门禁 (合并提交 [`0d8916d`](https://github.com/CavaiLAB/kun/commit/0d8916d8a55f22d019ac7d3dcfb05fbdc0a78a3e))

- `Kun.sln` 此前缺失 11 个 src 项目（Audit.EntityFrameworkCore、Caching、Configuration、Identity、Identity.AspNetCore、MultiTenancy、MultiTenancy.EntityFrameworkCore、Observability、RateLimit、Sandbox、Storage）与 9 个测试项目——CI 按 sln 构建，这些测试（200+ 用例，仅 Kun.Identity.Tests 即 47 个）从未在 CI 运行，src 侧仅靠 AOT audit 矩阵兜底。
- 同步修复缺失测试项目在 Release + TreatWarningsAsErrors 下积累的分析器违规（CA1305 / CA1861 / IDE0005）。

### Added — benchmarks 项目纳入 CI 编译门禁 (合并提交 [`a46c52f`](https://github.com/CavaiLAB/kun/commit/a46c52faf5ba17f933d77216ef6372ac22446ee2))

- `Kun.Benchmarks` 与 `Kun.Benchmarks.Grpc` 加入 `Kun.sln`，并修复其相对 `Kun.Storage` 现行 API 的编译漂移，防止基准代码静默腐烂。

### Fixed — Kun.Observability 稳定版打包被 NU5104 阻断 (合并提交 [`f53b178`](https://github.com/CavaiLAB/kun/commit/f53b178840f8fa6adf9c2dbf0f9425635e880f74))

- `OpenTelemetry.Instrumentation.EntityFrameworkCore` 上游从未发布稳定版（beta-only，当前 1.15.1-beta.1）。`Kun.Observability` 入 sln 后首次被 `dotnet pack` 打包，即触发 NU5104（稳定包不得携带预发布依赖）并被 TreatWarningsAsErrors 升级为错误，阻断发版。在该 csproj 定点 `NoWarn` 并以注释记录上游现状（上游转正后应回顾移除）。

### Fixed — 测试稳定性：消除两处高频 CI flaky (合并提交 [`8551293`](https://github.com/CavaiLAB/kun/commit/85512932c6a413c1de6985bb1214b0728ee4054a) / [`54e2fde`](https://github.com/CavaiLAB/kun/commit/54e2fde7337dd6196065138d8cd15c8853240803) / [`45182cf`](https://github.com/CavaiLAB/kun/commit/45182cfdb144259b89cc729b0fe1714e1394cc5e))

- `LogBaggageEnrichmentTests` 从未启动自身 trace 管道，依赖同程序集并行测试的进程级 ActivitySource 监听器“碰巧在场”；无监听器瞬间 `StartActivity` 返回 null，断言失败。现于 `BuildLoggerFactory` 中 eager resolve `TracerProvider` 使测试自给自足，静默 null 守卫改为显式断言 (合并提交 [`8551293`](https://github.com/CavaiLAB/kun/commit/85512932c6a413c1de6985bb1214b0728ee4054a))。
- PgBouncer 100 并发压测的 TCP+auth 风暴在高负载共享 runner 上压垮单线程 pgbouncer 的 accept 循环，内核重置表现为 `Exception while reading from stream`：并发收敛到 30 限流闸门（仍高于 25 个后端槽，连接换绑的被测语义不变）+ 仅对 transient 异常 ≤3 次退避重试，prepared-statement 错误（PostgresException）永不重试 (合并提交 [`54e2fde`](https://github.com/CavaiLAB/kun/commit/54e2fde7337dd6196065138d8cd15c8853240803))；fixture 补 `UntilPortIsAvailable(6432)` 等待策略，关闭“容器进程已起、监听 socket 未绑定”的启动竞态窗口 (合并提交 [`45182cf`](https://github.com/CavaiLAB/kun/commit/45182cfdb144259b89cc729b0fe1714e1394cc5e))。

### Added — Kun.Testing.EntityFrameworkCore module with OutboxAssertions (2026-04-24)

- **KN-SELF-014 (scaffold) + KN-SELF-016 (tests + docs) together
  close ADR-015 §F3**. Ships the sibling module to
  `Kun.Testing` for EF-persisted Outbox assertion helpers.
- **New `src/Kun.Testing.EntityFrameworkCore/` module** —
  `IsAotCandidate=false` (matches `Kun.EntityFrameworkCore`),
  `IsPackable=true`, `HasPublicApi=true`. ProjectReferences
  `Kun.Testing` (reuses `KunAssertionException`) +
  `Kun.EventBus` (the `OutboxMessage` type under test).
  PackageReference `Microsoft.EntityFrameworkCore`.
- **`OutboxAssertions` class** — six extension methods, 3
  helpers × 2 overload families:
  - `IEnumerable<OutboxMessage>` overloads (primary, no
    `DbContext` dep required): `ShouldHaveOutboxMessage(eventTypeId)`,
    `ShouldHaveExactlyOneOutboxMessage(eventTypeId)` (returns
    the matched `OutboxMessage`), `ShouldNotHaveOutboxMessage(eventTypeId)`.
  - `DbContext` overloads (one-liner convenience — delegate
    to `db.Set<OutboxMessage>().AsEnumerable().ShouldXxx(...)`):
    same three helpers.
- **Mismatch-message shape**: helper name + expected
  `EventTypeId` + observed match count + distinct
  `EventTypeId`s actually present (sorted, comma-joined).
  That last field tells the consumer what WAS staged on
  failure — dramatically shorter round-trip when the test
  was set up wrong.
- **Tests** (`tests/Kun.Testing.EntityFrameworkCore.Tests/`):
  11 tests covering every helper × overload × pass/fail path;
  `UseInMemoryDatabase(Guid.NewGuid().ToString())` per test
  keeps the `DbContext` overload fixtures hermetic.
- **Cross-links**: `Kun.EventBus/AGENTS.md` §Testing
  Requirements now points at the new module as the canonical
  Outbox test-helper surface; `Kun.Testing/AGENTS.md`
  sibling-module note links to the EntityFrameworkCore
  variant for tests that need an EF-Core dependency.
- **Consumer usage**:
  ```csharp
  await _mediator.Send(new PlaceOrderCommand(...), ct);
  _dbContext.ShouldHaveExactlyOneOutboxMessage("order.placed.v1");
  ```

### Security — Resolve NU1902 / GHSA-g94r-2vxg-569j on OpenTelemetry.Api 1.12.0 (2026-04-24)

- **KN-SELF-015** closes the NU1902 advisory
  ([GHSA-g94r-2vxg-569j](https://github.com/advisories/GHSA-g94r-2vxg-569j),
  moderate severity) against `OpenTelemetry.Api 1.12.0`, which
  was entering the transitive graph of `Kun.Sample.Catalog` +
  `Kun.Sample.Tests` via
  `OpenTelemetry.Instrumentation.EntityFrameworkCore 1.12.0-beta.1`.
- **Fix** (`ce910a4`): `Directory.Packages.props` OpenTelemetry
  group bumped — `OpenTelemetry.Api` explicitly pinned to
  **1.15.3** (patched), plus companion pins on `OpenTelemetry` /
  `OpenTelemetry.Extensions.Hosting` / `OpenTelemetry.Exporter.*`
  to 1.12.0 to keep the group coherent. CPM now force-resolves
  the transitive dependency up to the patched line.
- **Operator impact**: `dotnet build Kun.sln -c Release
  -p:TreatWarningsAsErrors=true` returns to 0 errors
  (NuGetAudit no longer promotes NU1902 to error). No source-
  or behaviour-level changes; purely a dependency graph
  tightening. Downstream consumers of Kun that pin
  `OpenTelemetry.Api` directly should align on 1.15.3 to
  avoid a version conflict at restore.

### Added — Non-generic `Result` variant of ValidationResultPipelineBehavior (2026-04-24)

- **KN-SELF-012** closes **ADR-014 §F1** — the deferred
  non-generic `Result` variant of the Result-returning validation
  pipeline behavior originally shipped by KN-SELF-007.
- **`Kun.Validation.Behaviors.ValidationResultPipelineBehavior<TRequest>`** —
  new sibling class in the same file as the generic
  `ValidationResultPipelineBehavior<TRequest, TValue>`, constrained
  as `IPipelineBehavior<TRequest, Result>` where
  `TRequest : IRequest<Result>`. Same validator loop, same
  `"; "`-joined `Error.Validation("request.invalid", …)` shape;
  returns `Result.Fail(...)` instead of `Result<TValue>.Fail(...)`.
- **`KunMediatorBuilder.AddValidationResultBehavior<TRequest>()`** —
  new single-type-param overload on
  `KunValidationServiceCollectionExtensions`, closing for
  `IRequest<Result>` specifically. Mirrors the existing
  two-type-param overload's registration pattern.
- **Consumer impact**: void-returning commands (e.g.
  `DeleteUserCommand : IRequest<Result>`) that want validation
  failures surfaced as `Result.Fail` no longer need to either
  throw `ValidationException` (via the legacy
  `ValidationPipelineBehavior`) or hand-roll a try/catch
  adapter. Register per-request exactly like the generic
  variant: `builder.AddValidator<DeleteUserCommand, DeleteUserValidator>()
  .AddValidationResultBehavior<DeleteUserCommand>();`.
- **Tests**: `tests/Kun.Validation.Tests/ValidationResultPipelineBehaviorTests.cs`
  picks up 3 new scenarios mirroring the generic-variant path —
  validator-fail → `Result.Fail(request.invalid)`, multi-validator
  aggregation, and handler-level `Result.Fail` preserved
  untouched. 20/20 Kun.Validation tests pass (17 existing + 3 new).
- **AOT**: behavior and DI overload are both AOT-safe — no
  reflection, closed generics compile-time-known,
  `Kun.Validation` ILC 0-warning.

### Changed — HasPublicApi sweep complete across all framework modules (2026-04-24)

- **KN-SELF-010** finished the pilot started by KN-SELF-008
  (Kun.Observability) and KN-SELF-009 (Kun.Sandbox). The remaining
  11 modules now track strict public API via
  `PublicApiAnalyzers`:
  - `Kun.Audit`, `Kun.Audit.EntityFrameworkCore`
  - `Kun.Caching`, `Kun.Configuration`
  - `Kun.Identity`, `Kun.Identity.AspNetCore`
  - `Kun.MultiTenancy`, `Kun.MultiTenancy.EntityFrameworkCore`
  - `Kun.RateLimit`, `Kun.Security`, `Kun.Storage`
- Each got its csproj flag flipped to `<HasPublicApi>true</HasPublicApi>`,
  a seeded empty `PublicAPI.Shipped.txt`, and an auto-populated
  `PublicAPI.Unshipped.txt` (via `dotnet format analyzers
  --diagnostics=RS0016` — the Roslyn code fix walks the assembly
  and writes canonical entries in one pass).
- **`Kun.Identity.AspNetCore/SignInHelper.cs`** picked up a
  file-level `#pragma warning disable RS0026` (same
  shape-distinguished dual overload pattern as
  `Kun.Sandbox/Abstractions/IProcesses.cs`).
- **Post-sweep invariant**: `grep -rE
  "<HasPublicApi>false</HasPublicApi>" src/` now returns zero.
  Any new framework module must ship with the flag on, matching
  the convention across the entire `src/Kun.*` tree.
- **Operator impact**: any future public-symbol change across
  any Kun module fails the analyzer gate until reviewed and
  added to the relevant `PublicAPI.Unshipped.txt`. Closes the
  drift surface that existed before KN-SELF-008.

### Changed — Kun.Sandbox now tracks public API strictly (2026-04-24)

- **`src/Kun.Sandbox/Kun.Sandbox.csproj`** flipped
  `<HasPublicApi>false</HasPublicApi>` → `true`.
- **Seeded `PublicAPI.Shipped.txt`** (empty) and
  **`PublicAPI.Unshipped.txt`** enumerating the ≈15 public types
  and their members — `ISandboxProvider` / `ISandboxHandle` /
  `IProcesses` / `IFiles` / `SandboxState`, the three
  implementations (`SupervisorSandboxProvider`,
  `E2BCloudSandboxProvider`, `InMemorySandboxProvider`), the
  E2B-shaped DTOs (`SandboxCreateOptions`,
  ［智能体引擎的沙箱扩展类型］, `SecretScopeConfig`,
  `BrowserSessionConfig`, `FileEntry`, `ProcessRunOptions`,
  `ProcessResult`), `KunSandboxOptions`, and the
  `AddKunSandbox` extension. 120 entries; auto-emitted by
  `dotnet format analyzers --diagnostics=RS0016` (the Roslyn
  code fix) rather than hand-written.
- **`IProcesses.RunAsync` pre-existing dual-overload**
  (`string[]` vs `string` first param) suppressed with a
  file-level `#pragma warning disable RS0026` and an explanatory
  comment — overload resolution is unambiguous on parameter
  shape, not optional-param count, so the analyzer's
  backcompat rule does not apply.
- **Operator impact**: any future public-symbol change in
  `Kun.Sandbox` now fails the `PublicApiAnalyzers` check until
  reviewed and added. No runtime behaviour changes.
- Second module in the KN-SELF-008 pilot sweep. Remaining
  `HasPublicApi=false` modules are the `*.EntityFrameworkCore`
  family + `Kun.AspNetCore`; next cycle evaluates per-module.

### Changed — Kun.Observability now tracks public API strictly (2026-04-24)

- **`src/Kun.Observability/Kun.Observability.csproj`** flipped
  `<HasPublicApi>false</HasPublicApi>` → `true`.
- **Seeded `PublicAPI.Shipped.txt`** (empty — convention for a new
  tracking file) and **`PublicAPI.Unshipped.txt`** enumerating the
  current public surface: `KunObservabilityOptions` + every
  property, `KunObservabilityServiceCollectionExtensions.AddKunObservability`,
  `Enrichment.KunBaggageKeys` + `TenantId` / `UserId` /
  `OutboxMessageId` constants + `All` array.
- **Operator impact**: any future public-symbol change in
  `Kun.Observability` now fails the `PublicApiAnalyzers` check
  until the diff is reviewed and added to the `.Unshipped.txt`,
  matching the discipline in place on `Kun.Core`, `Kun.Mediator`,
  `Kun.Validation`, and other user-facing modules. No runtime
  behaviour changes.
- **Scope note**: pilot for the remaining
  `HasPublicApi=false` modules (e.g. `Kun.Sandbox`). Those flips
  will land as separate KN-SELF-xxx tasks — one module at a time
  to keep each `PublicAPI.Unshipped.txt` review small.

### Added — ADR-014 Result-returning validation behavior (2026-04-24)

- **docs/architecture/ADR-014-Validation-Result-behavior.md** —
  accepted. Design for a sibling pipeline behavior to the existing
  exception-throwing `ValidationPipelineBehavior`: on validator
  failure, returns
  `Result<TValue>.Fail(Error.Validation("request.invalid", ...))`
  instead of throwing. Directly consumes ADR-011 `Result<T>`
  composition.
- **`Kun.Validation.Behaviors.ValidationResultPipelineBehavior<TRequest, TValue>`** —
  new sealed class implementing
  `IPipelineBehavior<TRequest, Result<TValue>>` with
  `where TRequest : IRequest<Result<TValue>>`. Aggregates
  validator errors into one joined message; empty-validators
  fast-path.
- **`AddValidationResultBehavior<TRequest, TValue>`** extension on
  `KunMediatorBuilder` — explicit per-request-type registration,
  matches the existing `AddValidator<>` / `AddValidationBehavior<>`
  pattern. The generic constraint on `TResponse` cannot be
  expressed at the open-generic DI layer (ADR-014 §DI
  registration).
- **5 new tests** in
  `tests/Kun.Validation.Tests/ValidationResultPipelineBehaviorTests.cs`
  covering: empty validators pass-through, validator pass, validator
  fail → Result.Fail with `request.invalid` code, multi-validator
  aggregation with `;` separator, handler-level `Result.Fail` is
  **not** intercepted (domain error preserved).
- **Coexistence rule** with the throwing `ValidationPipelineBehavior`:
  register one or the other per request type, not both — doc
  guardrail in `src/Kun.Validation/AGENTS.md`.
- **Scope notes**: non-generic `Result` variant deferred as ADR-014
  §F1. Structured per-field error payload deferred as §F2. Source-
  generator-based bulk registration deferred as §F3. No new
  `<PackageReference>`; `Kun.Validation` keeps
  `IsAotCandidate=true`.

### Added — ADR-013 §F1 log-record enricher (2026-04-24)

- **docs/architecture/ADR-013-Kun.Observability-enricher.md** —
  amended with a new §F1 resolution section. The prior §Follow-ups
  F1 entry is now annotated "Resolved 2026-04-24 (KN-SELF-006)"
  with a pointer to the new section.
- **`Kun.Observability.Enrichment.KunBaggageLogRecordProcessor`**
  (internal) — `BaseProcessor<LogRecord>` that runs at `OnEnd`,
  reads `Activity.Current?.Baggage`, and mirrors whitelisted keys
  (`tenant.id`, `user.id`, `outbox.message_id`) onto
  `LogRecord.Attributes`. Idempotent: never overwrites an
  attribute the log statement set explicitly. No-op when no
  `Activity.Current` exists.
- **`KunObservabilityOptions.EnrichLogsFromBaggage`** — new flag,
  default `false` (opt-in, mirrors `EnrichActivitiesFromBaggage`
  posture). Setting it to `true` registers the processor into the
  logging pipeline at `AddKunObservability` time.
- **4 new tests** in `tests/Kun.Observability.Tests/LogBaggageEnrichmentTests.cs`
  via `OpenTelemetry.Logs.InMemoryExporter`: baggage→attribute
  happy path, flag-off path, caller-set-attribute-preserved path,
  no-active-activity no-crash path.
- **AOT / package deltas**: zero new `<PackageReference>`;
  processor and tests use only already-referenced OpenTelemetry
  APIs. `Kun.Observability` keeps `IsAotCandidate=true`.
- **Operator impact**: flipping both
  `EnrichActivitiesFromBaggage = true` and
  `EnrichLogsFromBaggage = true` on `AddKunObservability`
  options now produces Aspire / Jaeger / Kibana queries where the
  same `tenant.id=…` filter works uniformly across the trace UI
  and the log view — no hand-joining.

### Added — ADR-013 Kun.Observability baggage enricher (2026-04-24)

- **docs/architecture/ADR-013-Kun.Observability-enricher.md** —
  accepted. Baggage-driven activity enrichment: consumer code
  pushes `tenant.id` / `user.id` / `outbox.message_id` into
  OpenTelemetry baggage at the request boundary; the framework
  mirrors those whitelisted keys onto Kun-sourced span tags so
  Aspire / Jaeger operators can filter by tenant / user without
  hand-correlating spans to logs.
- **`Kun.Observability.Enrichment.KunBaggageKeys`** — public
  static class exposing the canonical baggage key names and
  `All` array for iteration.
- **`Kun.Observability.Enrichment.KunBaggageActivityProcessor`**
  (internal) — `BaseProcessor<Activity>` that runs at `OnEnd`
  for activities whose `ActivitySource.Name` is in
  `KunActivitySources.All` and mirrors the whitelisted baggage
  subset to span tags.
- **`KunObservabilityOptions.EnrichActivitiesFromBaggage`** —
  new flag, default `false` (opt-in per ADR-013 §Default
  posture). Setting it to `true` registers the processor into
  the `TracerProviderBuilder` at `AddKunObservability` time.
- **7 new tests in `tests/Kun.Observability.Tests/BaggageEnrichmentTests.cs`**
  — processor-level unit tests (manual `TracerProviderBuilder` +
  `AlwaysOnSampler` + `OpenTelemetry.Exporter.InMemory`) plus
  DI-wired integration tests verifying the option flag both on
  and off.
- **AOT / package deltas**: zero new `<PackageReference>` on
  `Kun.Observability.csproj`; module stays `IsAotCandidate=true`.
  `HasPublicApi` stays `false` for this module — tracking a
  future flip as a separate framework-wide task so every
  `Kun.*` module adopts strict API tracking together.
- **Out of scope** (ADR-013 F1–F3): log-record enricher,
  metric-dimension enricher, Aspire sample-dashboard demo.

### Added — ADR-012 Kun.Testing module (2026-04-24)

- **docs/architecture/ADR-012-Kun.Testing-module.md** — accepted. New
  top-level `Kun.Testing` module at the Abstraction layer:
  AOT-candidate, packable, depends on `Kun.Core` only, takes no
  third-party runtime `PackageReference`. Framework-agnostic
  (`KunAssertionException` on mismatch rather than xUnit-coupled
  throws) so consumers on any test framework consume uniformly.
- **`src/Kun.Testing`** — new project wired into `Kun.sln` in the
  `src/` folder. `IsAotCandidate=true`, `IsPackable=true`,
  `HasPublicApi=true`.
- **`Kun.Testing.KunAssertionException`** — framework-agnostic
  assertion failure type.
- **`Kun.Testing.Results.ResultAssertions`** — fluent extensions on
  `Result` and `Result<T>`: `ShouldBeSuccess` (returns `T` on
  generic variant for chaining), `ShouldBeFailure`,
  `ShouldHaveErrorCode(string)`, `ShouldHaveErrorType(ErrorType)`.
- **`Kun.Testing.Domain.DomainEventAssertions`** — fluent extensions
  on `IAggregateRoot`: `ShouldRaise<TEvent>`,
  `ShouldRaiseExactly<TEvent>(int count)`, `ShouldNotRaise<TEvent>`.
  AOT-safe via compile-time-known `OfType<TEvent>()` filtering.
- **30 new tests in `tests/Kun.Testing.Tests`** — every helper on
  both pass-through and mismatch paths, exception-message shape
  anchored per ADR-012 §Error-message contract.
- **Smoke-adoption in `tests/Kun.Core.Tests`** — 4 existing
  assertions in `Results/ResultTests.cs` converted to Kun.Testing
  helpers. Validates cross-project consumption before other tests
  pull it in; 85 Kun.Core.Tests still pass.
- **Out of scope** (follow-ups F1–F3 in ADR-012): Outbox message
  assertions (would require a non-AOT companion module
  `Kun.Testing.EntityFrameworkCore`), Testcontainers fixtures,
  `WebApplicationFactory<T>` wrappers, repo-wide mass conversion
  sweep.

### Added — ADR-011 Result<T> composition extensions (2026-04-24)

- **docs/architecture/ADR-011-Result-async-extensions.md** — accepted.
  Specifies the canonical Map / Bind / Tap / Ensure / Match surface
  (sync + async) for `Kun.Core.Results` before more downstream consumers
  hand-roll their own. Normative short-circuit and cancellation
  semantics; alternatives (`Then`, LINQ `Select`/`SelectMany`,
  `Flatten`) explicitly deferred.
- **`Kun.Core.Results.ResultExtensions`** — synchronous extensions on
  `Result` and `Result<T>`: `Tap`, `Ensure`, `Match`, `Bind` (plus
  `Result<T>.Map` / `Result<T>.Bind` into `Result<TOut>` or `Result`).
- **`Kun.Core.Results.TaskResultExtensions`** — async extensions on
  `Task<Result>` / `Task<Result<T>>` receivers. Each op has two
  overloads: a sync-delegate variant and an async-delegate variant
  with required `CancellationToken`. Return type stays on the Task
  rail.
- **`Kun.Core.Results.ValueTaskResultExtensions`** — mirror of
  `TaskResultExtensions` for hot-path handlers that return
  `ValueTask<…>` (Mediator pipeline, Saga step executors). Same
  two-overload shape per op; async-delegate variant accepts
  `Func<…, ValueTask<…>>` and returns `ValueTask<…>`.
- **Deliberate deviation from ADR-011 sketch**: `CancellationToken`
  on async-delegate overloads is required, not `= default`. RS0026
  (PublicAPI analyzer "do not add multiple overloads with optional
  parameters") flags the alternative; we prefer passing the analyzer
  over masking it with a blanket suppression. Callers who want no
  token pass `default` or `CancellationToken.None` explicitly.
- **41 new tests in `tests/Kun.Core.Tests/Results/ResultExtensionsTests.cs`**
  — sync success / failure / short-circuit, async Task + ValueTask
  parity, CT forwarding, pre-cancelled CT surfaces
  `OperationCanceledException`, null-argument guards.
- **Scope note**: additive only. `Kun.Core` stays AOT-candidate with
  zero third-party deps; `PublicAPI.Unshipped.txt` listing every new
  symbol. Migration of ［下游的 C# 服务］ handlers off hand-rolled
  patterns is a separate follow-up task (ADR-011 §Follow-ups F1).

### Added — P2-A Part 2 benchmarks CI workflow (2026-04-24)

- **.github/workflows/benchmarks.yml** — weekly cron (Mon 06:00 UTC) + PR-label
  trigger (`benchmarks` label) + manual dispatch. Runs the
  `Kun.Benchmarks.Grpc` project with `--exporters json markdown` and uploads
  `BenchmarkDotNet.Artifacts/**` as a named artifact (30-day retention).
- Matrix includes `jit` only for now; the `aot` matrix entry is sketched in a
  comment and unlocks once a `PublishAot=true` variant of the benchmarks
  project lands (Part 2 follow-up).
- Out of scope (future Part 3): a runbook that downloads the artifact + diffs
  against a stored baseline to fail PRs on allocation regression.

### Added — P2-A Part 1 BenchmarkDotNet scaffold (2026-04-24)

- **benchmarks/Kun.Benchmarks.Grpc** — new standalone BenchmarkDotNet console project
  (intentionally NOT in Kun.sln so routine `dotnet build Kun.sln` ignores it).
  Builds clean at `dotnet build benchmarks/Kun.Benchmarks.Grpc -c Release`.
- `ResultToRpcInterceptorBenchmarks` — microbenchmarks the public `Result<T>` pattern
  (the internal `KunResultToRpcInterceptor` is not benchmarkable from a non-friend
  assembly; the `Result<T>` paths are the real hot path the translator catches).
  Compares `Result<T>.Ok` / `Fail` / `EnsureSuccess` / IsFailure+Error.Code against
  a plain-return baseline. `[MemoryDiagnoser]` attached so allocation deltas are
  visible.
- Directory.Packages.props centrally pins `BenchmarkDotNet` to `0.14.0` under a new
  "Benchmarks" label.
- **docs/benchmarks/grpc-aot.md** — methodology doc + run command + Part-2-plan
  (AOT matrix in CI, cold-start target <100ms).
- **Scope note**: Part 1 is scaffolding. Part 2 adds the AOT publish matrix on CI,
  JIT-vs-AOT comparison tables, and a `.github/workflows/benchmarks.yml` weekly +
  PR-label trigger. Kun.sln integration is intentionally deferred — benchmarks are
  run on-demand, not as part of the regular build matrix.

### Added — P2-C Part 3 Redis-backed rate-limit store + guide (2026-04-24)

- **Kun.Gateway.RateLimiting.IRedisRateLimitStore** + `RateLimitHitResult` record —
  low-level primitive for distributed rate limiting across multiple Gateway pods.
- **RedisFixedWindowRateLimitStore** — default implementation using atomic
  `INCR` + `EXPIRE`-on-first-hit semantics so counters self-expire when the window
  elapses (no cleanup job required). Key format `{KeyPrefix}:{partitionKey}`,
  default prefix `"kun:rl"`.
- **RedisRateLimitStoreOptions** — `KeyPrefix` knob, bound from
  `Kun:Gateway:RateLimit:Redis` configuration section.
- **AddKunRedisRateLimitStore** DI extension — `TryAddSingleton` so consumer
  pre-registrations win. Requires `IConnectionMultiplexer` to be registered
  separately by the consumer.
- **Kun.Gateway** csproj: new `StackExchange.Redis` `PackageReference` (version
  already centrally pinned to 2.8.16).
- **Scope note**: this is a primitive, not a full `PartitionedRateLimiter<HttpContext>`
  integration. Consumers today call `store.HitAsync(...)` from custom middleware
  and translate rejections to HTTP 429. Framework-level integration with
  `AddRateLimiter` named policies is tracked as Part 3b (requires a custom
  `RateLimiter` subclass with 5+ abstract members).
- Tests: 11 new facts in `tests/Kun.Gateway.Tests` (argument guards, first-hit
  TTL behavior, subsequent-hit TTL preservation, key-prefix application, DI
  registration). Gateway test total 48 → 59, all green.
- **docs**: new `docs/guides/gateway-rate-limit.md` covering the 3×3 resolver ×
  policy matrix + Redis store recipe + landing checklist.

### Added — P2-C Part 2 sliding-window + token-bucket rate-limit policies (2026-04-24)

- **Kun.Gateway.RateLimiting.KunSlidingWindowRateLimiterPolicy** (internal) + public
  `KunSlidingWindowOptions` + `AddKunSlidingWindowRateLimiter()` DI extension. Named
  policy `kun-sliding-window`. Defaults: 100 permits per 1-minute window split into
  6 segments, immediate rejection on exceed (QueueLimit=0). Smoother enforcement than
  fixed-window at the cost of proportionally higher memory per partition.
- **Kun.Gateway.RateLimiting.KunTokenBucketRateLimiterPolicy** (internal) + public
  `KunTokenBucketOptions` + `AddKunTokenBucketRateLimiter()` DI extension. Named
  policy `kun-token-bucket`. Defaults: 100-token burst budget refilled at 20 tokens
  every 10 seconds (≈120/min steady state) with `AutoReplenishment=true`. Absorbs
  bursty traffic; use for webhook fanout / batch import APIs.
- Both policies reuse the existing `IRateLimitPartitionResolver` abstraction so the
  three partition resolvers (Tenant / IP / UserPrincipal) compose freely with any
  of the three policies (fixed-window / sliding-window / token-bucket) via the
  matching `AddKunXxxRateLimiter` call.
- Tests: 9 new unit tests in `tests/Kun.Gateway.Tests`. P2-C remaining for Part 3:
  Redis-backed shared counter for distributed rate limiting + Testcontainers
  concurrent-precision test + dedicated guides.

### Added — P2-C Part 1 user-principal rate-limit partition resolver (2026-04-23)

- **Kun.Gateway.RateLimiting.Resolvers.UserPrincipalRateLimitPartitionResolver** —
  third partition resolver flavour, alongside the existing IP and tenant resolvers.
  Reads the authenticated user's `sub` claim and returns it as the rate-limit
  partition key; falls back to `"anonymous"` when the claim is absent. Pair with
  `AddKunRateLimiter` for free-tier APIs where rate limits are per-account rather
  than per-organization.
- Tests: 6 new unit tests in `tests/Kun.Gateway.Tests`. P2-C remaining items
  (sliding-window / token-bucket policies, Redis-backed shared counter,
  Testcontainers concurrent-precision test, dedicated guides) tracked for Part 2.

### Added — P2-D Part 3 DatabasePerTenant runtime (2026-04-23)

- **Kun.EntityFrameworkCore.MultiTenancy** (additive):
  - `IDbConnectionResolver` — resolves per-tenant connection strings at runtime.
  - `TenantsConnectionOptions` — static tenant → connection-string map, bound via
    `Configure<TenantsConnectionOptions>` from the `Kun:MultiTenancy:Tenants` config
    section. Suitable for small deployments; larger setups swap in a custom resolver.
  - `OptionsBasedTenantConnectionResolver` — default resolver reading from
    `IOptionsMonitor<TenantsConnectionOptions>`; supports on-the-fly reloads.
  - `DatabasePerTenantProvisioner<TContext>` — `ITenantProvisioner` that runs
    `Database.MigrateAsync()` against the tenant's database. `DeprovisionAsync` is a
    no-op by design (dropping a tenant database is an operations concern requiring
    admin credentials and coordinated cleanup). Both methods are `virtual` for
    subclassing when the app owns DB lifecycle end-to-end.
  - `AddKunMultiTenancy<TContext>` DatabasePerTenant branch now registers the default
    resolver + provisioner (TryAddSingleton, so consumer pre-registrations win).
- **docs**: ADR status upgraded to Part 3 Accepted; roadmap-p2 Part 3 row updated.

### Added — P2-D Part 2 SchemaPerTenant runtime (2026-04-23)

- **Kun.EntityFrameworkCore.MultiTenancy** (additive, existing Kun.EntityFrameworkCore is
  trim-safe / non-AOT; no new external dependencies):
  - `ITenantDbContextFactory<TContext>` — explicit factory for tenant-scoped DbContext
    construction, used by provisioning / admin flows that target a specific tenant
    regardless of the ambient `ITenantContext`.
  - `TenantAwareModelCacheKeyFactory` — `IModelCacheKeyFactory` implementation that
    partitions EF Core's compiled-model cache by `KunDbContext.TenantIdForModelCacheKey`
    in addition to context type + design-time flag.
  - `SchemaPerTenantProvisioner<TContext>` — `ITenantProvisioner` implementation that
    runs `CREATE SCHEMA IF NOT EXISTS "{tenantId}"` + `Database.MigrateAsync()` at
    provisioning time. Deprovisioning is gated behind
    `KunMultiTenancyOptions.AllowDestructiveDeprovision` (default false); unsafe tenant
    IDs (anything outside `[A-Za-z_][A-Za-z0-9_-]{0,62}`) are rejected with
    `ArgumentException` before reaching the database.
  - `KunMultiTenancyOptions` — `Strategy` + `AllowDestructiveDeprovision` knobs.
  - `AddKunMultiTenancy<TContext>(options => ...)` DI entry: Shared delegates to
    the existing `AddKunTenantRls()` + `NullTenantProvisioner`; SchemaPerTenant registers
    the factory + RLS interceptor (defence-in-depth) + SchemaPerTenantProvisioner;
    DatabasePerTenant throws `NotImplementedException` at resolution time pointing to
    the pending Part 3 track.
- **docs**: `docs/architecture/adr-p2d-multitenancy.md` status upgraded to Part 2 Accepted;
  `docs/roadmap-p2.md` P2-D table row updated.

### Added — P2-D Part 1 multi-tenancy foundation types (2026-04-23)

- **Kun.Core.MultiTenancy** (additive, AOT-safe, zero new dependencies):
  - `TenantIsolationStrategy` enum — `Shared` / `SchemaPerTenant` / `DatabasePerTenant`.
    Part 1 lands the type; runtime wiring for `SchemaPerTenant` / `DatabasePerTenant`
    is deferred to P2-D Part 2 / Part 3 (see ADR below).
  - `TenantInfo` sealed record — `(string TenantId, string? DisplayName)` with a null /
    empty / whitespace guard on `TenantId`. DTO crossing module boundaries.
  - `ITenantProvisioner` interface — `ProvisionAsync(TenantInfo)` / `DeprovisionAsync(string)`
    idempotent tenant lifecycle hook. Non-null implementations land with Part 2 / Part 3.
  - `NullTenantProvisioner` no-op default (Singleton `.Instance`) for Shared strategy
    consumers who don't need tenant lifecycle plumbing.
- **docs**:
  - `docs/architecture/adr-p2d-multitenancy.md` — ADR for the 3-strategy architecture,
    including the explicit YAGNI decision that Shared remains on the existing
    `AddKunTenantRls()` entry point (no wrapper API is added until a second strategy
    ships).
  - `docs/roadmap-p2.md` P2-D section updated with the Part 1 / Part 2 / Part 3 split.

### Added — Sprint 1 gap-closure (2026-04-22)

- **Kun.Storage.Abstractions / Kun.Storage.Local / Kun.Storage.S3** (P0-1): three
  new packages landing `IFileStorage` with the seven SAD §6.4 operations, a
  `FrozenSet` of the four canonical "active web" MIME types that trigger
  force-download on S3 presigned URLs, a dev-grade local filesystem
  implementation, and an AWSSDK-based S3 implementation. S3 is the repo's first
  non-AOT library module (A6) — explicitly excluded from the CI AOT-zero-warning
  matrix; documented in `src/Kun.Storage.S3/AGENTS.md`.
- **Kun.EntityFrameworkCore**:
  - `KunDbContext.DefaultSchema` virtual property + auto-registered
    `SchemaConsistencyInterceptor` (P0-3 M5). Overriding `DefaultSchema` without
    calling `.UseKunSchema(schema)` on the options builder now throws
    `InvalidOperationException` with a remediation message at the first
    `SaveChangesAsync`. Outbox rows, tenant/audit columns, and the migrations
    history table all co-locate under the service schema.
  - `UseKunSchema(schema)` extension method on `DbContextOptionsBuilder` that
    wires `MigrationsHistoryTableSchema` through `RelationalOptionsExtension`.
    Provider-agnostic — stays out of the Npgsql dependency.
  - `TenantRlsTransactionInterceptor` + `AddKunTenantRls()` (P1-4). Opt-in
    defence-in-depth layer that emits
    `SELECT set_config('app.current_tenant_id', @tenant, true)` at every
    transaction start so PostgreSQL RLS policies enforce isolation even when a
    caller bypasses the EF global query filter. Hooks
    `DbTransactionInterceptor.TransactionStartedAsync` — PgBouncer
    transaction-pool safe.
- **Kun.EventBus**: `IEventTypeRegistry.GetAll()` returns every registered
  `(eventId, clrType)` pair in stable alphabetical order, enabling the
  `tools/Kun.IntegrationEvents.SchemaGen` JSON-Schema export tool (P1-1).
- **Kun.Gateway**:
  - `ITracedCurrentUser` sub-interface + `trace_id` claim on the minted S2S
    JWT (P0-2). Default `EmitTraceIdClaim = true`; kill-switchable.
  - `KunInternalJwksEndpoint.MapKunInternalJwks()` — internal-network JWKS
    route gated on `IHostEnvironment.IsDevelopment()` OR
    `KunGatewayOptions.ExposeInternalJwks = true` (P1-3). Fail-closed:
    Production returns 404 unless opted in.
  - `IS2sPrincipalResolver` + `NoOpS2sPrincipalResolver` (P1-6). Optional
    enrichment hook the issuer runs before claim mapping. Default registered
    via DI. `S2sTokenIssuer` grows a 4-arg ctor taking the resolver; the
    3-arg overload is kept obsolete with `DiagnosticId = "KUN0002"` for one
    release, scheduled for removal in `0.2.0`.
- **Kun.EventBus.Outbox**: `IOutboxWriter` primitive (P0-4). Non-aggregate
  enqueuers (future audit interceptor, storage event adapters) can drop
  `OutboxMessage` rows into the current transaction without implementing the
  full `IIntegrationEventBus` surface. `IntegrationEventOutboxAccumulator`
  resolves to both `IIntegrationEventBus` and `IOutboxWriter` from a single
  scoped instance so aggregates and non-aggregate collaborators share one
  in-request buffer.
- **samples/Kun.Sample.Catalog** and **samples/Kun.Sample.Order**: override
  `DefaultSchema => "catalog"` / `"order"`; design-time factories chain
  `.UseKunSchema(...)`. Migrations regenerated; outbox, saga, business tables,
  and `__ef_migrations_history` all land under the service schema.
- **samples/Kun.Sample.Catalog**: `AddOpenApi()` + `MapOpenApi()` guarded on
  `IsDevelopment()` as the reference wiring for **P1-7** (OpenAPI pipeline
  template).
- **samples/Kun.Sample.Storage**: new Minimal API sample demonstrating
  force-download via `Kun.Storage.Local` — no AWS credentials required.
- **tools/Kun.IntegrationEvents.SchemaGen**: console tool (non-AOT) that emits
  `{eventId}.schema.json` per registered integration event using
  `JsonSchema.Net.Generation`. Consumers provide a public static zero-arg
  factory method that returns `IEventTypeRegistry` (P1-1).
- **docs**:
  - `docs/roadmap.md` — consolidated public roadmap (replaces the 495-line
    `TODO.md` at repo root, merged 2026-04-22).
  - `docs/schema-per-service.md` (P1-8) — end-to-end recipe for running many
    Kun services against one Postgres with service-scoped schemas.
  - `docs/openapi-template.md` (P1-7) — source-gen-friendly OpenAPI wiring
    guidance with environment gating + CI smoke-test recipe.
  - `src/Kun.EntityFrameworkCore/README.md` (P1-5) — PgBouncer transaction-pool
    compatibility notes (required connection-string parameters) +
    RLS-with-explicit-transactions contract.
- **tests**:
  - `tests/Kun.EntityFrameworkCore.Tests` — new xunit.v3 project with schema /
    interceptor / RLS unit coverage (11 cases).
  - `tests/Kun.IntegrationTests/Persistence/{RlsEnforcementTests,TenantRlsInterceptorPgBouncerTests,PgBouncerCompatTests,SchemaPerServiceTests}` +
    `tests/Kun.IntegrationTests/Fixtures/PgBouncerPostgresFixture` — acceptance
    paths for P0-3 / P1-4 / P1-5, Docker-gated with `Assert.Skip` when
    containers aren't reachable.
  - `tests/Kun.Gateway.Tests` grows five new cases for
    `KunInternalJwksEndpoint` (gating matrix + document shape) and five for
    `IS2sPrincipalResolver` / obsolete-ctor compat.
- **Kun.Auditing** (P1-2): new AOT-candidate module wiring an EF Core
  `SavingChanges` interceptor that iterates `ChangeTracker.Entries()`,
  keeps only entity types registered via explicit
  `services.AddAuditable<TEntity>()` (no runtime attribute scan — A3
  path c), and enqueues one `AuditEventIntegrationEvent` per mutation
  through `IOutboxWriter` (the P0-4 primitive). Payload is the M4
  13-field record with stable event id `kun.audit.event.v1`; JSON via a
  source-generated `AuditJsonContext`. `AuditContextMiddleware` captures
  IP / User-Agent / TraceId into a scoped `AuditContext` before the EF
  interceptor runs. DI helpers: `AddKunAuditing`, `AddAuditable<T>`,
  `AddAuditingInterceptor(DbContextOptionsBuilder, IServiceProvider)`;
  stable event id wired via `IEventTypeRegistrar.RegisterAuditing()`.
- **Kun.EventBus** (architect M2): `IEventTypeRegistrar.Register<TEvent>`
  gains an overload that accepts a source-generated `JsonTypeInfo<TEvent>`;
  `IEventTypeRegistry.GetJsonTypeInfo(Type)` exposes it. The outbox
  accumulator now prefers the typed serializer when registered and falls
  back to reflection-based `JsonSerializer.Serialize(obj)` only when no
  info is supplied — the fallback path carries `[RequiresUnreferencedCode]`
  and `[RequiresDynamicCode]` annotations so the AOT analyzer flags
  callers that haven't migrated.
- **Kun.Gateway** (review #7): `MapKunJwks` + `MapKunInternalJwks` accept
  an optional `Func<HttpContext, bool>? additionalGate` that runs after
  the environment / `ExposeInternalJwks` check. Typical use: RFC1918
  allow-list for production deployments without service-mesh isolation.
- **Kun.Gateway** (review #2): `S2sTokenIssuer.IssueForAsync(ClaimsPrincipal, CancellationToken)`
  lets callers await the configured `IS2sPrincipalResolver` without
  `.GetAwaiter().GetResult()` blocking. The sync `IssueFor` stays for
  back-compat and delegates to the async path internally.
- **Kun.Gateway** (review #1): both JWKS endpoints share a DI-registered
  `JwksResponseBuilder` singleton that caches the JWK response keyed on
  public-key-file mtime. Rebuilds only on key rotation; typical downstream
  JWKS fetch is a single `File.GetLastWriteTimeUtc` stat.
- **Kun.Storage.Local** (review #3): streaming `MaxBytes` enforcement.
  `UploadAsync` now rejects oversized uploads mid-copy via a chunked
  `ReadAsync` loop backed by `ArrayPool<byte>` — no more "park a 1 GB
  payload on disk before deleting it" failure mode.
- **Kun.Storage** (review #6): `ObjectMetadata.ContentType` is now
  `string?`. `S3FileStorage.ListAsync` reports `null` instead of a
  synthetic `"application/octet-stream"` when the S3 listing API omits
  the content type — callers needing it follow up with
  `GetMetadataAsync` per item.

### Migration guide — `alpha.1` → `alpha.2`

All Sprint 1 changes are **additive**. `PublicAPI.Shipped.txt` is untouched;
every new member lives in `PublicAPI.Unshipped.txt`. Consumers upgrading
from `0.1.0-alpha.1` do not need to change code to keep the old behaviour,
but the new capabilities below require explicit opt-in:

1. **Per-service schemas** (P0-3). To run multiple Kun services against one
   Postgres instance without name collisions:
   - Override `DefaultSchema` on your `KunDbContext` subclass: `protected override string? DefaultSchema => "catalog";`.
   - Chain `.UseKunSchema("catalog")` on the options builder alongside
     `.UseNpgsql(...)`.
   - Regenerate migrations so they emit `schema: "catalog"` — see
     [docs/schema-per-service.md](docs/guides/schema-per-service.md).
   - `SchemaConsistencyInterceptor` auto-registers via `KunDbContext.OnConfiguring`
     and will throw at first `SaveChangesAsync` if you miss either step.
2. **trace_id claim** (P0-2). `S2sTokenIssuer` now emits a `trace_id` claim
   when `Activity.Current` is live AND an HTTP context is in scope (M3
   guard). Downstream services see it on `JwtCurrentUser` via
   `ITracedCurrentUser.TraceId`. Default on; kill-switch via
   `KunGatewayOptions.EmitTraceIdClaim = false`.
3. **IFileStorage** (P0-1). Add `.AddKunLocalFileStorage(...)` (dev) or
   `.AddKunS3FileStorage(...)` (prod) to compose storage. `Kun.Storage.S3`
   is non-AOT — exclude from your AOT publish matrix if you have one.
4. **IOutboxWriter** (P0-4). Non-aggregate code can now enqueue outbox
   messages via `IOutboxWriter.Enqueue<TEvent>(evt)`. Existing
   `IIntegrationEventBus.Publish<T>` remains — both resolve to the same
   scoped accumulator instance. Prefer `IOutboxWriter` from interceptors
   and infrastructure code; `IIntegrationEventBus` for aggregate flows.
5. **Tenant RLS** (P1-4). Opt-in via `services.AddKunTenantRls()`. Wire the
   resulting interceptor into your DbContext options with
   `.AddInterceptors(sp.GetRequiredService<TenantRlsTransactionInterceptor>())`.
   Only needed when the database has `CREATE POLICY` rules using
   `app.current_tenant_id`.
6. **IS2sPrincipalResolver** (P1-6). `S2sTokenIssuer`'s 3-arg ctor is
   marked `[Obsolete(DiagnosticId = "KUN0002")]` and scheduled for removal
   in `0.2.0`. Switch manual construction sites to the 4-arg ctor, passing
   `NoOpS2sPrincipalResolver.Instance` for the identity transform. Custom
   enrichment: register your own `IS2sPrincipalResolver` implementation
   before `AddKunGateway`. DI-wired consumers do not need code changes.
7. **Internal JWKS endpoint** (P1-3). `app.MapKunInternalJwks()` at
   `/internal/.well-known/jwks.json`. Gated: Development auto-on, Production
   returns 404 unless `KunGatewayOptions.ExposeInternalJwks = true`. The
   existing public `/.well-known/jwks.json` route is unchanged.
8. **PgBouncer transaction mode** (P1-5). Append
   `No Reset On Close=true;Max Auto Prepare=0` to any Npgsql connection
   string routed through PgBouncer; see
   [src/Kun.EntityFrameworkCore/README.md](src/Kun.EntityFrameworkCore/README.md).
9. **OpenAPI** (P1-7). `builder.Services.AddOpenApi();` +
   `if (app.Environment.IsDevelopment()) app.MapOpenApi();`. Requires the
   `Microsoft.AspNetCore.OpenApi` package.
10. **JSON Schema export** (P1-1). `IEventTypeRegistry.GetAll()` enumerates
    every `(eventId, clrType)` pair in stable order — used by
    `tools/Kun.IntegrationEvents.SchemaGen`. Provide a public static
    zero-arg factory method that returns your registry to plug into the
    tool; see `tools/Kun.IntegrationEvents.SchemaGen/README.md`.
11. **Auditing** (P1-2). Opt-in via `services.AddKunAuditing()` +
    `services.AddAuditable<YourEntity>()` for each entity you want audited.
    Attach the interceptor to your DbContext with
    `options.AddAuditingInterceptor(sp)` inside the factory lambda.
    Wire the stable event id via
    `services.AddKunEventBus(b => b.RegisterAuditing())`.
12. **AOT-safe outbox serialization** (architect M2). Upgrade existing
    `bus.Register<T>("t")` calls to
    `bus.Register<T>("t", MyJsonContext.Default.T)` to thread a
    source-generated `JsonTypeInfo<T>` through the outbox accumulator.
    Reflection fallback still works for non-AOT consumers but now warns
    via `[RequiresUnreferencedCode]` at call sites.
13. **JWKS `additionalGate`** (review #7, optional). Pass a predicate to
    `MapKunJwks` / `MapKunInternalJwks` to restrict by IP range or header
    when NetworkPolicy / mesh isn't available. Default behavior unchanged
    when the parameter is omitted.
14. **`S2sTokenIssuer.IssueForAsync`** (review #2). Prefer the async
    overload for hot-path token minting; the sync `IssueFor` delegates to
    it via `.GetAwaiter().GetResult()` and stays available for existing
    synchronous call sites.

### Highlights (2026-04-24 / 04-25)

**Phase 1 ［下游的智能体产品］ consumer gap fully closed.** Five tickets landed in one burst (23 commits on `main`, all pushed to `origin`):

- **P1-KN-001** `Kun.Storage` — new 15th module (`IFileStorage` 7-method contract + `ContentDispositionPolicy` + S3/MinIO/Local backends).
- **P1-KN-002** S2S JWT claims alignment (`Kun.Security.S2SJwt` rewritten around `S2SJwtIssueRequest`, emits `roles`/`membership_id`/`trace_id`, default TTL 15 min, multi-aud header bug fixed).
- **P1-KN-003** Attribute-driven integration-event registration (`[IntegrationEventId("event.v1")]` + `Register<TEvent>()` overload).
- **P1-CS-011** `TenantRlsConnectionInterceptor` — opt-in defence-in-depth behind the EF global filter; PG-only, whitelist-guarded.
- **P1-CS-019** `Kun.Audit` — new 16th-slot abstraction module (tamper-evident hash-chained operation audit + Mediator pipeline behaviour + 21 xunit cases).

Framework module count: 13 → 15. Test projects: 11 → 13. Validation / AOT publish matrix / NuGet release / Aspire runtime drill / SG regression remain open — see `docs/roadmap.md` for the outstanding-work roadmap（原 `TODO-remaining.md` 已于 2026-06-12 并入该文件）.

### Added — Kun.Audit module (Phase 1 P1-CS-019)

- New `Kun.Audit` abstraction module (IsAotCandidate=true, HasPublicApi=true): tamper-evident operation audit for CQRS commands via a Mediator pipeline behaviour + pluggable `IAuditSink`. Matches SAD v3.0 §15.5 `audit.operations` contract (prev_hash + content_hash).
- `AuditContentHasher.Compute`: canonical `id|tenant|actor|command|ISO-timestamp|latency|outcome|errorCode` + SHA-256 over `prev || canonical`, lower-case hex. Timestamp pinned to round-trip `O` format with `InvariantCulture` so hashes are stable across culture / timezone.
- `IAuditChainState.AppendAsync`: atomic read-previous / compute-new / commit-new, serialised per tenant key so concurrent commands on the same tenant cannot fork the chain. `InMemoryAuditChainState` uses per-tenant `SemaphoreSlim`; production deployments are expected to back this with a durable DB-level coordinator (documented in `docs/security/audit-log.md`).
- `AuditLogPipelineBehavior<TRequest, TResponse>`: captures actor/tenant/timing/outcome per handler invocation. Original handler exception is preserved via `ExceptionDispatchInfo`. Audit-write failures are logged at Warning and swallowed — audit MUST NOT convert a successful handler into a failure nor mask a business exception.
- DI: `AddKunAudit()` wires `InMemoryAuditChainState` + `InMemoryAuditSink` + `TimeProvider.System` as `TryAdd` defaults; `AddAuditBehavior<TCommand, TResponse>()` on `KunMediatorBuilder` opts one command in at a time so queries stay unaudited.
- New doc `docs/security/audit-log.md` covers record shape, chain semantics, serialisation contract, in-memory limitations, production shape, wiring, failure semantics, and chain-verification procedure.
- 21 xunit cases in `tests/Kun.Audit.Tests` cover the hash matrix, concurrent chain serialisation (100-task storm), sink ordering/null-guards, behaviour success/exception paths, sink-failure non-propagation, consecutive-entry linking, and per-tenant chain isolation.
- Registered in `Kun.sln` under GUID `{A100000E-...}`; `src/AGENTS.md` / `README.md` module matrix / `docs/architecture/module-map.md` updated.

### Added — Kun.EntityFrameworkCore tenant-RLS interceptor (Phase 1 P1-CS-011)

- New `Kun.EntityFrameworkCore.Interceptors.TenantRlsConnectionInterceptor`: opt-in EF `DbConnectionInterceptor` that pushes `ITenantContext.TenantId` into the PostgreSQL session variable `app.current_tenant_id` on connection open and RESETs it on close. Gives ［下游的智能体产品］ a second isolation layer behind the existing EF global query filter when paired with RLS policies on the database schema.
- Activates only on Npgsql connections (nominal FQN check) so `Kun.EntityFrameworkCore` does not need a direct Npgsql package dependency; MySQL / SQL Server / other providers silently no-op.
- Tenant id is whitelisted against `^[A-Za-z0-9._-]{1,128}$` before inlining into the `SET` command (PostgreSQL `SET` cannot be parameterised); unsafe values throw `InvalidOperationException` rather than being coerced.
- Registered as Scoped by `AddKunEntityFrameworkCore`; callers opt in per `DbContextOptionsBuilder` via `opts.AddInterceptors(sp.GetRequiredService<TenantRlsConnectionInterceptor>())`.
- New doc `docs/security/multi-tenancy-rls.md` covers defence-in-depth rationale, policy template (ENABLE + FORCE ROW LEVEL SECURITY + USING + WITH CHECK), wiring, and ops notes.
- Three xunit integration cases in `tests/Kun.IntegrationTests/MultiTenancy/TenantRlsTests.cs` run against a live Postgres 16 container: RLS isolation proven *without* the EF filter (test entity deliberately omits `IMultiTenant`), cross-tenant write rejection via WITH CHECK, and whitelist rejection of unsafe tenant ids.

### Added — Kun.EventBus attribute-driven registration (Phase 1 P1-KN-003)

- New `[IntegrationEventId("bounded-context.event.v1")]` attribute so the stable token lives next to the event record instead of being repeated in DI wiring.
- `IEventTypeRegistrar.Register<TEvent>()` no-arg overload reads the attribute at DI-registration time. Marked `[RequiresUnreferencedCode]`; AOT-strict consumers can keep calling `Register<TEvent>(string)` to stay reflection-free. The resulting runtime registry is still the zero-reflection `FrozenDictionary`-backed `EventTypeRegistry`.
- Missing attribute yields an actionable `InvalidOperationException` pointing the caller at either decorating the type or using the explicit string overload.
- Samples (`Kun.Sample.Catalog.Contracts`, `Kun.Sample.Order.Contracts`) migrated to the attribute so the pattern is visible to framework users.
- 7 new xunit cases in `Kun.EventBus.Tests/IntegrationEventIdAttributeTests.cs` cover attribute validation, presence/absence, attribute-vs-string equivalence, and fluent chaining.

### Changed — Kun.Security.S2SJwt (Phase 1 P1-KN-002)

- `IS2SJwtIssuer.IssueAsync` now takes a single `S2SJwtIssueRequest` record. The prior 5-positional-parameter signature is removed (API was in `PublicAPI.Unshipped.txt` with no in-repo consumers besides tests).
- New claim emission, aligned with SAD v3.0 §12: `roles` (multi-valued), `membership_id`, `trace_id` — all optional via the request record. Pre-existing `sub`, `jti`, `tenant_id`, `scope`, `session_id` remain.
- Multi-audience tokens now fold `aud` into the payload as a JSON array (previously, multi-audience tokens leaked `aud` into the JWT *header* — correctness bug, now fixed).
- `S2SJwtOptions.DefaultLifetimeSeconds` default bumped from 300 → **900** (15 minutes) per SAD §12; individual calls can still override via `S2SJwtIssueRequest.Lifetime`.
- `docs/security/s2s-jwt.md` extended with a ［下游的智能体产品］ Set B section explaining why both `Kun.Gateway.S2sTokenIssuer` (RS256, 60s) and `Kun.Security.S2SJwt.S2SJwtIssuer` (ES256, 15min) coexist and when to use each.

### Added — Kun.Storage (Phase 1 P1-KN-001)

- **Kun.Storage** module (trim-safe, `IsAotCandidate=false` due to AWSSDK.S3 reflection):
  - `IFileStorage` seven-method contract: `PutObject` / `GetObject` / `HeadObject` / `DeleteObject` / `ListObjects` / `CreatePresignedUploadUrl` / `CreatePresignedDownloadUrl`.
  - Supporting types: `ObjectMetadata` / `ObjectSummary` / `ObjectDownload` (IAsyncDisposable) / `PutObjectOptions` / `PresignedUploadRequest+Url` / `PresignedDownloadRequest+Url`.
  - `ContentDispositionPolicy`: active-web MIME whitelist (HTML/SVG/XML/JS/WASM/XHTML/ECMAScript) + RFC 5987 attachment builder. Enforced at every write path and every presigned-download URL so user-uploaded content cannot render inline.
  - `S3FileStorage` (AWSSDK.S3, compatible with MinIO via path-style + custom endpoint). Auto-probes stored MIME on presigned GET to inject `ResponseContentDisposition` attachment override.
  - `LocalFileStorage`: 2-layer path-traversal defense (syntactic + semantic), `.kunmeta` JSON sidecar for metadata, HMAC-SHA256-signed `kun-local://` stub URLs for presigned ops (dev-only).
  - DI: `AddKunStorage` / `AddLocalStorage` / `AddS3Storage` with central `StorageOptions`.
- **Tests**:
  - `tests/Kun.Storage.Tests`: ContentDispositionPolicy matrix, LocalFileStorage round-trips + traversal + attachment enforcement, mocked-S3 unit tests, DI resolution tests.
  - `tests/Kun.IntegrationTests/Storage/S3FileStorageIntegrationTests.cs` + `Fixtures/MinioFixture.cs`: MinIO Testcontainer with real `HttpClient` round-trips against presigned URLs.
- **Docs**: new `src/Kun.Storage/AGENTS.md`, module added to `src/AGENTS.md`, README, `docs/architecture/module-map.md`; ［第一阶段差距盘点］ updated to mark P1-KN-001 complete.

### Merge — main → develop reconciliation (2026-04-25, `sync-main-into-develop`)

- Union merge across five design-divergent groups, arbitrated against `docs/whitepaper.md`:
  - **Kun.Audit** — writer/reader abstraction (develop) + hash-chain pipeline behaviour (main) coexist in one module. `AddKunAudit().UseEventBus()` and `AddAuditBehavior<TReq,TResp>()` both available.
  - **Kun.EventBus** — three `Register<TEvent>` overloads (string / string+JsonTypeInfo / [IntegrationEventId] attribute) all retained.
  - **Kun.EntityFrameworkCore** — `TenantRlsConnectionInterceptor` (connection-scoped) + `TenantRlsTransactionInterceptor` + `SchemaConsistencyInterceptor` all retained; consumers pick per-deployment.
  - **Kun.Security.S2SJwt** — dual `IssueAsync` overloads (record `S2SJwtIssueRequest` + legacy 5-param). `DefaultLifetimeSeconds` set to **300** per whitepaper §9.6 zero-trust TTL upper bound (60–300s).
  - **Kun.Storage** — main's 7-method `IFileStorage` contract wins (whitepaper §13.1; develop's variant lacked `RequiredHeaders` on presigned upload, making SigV4 S3 uploads unreliable, and had no response-Content-Disposition override for active-MIME attachment enforcement). develop's split-module skeleton `Kun.Storage.Abstractions` / `.Local` / `.S3` preserved for Phase 2 implementation; `src/Kun.Storage/` old legacy files removed.
- **P1-KN-002 TTL correction** — the main-side note about `DefaultLifetimeSeconds` bumping to 900 is superseded: per whitepaper §9.6 ("TTL 控制在 60 秒到 5 分钟"), default is 300 seconds. Explicit callers may still override via `S2SJwtIssueRequest.Lifetime`.
- Pre-push hook installed at `.git/hooks/pre-push` to refuse direct pushes to `main`.

### Fixed

- **Kun.Gateway**: `S2sTokenIssuer` now injects `IHostEnvironment` and throws `InvalidOperationException` when the signing key file is missing in non-Development environments, preventing silent ephemeral-key self-signing in production.
- **Kun.Gateway**: `AddKunGateway` without a `reverseProxyConfig` section now registers a startup-time `IHostedService` that fails fast with a user-friendly message if no `IProxyConfigProvider` is registered, instead of deferring the error to the first proxied request.
- **Kun.Gateway.Tests**: regression tests added for production key-file safety guard, startup config validation, and double-registration behavior.

- **Kun.Saga**: registered `ISystemClock` in `AddKunSaga` (required by
  `SagaOrchestrator`); `SagaStepExecutor` now persists state via
  `DbContext.SaveChangesAsync` after each entry; forward execution advances
  step index and marks completed; reverse compensation walks back to 0 and
  marks compensated; `ISagaOrchestrator` moved from Singleton to Scoped to
  match its Scoped dependency `ISagaInstanceStore`.

### Added — M1 交付（阶段一：奠基）

**Kun.Core** (v1.1)
- `Domain/DomainException`：带 `Code` 字段的领域异常基类
- `Domain/IDomainEventDispatcher`：同进程内领域事件分发接口
- `MultiTenancy/NullTenantContext`：非多租户默认实现
- `Security/AnonymousCurrentUser`：未认证默认实现
- `Time/ISystemClock` + `Time/SystemClock`：可注入时钟，默认由 `TimeProvider.System` 支持
- 为已有公开类型（AggregateRoot / Entity / ValueObject / Result / Error / Auditing 标记）补齐 XML 注释
- 单元测试 32/32（Domain + Results）

**Kun.Mediator** (v1.2)
- 核心接口：`ISender`、`IPublisher`、`INotification`、`INotificationHandler<T>`、`IPipelineBehavior<TRequest, TResponse>`、`RequestHandlerDelegate<TResponse>`
- `KunMediator`：基于 `FrozenDictionary<Type, RequestHandlerBase>` 的 O(1) 无反射调度
- `KunMediatorBuilder`：手工注册 API —— `AddRequestHandler<TReq, TResp, THandler>()`、`AddPipelineBehavior<TReq, TResp, TBehavior>()`、`AddNotificationHandler<TNotification, THandler>()`
- `DependencyInjection/AddKunMediator` DI 入口
- 内置 Behavior：`LoggingBehavior`、`PerformanceBehavior`（500ms 阈值）、`TracingBehavior`（`ActivitySource("Kun.Mediator")`）
- AOT 安全：所有泛型类型参数带 `[DynamicallyAccessedMembers(PublicConstructors)]`，构建时零 trim 警告
- 单元测试 8/8（Dispatch + Pipeline 洋葱顺序）

**Kun.Validation** (v1.3)
- `IValidator<T>` + `ValidationResult` + `ValidationError` + `ValidationException`
- `Behaviors/ValidationPipelineBehavior<TReq, TResp>`：聚合所有验证器，invalid 时抛 `ValidationException`
- `DependencyInjection/KunValidationServiceCollectionExtensions`：`AddValidator<T, TValidator>()` + `AddValidationBehavior<TReq, TResp>()`
- 单元测试 12/12

**Kun.Sample.Minimal**（新增）
- AOT 候选的控制台样例，演示 Command / Query / Notification 经 `KunMediator` 的完整调度链路
- 附带 `ActivityListener` 观察 `TracingBehavior` 输出
- `dotnet run` 本地验证通过

### Added — 基础设施
- 项目初始化：13 个核心模块骨架
- 白皮书、贡献指南、6 个 Claude 专属 Skills
- `Directory.Build.props` / `Directory.Packages.props` 集中化构建与依赖管理
- CI 工作流：build、test、AOT 发布验证、layering 检查、dotnet format
- `Microsoft.CodeAnalysis.PublicApiAnalyzers` 接入 Kun.Core / Kun.Mediator / Kun.Infrastructure / Kun.EventBus / Kun.Validation
- `scripts/check-layering.sh`：强制 Kun.Core 无第三方 PackageReference
- `CLAUDE.md` 顶层 + 30 个 `AGENTS.md` 分层导航文档
- `Version`：`VersionPrefix=0.1.0`，本地 `VersionSuffix=alpha.1`
- `develop` 分支作为开发主干

### Changed
- `Kun.AspNetCore` / `Kun.Grpc` csproj：从 `Microsoft.NET.Sdk.Web` 修正为 `Microsoft.NET.Sdk`（它们是库，不是 Host）
- `PublicAPI.Unshipped.txt` → `PublicAPI.Shipped.txt`：Kun.Core (137)、Kun.Mediator (32)、Kun.Validation (25) 的 API 冻结入库

### Deferred（M1 已决定延后的项，详见 docs/roadmap.md）
- `Kun.Mediator.SourceGenerator` / `Kun.Validation.SourceGenerator`：当前用手工注册 API 替代，SG 作为 stretch goal（ralplan ADR fallback 路径）
- `Kun.Validation` 属性包（`[Required]`、`[MaxLength]` 等）：无 SG 时属性无运行时意义，与 SG 一同回归
- Result-aware 验证 Behavior（返回 `Result.Fail(Error.Validation(...))`）：可在核心抛异常 Behavior 之上封装，follow-up
- AOT 启动时间 benchmark：本地 Windows 缺 C++ linker，延至 CI 或用户本地 Linux 跑

### Added — M2 交付（阶段二：持久化双引擎）

**Kun.EventBus** (v2.1, AOT-candidate abstractions)
- `OutboxMessage`：富领域模型，含 `MarkPublished` / `MarkFailed` / `IncrementAttempt` 状态转换方法
- `OutboxStatus`：`Pending` / `Published` / `Failed` 枚举
- `IntegrationEventBuffer`：scoped 累加器，在 `SaveChangesAsync` 前收集待发事件
- `EventTypeRegistry`：`FrozenDictionary<string, Type>` 支持的 O(1) 稳定事件类型查找；`IEventTypeRegistry` / `IEventTypeRegistrar` 接口
- `IIntegrationEventHandler<TEvent>` + `MessageContext`：消费端处理器契约
- `IIntegrationEventPublisher`：每条消息的 broker 传输抽象
- `IProcessedMessageStore`：幂等消费存储抽象；`InMemoryProcessedMessageStore`（测试用）、`RedisProcessedMessageStore`（SETNX + EX，键格式 `idemp:{consumer}:{messageId}`）
- `IOutboxMessageStore`：持久层扫描抽象
- `OutboxPublisherBackgroundService`：BackgroundService，可配置批量大小、轮询间隔、最大重试次数、指数退避；每条消息启动 `ActivitySource("Kun.EventBus.Outbox")` span，从 `TraceParent`/`TraceState` 列恢复父 trace
- `OutboxPublisherOptions`：BatchSize=50 / PollInterval=2s / MaxAttempts=5 / RetryBaseDelay=1s
- DI 构建器：`KunEventBusBuilder`（`.UseInMemoryIdempotency()` / `.UseRedisIdempotency()`）、`KunEventBusServiceCollectionExtensions`、`KunOutboxPublisherExtensions`
- 单元测试 11/11

**Kun.EntityFrameworkCore** (v2.2, trim-safe, IsAotCandidate=false)
- `KunDbContext`：`IMultiTenant` 全局查询过滤器 + `ISoftDelete` 软删除过滤器
- `OutboxMessageConfiguration`：EF Core `IEntityTypeConfiguration<OutboxMessage>`，含索引
- `EfUnitOfWork`：单次 `SaveChangesAsync` 原子提交聚合变更 + Outbox 行 + 领域事件分发
- `IntegrationEventOutboxAccumulator`：W3C `TraceParent`/`TraceState` 捕获（via `Activity.Current`）
- `UnitOfWorkPipelineBehavior`：Mediator pipeline behavior，写命令专用
- `NoOpDomainEventDispatcher`：默认无操作实现；`MediatorDomainEventDispatcher` 由样例服务提供
- `IUnitOfWork` 接口
- DI：`KunEntityFrameworkCoreServiceCollectionExtensions`
- 单元测试集成（Testcontainers PostgreSQL，Docker 不可用时自动跳过）

**Kun.Dapper** (v2.3, AOT-candidate)
- `IReadDbConnectionFactory`：读端连接工厂抽象；调用方负责 dispose
- `PageRequest`：1-based 页码 + `Offset` 计算属性
- `PagedResult<T>`：`TotalPages` / `HasNextPage` / `HasPreviousPage` + `Empty(PageRequest)` 工厂方法
- `ReadDbOptions`：`ConnectionString` 配置
- DI：`KunDapperServiceCollectionExtensions.AddKunDapper`
- 单元测试 10/10

### Added — M3 交付（阶段三：消息总线）

**Kun.EventBus** (v3.x 追加)
- `IIntegrationEventPublisher`：每条消息的 broker 传输接口（从 M2 正式确定契约）
- `IProcessedMessageStore` 完整实现（InMemory + Redis），含幂等键格式文档

**Kun.EventBus.RabbitMQ** (AOT-candidate，RabbitMQ.Client 7.x)
- `RabbitMqOptions`：ConnectionString / ExchangeName / ConsumerName / PrefetchCount / MaxDeliveryAttempts / IdempotencyTtl
- `RabbitMqConnectionProvider`：单例懒初始化连接，`AutomaticRecoveryEnabled=true`，`IAsyncDisposable`
- `RabbitMqIntegrationEventPublisher`：`IIntegrationEventPublisher` 实现；topic exchange；headers 含 traceparent/tracestate/tenantId/messageId/occurredAt；持久化消息
- `RabbitMqIntegrationEventSubscriber`：BackgroundService；`AsyncEventingBasicConsumer`；per-type durable 队列 + binding；prefetch；W3C trace 恢复；x-death 计数 → DLX
- `IdempotentConsumerDecorator`：handler 成功后调用 `IProcessedMessageStore.TryMarkProcessedAsync`
- `DeadLetterHandler`：`{exchange}.dlx` fanout + `{consumer}.dead-letter` 队列
- `ISubscriptionDispatcher` + `TypedSubscriptionDispatcher<TEvent>`：AOT 安全有类型调度，无 `MakeGenericType`
- `KunRabbitMqBuilder`：`.Subscribe<TEvent, THandler>(eventTypeId, typeInfo)` 流式注册
- `KunRabbitMqServiceCollectionExtensions.AddRabbitMq`
- 单元测试新增 6 个，总计 79/79 通过
- M3 3.4（Testcontainers.RabbitMq 轮转测试）延后至 CI

### Added — M4 交付（阶段四：Saga 编排器）

**Kun.Saga** (AOT-candidate)
- 抽象：`ISagaContext` / `ISaga<TContext>` / `ISagaStep<TContext>` / `ISagaOrchestrator`
- 异常：`SagaStepFailedException` / `SagaNotFoundException`
- 持久化：`SagaInstance`（富状态转换：`StartCompensating` / `Complete` / `Fail`）、`SagaStepQueueEntry`、`SagaState`（Running/Compensating/Completed/Compensated/Failed）、`StepDirection`（Execute/Compensate）
- `ISagaInstanceStore`：持久层抽象；`EfSagaInstanceStore`：EF Core 实现，含 `(state, timeout_at)` 和 `(processed_at, enqueued_at)` 索引
- `SagaOrchestrator`：原子 start + 首步 enqueue（与触发命令同一 EF Core 事务）
- `SagaStepExecutor`：BackgroundService，正向执行 + 逆序补偿；`ActivitySource("Kun.Saga")`，span 名 `saga.step.execute` / `saga.step.compensate`，tags: `saga.step_index` / `saga.saga_id` / `saga.direction` / `saga.name`
- `SagaTimeoutDetector`：BackgroundService，每 5 秒扫描超时实例并触发补偿
- `SagaRegistry`：`FrozenDictionary` + 闭包泛型委托，AOT 安全，无 `MakeGenericType`
- DI：`KunSagaBuilder` + `AddKunSaga` 扩展
- 单元测试 18/18

### Added — M5 交付（阶段五：宿主层、RPC 与安全网关）

**Kun.Security** (M5.1, AOT-candidate)
- `S2sTokenOptions`：Issuer / Audience（默认 `kun-internal`）/ Lifetime（默认 60s）/ SigningAlgorithm（默认 RS256，HS256 明确禁止）/ JwksUri / ValidIssuers
- `IKunJwksProvider` + `FileKunJwksProvider`：文件监视 JWKS 提供者
- `KunServiceAuthenticationExtensions.AddKunServiceAuthentication`：JwtBearer 验证，`ValidAlgorithms=["RS256","ES256"]`，HS256 被拒绝
- `JwtTenantContext`：从 `ClaimsPrincipal` 提取 `tenant_id` 实现 `ITenantContext`
- `JwtCurrentUser`：从 `ClaimsPrincipal` 提取 `sub` / `name` / `roles` 实现 `ICurrentUser`
- `PropagateS2sTokenHandler`：`DelegatingHandler`，将入站 `Authorization` 头复制到出站请求
- DI：`AddKunSecurity`（注册 `ITenantContext` + `ICurrentUser`）/ `HttpClientBuilder.AddKunServiceAuth`
- 单元测试 7/7（valid、HS256 拒绝、wrong aud、expired、key rotation）

**Kun.AspNetCore** (M5.2, AOT-candidate)
- `KunEndpointRouteBuilderExtensions`：`MapCommand<TCommand, TResp>` + `MapQuery<TQuery, TResp>`（ISender glue）
- `ErrorToProblemDetailsExtensions`：`Error.ToProblemDetails()` 映射（Validation→400 / NotFound→404 / Conflict→409 / Unauthorized→401 / Forbidden→403）
- `KunExceptionHandler`：`DomainException` → 400 ProblemDetails
- `KunHealthChecksExtensions`：健康检查注册
- DI：`KunAspNetCoreServiceCollectionExtensions`
- 单元测试 11/11

**Kun.Grpc** (M5.3, AOT-candidate)
- `KunResultToRpcInterceptor`：`ResultFailureException` → `RpcException` 状态码映射；`Error.Code` 复制到 trailers
- `KunServerAuthInterceptor`：强制 `HttpContext.User` 已认证且含 `tenant_id` claim
- `KunTracingInterceptor`：`ActivitySource("Kun.Grpc")` span，`rpc.system=grpc` / `rpc.method` tags；拦截器顺序：Tracing → Auth → ResultMapping
- DI：`AddKunGrpc` 链式注册三个拦截器
- 单元测试 13/13

**Kun.Gateway** (M5.4, AOT-candidate，Web Host)
- `KunGatewayOptions`：ExternalIssuer / ExternalAudience / ExternalJwksUri / InternalIssuer / InternalAudience / InternalTokenLifetime(60s) / SigningKeyPath / PublicKeyPath
- `S2sTokenIssuer`：加载 PEM RSA 私钥，铸造 RS256 JWT，复制 `sub` / `tenant_id` / `roles` 声明
- `S2sAuthTransform`：YARP `RequestTransform`，将 Authorization 头替换为内部 token
- `JwksEndpoint.MapKunJwks`：`/.well-known/jwks.json`，`kid` = SHA-256(modulus) 前 16 字符，AOT 安全（`JwksJsonContext`）
- `IRateLimitPartitionResolver` + `TenantRateLimitPartitionResolver`（claim-based）+ `IpRateLimitPartitionResolver`（X-Forwarded-For aware）
- `KunRateLimiterExtensions`：固定窗口 100 req/min，可插拔分区解析器
- DI：`AddKunGateway` 配置 YARP + S2S issuer + transform
- `Program.cs` 组合根 + `appsettings.json` 骨架
- 单元测试 12/12
- M5 5.5 合约测试（Docker 依赖，延后）

全解决方案：0 错误，0 警告，140/140 测试通过 + 2 集成跳过

### Added — M6 交付（阶段六：示例应用）

**Kun.Sample.Catalog** (M6.1)
- 领域：`Product` AggregateRoot，`Create` / `ChangePrice` / `AdjustStock` 触发 `ProductCreatedDomainEvent` / `PriceChangedDomainEvent` / `StockChangedDomainEvent`
- 契约：`ProductPublishedIntegrationEvent`（稳定 ID `product.published.v1`）/ `InventoryChangedIntegrationEvent`（`inventory.changed.v1`）
- 应用层：`CreateProductCommand` + 验证器 + Handler；`AdjustStockCommand`；`GetProductByIdQuery`（Dapper 读端）；`ProductCreatedHandler`（领域事件 → `IIntegrationEventBus` 桥接）
- 基础设施：`CatalogDbContext`（继承 `KunDbContext`）/ `ProductConfiguration` / `NpgsqlReadDbConnectionFactory` / `MediatorDomainEventDispatcher`
- API：`Program.cs` 组合根；`EnsureCreated` 启动引导；`POST /products` / `GET /products/{id}` / `/health`

**Kun.Sample.Order** (M6.2, Saga 展示)
- 领域：`Order` AggregateRoot，`MarkReserved` / `Paid` / `Shipped` / `Completed` + `Cancel`；`OrderLine` owned value object；`OrderPlaced` / `OrderCancelled` 领域事件
- Saga：`OrderCreationSaga`（4 步，`DefaultStepTimeout=30s`）
  - `ReserveInventoryStep` → `ProcessPaymentStep` → `ShipOrderStep` → `NotifyCustomerStep`
  - 每步含 `ExecuteAsync` + 幂等 `CompensateAsync`
  - `ProcessPaymentStep` 在 `SimulateFailure=="payment"` 时抛异常（演示补偿）
  - `ShipOrderStep` 在 `SimulateFailure=="timeout"` 时 sleep 60s（演示超时）
- 契约：`OrderCompletedIntegrationEvent`（`order.completed.v1`）
- 应用层：`PlaceOrderCommand` / `GetOrderById` / `GetSagaStatus` 查询
- 基础设施：`OrderDbContext` 应用 `ApplyKunSagaConfiguration`；`EfOutboxMessageStore`
- API Endpoints：`POST /demo/happy-path` / `POST /demo/trigger-payment-failure` / `POST /demo/trigger-timeout` / `GET /demo/sagas/{id}`

**Kun.Sample.AppHost** (M6.3, .NET Aspire 9.0)
- 编排：PostgreSQL（+pgAdmin）/ RabbitMQ（+management plugin）/ Redis / Catalog / Order / Gateway
- `WaitFor` 依赖链：各服务等待 postgres + rabbitmq 就绪后启动
- Gateway 引用 catalog + order 用于 YARP 服务发现
- 单命令启动：`dotnet run --project samples/Kun.Sample.AppHost`

**Kun.Sample.Shared** (M6.4, IsAotCandidate=false)
- `OpenTelemetryExtensions.AddKunObservability(serviceName)`：
  - Tracing：ActivitySources `Kun.Mediator` / `Kun.EventBus.Outbox` / `Kun.Grpc` / `Kun.Saga` + AspNetCore + HttpClient + gRPC client
  - Metrics：AspNetCore + HttpClient
  - OTLP exporter（默认指向 Aspire Dashboard collector）
- 框架模块（`src/Kun.*`）保持无 OTel 包依赖；OTel 仅在样例组合根注入
- Gateway 未注入可观测性（AOT-candidate，OTel 包尚未 AOT 安全）
- `OpenTelemetry.Api` 升级至 1.11.x（修复 GHSA-8785-wc3w-h8q6）

### Changed（M2–M6 期间）
- `Directory.Packages.props`：新增 Dapper 2.1.35 / RabbitMQ.Client 7.x / OpenTelemetry 1.11.x / YARP 等包版本
- `Kun.AspNetCore` / `Kun.Grpc` csproj：`IsAotCandidate=true` 确认（库，非 Host）
- `Kun.EntityFrameworkCore` csproj：`IsAotCandidate=false`（trim-safe but not AOT）
- `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`：Kun.Dapper / Kun.EventBus / Kun.EntityFrameworkCore / Kun.Saga / Kun.Security / Kun.Gateway / Kun.Grpc 接入 RS0016 公开 API 追踪

### Deferred（M2–M6 已决定延后的项）
- Source Generators（Mediator / Validation / Saga）：按 ADR fallback 路径继续使用手工注册
- Testcontainers 集成测试（M2 2.4 / M3 3.4 / M5 5.5）：Docker 依赖，CI 环境运行，本地跳过
- `AddKunDapper` API rework：`ReadDbOptions.ConnectionString` 为 `required init`，消费方需在回调内设置（当前 API 略显冗余）
- `IReadDbConnectionFactory` 默认 Npgsql 实现：框架不内置，由消费方提供（参见样例 `NpgsqlReadDbConnectionFactory`）
- `MapQuery` nullable TResponse via JsonTypeInfo：类型系统限制，follow-up
- Gateway 可观测性：OTel 包达到 AOT parity 后追加
- NuGet 发布仪式（手动，TBD）

## [0.1.0-phase1] - 2026-04-23

Phase 1 首个可对［下游的智能体产品］集成发版。10 个新模块落地（Storage / Identity /
MultiTenancy / Audit / RateLimit / Caching / Observability / Security / Configuration /
Sandbox），全仓 AOT 一致性通过审计，Directory.Build.props 统一 AOT 策略，CI 新增
`aot-audit` 矩阵 job 与 `sample-aot-integration` job。

[Unreleased]: https://github.com/CavaiLAB/kun/compare/v2026.10.25...HEAD
[2026.10.25]: https://github.com/CavaiLAB/kun/compare/v2026.10.23...v2026.10.25
[2026.10.23]: https://github.com/CavaiLAB/kun/compare/v2026.10.14...v2026.10.23
[2026.10.14]: https://github.com/CavaiLAB/kun/compare/v2026.10.13...v2026.10.14
[2026.10.13]: https://github.com/CavaiLAB/kun/compare/v2026.10.12-hotfix.3...v2026.10.13
[2026.10.12-hotfix.3]: https://github.com/CavaiLAB/kun/compare/v2026.10.12-hotfix.2...v2026.10.12-hotfix.3
[2026.10.12-hotfix.2]: https://github.com/CavaiLAB/kun/compare/v2026.10.12...v2026.10.12-hotfix.2
[2026.10.12]: https://github.com/CavaiLAB/kun/compare/v2026.10.11...v2026.10.12
[2026.10.11]: https://github.com/CavaiLAB/kun/compare/v2026.10.9...v2026.10.11
[2026.10.9]: https://github.com/CavaiLAB/kun/compare/v2026.10.8...v2026.10.9
[2026.10.8]: https://github.com/CavaiLAB/kun/compare/v2026.9.13...v2026.10.8
[2026.9.13]: https://github.com/CavaiLAB/kun/compare/v2026.6.12...v2026.9.13
[0.1.0-phase1]: https://github.com/CavaiLAB/kun/tree/741c68caa6d6a1c0ef9cb016554258ef2fff1d20
