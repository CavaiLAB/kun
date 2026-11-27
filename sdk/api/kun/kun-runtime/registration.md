> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class Registration

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

目录与执行共用的注册描述；Name 为空表示键本身，具名 seam 使用独立名称。

```csharp
public sealed record Registration : IEquatable<Registration>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[Registration](/sdk/api/kun/kun-runtime/registration.md)

#### Implements

[IEquatable\<Registration>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### Registration(ServiceKey, string, ScopeKey, string, object) \{#Kun_Runtime_Registration__ctor_Kun_Runtime_ServiceKey_System_String_Kun_Runtime_ScopeKey_System_String_System_Object\_}

目录与执行共用的注册描述；Name 为空表示键本身，具名 seam 使用独立名称。

```csharp
public Registration(ServiceKey Key, string Name, ScopeKey Scope, string OwnerEntryId, object Value)
```

#### Parameters

`Key` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

`Name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Scope` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

`OwnerEntryId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Value` [object](https://learn.microsoft.com/dotnet/api/system.object)

## Properties

### Key \{#Kun_Runtime_Registration_Key}

```csharp
public ServiceKey Key { get; init; }
```

#### Property Value

[ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

### Name \{#Kun_Runtime_Registration_Name}

```csharp
public string Name { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### OwnerEntryId \{#Kun_Runtime_Registration_OwnerEntryId}

```csharp
public string OwnerEntryId { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Scope \{#Kun_Runtime_Registration_Scope}

```csharp
public ScopeKey Scope { get; init; }
```

#### Property Value

[ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

### Value \{#Kun_Runtime_Registration_Value}

```csharp
public object Value { get; init; }
```

#### Property Value

[object](https://learn.microsoft.com/dotnet/api/system.object)

## Methods

### \<Clone>$() \{#Kun_Runtime_Registration__Clone\_\_}

```csharp
public Registration <Clone>$()
```

#### Returns

[Registration](/sdk/api/kun/kun-runtime/registration.md)

### Deconstruct(out ServiceKey, out string, out ScopeKey, out string, out object) \{#Kun_Runtime_Registration_Deconstruct_Kun_Runtime_ServiceKey__System_String__Kun_Runtime_ScopeKey__System_String__System_Object\_\_}

```csharp
public void Deconstruct(out ServiceKey Key, out string Name, out ScopeKey Scope, out string OwnerEntryId, out object Value)
```

#### Parameters

`Key` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

`Name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Scope` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

`OwnerEntryId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Value` [object](https://learn.microsoft.com/dotnet/api/system.object)

### Equals(object?) \{#Kun_Runtime_Registration_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(Registration?) \{#Kun_Runtime_Registration_Equals_Kun_Runtime_Registration\_}

```csharp
public bool Equals(Registration? other)
```

#### Parameters

`other` [Registration](/sdk/api/kun/kun-runtime/registration.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Runtime_Registration_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Runtime_Registration_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(Registration?, Registration?) \{#Kun_Runtime_Registration_op_Equality_Kun_Runtime_Registration_Kun_Runtime_Registration\_}

```csharp
public static bool operator ==(Registration? left, Registration? right)
```

#### Parameters

`left` [Registration](/sdk/api/kun/kun-runtime/registration.md)?

`right` [Registration](/sdk/api/kun/kun-runtime/registration.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(Registration?, Registration?) \{#Kun_Runtime_Registration_op_Inequality_Kun_Runtime_Registration_Kun_Runtime_Registration\_}

```csharp
public static bool operator !=(Registration? left, Registration? right)
```

#### Parameters

`left` [Registration](/sdk/api/kun/kun-runtime/registration.md)?

`right` [Registration](/sdk/api/kun/kun-runtime/registration.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
