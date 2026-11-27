> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IIntegrationEventOutbox

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus.Outbox](/sdk/api/kun/kun-eventbus-outbox/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

Appends outbox messages to the current unit-of-work boundary.

```csharp
public interface IIntegrationEventOutbox
```

## Methods

### Add(OutboxMessage) \{#Kun_EventBus_Outbox_IIntegrationEventOutbox_Add_Kun_EventBus_Outbox_OutboxMessage\_}

Stages `message` for atomic persistence with the business transaction.

```csharp
void Add(OutboxMessage message)
```

#### Parameters

`message` [OutboxMessage](/sdk/api/kun/kun-eventbus-outbox/outboxmessage.md)

### GetPendingMessages() \{#Kun_EventBus_Outbox_IIntegrationEventOutbox_GetPendingMessages}

Returns all staged messages pending flush.

```csharp
IReadOnlyList<OutboxMessage> GetPendingMessages()
```

#### Returns

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[OutboxMessage](/sdk/api/kun/kun-eventbus-outbox/outboxmessage.md)>
