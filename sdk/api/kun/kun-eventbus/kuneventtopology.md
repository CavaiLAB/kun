> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunEventTopology

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

RabbitMQ 拓扑命名（PRD 7.10）：topic exchange `qian.events`，路由键为信封 subject；
竞争消费用每服务一个 quorum 队列 `q.<service>.<subscription>`，广播用每实例一个自动删除队列 `q.<service>.<instance>`。
消费端另有每订阅一个死信队列 `q.<service>.<subscription>-dlq` 与按次数、间隔命名的重试队列 `<主队列>-retry-<n>-<毫秒>`；
这两种后缀保留给消费端，订阅名与实例名不能以它们结尾。

```csharp
public static class KunEventTopology
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunEventTopology](/sdk/api/kun/kun-eventbus/kuneventtopology.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### DeadLetterExchange \{#Kun_EventBus_KunEventTopology_DeadLetterExchange}

死信 exchange；消费端重试耗尽或确定性失败的消息投递到这里，路由键为订阅的死信队列名。

```csharp
public const string DeadLetterExchange = "qian.dlx"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Exchange \{#Kun_EventBus_KunEventTopology_Exchange}

集成事件 topic exchange。

```csharp
public const string Exchange = "qian.events"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### BroadcastQueue(string, string) \{#Kun_EventBus_KunEventTopology_BroadcastQueue_System_String_System_String\_}

广播队列名：每个实例一个，实例下线即删除。

```csharp
public static string BroadcastQueue(string service, string instance)
```

#### Parameters

`service` [string](https://learn.microsoft.com/dotnet/api/system.string)

`instance` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### DeadLetterQueue(string, string) \{#Kun_EventBus_KunEventTopology_DeadLetterQueue_System_String_System_String\_}

订阅的死信队列名，也是发往 [KunEventTopology.DeadLetterExchange](/sdk/api/kun/kun-eventbus/kuneventtopology.md#Kun_EventBus_KunEventTopology_DeadLetterExchange) 的路由键：每订阅一个、跨实例共享，竞争与广播订阅都有。
死信按队列名精确绑定，不按 subject，否则订阅同一事件的各个订阅会收到彼此的死信。

```csharp
public static string DeadLetterQueue(string service, string subscription)
```

#### Parameters

`service` [string](https://learn.microsoft.com/dotnet/api/system.string)

`subscription` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### RetryQueue(string, string, int, TimeSpan) \{#Kun_EventBus_KunEventTopology_RetryQueue_System_String_System_String_System_Int32_System_TimeSpan\_}

第 `attempt` 次失败后的重试队列名：消息在其中停留 `delay` 后经默认 exchange 回到 `name` 的主队列。
名字带次数与毫秒数，改了退避起点也不会以不同参数重声明同名队列。`name` 为竞争订阅名或广播实例名。

```csharp
public static string RetryQueue(string service, string name, int attempt, TimeSpan delay)
```

#### Parameters

`service` [string](https://learn.microsoft.com/dotnet/api/system.string)

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`attempt` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`delay` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### RoutingKey(string) \{#Kun_EventBus_KunEventTopology_RoutingKey_System_String\_}

事件的路由键，即信封 subject。

```csharp
public static string RoutingKey(string type)
```

#### Parameters

`type` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### SubscriptionQueue(string, string) \{#Kun_EventBus_KunEventTopology_SubscriptionQueue_System_String_System_String\_}

竞争消费队列名：同一服务的所有实例共享，quorum 类型。

```csharp
public static string SubscriptionQueue(string service, string subscription)
```

#### Parameters

`service` [string](https://learn.microsoft.com/dotnet/api/system.string)

`subscription` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
