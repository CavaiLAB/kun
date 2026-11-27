> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class NativeDataTable

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Data.Native.Postgres` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Data.Native.Postgres`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Data.Native.Postgres)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Data.Native.Postgres](/sdk/api/kun/kun-data-native-postgres/index.md)\
Assembly: Kun.Data.Native.Postgres.dll

固定迁移契约：精确的 pg_get_expr 输出必须来自受审查的迁移真相源，不能从当前数据库学习后回填。

```csharp
public sealed record NativeDataTable : IEquatable<NativeDataTable>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[NativeDataTable](/sdk/api/kun/kun-data-native-postgres/nativedatatable.md)

#### Implements

[IEquatable\<NativeDataTable>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### NativeDataTable(string, string, string, string, string) \{#Kun_Data_Native_Postgres_NativeDataTable__ctor_System_String_System_String_System_String_System_String_System_String\_}

固定迁移契约：精确的 pg_get_expr 输出必须来自受审查的迁移真相源，不能从当前数据库学习后回填。

```csharp
public NativeDataTable(string Schema, string Name, string GuardPolicy, string ExpectedUsing, string ExpectedCheck)
```

#### Parameters

`Schema` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`GuardPolicy` [string](https://learn.microsoft.com/dotnet/api/system.string)

`ExpectedUsing` [string](https://learn.microsoft.com/dotnet/api/system.string)

`ExpectedCheck` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### ExpectedCheck \{#Kun_Data_Native_Postgres_NativeDataTable_ExpectedCheck}

```csharp
public string ExpectedCheck { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ExpectedUsing \{#Kun_Data_Native_Postgres_NativeDataTable_ExpectedUsing}

```csharp
public string ExpectedUsing { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### GuardPolicy \{#Kun_Data_Native_Postgres_NativeDataTable_GuardPolicy}

```csharp
public string GuardPolicy { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Name \{#Kun_Data_Native_Postgres_NativeDataTable_Name}

```csharp
public string Name { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Schema \{#Kun_Data_Native_Postgres_NativeDataTable_Schema}

```csharp
public string Schema { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### \<Clone>$() \{#Kun_Data_Native_Postgres_NativeDataTable__Clone\_\_}

```csharp
public NativeDataTable <Clone>$()
```

#### Returns

[NativeDataTable](/sdk/api/kun/kun-data-native-postgres/nativedatatable.md)

### Deconstruct(out string, out string, out string, out string, out string) \{#Kun_Data_Native_Postgres_NativeDataTable_Deconstruct_System_String__System_String__System_String__System_String__System_String\_\_}

```csharp
public void Deconstruct(out string Schema, out string Name, out string GuardPolicy, out string ExpectedUsing, out string ExpectedCheck)
```

#### Parameters

`Schema` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`GuardPolicy` [string](https://learn.microsoft.com/dotnet/api/system.string)

`ExpectedUsing` [string](https://learn.microsoft.com/dotnet/api/system.string)

`ExpectedCheck` [string](https://learn.microsoft.com/dotnet/api/system.string)

### Equals(object?) \{#Kun_Data_Native_Postgres_NativeDataTable_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(NativeDataTable?) \{#Kun_Data_Native_Postgres_NativeDataTable_Equals_Kun_Data_Native_Postgres_NativeDataTable\_}

```csharp
public bool Equals(NativeDataTable? other)
```

#### Parameters

`other` [NativeDataTable](/sdk/api/kun/kun-data-native-postgres/nativedatatable.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Data_Native_Postgres_NativeDataTable_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Data_Native_Postgres_NativeDataTable_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(NativeDataTable?, NativeDataTable?) \{#Kun_Data_Native_Postgres_NativeDataTable_op_Equality_Kun_Data_Native_Postgres_NativeDataTable_Kun_Data_Native_Postgres_NativeDataTable\_}

```csharp
public static bool operator ==(NativeDataTable? left, NativeDataTable? right)
```

#### Parameters

`left` [NativeDataTable](/sdk/api/kun/kun-data-native-postgres/nativedatatable.md)?

`right` [NativeDataTable](/sdk/api/kun/kun-data-native-postgres/nativedatatable.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(NativeDataTable?, NativeDataTable?) \{#Kun_Data_Native_Postgres_NativeDataTable_op_Inequality_Kun_Data_Native_Postgres_NativeDataTable_Kun_Data_Native_Postgres_NativeDataTable\_}

```csharp
public static bool operator !=(NativeDataTable? left, NativeDataTable? right)
```

#### Parameters

`left` [NativeDataTable](/sdk/api/kun/kun-data-native-postgres/nativedatatable.md)?

`right` [NativeDataTable](/sdk/api/kun/kun-data-native-postgres/nativedatatable.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
