> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class LocalTransferRequest

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Storage.Local` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Storage.Local`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Storage.Local)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Storage.Local](/sdk/api/kun/kun-storage-local/index.md)\
Assembly: Kun.Storage.Local.dll

一次直传 / 直下请求：对象键取自路径，查询参数与请求头原样传入，由网关校验签名。

```csharp
public sealed record LocalTransferRequest : IEquatable<LocalTransferRequest>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[LocalTransferRequest](/sdk/api/kun/kun-storage-local/localtransferrequest.md)

#### Implements

[IEquatable\<LocalTransferRequest>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### LocalTransferRequest(string, IReadOnlyDictionary\<string, string>, IReadOnlyDictionary\<string, string>) \{#Kun_Storage_Local_LocalTransferRequest__ctor_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String\_\_}

一次直传 / 直下请求：对象键取自路径，查询参数与请求头原样传入，由网关校验签名。

```csharp
public LocalTransferRequest(string ObjectKey, IReadOnlyDictionary<string, string> Query, IReadOnlyDictionary<string, string> Headers)
```

#### Parameters

`ObjectKey` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Query` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

`Headers` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

## Properties

### Headers \{#Kun_Storage_Local_LocalTransferRequest_Headers}

```csharp
public IReadOnlyDictionary<string, string> Headers { get; init; }
```

#### Property Value

[IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

### ObjectKey \{#Kun_Storage_Local_LocalTransferRequest_ObjectKey}

```csharp
public string ObjectKey { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Query \{#Kun_Storage_Local_LocalTransferRequest_Query}

```csharp
public IReadOnlyDictionary<string, string> Query { get; init; }
```

#### Property Value

[IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

## Methods

### \<Clone>$() \{#Kun_Storage_Local_LocalTransferRequest__Clone\_\_}

```csharp
public LocalTransferRequest <Clone>$()
```

#### Returns

[LocalTransferRequest](/sdk/api/kun/kun-storage-local/localtransferrequest.md)

### Deconstruct(out string, out IReadOnlyDictionary\<string, string>, out IReadOnlyDictionary\<string, string>) \{#Kun_Storage_Local_LocalTransferRequest_Deconstruct_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String___System_Collections_Generic_IReadOnlyDictionary_System_String_System_String\_\_\_}

```csharp
public void Deconstruct(out string ObjectKey, out IReadOnlyDictionary<string, string> Query, out IReadOnlyDictionary<string, string> Headers)
```

#### Parameters

`ObjectKey` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Query` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

`Headers` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

### Equals(object?) \{#Kun_Storage_Local_LocalTransferRequest_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(LocalTransferRequest?) \{#Kun_Storage_Local_LocalTransferRequest_Equals_Kun_Storage_Local_LocalTransferRequest\_}

```csharp
public bool Equals(LocalTransferRequest? other)
```

#### Parameters

`other` [LocalTransferRequest](/sdk/api/kun/kun-storage-local/localtransferrequest.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Storage_Local_LocalTransferRequest_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Storage_Local_LocalTransferRequest_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(LocalTransferRequest?, LocalTransferRequest?) \{#Kun_Storage_Local_LocalTransferRequest_op_Equality_Kun_Storage_Local_LocalTransferRequest_Kun_Storage_Local_LocalTransferRequest\_}

```csharp
public static bool operator ==(LocalTransferRequest? left, LocalTransferRequest? right)
```

#### Parameters

`left` [LocalTransferRequest](/sdk/api/kun/kun-storage-local/localtransferrequest.md)?

`right` [LocalTransferRequest](/sdk/api/kun/kun-storage-local/localtransferrequest.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(LocalTransferRequest?, LocalTransferRequest?) \{#Kun_Storage_Local_LocalTransferRequest_op_Inequality_Kun_Storage_Local_LocalTransferRequest_Kun_Storage_Local_LocalTransferRequest\_}

```csharp
public static bool operator !=(LocalTransferRequest? left, LocalTransferRequest? right)
```

#### Parameters

`left` [LocalTransferRequest](/sdk/api/kun/kun-storage-local/localtransferrequest.md)?

`right` [LocalTransferRequest](/sdk/api/kun/kun-storage-local/localtransferrequest.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
