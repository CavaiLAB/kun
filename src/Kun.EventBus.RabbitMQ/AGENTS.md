# Kun.EventBus.RabbitMQ

## 发布与订阅

`AddKunEventsBroker` 登记发布端与独立的消费连接配置，`AddKunEventsSubscriber` 以服务名、订阅名和具体 handler 类型登记订阅。宿主使用源生成的 JSON 元数据登记事件。投递端核对 Outbox 行与信封的 ID、类型及租户，以信封的 subject 为路由键，在 broker 确认后返回。

- `RabbitMqBroker.cs` 管理发布连接和确认通道，定义交换机、重试队列、死信队列与绑定。
- `Publishing/KunEnvelopePublisher.cs` 从已校验的信封生成 AMQP 属性与消息体。
- `Consuming/KunEventSubscriber.cs` 处理消息、恢复租户和 trace、登记幂等记录。每条投递捕获自己的 `ConsumeSession`；关闭通道之前先等已开始写的确认帧完成。
- `Consuming/KunEventsSubscriberBuilder.cs` 将已登记事件绑定到具体 handler。
- 行为变更须复核 `Kun.IntegrationTests` 的停机、重连、确认、版本分发用例以及 `Kun.EventBus.IntegrationTests` 的投递、幂等和死信用例。

## Dependencies

### Internal

- `Kun.EventBus` — `IIntegrationEventPublisher`, `OutboxMessage`, `IEventTypeRegistry`, `IProcessedMessageStore`, `IIntegrationEventHandler<T>`, `MessageContext`

### External

- `RabbitMQ.Client` 7.x (async-native, AOT-compatible)
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Hosting.Abstractions`
- `Microsoft.Extensions.Logging.Abstractions`
- `Microsoft.Extensions.Options`

## 信封订阅器

- 消费端以「handler 成功并写入幂等记录后才确认」为前提：重试一律复制进重试队列并在副本确认后确认原消息，不靠 nack 放回；重试队列经默认 exchange 回原队列，不经 qian.events 回投；失败分类与计数口径见 README「失败分类」，改动要同步测试。
- 重试或死信副本转发失败时原地退避重发、不放回原消息（quorum 队列的 nack 放回不计入投递上限，放回会让 handler 无上限地重跑），连续失败后 nack 不放回交 broker 死信；改动要同步 SubscriberTests 的转发失败用例。
- 租户只取已校验信封的 tenant_id，不读载荷；每条消息在全新作用域里进入，进入后核对当前租户等于信封租户（R122），消费循环不继承宿主启动时的执行上下文。
