> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.EventBus.RabbitMQ

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.RabbitMQ` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.RabbitMQ`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.RabbitMQ)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[KunEventsRabbitMqExtensions](/sdk/api/kun/kun-eventbus-rabbitmq/kuneventsrabbitmqextensions.md)

Registers the RabbitMQ envelope publisher and subscribers.

[KunEventsSubscriberBuilder](/sdk/api/kun/kun-eventbus-rabbitmq/kuneventssubscriberbuilder.md)

订阅登记（PRD 7.10 的 `Subscribe<TEvent, THandler>()`）：每次登记把一个 Kun handler 挂到一个订阅上。
同一订阅可以覆盖多个事件，但一个事件在同一订阅里只有一个 handler；广播订阅共用实例队列，同一事件在一个服务里只能有一个广播订阅。

[KunEventsSubscriberOptions](/sdk/api/kun/kun-eventbus-rabbitmq/kuneventssubscriberoptions.md)

消费端的宿主级设置。重试语义与 Outbox 投递侧不同：这里的 [KunEventsSubscriberOptions.MaxRetries](/sdk/api/kun/kun-eventbus-rabbitmq/kuneventssubscriberoptions.md#Kun_EventBus_RabbitMQ_KunEventsSubscriberOptions_MaxRetries) 是首投之外的重试次数，
默认首投 1 次 + 重试 5 次，第 6 次仍失败即进死信队列。
