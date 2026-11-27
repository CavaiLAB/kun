> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum OutboxLockMode

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus.EntityFrameworkCore](/sdk/api/kun/kun-eventbus-entityframeworkcore/index.md)\
Assembly: Kun.EventBus.EntityFrameworkCore.dll

Outbox 领取的互斥方式。

```csharp
public enum OutboxLockMode
```

## Fields

`Redis = 1`

在行锁之外再按上下文 schema 取 Redis 租约，同一时刻只有一个副本投递该表；需要宿主登记 IConnectionMultiplexer。

`SkipLocked = 0`

各副本并行，以 `FOR UPDATE SKIP LOCKED` 加租约分摊行。
