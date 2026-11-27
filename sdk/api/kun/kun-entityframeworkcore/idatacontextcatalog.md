> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IDataContextCatalog

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore](/sdk/api/kun/kun-entityframeworkcore/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

宿主经 AddKunData 登记的全部业务上下文；Outbox 投递等宿主级组件据此逐个处理各上下文的表。

```csharp
public interface IDataContextCatalog
```

## Properties

### ContextTypes \{#Kun_EntityFrameworkCore_IDataContextCatalog_ContextTypes}

登记顺序的上下文类型，均继承 [KunStrictDbContext](/sdk/api/kun/kun-entityframeworkcore/kunstrictdbcontext.md)。

```csharp
IReadOnlyList<Type> ContextTypes { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[Type](https://learn.microsoft.com/dotnet/api/system.type)>
