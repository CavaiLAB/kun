> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Namespace Kun.Core.Results

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Core` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Core`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Core)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

### Classes

[ResultExtensions](/sdk/api/kun/kun-core-results/resultextensions.md)

Synchronous railway-oriented extensions for [Result](/sdk/api/kun/kun-core-results/result.md) and
[Result\<T>](/sdk/api/kun/kun-core-results/result-1.md). Follows ADR-011.

[TaskResultExtensions](/sdk/api/kun/kun-core-results/taskresultextensions.md)

Asynchronous railway-oriented extensions for [`Task`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)
receivers whose result type is [Result](/sdk/api/kun/kun-core-results/result.md) or
[Result\<T>](/sdk/api/kun/kun-core-results/result-1.md). Follows ADR-011.

[ValueTaskResultExtensions](/sdk/api/kun/kun-core-results/valuetaskresultextensions.md)

Asynchronous railway-oriented extensions for
[`ValueTask`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1) receivers whose result type is
[Result](/sdk/api/kun/kun-core-results/result.md) or [Result\<T>](/sdk/api/kun/kun-core-results/result-1.md). Follows ADR-011;
mirror of [TaskResultExtensions](/sdk/api/kun/kun-core-results/taskresultextensions.md) for hot paths that
avoid [`Task`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task) allocation (Mediator pipeline).

### Structs

[Result](/sdk/api/kun/kun-core-results/result.md)

无返回值的 Result。Handler 在不需要返回数据时使用。失败时携带 [Error](/sdk/api/kun/kun-errors/error.md)：
一个登记在错误码目录里的错误码，加文案参数与出错位置。

[Result\<T>](/sdk/api/kun/kun-core-results/result-1.md)

带返回值的 Result。Handler 成功时携带 `T`，失败时携带 [Error](/sdk/api/kun/kun-errors/error.md)。
