> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.EventBus.Background

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[RedisOutboxLockProvider](/sdk/api/kun/kun-eventbus-background/redisoutboxlockprovider.md)

Redis-backed [IOutboxLockProvider](/sdk/api/kun/kun-eventbus-background/ioutboxlockprovider.md) using `SET key value NX EX` semantics.
Safe for multi-pod deployments: only one pod holds the lock per TTL window.

### Interfaces

[IOutboxLockProvider](/sdk/api/kun/kun-eventbus-background/ioutboxlockprovider.md)

Provides a distributed lease that prevents multiple pods from publishing
the same outbox batch simultaneously.
