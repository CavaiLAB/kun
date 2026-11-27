> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class LocalTransferRead

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Storage.Local` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Storage.Local`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Storage.Local)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Storage.Local](/sdk/api/kun/kun-storage-local/index.md)\
Assembly: Kun.Storage.Local.dll

直下内容：响应头取自签名参数与存储时的嗅探类型。

```csharp
public sealed record LocalTransferRead : IEquatable<LocalTransferRead>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[LocalTransferRead](/sdk/api/kun/kun-storage-local/localtransferread.md)

#### Implements

[IEquatable\<LocalTransferRead>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### LocalTransferRead(Stream, long, string, string) \{#Kun_Storage_Local_LocalTransferRead__ctor_System_IO_Stream_System_Int64_System_String_System_String\_}

直下内容：响应头取自签名参数与存储时的嗅探类型。

```csharp
public LocalTransferRead(Stream Content, long Length, string ContentType, string ContentDisposition)
```

#### Parameters

`Content` [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream)

`Length` [long](https://learn.microsoft.com/dotnet/api/system.int64)

`ContentType` [string](https://learn.microsoft.com/dotnet/api/system.string)

`ContentDisposition` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Content \{#Kun_Storage_Local_LocalTransferRead_Content}

```csharp
public Stream Content { get; init; }
```

#### Property Value

[Stream](https://learn.microsoft.com/dotnet/api/system.io.stream)

### ContentDisposition \{#Kun_Storage_Local_LocalTransferRead_ContentDisposition}

```csharp
public string ContentDisposition { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ContentType \{#Kun_Storage_Local_LocalTransferRead_ContentType}

```csharp
public string ContentType { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Length \{#Kun_Storage_Local_LocalTransferRead_Length}

```csharp
public long Length { get; init; }
```

#### Property Value

[long](https://learn.microsoft.com/dotnet/api/system.int64)

## Methods

### \<Clone>$() \{#Kun_Storage_Local_LocalTransferRead__Clone\_\_}

```csharp
public LocalTransferRead <Clone>$()
```

#### Returns

[LocalTransferRead](/sdk/api/kun/kun-storage-local/localtransferread.md)

### Deconstruct(out Stream, out long, out string, out string) \{#Kun_Storage_Local_LocalTransferRead_Deconstruct_System_IO_Stream__System_Int64__System_String__System_String\_\_}

```csharp
public void Deconstruct(out Stream Content, out long Length, out string ContentType, out string ContentDisposition)
```

#### Parameters

`Content` [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream)

`Length` [long](https://learn.microsoft.com/dotnet/api/system.int64)

`ContentType` [string](https://learn.microsoft.com/dotnet/api/system.string)

`ContentDisposition` [string](https://learn.microsoft.com/dotnet/api/system.string)

### Equals(object?) \{#Kun_Storage_Local_LocalTransferRead_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(LocalTransferRead?) \{#Kun_Storage_Local_LocalTransferRead_Equals_Kun_Storage_Local_LocalTransferRead\_}

```csharp
public bool Equals(LocalTransferRead? other)
```

#### Parameters

`other` [LocalTransferRead](/sdk/api/kun/kun-storage-local/localtransferread.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Storage_Local_LocalTransferRead_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Storage_Local_LocalTransferRead_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(LocalTransferRead?, LocalTransferRead?) \{#Kun_Storage_Local_LocalTransferRead_op_Equality_Kun_Storage_Local_LocalTransferRead_Kun_Storage_Local_LocalTransferRead\_}

```csharp
public static bool operator ==(LocalTransferRead? left, LocalTransferRead? right)
```

#### Parameters

`left` [LocalTransferRead](/sdk/api/kun/kun-storage-local/localtransferread.md)?

`right` [LocalTransferRead](/sdk/api/kun/kun-storage-local/localtransferread.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(LocalTransferRead?, LocalTransferRead?) \{#Kun_Storage_Local_LocalTransferRead_op_Inequality_Kun_Storage_Local_LocalTransferRead_Kun_Storage_Local_LocalTransferRead\_}

```csharp
public static bool operator !=(LocalTransferRead? left, LocalTransferRead? right)
```

#### Parameters

`left` [LocalTransferRead](/sdk/api/kun/kun-storage-local/localtransferread.md)?

`right` [LocalTransferRead](/sdk/api/kun/kun-storage-local/localtransferread.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
