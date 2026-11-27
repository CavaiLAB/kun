> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface ISender

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Mediator` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Mediator`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Mediator)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Mediator](/sdk/api/kun/kun-mediator/index.md)\
Assembly: Kun.Mediator.dll

Dispatches a single request to its registered handler.

```csharp
public interface ISender
```

## Methods

### Send\<TResponse>(IRequest\<TResponse>, CancellationToken) \{#Kun_Mediator_ISender_Send__1_Kun_Core_Cqrs_IRequest___0__System_Threading_CancellationToken\_}

Send `request` to its handler and return the response.

```csharp
ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
```

#### Parameters

`request` IRequest\<TResponse>

The request to dispatch.

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

Cancellation token.

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<TResponse>

The response produced by the registered handler.

#### Type Parameters

`TResponse`

The expected response type.
