> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class OutboxMessageConfiguration

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore](/sdk/api/kun/kun-entityframeworkcore/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

EF Core mapping for [OutboxMessage](/sdk/api/kun/kun-eventbus-outbox/outboxmessage.md).

```csharp
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[OutboxMessageConfiguration](/sdk/api/kun/kun-entityframeworkcore/outboxmessageconfiguration.md)

#### Implements

[IEntityTypeConfiguration\<OutboxMessage>](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.ientitytypeconfiguration-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### OutboxMessageConfiguration() \{#Kun_EntityFrameworkCore_OutboxMessageConfiguration__ctor}

```csharp
public OutboxMessageConfiguration()
```

## Methods

### Configure(EntityTypeBuilder\<OutboxMessage>) \{#Kun_EntityFrameworkCore_OutboxMessageConfiguration_Configure_Microsoft_EntityFrameworkCore_Metadata_Builders_EntityTypeBuilder_Kun_EventBus_Outbox_OutboxMessage\_\_}

Configures the entity of type [OutboxMessage](/sdk/api/kun/kun-eventbus-outbox/outboxmessage.md).

```csharp
public void Configure(EntityTypeBuilder<OutboxMessage> builder)
```

#### Parameters

`builder` [EntityTypeBuilder](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1)\<OutboxMessage>

The builder to be used to configure the entity type.
