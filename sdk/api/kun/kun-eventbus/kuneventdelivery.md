> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunEventDelivery

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

订阅器正在派发的一次投递：已解析、已与 AMQP 属性核对过的 v1 信封，以及订阅与重试计数。
订阅器为每条消息新建一个依赖注入作用域，handler 可在构造函数里注入本类型读取 producer、actor 与因果链；
幂等键、发生时间与租户一律以这里的信封为准，不读载荷里的同名字段（S1-DEC-020）。
只能在订阅器派发的作用域内解析，其他位置解析即抛 EVENTS_MISCONFIGURED。

```csharp
public sealed class KunEventDelivery
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunEventDelivery](/sdk/api/kun/kun-eventbus/kuneventdelivery.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### KunEventDelivery() \{#Kun_EventBus_KunEventDelivery__ctor}

```csharp
public KunEventDelivery()
```

## Properties

### Attempt \{#Kun_EventBus_KunEventDelivery_Attempt}

本次是第几次投递，首投为 1；每经过一次退避重试加一。

```csharp
public required int Attempt { get; init; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### Consumer \{#Kun_EventBus_KunEventDelivery_Consumer}

幂等记录使用的消费者名：竞争消费为 `<service>.<subscription>`，广播另带 `@<instance>`。

```csharp
public required string Consumer { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Envelope \{#Kun_EventBus_KunEventDelivery_Envelope}

已校验的信封。

```csharp
public required KunEventEnvelope Envelope { get; init; }
```

#### Property Value

[KunEventEnvelope](/sdk/api/kun/kun-eventbus/kuneventenvelope.md)

### Mode \{#Kun_EventBus_KunEventDelivery_Mode}

订阅模式。

```csharp
public required EventSubscriptionMode Mode { get; init; }
```

#### Property Value

[EventSubscriptionMode](/sdk/api/kun/kun-eventbus/eventsubscriptionmode.md)

### Service \{#Kun_EventBus_KunEventDelivery_Service}

订阅方服务名。

```csharp
public required string Service { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Subscription \{#Kun_EventBus_KunEventDelivery_Subscription}

订阅名。

```csharp
public required string Subscription { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### ToString() \{#Kun_EventBus_KunEventDelivery_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
