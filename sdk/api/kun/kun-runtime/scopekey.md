> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Struct ScopeKey

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

两层扁平作用域：[ScopeKey.Global](/sdk/api/kun/kun-runtime/scopekey.md#Kun_Runtime_ScopeKey_Global)，或「种类 + 非空 id」；作用域之间没有继承关系。
运行时只查语法（种类是小写字母开头、由小写字母数字与连字符组成的名字），种类有哪些由平台档定。

```csharp
public readonly struct ScopeKey : IEquatable<ScopeKey>
```

#### Implements

[IEquatable\<ScopeKey>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### ScopeKey(string, string) \{#Kun_Runtime_ScopeKey__ctor_System_String_System_String\_}

两层扁平作用域：[ScopeKey.Global](/sdk/api/kun/kun-runtime/scopekey.md#Kun_Runtime_ScopeKey_Global)，或「种类 + 非空 id」；作用域之间没有继承关系。
运行时只查语法（种类是小写字母开头、由小写字母数字与连字符组成的名字），种类有哪些由平台档定。

```csharp
public ScopeKey(string Kind, string Id)
```

#### Parameters

`Kind` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Id` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Fields

### Global \{#Kun_Runtime_ScopeKey_Global}

全局注册的作用域。

```csharp
public static readonly ScopeKey Global
```

#### Field Value

[ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

## Properties

### Id \{#Kun_Runtime_ScopeKey_Id}

```csharp
public string Id { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Kind \{#Kun_Runtime_ScopeKey_Kind}

```csharp
public string Kind { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### Deconstruct(out string, out string) \{#Kun_Runtime_ScopeKey_Deconstruct_System_String__System_String\_\_}

```csharp
public void Deconstruct(out string Kind, out string Id)
```

#### Parameters

`Kind` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Id` [string](https://learn.microsoft.com/dotnet/api/system.string)

### Equals(object) \{#Kun_Runtime_ScopeKey_Equals_System_Object\_}

```csharp
public override bool Equals(object obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(ScopeKey) \{#Kun_Runtime_ScopeKey_Equals_Kun_Runtime_ScopeKey\_}

```csharp
public bool Equals(ScopeKey other)
```

#### Parameters

`other` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Runtime_ScopeKey_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Runtime_ScopeKey_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(ScopeKey, ScopeKey) \{#Kun_Runtime_ScopeKey_op_Equality_Kun_Runtime_ScopeKey_Kun_Runtime_ScopeKey\_}

```csharp
public static bool operator ==(ScopeKey left, ScopeKey right)
```

#### Parameters

`left` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

`right` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(ScopeKey, ScopeKey) \{#Kun_Runtime_ScopeKey_op_Inequality_Kun_Runtime_ScopeKey_Kun_Runtime_ScopeKey\_}

```csharp
public static bool operator !=(ScopeKey left, ScopeKey right)
```

#### Parameters

`left` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

`right` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
