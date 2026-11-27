> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ErrorDetail

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors](/sdk/api/kun/kun-errors/index.md)\
Assembly: Kun.Errors.dll

错误响应体（contracts/v0/error-detail.schema.json）：RFC 9457 problem+json 的 Kun 配置文件。
由 [ErrorCatalog.CreateDetail](/sdk/api/kun/kun-errors/errorcatalog.md#Kun_Errors_ErrorCatalog_CreateDetail_Kun_Errors_Error_System_String_System_Globalization_CultureInfo_System_String_) 生成；序列化用 [ErrorDetailJson](/sdk/api/kun/kun-errors/errordetailjson.md)。
v0 已冻结，只接受向后兼容增量；冻结标记是 Schema 元数据，不属于错误响应体。

```csharp
public sealed class ErrorDetail
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ErrorDetail](/sdk/api/kun/kun-errors/errordetail.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### ErrorDetail() \{#Kun_Errors_ErrorDetail__ctor}

```csharp
public ErrorDetail()
```

## Properties

### Allowed \{#Kun_Errors_ErrorDetail_Allowed}

该位置允许的取值。

```csharp
[JsonPropertyName("allowed")]
[JsonPropertyOrder(9)]
public IReadOnlyList<JsonElement>? Allowed { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)>?

### Code \{#Kun_Errors_ErrorDetail_Code}

稳定错误码。

```csharp
[JsonPropertyName("code")]
[JsonPropertyOrder(5)]
public required string Code { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Detail \{#Kun_Errors_ErrorDetail_Detail}

RFC 9457 通用客户端读取的说明，与 [ErrorDetail.Message](/sdk/api/kun/kun-errors/errordetail.md#Kun_Errors_ErrorDetail_Message) 相同。

```csharp
[JsonPropertyName("detail")]
[JsonPropertyOrder(3)]
public string? Detail { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### Errors \{#Kun_Errors_ErrorDetail_Errors}

逐项的字段错误。

```csharp
[JsonPropertyName("errors")]
[JsonPropertyOrder(10)]
public IReadOnlyList<FieldErrorDetail>? Errors { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[FieldErrorDetail](/sdk/api/kun/kun-errors/fielderrordetail.md)>?

### Instance \{#Kun_Errors_ErrorDetail_Instance}

出错的请求路径。

```csharp
[JsonPropertyName("instance")]
[JsonPropertyOrder(4)]
public string? Instance { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### Message \{#Kun_Errors_ErrorDetail_Message}

说明（已本地化，由文案模板渲染）。

```csharp
[JsonPropertyName("message")]
[JsonPropertyOrder(6)]
public required string Message { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Pointer \{#Kun_Errors_ErrorDetail_Pointer}

出错的输入位置。

```csharp
[JsonPropertyName("pointer")]
[JsonPropertyOrder(8)]
public string? Pointer { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### Status \{#Kun_Errors_ErrorDetail_Status}

HTTP 状态码。

```csharp
[JsonPropertyName("status")]
[JsonPropertyOrder(2)]
public required int Status { get; init; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### Title \{#Kun_Errors_ErrorDetail_Title}

问题类型的简短摘要（已本地化）。

```csharp
[JsonPropertyName("title")]
[JsonPropertyOrder(1)]
public required string Title { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TraceId \{#Kun_Errors_ErrorDetail_TraceId}

W3C trace-id（32 位小写十六进制）。

```csharp
[JsonPropertyName("trace_id")]
[JsonPropertyOrder(7)]
public required string TraceId { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Type \{#Kun_Errors_ErrorDetail_Type}

问题类型 URI：`https://qian.dev/problems/<protocol|business>/<code>`。

```csharp
[JsonPropertyName("type")]
[JsonPropertyOrder(0)]
public required string Type { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
