> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IEventsService

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

事件 Provider 的只读视图。发布不经过这里：命令 handler 通过同一数据会话的 Outbox 写入端发布，
信封由 Provider 在入队时按 [EventEnvelopeScope](/sdk/api/kun/kun-eventbus/eventenvelopescope.md) 生成。

```csharp
public interface IEventsService
```

## Properties

### Catalog \{#Kun_EventBus_IEventsService_Catalog}

宿主登记的全部集成事件，按事件 ID 排序。

```csharp
IReadOnlyList<IntegrationEventDescriptor> Catalog { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md)>

### Subscriptions \{#Kun_EventBus_IEventsService_Subscriptions}

宿主订阅器登记的全部订阅，每个（订阅, 事件 ID）一项，按订阅名与事件 ID 排序；宿主没有订阅器时为空。

```csharp
IReadOnlyList<EventSubscriptionDescriptor> Subscriptions { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[EventSubscriptionDescriptor](/sdk/api/kun/kun-eventbus/eventsubscriptiondescriptor.md)>

## Methods

### Find(string) \{#Kun_EventBus_IEventsService_Find_System_String\_}

按事件 ID 查找；未登记时返回 null。

```csharp
IntegrationEventDescriptor? Find(string type)
```

#### Parameters

`type` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md)?
