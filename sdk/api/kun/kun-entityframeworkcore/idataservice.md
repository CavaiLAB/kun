> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IDataService

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore](/sdk/api/kun/kun-entityframeworkcore/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

创建独立工作单元；不把不同业务上下文绑定到一个全局 KunDbContext。

```csharp
public interface IDataService
```

## Methods

### OpenAsync\<TContext>(CancellationToken) \{#Kun_EntityFrameworkCore_IDataService_OpenAsync__1_System_Threading_CancellationToken\_}

打开已由组件注册的上下文；调用方负责释放会话。

```csharp
ValueTask<IDataSession<TContext>> OpenAsync<TContext>(CancellationToken cancellationToken = default) where TContext : KunStrictDbContext
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[IDataSession](/sdk/api/kun/kun-entityframeworkcore/idatasession-1.md)\<TContext>>

#### Type Parameters

`TContext`
