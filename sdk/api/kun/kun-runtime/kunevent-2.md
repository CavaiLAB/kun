> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunEvent\<TPayload, TResult>

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

Definition 定义的事件。TResult 是整个分发的结果；扇出与按序模式用 IReadOnlyList\<T>。

```csharp
public sealed record KunEvent<TPayload, TResult> : IEquatable<KunEvent<TPayload, TResult>>
```

#### Type Parameters

`TPayload`

`TResult`

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunEvent\<TPayload, TResult>](/sdk/api/kun/kun-runtime/kunevent-2.md)

#### Implements

[IEquatable\<KunEvent\<TPayload, TResult>>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### KunEvent(string, DispatchMode) \{#Kun_Runtime_KunEvent_2__ctor_System_String_Kun_Runtime_DispatchMode\_}

Definition 定义的事件。TResult 是整个分发的结果；扇出与按序模式用 IReadOnlyList\<T>。

```csharp
public KunEvent(string Name, DispatchMode Mode)
```

#### Parameters

`Name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Mode` [DispatchMode](/sdk/api/kun/kun-runtime/dispatchmode.md)

## Properties

### Mode \{#Kun_Runtime_KunEvent_2_Mode}

```csharp
public DispatchMode Mode { get; init; }
```

#### Property Value

[DispatchMode](/sdk/api/kun/kun-runtime/dispatchmode.md)

### Name \{#Kun_Runtime_KunEvent_2_Name}

```csharp
public string Name { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Terminal \{#Kun_Runtime_KunEvent_2_Terminal}

waterfall 的默认链尾；调用方也可在 DispatchAsync 时覆盖。

```csharp
public Func<TPayload, ValueTask<TResult>>? Terminal { get; init; }
```

#### Property Value

[Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<TPayload, [ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<TResult>>?

## Methods

### \<Clone>$() \{#Kun_Runtime_KunEvent_2__Clone\_\_}

```csharp
public KunEvent<TPayload, TResult> <Clone>$()
```

#### Returns

[KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, TResult>

### Deconstruct(out string, out DispatchMode) \{#Kun_Runtime_KunEvent_2_Deconstruct_System_String__Kun_Runtime_DispatchMode\_\_}

```csharp
public void Deconstruct(out string Name, out DispatchMode Mode)
```

#### Parameters

`Name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Mode` [DispatchMode](/sdk/api/kun/kun-runtime/dispatchmode.md)

### Equals(object?) \{#Kun_Runtime_KunEvent_2_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(KunEvent\<TPayload, TResult>?) \{#Kun_Runtime_KunEvent_2_Equals_Kun_Runtime_KunEvent__0__1\_\_}

```csharp
public bool Equals(KunEvent<TPayload, TResult>? other)
```

#### Parameters

`other` [KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, TResult>?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Runtime_KunEvent_2_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Runtime_KunEvent_2_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(KunEvent\<TPayload, TResult>?, KunEvent\<TPayload, TResult>?) \{#Kun_Runtime_KunEvent_2_op_Equality_Kun_Runtime_KunEvent__0__1__Kun_Runtime_KunEvent__0__1\_\_}

```csharp
public static bool operator ==(KunEvent<TPayload, TResult>? left, KunEvent<TPayload, TResult>? right)
```

#### Parameters

`left` [KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, TResult>?

`right` [KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, TResult>?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(KunEvent\<TPayload, TResult>?, KunEvent\<TPayload, TResult>?) \{#Kun_Runtime_KunEvent_2_op_Inequality_Kun_Runtime_KunEvent__0__1__Kun_Runtime_KunEvent__0__1\_\_}

```csharp
public static bool operator !=(KunEvent<TPayload, TResult>? left, KunEvent<TPayload, TResult>? right)
```

#### Parameters

`left` [KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, TResult>?

`right` [KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, TResult>?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
