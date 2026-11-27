> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.EventBus

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Namespaces

[Kun.EventBus.Background](/sdk/api/kun/kun-eventbus-background/index.md)

[Kun.EventBus.DependencyInjection](/sdk/api/kun/kun-eventbus-dependencyinjection/index.md)

[Kun.EventBus.EntityFrameworkCore](/sdk/api/kun/kun-eventbus-entityframeworkcore/index.md)

[Kun.EventBus.Idempotency](/sdk/api/kun/kun-eventbus-idempotency/index.md)

[Kun.EventBus.Outbox](/sdk/api/kun/kun-eventbus-outbox/index.md)

[Kun.EventBus.RabbitMQ](/sdk/api/kun/kun-eventbus-rabbitmq/index.md)

### Classes

[EventActor](/sdk/api/kun/kun-eventbus/eventactor.md)

触发事件的主体；取值与 S2S principal_kind、审计 actor_type 一致。

[EventEnvelopeScope](/sdk/api/kun/kun-eventbus/eventenvelopescope.md)

当前异步流的事件来源。集成事件只能在命令 handler 的同一作用域内发出（ADR-008 D8.2）：
SDK 命令管道在执行 handler 与保存期间打开它，Outbox 写入端缺少来源时拒绝发布。
定时作业、生命周期控制器等不经命令管道的平台发布方显式以服务主体打开。

[EventOrigin](/sdk/api/kun/kun-eventbus/eventorigin.md)

集成事件的来源：谁、以哪个组件的名义、因哪一次调用发布。信封的 producer、actor 与因果链取自这里。

[EventSubscriptionDescriptor](/sdk/api/kun/kun-eventbus/eventsubscriptiondescriptor.md)

订阅目录中的一项，与 [IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md) 按事件 ID 对齐，是事件目录「订阅者」的来源。

[EventTypeRegistry](/sdk/api/kun/kun-eventbus/eventtyperegistry.md)

