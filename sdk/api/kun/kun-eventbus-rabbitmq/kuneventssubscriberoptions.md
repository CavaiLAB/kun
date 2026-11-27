> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunEventsSubscriberOptions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.RabbitMQ` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.RabbitMQ`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.RabbitMQ)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus.RabbitMQ](/sdk/api/kun/kun-eventbus-rabbitmq/index.md)\
Assembly: Kun.EventBus.RabbitMQ.dll

消费端的宿主级设置。重试语义与 Outbox 投递侧不同：这里的 [KunEventsSubscriberOptions.MaxRetries](/sdk/api/kun/kun-eventbus-rabbitmq/kuneventssubscriberoptions.md#Kun_EventBus_RabbitMQ_KunEventsSubscriberOptions_MaxRetries) 是首投之外的重试次数，
默认首投 1 次 + 重试 5 次，第 6 次仍失败即进死信队列。

```csharp
public sealed class KunEventsSubscriberOptions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunEventsSubscriberOptions](/sdk/api/kun/kun-eventbus-rabbitmq/kuneventssubscriberoptions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### KunEventsSubscriberOptions() \{#Kun_EventBus_RabbitMQ_KunEventsSubscriberOptions__ctor}

```csharp
public KunEventsSubscriberOptions()
```

## Properties

### IdempotencyTtl \{#Kun_EventBus_RabbitMQ_KunEventsSubscriberOptions_IdempotencyTtl}

幂等记录的保留时长（PRD 7.10、R63）。默认 7 天。

```csharp
public TimeSpan IdempotencyTtl { get; set; }
```

#### Property Value

[TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

### Instance \{#Kun_EventBus_RabbitMQ_KunEventsSubscriberOptions_Instance}

本实例的名字，广播队列 `q.<service>.<instance>` 与广播幂等记录用它区分实例。默认取规范化后的机器名加进程号。

```csharp
public string Instance { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### MaxRetries \{#Kun_EventBus_RabbitMQ_KunEventsSubscriberOptions_MaxRetries}

首投之外的最大重试次数；用尽后再失败即进死信队列。默认 5，即最多投递 6 次。

```csharp
public int MaxRetries { get; set; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### PrefetchCount \{#Kun_EventBus_RabbitMQ_KunEventsSubscriberOptions_PrefetchCount}

每个消费通道未确认消息的上限。默认 16。

```csharp
public ushort PrefetchCount { get; set; }
```

#### Property Value

[ushort](https://learn.microsoft.com/dotnet/api/system.uint16)

### RetryBaseDelay \{#Kun_EventBus_RabbitMQ_KunEventsSubscriberOptions_RetryBaseDelay}

重试退避起点：第 n 次重试前等待 `RetryBaseDelay × 2^(n-1)`，封顶一分钟，与 Outbox 投递侧对称。默认 1 秒。

```csharp
public TimeSpan RetryBaseDelay { get; set; }
```

#### Property Value

[TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)
