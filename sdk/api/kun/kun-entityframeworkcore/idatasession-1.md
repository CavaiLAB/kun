> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IDataSession\<TContext>

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore](/sdk/api/kun/kun-entityframeworkcore/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

同一上下文中的业务变更、领域通知与 Outbox 保存边界。

```csharp
public interface IDataSession<out TContext> : IUnitOfWork, IAsyncDisposable where TContext : KunStrictDbContext
```

#### Type Parameters

`TContext`

#### Implements

[IUnitOfWork](/sdk/api/kun/kun-entityframeworkcore/iunitofwork.md),
[IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable)

## Properties

### Context \{#Kun_EntityFrameworkCore_IDataSession_1_Context}

此会话独占的业务上下文；不能跨线程并发使用。

```csharp
TContext Context { get; }
```

#### Property Value

TContext

### Outbox \{#Kun_EntityFrameworkCore_IDataSession_1_Outbox}

暂存到此会话的 Outbox；只有 SaveChangesAsync 成功后才落库。

```csharp
IOutboxWriter Outbox { get; }
```

#### Property Value

IOutboxWriter
