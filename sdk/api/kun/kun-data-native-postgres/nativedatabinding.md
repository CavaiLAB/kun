> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class NativeDataBinding

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Data.Native.Postgres` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Data.Native.Postgres`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Data.Native.Postgres)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Data.Native.Postgres](/sdk/api/kun/kun-data-native-postgres/index.md)\
Assembly: Kun.Data.Native.Postgres.dll

受信任宿主的具名连接绑定。凭据工厂仅在组件挂载时读取，密钥轮换后重挂载；对象打印不暴露工厂或凭据。

```csharp
public sealed record NativeDataBinding : IEquatable<NativeDataBinding>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[NativeDataBinding](/sdk/api/kun/kun-data-native-postgres/nativedatabinding.md)

#### Implements

[IEquatable\<NativeDataBinding>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### NativeDataBinding(string, string, NativeDataAccess, Func\<CancellationToken, ValueTask\<string>>, IReadOnlyList\<NativeDataTable>) \{#Kun_Data_Native_Postgres_NativeDataBinding__ctor_System_String_System_String_Kun_Data_Native_Postgres_NativeDataAccess_System_Func_System_Threading_CancellationToken_System_Threading_Tasks_ValueTask_System_String___System_Collections_Generic_IReadOnlyList_Kun_Data_Native_Postgres_NativeDataTable\_\_}

受信任宿主的具名连接绑定。凭据工厂仅在组件挂载时读取，密钥轮换后重挂载；对象打印不暴露工厂或凭据。

```csharp
public NativeDataBinding(string Name, string Role, NativeDataAccess Access, Func<CancellationToken, ValueTask<string>> ConnectionString, IReadOnlyList<NativeDataTable> Tables)
```

#### Parameters

`Name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Role` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Access` [NativeDataAccess](/sdk/api/kun/kun-data-native-postgres/nativedataaccess.md)

`ConnectionString` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken), [ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>>

`Tables` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[NativeDataTable](/sdk/api/kun/kun-data-native-postgres/nativedatatable.md)>

## Properties

### Access \{#Kun_Data_Native_Postgres_NativeDataBinding_Access}

```csharp
public NativeDataAccess Access { get; init; }
```

#### Property Value

[NativeDataAccess](/sdk/api/kun/kun-data-native-postgres/nativedataaccess.md)

### ConnectionString \{#Kun_Data_Native_Postgres_NativeDataBinding_ConnectionString}

```csharp
public Func<CancellationToken, ValueTask<string>> ConnectionString { get; init; }
```

#### Property Value

[Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken), [ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>>

### Name \{#Kun_Data_Native_Postgres_NativeDataBinding_Name}

```csharp
public string Name { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Role \{#Kun_Data_Native_Postgres_NativeDataBinding_Role}

```csharp
public string Role { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Tables \{#Kun_Data_Native_Postgres_NativeDataBinding_Tables}

```csharp
public IReadOnlyList<NativeDataTable> Tables { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[NativeDataTable](/sdk/api/kun/kun-data-native-postgres/nativedatatable.md)>

## Methods

### \<Clone>$() \{#Kun_Data_Native_Postgres_NativeDataBinding__Clone\_\_}

```csharp
public NativeDataBinding <Clone>$()
```

#### Returns

[NativeDataBinding](/sdk/api/kun/kun-data-native-postgres/nativedatabinding.md)

### Deconstruct(out string, out string, out NativeDataAccess, out Func\<CancellationToken, ValueTask\<string>>, out IReadOnlyList\<NativeDataTable>) \{#Kun_Data_Native_Postgres_NativeDataBinding_Deconstruct_System_String__System_String__Kun_Data_Native_Postgres_NativeDataAccess__System_Func_System_Threading_CancellationToken_System_Threading_Tasks_ValueTask_System_String____System_Collections_Generic_IReadOnlyList_Kun_Data_Native_Postgres_NativeDataTable\_\_\_}

```csharp
public void Deconstruct(out string Name, out string Role, out NativeDataAccess Access, out Func<CancellationToken, ValueTask<string>> ConnectionString, out IReadOnlyList<NativeDataTable> Tables)
```

#### Parameters

`Name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Role` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Access` [NativeDataAccess](/sdk/api/kun/kun-data-native-postgres/nativedataaccess.md)

`ConnectionString` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken), [ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>>

`Tables` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[NativeDataTable](/sdk/api/kun/kun-data-native-postgres/nativedatatable.md)>

### Equals(object?) \{#Kun_Data_Native_Postgres_NativeDataBinding_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(NativeDataBinding?) \{#Kun_Data_Native_Postgres_NativeDataBinding_Equals_Kun_Data_Native_Postgres_NativeDataBinding\_}

```csharp
public bool Equals(NativeDataBinding? other)
```

#### Parameters

`other` [NativeDataBinding](/sdk/api/kun/kun-data-native-postgres/nativedatabinding.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Data_Native_Postgres_NativeDataBinding_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Data_Native_Postgres_NativeDataBinding_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(NativeDataBinding?, NativeDataBinding?) \{#Kun_Data_Native_Postgres_NativeDataBinding_op_Equality_Kun_Data_Native_Postgres_NativeDataBinding_Kun_Data_Native_Postgres_NativeDataBinding\_}

```csharp
public static bool operator ==(NativeDataBinding? left, NativeDataBinding? right)
```

#### Parameters

`left` [NativeDataBinding](/sdk/api/kun/kun-data-native-postgres/nativedatabinding.md)?

`right` [NativeDataBinding](/sdk/api/kun/kun-data-native-postgres/nativedatabinding.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(NativeDataBinding?, NativeDataBinding?) \{#Kun_Data_Native_Postgres_NativeDataBinding_op_Inequality_Kun_Data_Native_Postgres_NativeDataBinding_Kun_Data_Native_Postgres_NativeDataBinding\_}

```csharp
public static bool operator !=(NativeDataBinding? left, NativeDataBinding? right)
```

#### Parameters

`left` [NativeDataBinding](/sdk/api/kun/kun-data-native-postgres/nativedatabinding.md)?

`right` [NativeDataBinding](/sdk/api/kun/kun-data-native-postgres/nativedatabinding.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
