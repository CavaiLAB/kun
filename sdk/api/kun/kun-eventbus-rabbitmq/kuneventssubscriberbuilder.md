> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunEventsSubscriberBuilder

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.RabbitMQ` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.RabbitMQ`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.RabbitMQ)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus.RabbitMQ](/sdk/api/kun/kun-eventbus-rabbitmq/index.md)\
Assembly: Kun.EventBus.RabbitMQ.dll

订阅登记（PRD 7.10 的 `Subscribe<TEvent, THandler>()`）：每次登记把一个 Kun handler 挂到一个订阅上。
同一订阅可以覆盖多个事件，但一个事件在同一订阅里只有一个 handler；广播订阅共用实例队列，同一事件在一个服务里只能有一个广播订阅。

```csharp
public sealed class KunEventsSubscriberBuilder
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunEventsSubscriberBuilder](/sdk/api/kun/kun-eventbus-rabbitmq/kuneventssubscriberbuilder.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Properties

### Service \{#Kun_EventBus_RabbitMQ_KunEventsSubscriberBuilder_Service}

订阅方服务名。

```csharp
public string Service { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### Subscribe\<TEvent, THandler>(string, EventSubscriptionMode, bool) \{#Kun_EventBus_RabbitMQ_KunEventsSubscriberBuilder_Subscribe__2_System_String_Kun_EventBus_EventSubscriptionMode_System_Boolean\_}

把 `THandler` 登记为订阅 `subscription` 里 `TEvent` 的处理器，并以 scoped 登记该 handler 类型。
事件类型须已在 `AddKunEventBus` 以源生成 JsonTypeInfo 登记；宿主启动时核对，不满足即拒绝启动。

```csharp
public KunEventsSubscriberBuilder Subscribe<TEvent, THandler>(string subscription, EventSubscriptionMode mode = EventSubscriptionMode.Competing, bool acceptPlatformEvents = false) where TEvent : class where THandler : class, IIntegrationEventHandler<TEvent>
```

#### Parameters

`subscription` [string](https://learn.microsoft.com/dotnet/api/system.string)

订阅名，小写短横线标识，决定竞争队列 `q.<service>.<subscription>`、死信队列与幂等记录的消费者名。

`mode` EventSubscriptionMode

竞争消费或广播。

`acceptPlatformEvents` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

是否派发 tenant_id 为 null 的平台级事件；默认拒绝，派发时不开租户作用域。

#### Returns

[KunEventsSubscriberBuilder](/sdk/api/kun/kun-eventbus-rabbitmq/kuneventssubscriberbuilder.md)

#### Type Parameters

`TEvent`

`THandler`
