> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretGrant

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

与 policy.permissions.secrets 条目对应的权限，不因声明本身而自动获批。

```csharp
public sealed record SecretGrant : IEquatable<SecretGrant>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SecretGrant](/sdk/api/kun/kun-configuration/secretgrant.md)

#### Implements

[IEquatable\<SecretGrant>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### SecretGrant(string, string) \{#Kun_Configuration_SecretGrant__ctor_System_String_System_String\_}

与 policy.permissions.secrets 条目对应的权限，不因声明本身而自动获批。

```csharp
public SecretGrant(string Name, string Scope)
```

#### Parameters

`Name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Scope` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Name \{#Kun_Configuration_SecretGrant_Name}

```csharp
public string Name { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Scope \{#Kun_Configuration_SecretGrant_Scope}

```csharp
public string Scope { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### \<Clone>$() \{#Kun_Configuration_SecretGrant__Clone\_\_}

```csharp
public SecretGrant <Clone>$()
```

#### Returns

[SecretGrant](/sdk/api/kun/kun-configuration/secretgrant.md)

### Deconstruct(out string, out string) \{#Kun_Configuration_SecretGrant_Deconstruct_System_String__System_String\_\_}

```csharp
public void Deconstruct(out string Name, out string Scope)
```

#### Parameters

`Name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Scope` [string](https://learn.microsoft.com/dotnet/api/system.string)

### Equals(object?) \{#Kun_Configuration_SecretGrant_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(SecretGrant?) \{#Kun_Configuration_SecretGrant_Equals_Kun_Configuration_SecretGrant\_}

```csharp
public bool Equals(SecretGrant? other)
```

#### Parameters

`other` [SecretGrant](/sdk/api/kun/kun-configuration/secretgrant.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Configuration_SecretGrant_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Configuration_SecretGrant_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(SecretGrant?, SecretGrant?) \{#Kun_Configuration_SecretGrant_op_Equality_Kun_Configuration_SecretGrant_Kun_Configuration_SecretGrant\_}

```csharp
public static bool operator ==(SecretGrant? left, SecretGrant? right)
```

#### Parameters

`left` [SecretGrant](/sdk/api/kun/kun-configuration/secretgrant.md)?

`right` [SecretGrant](/sdk/api/kun/kun-configuration/secretgrant.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(SecretGrant?, SecretGrant?) \{#Kun_Configuration_SecretGrant_op_Inequality_Kun_Configuration_SecretGrant_Kun_Configuration_SecretGrant\_}

```csharp
public static bool operator !=(SecretGrant? left, SecretGrant? right)
```

#### Parameters

`left` [SecretGrant](/sdk/api/kun/kun-configuration/secretgrant.md)?

`right` [SecretGrant](/sdk/api/kun/kun-configuration/secretgrant.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
