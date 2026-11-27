> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class AuditChainEntry

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit](/sdk/api/kun/kun-audit/index.md)\
Assembly: Kun.Audit.Abstractions.dll

持久化的链节点；序号和前序都参与哈希验证。

```csharp
public sealed record AuditChainEntry : IEquatable<AuditChainEntry>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)

#### Implements

[IEquatable\<AuditChainEntry>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### AuditChainEntry(long, string?, string, AuditRecord) \{#Kun_Audit_AuditChainEntry__ctor_System_Int64_System_String_System_String_Kun_Audit_AuditRecord\_}

持久化的链节点；序号和前序都参与哈希验证。

```csharp
public AuditChainEntry(long Sequence, string? PreviousHash, string ContentHash, AuditRecord Record)
```

#### Parameters

`Sequence` [long](https://learn.microsoft.com/dotnet/api/system.int64)

`PreviousHash` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`ContentHash` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Record` [AuditRecord](/sdk/api/kun/kun-audit/auditrecord.md)

## Properties

### ContentHash \{#Kun_Audit_AuditChainEntry_ContentHash}

```csharp
public string ContentHash { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### PreviousHash \{#Kun_Audit_AuditChainEntry_PreviousHash}

```csharp
public string? PreviousHash { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### Record \{#Kun_Audit_AuditChainEntry_Record}

```csharp
public AuditRecord Record { get; init; }
```

#### Property Value

[AuditRecord](/sdk/api/kun/kun-audit/auditrecord.md)

### Sequence \{#Kun_Audit_AuditChainEntry_Sequence}

```csharp
public long Sequence { get; init; }
```

#### Property Value

[long](https://learn.microsoft.com/dotnet/api/system.int64)

## Methods

### \<Clone>$() \{#Kun_Audit_AuditChainEntry__Clone\_\_}

```csharp
public AuditChainEntry <Clone>$()
```

#### Returns

[AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)

### Deconstruct(out long, out string?, out string, out AuditRecord) \{#Kun_Audit_AuditChainEntry_Deconstruct_System_Int64__System_String__System_String__Kun_Audit_AuditRecord\_\_}

```csharp
public void Deconstruct(out long Sequence, out string? PreviousHash, out string ContentHash, out AuditRecord Record)
```

#### Parameters

`Sequence` [long](https://learn.microsoft.com/dotnet/api/system.int64)

`PreviousHash` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`ContentHash` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Record` [AuditRecord](/sdk/api/kun/kun-audit/auditrecord.md)

### Equals(object?) \{#Kun_Audit_AuditChainEntry_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(AuditChainEntry?) \{#Kun_Audit_AuditChainEntry_Equals_Kun_Audit_AuditChainEntry\_}

```csharp
public bool Equals(AuditChainEntry? other)
```

#### Parameters

`other` [AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Audit_AuditChainEntry_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Audit_AuditChainEntry_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(AuditChainEntry?, AuditChainEntry?) \{#Kun_Audit_AuditChainEntry_op_Equality_Kun_Audit_AuditChainEntry_Kun_Audit_AuditChainEntry\_}

```csharp
public static bool operator ==(AuditChainEntry? left, AuditChainEntry? right)
```

#### Parameters

`left` [AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)?

`right` [AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(AuditChainEntry?, AuditChainEntry?) \{#Kun_Audit_AuditChainEntry_op_Inequality_Kun_Audit_AuditChainEntry_Kun_Audit_AuditChainEntry\_}

```csharp
public static bool operator !=(AuditChainEntry? left, AuditChainEntry? right)
```

#### Parameters

`left` [AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)?

`right` [AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
