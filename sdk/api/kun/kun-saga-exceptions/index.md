> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Saga.Exceptions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Saga` 2026.10.25 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Saga`](https://github.com/CavaiLAB/kun/tree/v2026.10.25/src/Kun.Saga)（`v2026.10.25`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[SagaNotFoundException](/sdk/api/kun/kun-saga-exceptions/saganotfoundexception.md)

Thrown when a saga instance cannot be found in the persistent store by its
[SagaNotFoundException.SagaId](/sdk/api/kun/kun-saga-exceptions/saganotfoundexception.md#Kun_Saga_Exceptions_SagaNotFoundException_SagaId), typically indicating the instance was never started or has been
purged.

[SagaStepFailedException](/sdk/api/kun/kun-saga-exceptions/sagastepfailedexception.md)

Thrown by the saga step executor when a step's `ExecuteAsync` or `CompensateAsync`
terminates with an unhandled exception. The inner exception carries the original failure.
