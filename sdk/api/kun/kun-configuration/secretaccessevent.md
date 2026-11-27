> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretAccessEvent

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

一次解析审计，只含引用名、调用方、租户和结果，不含值。

```csharp
public sealed record SecretAccessEvent : IEquatable<SecretAccessEvent>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SecretAccessEvent](/sdk/api/kun/kun-configuration/secretaccessevent.md)

#### Implements

[IEquatable\<SecretAccessEvent>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### SecretAccessEvent(string, string, string?, bool, string) \{#Kun_Configuration_SecretAccessEvent__ctor_System_String_System_String_System_String_System_Boolean_System_String\_}

一次解析审计，只含引用名、调用方、租户和结果，不含值。

```csharp
public SecretAccessEvent(string Caller, string Reference, string? TenantId, bool Allowed, string Outcome)
```

#### Parameters

`Caller` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Reference` [string](https://learn.microsoft.com/dotnet/api/system.string)

`TenantId` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`Allowed` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

`Outcome` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Allowed \{#Kun_Configuration_SecretAccessEvent_Allowed}

```csharp
public bool Allowed { get; init; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Caller \{#Kun_Configuration_SecretAccessEvent_Caller}

```csharp
public string Caller { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Outcome \{#Kun_Configuration_SecretAccessEvent_Outcome}

```csharp
public string Outcome { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Reference \{#Kun_Configuration_SecretAccessEvent_Reference}

```csharp
public string Reference { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TenantId \{#Kun_Configuration_SecretAccessEvent_TenantId}

```csharp
public string? TenantId { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

## Methods

### \<Clone>$() \{#Kun_Configuration_SecretAccessEvent__Clone\_\_}

```csharp
public SecretAccessEvent <Clone>$()
```

#### Returns

[SecretAccessEvent](/sdk/api/kun/kun-configuration/secretaccessevent.md)

### Deconstruct(out string, out string, out string?, out bool, out string) \{#Kun_Configuration_SecretAccessEvent_Deconstruct_System_String__System_String__System_String__System_Boolean__System_String\_\_}

```csharp
public void Deconstruct(out string Caller, out string Reference, out string? TenantId, out bool Allowed, out string Outcome)
```

#### Parameters

`Caller` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Reference` [string](https://learn.microsoft.com/dotnet/api/system.string)

`TenantId` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`Allowed` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

`Outcome` [string](https://learn.microsoft.com/dotnet/api/system.string)

### Equals(object?) \{#Kun_Configuration_SecretAccessEvent_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(SecretAccessEvent?) \{#Kun_Configuration_SecretAccessEvent_Equals_Kun_Configuration_SecretAccessEvent\_}

```csharp
public bool Equals(SecretAccessEvent? other)
```

#### Parameters

`other` [SecretAccessEvent](/sdk/api/kun/kun-configuration/secretaccessevent.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Configuration_SecretAccessEvent_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Configuration_SecretAccessEvent_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(SecretAccessEvent?, SecretAccessEvent?) \{#Kun_Configuration_SecretAccessEvent_op_Equality_Kun_Configuration_SecretAccessEvent_Kun_Configuration_SecretAccessEvent\_}

```csharp
public static bool operator ==(SecretAccessEvent? left, SecretAccessEvent? right)
```

#### Parameters

`left` [SecretAccessEvent](/sdk/api/kun/kun-configuration/secretaccessevent.md)?

`right` [SecretAccessEvent](/sdk/api/kun/kun-configuration/secretaccessevent.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(SecretAccessEvent?, SecretAccessEvent?) \{#Kun_Configuration_SecretAccessEvent_op_Inequality_Kun_Configuration_SecretAccessEvent_Kun_Configuration_SecretAccessEvent\_}

```csharp
public static bool operator !=(SecretAccessEvent? left, SecretAccessEvent? right)
```

#### Parameters

`left` [SecretAccessEvent](/sdk/api/kun/kun-configuration/secretaccessevent.md)?

`right` [SecretAccessEvent](/sdk/api/kun/kun-configuration/secretaccessevent.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
