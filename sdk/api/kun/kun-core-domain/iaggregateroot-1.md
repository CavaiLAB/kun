> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IAggregateRoot\<TKey>

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Core` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Core`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Core)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Core.Domain](/sdk/api/kun/kun-core-domain/index.md)\
Assembly: Kun.Core.dll

带强类型 Key 的聚合根标记接口。

```csharp
public interface IAggregateRoot<out TKey> : IAggregateRoot where TKey : notnull
```

#### Type Parameters

`TKey`

#### Implements

[IAggregateRoot](/sdk/api/kun/kun-core-domain/iaggregateroot.md)

## Properties

### Id \{#Kun_Core_Domain_IAggregateRoot_1_Id}

Aggregate root primary key.

```csharp
TKey Id { get; }
```

#### Property Value

TKey
