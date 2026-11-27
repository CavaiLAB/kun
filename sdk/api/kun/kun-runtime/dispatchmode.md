> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Enum DispatchMode

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

进程内扩展事件的分发模式。

```csharp
public enum DispatchMode
```

## Fields

`Bail = 4`

依次执行，首个非空结果中断。

`Emit = 0`

依次通知，记录并隔离监听器异常。

`Parallel = 2`

并发执行，等待全部完成并聚合失败。

`Serial = 3`

依次执行，首个失败中断。

`Waterfall = 1`

通过 next 委托的环绕中间件。
