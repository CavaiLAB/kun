> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum OutboxStatus

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus` 2026.10.25 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus`](https://github.com/CavaiLAB/kun/tree/v2026.10.25/src/Kun.EventBus)（`v2026.10.25`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus.Outbox](/sdk/api/kun/kun-eventbus-outbox/index.md)\
Assembly: Kun.EventBus.dll

Lifecycle state of a persisted Outbox message.

```csharp
public enum OutboxStatus
```

## Fields

`Failed = 2`

Publish permanently failed after exhausting retries.

`Pending = 0`

Persisted but not yet published to the message bus.

`Published = 1`

Successfully published.
