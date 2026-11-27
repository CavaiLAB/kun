> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunEventsRabbitMqExtensions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.RabbitMQ` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.RabbitMQ`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.RabbitMQ)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus.RabbitMQ](/sdk/api/kun/kun-eventbus-rabbitmq/index.md)\
Assembly: Kun.EventBus.RabbitMQ.dll

Registers the RabbitMQ envelope publisher and subscribers.

```csharp
public static class KunEventsRabbitMqExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunEventsRabbitMqExtensions](/sdk/api/kun/kun-eventbus-rabbitmq/kuneventsrabbitmqextensions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### AddKunEventsBroker(IServiceCollection, Func\<IServiceProvider, string>, string?) \{#Kun_EventBus_RabbitMQ_KunEventsRabbitMqExtensions_AddKunEventsBroker_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Func_System_IServiceProvider_System_String__System_String\_}

登记 RabbitMQ 发布端。AMQP URI 在首次连接时经 `resolveAmqpUri` 读取，值不进入组件清单；
`brokerReference` 是组件配置 `broker` 应匹配的 secretRef。

```csharp
public static IServiceCollection AddKunEventsBroker(this IServiceCollection services, Func<IServiceProvider, string> resolveAmqpUri, string? brokerReference = null)
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

`resolveAmqpUri` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[IServiceProvider](https://learn.microsoft.com/dotnet/api/system.iserviceprovider), [string](https://learn.microsoft.com/dotnet/api/system.string)>

`brokerReference` [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

### AddKunEventsSubscriber(IServiceCollection, string, Action\<KunEventsSubscriberBuilder>, Action\<KunEventsSubscriberOptions>?) \{#Kun_EventBus_RabbitMQ_KunEventsRabbitMqExtensions_AddKunEventsSubscriber_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_String_System_Action_Kun_EventBus_RabbitMQ_KunEventsSubscriberBuilder__System_Action_Kun_EventBus_RabbitMQ_KunEventsSubscriberOptions\_\_}

登记消费端（S1-EVT-002）：`service` 是订阅方服务名，`subscribe` 里逐个 `Subscribe<TEvent, THandler>()`。
消费连接与发布端分开（ClientProvidedName 为 qian.events.subscriber），AMQP 地址复用 [KunEventsRabbitMqExtensions.AddKunEventsBroker](/sdk/api/kun/kun-eventbus-rabbitmq/kuneventsrabbitmqextensions.md#Kun_EventBus_RabbitMQ_KunEventsRabbitMqExtensions_AddKunEventsBroker_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Func_System_IServiceProvider_System_String__System_String_) 登记的引用；
幂等记录默认写 Redis（宿主登记 IConnectionMultiplexer，或以 Kun 的 UseRedisIdempotency 登记 IProcessedMessageStore）。
可以多次调用以追加订阅，服务名必须相同；缺少 broker、幂等存储或事件登记时宿主拒绝启动。

```csharp
public static IServiceCollection AddKunEventsSubscriber(this IServiceCollection services, string service, Action<KunEventsSubscriberBuilder> subscribe, Action<KunEventsSubscriberOptions>? configure = null)
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

`service` [string](https://learn.microsoft.com/dotnet/api/system.string)

`subscribe` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[KunEventsSubscriberBuilder](/sdk/api/kun/kun-eventbus-rabbitmq/kuneventssubscriberbuilder.md)>

`configure` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[KunEventsSubscriberOptions](/sdk/api/kun/kun-eventbus-rabbitmq/kuneventssubscriberoptions.md)>?

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)
