> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretDiagnostic

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

不含原值的配置扫描诊断。

```csharp
public sealed record SecretDiagnostic : IEquatable<SecretDiagnostic>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SecretDiagnostic](/sdk/api/kun/kun-configuration/secretdiagnostic.md)

#### Implements

[IEquatable\<SecretDiagnostic>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### SecretDiagnostic(string, string, string) \{#Kun_Configuration_SecretDiagnostic__ctor_System_String_System_String_System_String\_}

不含原值的配置扫描诊断。

```csharp
public SecretDiagnostic(string Code, string Location, string Reason)
```

#### Parameters

`Code` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Location` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Reason` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Code \{#Kun_Configuration_SecretDiagnostic_Code}

```csharp
public string Code { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Location \{#Kun_Configuration_SecretDiagnostic_Location}

```csharp
public string Location { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Reason \{#Kun_Configuration_SecretDiagnostic_Reason}

```csharp
public string Reason { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### \<Clone>$() \{#Kun_Configuration_SecretDiagnostic__Clone\_\_}

```csharp
public SecretDiagnostic <Clone>$()
```

#### Returns

[SecretDiagnostic](/sdk/api/kun/kun-configuration/secretdiagnostic.md)

### Deconstruct(out string, out string, out string) \{#Kun_Configuration_SecretDiagnostic_Deconstruct_System_String__System_String__System_String\_\_}

```csharp
public void Deconstruct(out string Code, out string Location, out string Reason)
```

#### Parameters

`Code` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Location` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Reason` [string](https://learn.microsoft.com/dotnet/api/system.string)

### Equals(object?) \{#Kun_Configuration_SecretDiagnostic_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(SecretDiagnostic?) \{#Kun_Configuration_SecretDiagnostic_Equals_Kun_Configuration_SecretDiagnostic\_}

```csharp
public bool Equals(SecretDiagnostic? other)
```

#### Parameters

`other` [SecretDiagnostic](/sdk/api/kun/kun-configuration/secretdiagnostic.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Configuration_SecretDiagnostic_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Configuration_SecretDiagnostic_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(SecretDiagnostic?, SecretDiagnostic?) \{#Kun_Configuration_SecretDiagnostic_op_Equality_Kun_Configuration_SecretDiagnostic_Kun_Configuration_SecretDiagnostic\_}

```csharp
public static bool operator ==(SecretDiagnostic? left, SecretDiagnostic? right)
```

#### Parameters

`left` [SecretDiagnostic](/sdk/api/kun/kun-configuration/secretdiagnostic.md)?

`right` [SecretDiagnostic](/sdk/api/kun/kun-configuration/secretdiagnostic.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(SecretDiagnostic?, SecretDiagnostic?) \{#Kun_Configuration_SecretDiagnostic_op_Inequality_Kun_Configuration_SecretDiagnostic_Kun_Configuration_SecretDiagnostic\_}

```csharp
public static bool operator !=(SecretDiagnostic? left, SecretDiagnostic? right)
```

#### Parameters

`left` [SecretDiagnostic](/sdk/api/kun/kun-configuration/secretdiagnostic.md)?

`right` [SecretDiagnostic](/sdk/api/kun/kun-configuration/secretdiagnostic.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
