> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.EventBus.EntityFrameworkCore

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[EventsPlugin](/sdk/api/kun/kun-eventbus-entityframeworkcore/eventsplugin.md)

qian.events 的真实 Provider：挂载时核对组件配置与宿主装配一致，提供 Events 服务键；卸载撤销服务与错误码。

[KunEventRelayExtensions](/sdk/api/kun/kun-eventbus-entityframeworkcore/kuneventrelayextensions.md)

Registers the EF-backed event relay and its host settings.

[KunEventsOptions](/sdk/api/kun/kun-eventbus-entityframeworkcore/kuneventsoptions.md)

Outbox 投递的宿主级设置；组件配置 `outbox.lock` 必须与 [KunEventsOptions.OutboxLock](/sdk/api/kun/kun-eventbus-entityframeworkcore/kuneventsoptions.md#Kun_EventBus_EntityFrameworkCore_KunEventsOptions_OutboxLock) 一致。

### Enums

[OutboxLockMode](/sdk/api/kun/kun-eventbus-entityframeworkcore/outboxlockmode.md)

Outbox 领取的互斥方式。