Immutable, O(1) event-type registry backed by [`FrozenDictionary`](https://learn.microsoft.com/dotnet/api/system.collections.frozen.frozendictionary-2).

[EventsDefinition](/sdk/api/kun/kun-eventbus/eventsdefinition.md)

事件 seam 的稳定服务键；业务组件只依赖 Definition。

[EventsErrors](/sdk/api/kun/kun-eventbus/eventserrors.md)

事件组件的稳定错误码定义。

[IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md)

事件目录中的一项：稳定 ID、subject、payload 版本与由 JsonTypeInfo 导出的 payload Schema。

[IntegrationEventIdAttribute](/sdk/api/kun/kun-eventbus/integrationeventidattribute.md)

Declares the stable event-type identifier (token) for an integration event CLR type.
Use with [IEventTypeRegistrar.Register](/sdk/api/kun/kun-eventbus/ieventtyperegistrar.md#Kun_EventBus_IEventTypeRegistrar_Register__1) so the token lives next
to the type definition rather than being repeated in DI wiring.

[IntegrationEventSchemas](/sdk/api/kun/kun-eventbus/integrationeventschemas.md)

由登记时的源生成 JsonTypeInfo 导出 payload 的 JSON Schema（2020-12），与 Outbox 实际写出的 JSON 同一份契约；
不用反射序列化补位：没有 JsonTypeInfo 的事件不能发布，也不进目录。

[IntegrationEventTypes](/sdk/api/kun/kun-eventbus/integrationeventtypes.md)

稳定事件 ID（`[IntegrationEventId]` 的值）与信封 subject 的换算规则。

[KunEventDelivery](/sdk/api/kun/kun-eventbus/kuneventdelivery.md)

订阅器正在派发的一次投递：已解析、已与 AMQP 属性核对过的 v1 信封，以及订阅与重试计数。
订阅器为每条消息新建一个依赖注入作用域，handler 可在构造函数里注入本类型读取 producer、actor 与因果链；
幂等键、发生时间与租户一律以这里的信封为准，不读载荷里的同名字段（S1-DEC-020）。
只能在订阅器派发的作用域内解析，其他位置解析即抛 EVENTS_MISCONFIGURED。

[KunEventEnvelope](/sdk/api/kun/kun-eventbus/kuneventenvelope.md)

跨进程集成事件信封 v1，与 contracts/v0/event-envelope.schema.json 同形：Outbox 行与 RabbitMQ 消息体都是这份 JSON。
payload 由事件目录里该 [KunEventEnvelope.Type](/sdk/api/kun/kun-eventbus/kuneventenvelope.md#Kun_EventBus_KunEventEnvelope_Type) 的 Schema 约束，信封不约束其内容；进程内运行时事件不用本信封。

[KunEventTopology](/sdk/api/kun/kun-eventbus/kuneventtopology.md)

RabbitMQ 拓扑命名（PRD 7.10）：topic exchange `qian.events`，路由键为信封 subject；
竞争消费用每服务一个 quorum 队列 `q.<service>.<subscription>`，广播用每实例一个自动删除队列 `q.<service>.<instance>`。
消费端另有每订阅一个死信队列 `q.<service>.<subscription>-dlq` 与按次数、间隔命名的重试队列 `<主队列>-retry-<n>-<毫秒>`；
这两种后缀保留给消费端，订阅名与实例名不能以它们结尾。

[KunEventsException](/sdk/api/kun/kun-eventbus/kuneventsexception.md)

事件装配、发布或消费被拒绝；携带可渲染为 ErrorDetail 的错误。

[KunEventsExtensions](/sdk/api/kun/kun-eventbus/kuneventsextensions.md)

Registers the event envelope writer and event catalog.

[MessageContext](/sdk/api/kun/kun-eventbus/messagecontext.md)

Metadata accompanying an integration event during handler dispatch.

### Interfaces

[IEventTypeRegistrar](/sdk/api/kun/kun-eventbus/ieventtyperegistrar.md)

Fluent builder for populating the [IEventTypeRegistry](/sdk/api/kun/kun-eventbus/ieventtyperegistry.md).

[IEventTypeRegistry](/sdk/api/kun/kun-eventbus/ieventtyperegistry.md)

Maps stable event-type tokens to their CLR types at runtime.

[IEventsService](/sdk/api/kun/kun-eventbus/ieventsservice.md)

事件 Provider 的只读视图。发布不经过这里：命令 handler 通过同一数据会话的 Outbox 写入端发布，
信封由 Provider 在入队时按 [EventEnvelopeScope](/sdk/api/kun/kun-eventbus/eventenvelopescope.md) 生成。

[IIntegrationEventBus](/sdk/api/kun/kun-eventbus/iintegrationeventbus.md)

High-level façade for publishing integration events via the Outbox pattern.

[IIntegrationEventHandler\<TEvent>](/sdk/api/kun/kun-eventbus/iintegrationeventhandler-1.md)

Handles a strongly-typed integration event received from the broker.

[IIntegrationEventPublisher](/sdk/api/kun/kun-eventbus/iintegrationeventpublisher.md)

Broker-specific transport that publishes a single [OutboxMessage](/sdk/api/kun/kun-eventbus-outbox/outboxmessage.md) to the message bus.
Implementations live in broker adapters (e.g. `Kun.EventBus.RabbitMQ`).

[IOutboxPublisher](/sdk/api/kun/kun-eventbus/ioutboxpublisher.md)

Dispatches pending outbox messages to the underlying broker.

### Enums

[EventActorKind](/sdk/api/kun/kun-eventbus/eventactorkind.md)

事件主体类型；agent 为 S4 Agent 触发器预留，S1 已可写入信封。

[EventSubscriptionMode](/sdk/api/kun/kun-eventbus/eventsubscriptionmode.md)

订阅模式（PRD 7.10、ADR-008 D8.9）。
