> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class LocalSignedTransfers

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Storage.Local` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Storage.Local`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Storage.Local)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Storage.Local](/sdk/api/kun/kun-storage-local/index.md)\
Assembly: Kun.Storage.Local.dll

为本地存储签发和执行限时上传、下载请求。键的用途由宿主指定。

```csharp
public sealed class LocalSignedTransfers
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[LocalSignedTransfers](/sdk/api/kun/kun-storage-local/localsignedtransfers.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### LocalSignedTransfers(LocalFileSystemStorage, ReadOnlyMemory\<byte>, TimeProvider, Func\<string, bool>, Func\<string, bool>) \{#Kun_Storage_Local_LocalSignedTransfers__ctor_Kun_Storage_Local_LocalFileSystemStorage_System_ReadOnlyMemory_System_Byte__System_TimeProvider_System_Func_System_String_System_Boolean__System_Func_System_String_System_Boolean\_\_}

使用指定存储、签名密钥、时钟及对象键策略。

```csharp
public LocalSignedTransfers(LocalFileSystemStorage storage, ReadOnlyMemory<byte> signingKey, TimeProvider clock, Func<string, bool> acceptUploadKey, Func<string, bool> acceptDownloadKey)
```

#### Parameters

`storage` [LocalFileSystemStorage](/sdk/api/kun/kun-storage-local/localfilesystemstorage.md)

`signingKey` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory-1)\<[byte](https://learn.microsoft.com/dotnet/api/system.byte)>

`clock` [TimeProvider](https://learn.microsoft.com/dotnet/api/system.timeprovider)

`acceptUploadKey` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [bool](https://learn.microsoft.com/dotnet/api/system.boolean)>

`acceptDownloadKey` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [bool](https://learn.microsoft.com/dotnet/api/system.boolean)>

## Methods

### CreateDownloadAsync(Uri, string, string, TimeSpan, CancellationToken) \{#Kun_Storage_Local_LocalSignedTransfers_CreateDownloadAsync_System_Uri_System_String_System_String_System_TimeSpan_System_Threading_CancellationToken\_}

签发始终以附件形式下载的地址。

```csharp
public ValueTask<PresignedDownloadUrl> CreateDownloadAsync(Uri endpoint, string objectKey, string fileName, TimeSpan lifetime, CancellationToken cancellationToken = default)
```

#### Parameters

`endpoint` Uri

`objectKey` [string](https://learn.microsoft.com/dotnet/api/system.string)

`fileName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`lifetime` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<PresignedDownloadUrl>

### CreateUploadAsync(Uri, string, string, long, TimeSpan, CancellationToken) \{#Kun_Storage_Local_LocalSignedTransfers_CreateUploadAsync_System_Uri_System_String_System_String_System_Int64_System_TimeSpan_System_Threading_CancellationToken\_}

签发限制类型与大小的单次写入地址。

```csharp
public ValueTask<PresignedUploadUrl> CreateUploadAsync(Uri endpoint, string objectKey, string contentType, long maxBytes, TimeSpan lifetime, CancellationToken cancellationToken = default)
```

#### Parameters

`endpoint` Uri

`objectKey` [string](https://learn.microsoft.com/dotnet/api/system.string)

`contentType` [string](https://learn.microsoft.com/dotnet/api/system.string)

`maxBytes` [long](https://learn.microsoft.com/dotnet/api/system.int64)

`lifetime` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<PresignedUploadUrl>

### OpenAsync(LocalTransferRequest, CancellationToken) \{#Kun_Storage_Local_LocalSignedTransfers_OpenAsync_Kun_Storage_Local_LocalTransferRequest_System_Threading_CancellationToken\_}

读取有效下载地址指向的对象及附件响应元数据。

```csharp
public ValueTask<LocalTransferRead?> OpenAsync(LocalTransferRequest request, CancellationToken cancellationToken = default)
```

#### Parameters

`request` [LocalTransferRequest](/sdk/api/kun/kun-storage-local/localtransferrequest.md)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[LocalTransferRead](/sdk/api/kun/kun-storage-local/localtransferread.md)?>

### ReceiveAsync(LocalTransferRequest, Stream, CancellationToken) \{#Kun_Storage_Local_LocalSignedTransfers_ReceiveAsync_Kun_Storage_Local_LocalTransferRequest_System_IO_Stream_System_Threading_CancellationToken\_}

校验签名、有效期、类型和长度，通过后原子创建对象。

```csharp
public ValueTask<LocalTransferWriteResult> ReceiveAsync(LocalTransferRequest request, Stream content, CancellationToken cancellationToken = default)
```

#### Parameters

`request` [LocalTransferRequest](/sdk/api/kun/kun-storage-local/localtransferrequest.md)

`content` [Stream](https://learn.microsoft.com/dotnet/api/system.io.stream)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[LocalTransferWriteResult](/sdk/api/kun/kun-storage-local/localtransferwriteresult.md)>
