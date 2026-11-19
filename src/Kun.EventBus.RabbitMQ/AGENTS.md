# Kun.EventBus.RabbitMQ

## Purpose

RabbitMQ broker adapter implementing `IIntegrationEventPublisher`. Publishes Outbox-drained messages to a topic exchange, attaches W3C Trace Context headers, and provides a consumer background service (`RabbitMqIntegrationEventSubscriber`) that dispatches incoming messages through `IdempotentConsumerDecorator` to `IIntegrationEventHandler<TEvent>` implementations.

## Key Files

| File | Description |
|------|-------------|
| `Kun.EventBus.RabbitMQ.csproj` | `IsAotCompatible=true`; references `RabbitMQ.Client` 7.x, `Microsoft.Extensions.*` |
| `RabbitMqConnectionProvider.cs` | Singleton lazy-open `IConnection` with `AutomaticRecoveryEnabled=true`; `IAsyncDisposable` |
| `Options/RabbitMqOptions.cs` | `ConnectionString`, `ExchangeName`, `ConsumerName`, `PrefetchCount`, `MaxDeliveryAttempts`, `IdempotencyTtl` |
| `Publishing/RabbitMqIntegrationEventPublisher.cs` | Implements `IIntegrationEventPublisher`; opens a channel per publish; declares exchange; writes trace/tenant headers |
| `Consuming/RabbitMqIntegrationEventSubscriber.cs` | `BackgroundService`; declares queues, DLX/DLQ; registers `AsyncEventingBasicConsumer` per subscription |
| `Consuming/ConsumeSession.cs` | One consume session's channel plus the gate between settling and closing: once the close starts, nothing more is acked or nacked on that channel |
| `Consuming/IdempotentConsumerDecorator.cs` | Invokes handler then calls `IProcessedMessageStore.TryMarkProcessedAsync` |
| `Consuming/DeadLetterHandler.cs` | Declares `{exchange}.dlx` fanout + `{consumer}.dead-letter` queue; provides `x-dead-letter-exchange` queue args |
| `Consuming/ISubscriptionDispatcher.cs` | Internal per-event dispatch contract (avoids `MakeGenericType`) |
| `Consuming/TypedSubscriptionDispatcher.cs` | AOT-safe generic dispatcher; uses `JsonTypeInfo<TEvent>` for deserialization |
| `Consuming/SubscriptionRegistration.cs` | Value type recording `EventTypeId` → `EventClrType` for queue declaration |
| `DependencyInjection/KunRabbitMqBuilder.cs` | Fluent `.Subscribe<TEvent, THandler>(eventTypeId, typeInfo)` — registers handler + dispatcher + registration |
| `DependencyInjection/KunRabbitMqServiceCollectionExtensions.cs` | `AddRabbitMq(Action<RabbitMqOptions>, Action<KunRabbitMqBuilder>?)` — wires all singletons + hosted service |
| `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt` | RS0016 public surface tracking |

## For AI Agents

### Working In This Directory

- **AOT-safe**: `IsAotCompatible=true`. No `Activator.CreateInstance`, no `MakeGenericType`, no `Type.GetMethod`. `TypedSubscriptionDispatcher<TEvent>` is registered at DI wire-up time (compile-time generic instantiation). `JsonTypeInfo<TEvent>` is passed by the caller from their `JsonSerializerContext`.
- **Trace context headers are mandatory**: on publish, copy `TraceParent`/`TraceState` from the Outbox row into AMQP `BasicProperties.Headers` as `traceparent`/`tracestate`. On consume, restore via `ActivityContext.TryParse`.
- **Stable event IDs on the wire**: routing key = `message.EventTypeId` (e.g. `order.placed.v1`). `IEventTypeRegistry.Resolve(routingKey)` converts back to CLR type.
- **幂等与确认**：先查成功记录，执行成功后才标记并 ack；失败经持久化 quorum 队列退避，重试副本被确认后才 ack 原消息。业务 handler 仍必须幂等。
- **死信**：`x-kun-attempts` 达到 `MaxDeliveryAttempts` 后确认发布到 `{exchange}.dlx`，再 ack 原消息；兼容读取旧 x-death。
- **停止与确认**：订阅端停止时先置「正在关」，等已经开始写的确认帧写完再关通道；此后的 ack / nack 一律不发，由 RabbitMQ 在通道关闭时重新入队，幂等记录挡住重复处理。确认只经 `ConsumeSession.TrySettleAsync`，不要直接在通道上调 `BasicAckAsync` / `BasicNackAsync`。客户端关通道要等正在执行的处理器返回，所以关通道时不能拿着处理器也要等的锁。
- **RabbitMQ.Client 7.x async API**: `CreateChannelAsync`, `BasicPublishAsync`, `BasicQosAsync`, `ExchangeDeclareAsync`, `QueueDeclareAsync`, `QueueBindAsync`, `BasicConsumeAsync`, `BasicAckAsync`, `BasicNackAsync`. Never use `.Result`/`.Wait()`.
- **LoggerMessage delegates** (`[LoggerMessage]` on nested `partial class Log`) are used throughout — do not regress to `LogInformation(..., args)`.
- Payload is already a JSON string from the Outbox — no re-serialization needed on publish. On consume, `JsonSerializer.Deserialize(payload, typeInfo)` uses the caller-supplied `JsonTypeInfo`.
- Channel per publish with publisher confirmations and mandatory routing (not pooled) — acceptable for Outbox throughput; channel creation is cheap in 7.x.

### Testing Requirements

Use `Testcontainers.RabbitMq` in `tests/Kun.IntegrationTests` (M3 3.4, Docker-deferred). Validate:
- Trace-context round-tripping (publish → consume header restoration).
- At-least-once delivery (nack + requeue → retry).
- Idempotent consumer under redelivery (second delivery should log warning, not fail).
- Dead-letter routing after `MaxDeliveryAttempts`.

### Common Patterns

```csharp
// Wire-up (in Program.cs / Startup)
services.AddKunEventBus(bus => bus
    .Register<OrderPlacedIntegrationEvent>("order.placed.v1"));

services.AddRabbitMq(
    opts =>
    {
        opts.ConnectionString = "amqp://guest:guest@localhost:5672/";
        opts.ConsumerName = "catalog-service";
    },
    rmq => rmq.Subscribe<OrderPlacedIntegrationEvent, OrderPlacedHandler>(
        "order.placed.v1",
        MyJsonContext.Default.OrderPlacedIntegrationEvent));
```

Exchange type is `topic`; routing key = stable event ID. Consumer queues are named `{ConsumerName}.{EventTypeId}`.

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
