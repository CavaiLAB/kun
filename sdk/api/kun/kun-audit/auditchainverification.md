> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class AuditChainVerification

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit](/sdk/api/kun/kun-audit/index.md)\
Assembly: Kun.Audit.Abstractions.dll

全链验证结果；BrokenAt 指出最早不一致的位置，不回显敏感记录。

```csharp
public sealed record AuditChainVerification : IEquatable<AuditChainVerification>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[AuditChainVerification](/sdk/api/kun/kun-audit/auditchainverification.md)

#### Implements

[IEquatable\<AuditChainVerification>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### AuditChainVerification(bool, long, long?, string?) \{#Kun_Audit_AuditChainVerification__ctor_System_Boolean_System_Int64_System_Nullable_System_Int64__System_String\_}

全链验证结果；BrokenAt 指出最早不一致的位置，不回显敏感记录。

```csharp
public AuditChainVerification(bool Valid, long CheckedEntries, long? BrokenAt, string? Reason)
```

#### Parameters

`Valid` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

`CheckedEntries` [long](https://learn.microsoft.com/dotnet/api/system.int64)

`BrokenAt` [long](https://learn.microsoft.com/dotnet/api/system.int64)?

`Reason` [string](https://learn.microsoft.com/dotnet/api/system.string)?

## Properties

### BrokenAt \{#Kun_Audit_AuditChainVerification_BrokenAt}

```csharp
public long? BrokenAt { get; init; }
```

#### Property Value

[long](https://learn.microsoft.com/dotnet/api/system.int64)?

### CheckedEntries \{#Kun_Audit_AuditChainVerification_CheckedEntries}

```csharp
public long CheckedEntries { get; init; }
```

#### Property Value

[long](https://learn.microsoft.com/dotnet/api/system.int64)

### Reason \{#Kun_Audit_AuditChainVerification_Reason}

```csharp
public string? Reason { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### Valid \{#Kun_Audit_AuditChainVerification_Valid}

```csharp
public bool Valid { get; init; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

## Methods

### \<Clone>$() \{#Kun_Audit_AuditChainVerification__Clone\_\_}

```csharp
public AuditChainVerification <Clone>$()
```

#### Returns

[AuditChainVerification](/sdk/api/kun/kun-audit/auditchainverification.md)

### Deconstruct(out bool, out long, out long?, out string?) \{#Kun_Audit_AuditChainVerification_Deconstruct_System_Boolean__System_Int64__System_Nullable_System_Int64___System_String\_\_}

```csharp
public void Deconstruct(out bool Valid, out long CheckedEntries, out long? BrokenAt, out string? Reason)
```

#### Parameters

`Valid` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

`CheckedEntries` [long](https://learn.microsoft.com/dotnet/api/system.int64)

`BrokenAt` [long](https://learn.microsoft.com/dotnet/api/system.int64)?

`Reason` [string](https://learn.microsoft.com/dotnet/api/system.string)?

### Equals(object?) \{#Kun_Audit_AuditChainVerification_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(AuditChainVerification?) \{#Kun_Audit_AuditChainVerification_Equals_Kun_Audit_AuditChainVerification\_}

```csharp
public bool Equals(AuditChainVerification? other)
```

#### Parameters

`other` [AuditChainVerification](/sdk/api/kun/kun-audit/auditchainverification.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Audit_AuditChainVerification_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Audit_AuditChainVerification_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(AuditChainVerification?, AuditChainVerification?) \{#Kun_Audit_AuditChainVerification_op_Equality_Kun_Audit_AuditChainVerification_Kun_Audit_AuditChainVerification\_}

```csharp
public static bool operator ==(AuditChainVerification? left, AuditChainVerification? right)
```

#### Parameters

`left` [AuditChainVerification](/sdk/api/kun/kun-audit/auditchainverification.md)?

`right` [AuditChainVerification](/sdk/api/kun/kun-audit/auditchainverification.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(AuditChainVerification?, AuditChainVerification?) \{#Kun_Audit_AuditChainVerification_op_Inequality_Kun_Audit_AuditChainVerification_Kun_Audit_AuditChainVerification\_}

```csharp
public static bool operator !=(AuditChainVerification? left, AuditChainVerification? right)
```

#### Parameters

`left` [AuditChainVerification](/sdk/api/kun/kun-audit/auditchainverification.md)?

`right` [AuditChainVerification](/sdk/api/kun/kun-audit/auditchainverification.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
