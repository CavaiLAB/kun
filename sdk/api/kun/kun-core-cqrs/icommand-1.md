> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface ICommand\<TResponse>

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Core` 2026.10.25 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Core`](https://github.com/CavaiLAB/kun/tree/v2026.10.25/src/Kun.Core)（`v2026.10.25`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Core.Cqrs](/sdk/api/kun/kun-core-cqrs/index.md)\
Assembly: Kun.Core.dll

Command：写操作请求，带返回值。

```csharp
public interface ICommand<TResponse> : IRequest<TResponse>
```

#### Type Parameters

`TResponse`

#### Implements

[IRequest\<TResponse>](/sdk/api/kun/kun-core-cqrs/irequest-1.md)
