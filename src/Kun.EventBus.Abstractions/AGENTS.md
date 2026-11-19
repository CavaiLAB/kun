# Kun.EventBus.Abstractions

- 本包是事件契约入口，只允许依赖 BCL 与内核档的 Kun.Core、Kun.Runtime（经它带进 Kun.Errors 与 Microsoft.Extensions 的依赖注入、日志抽象）；禁止引入传输、Redis、EF Core 或后台宿主依赖，也不引任何 Provider。
- 保持 `Kun.EventBus` 中既有类型的命名空间与签名；迁过来的类型由 `Kun.EventBus` 程序集做类型转发。
- IIntegrationEvent 复用 Kun.Core.Domain 中的唯一类型，不创建第二套标记，也不建立 Core 的反向依赖。
- 权威设计为 PRD §7.10、ADR-008、S1-EVT-001 与 S1-EVT-002；主文档为 README.md。信封形状以 `_import/qian/contracts/v0/event-envelope.schema.json` 为准，改字段必须同时改 Schema、fixture 与 Kun.EventBus.IntegrationTests 的往返用例。
- 消费端契约（投递上下文、订阅模式与订阅目录）不引用 handler 接口，受 `IIntegrationEventHandler<TEvent>` 约束的 Subscribe 入口放在 Provider。
- 队列命名的保留后缀（-dlq、-retry-<n>-<毫秒>）不能放宽，否则订阅名会与别的订阅的死信或重试队列同名。
- 事件来源只能由可信执行端（SDK 命令管道、宿主后台）开启，不从请求输入读取 producer 或 actor。
- 公共 API 登记 PublicAPI 文件；注册与序列化优先使用源生成 JsonTypeInfo；新增错误码在本包定义，遵守统一 ErrorDetail。
