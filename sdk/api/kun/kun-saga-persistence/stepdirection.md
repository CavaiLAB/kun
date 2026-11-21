> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum StepDirection

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Saga` 2026.10.25 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Saga`](https://github.com/CavaiLAB/kun/tree/v2026.10.25/src/Kun.Saga)（`v2026.10.25`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Saga.Persistence](/sdk/api/kun/kun-saga-persistence/index.md)\
Assembly: Kun.Saga.dll

Indicates whether a queued saga step entry is a forward execution or a compensation.

```csharp
public enum StepDirection
```

## Fields

`Compensate = 1`

The step should call `ISagaStep.CompensateAsync`.

`Execute = 0`

The step should call `ISagaStep.ExecuteAsync`.
