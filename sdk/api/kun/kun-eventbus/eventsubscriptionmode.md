> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum EventSubscriptionMode

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

订阅模式（PRD 7.10、ADR-008 D8.9）。

```csharp
public enum EventSubscriptionMode
```

## Fields

`Broadcast = 1`

广播：每个实例一个 `q.<service>.<instance>`，每个在线实例各处理一次；实例下线后队列删除。

`Competing = 0`

竞争消费：同一服务的全部实例共享 `q.<service>.<subscription>`，每条消息只由其中一个实例处理。
