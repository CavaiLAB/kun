> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ErrorDetailJson

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors](/sdk/api/kun/kun-errors/index.md)\
Assembly: Kun.Errors.dll

[ErrorDetail](/sdk/api/kun/kun-errors/errordetail.md) 的 JSON 序列化（源生成，AOT 可用；空成员不输出；中文不转义）。

```csharp
public static class ErrorDetailJson
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ErrorDetailJson](/sdk/api/kun/kun-errors/errordetailjson.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### MediaType \{#Kun_Errors_ErrorDetailJson_MediaType}

错误响应的媒体类型。

```csharp
public const string MediaType = "application/problem+json"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### TypeInfo \{#Kun_Errors_ErrorDetailJson_TypeInfo}

序列化元数据，供 `HttpResponse.WriteAsJsonAsync` 等使用。

```csharp
public static JsonTypeInfo<ErrorDetail> TypeInfo { get; }
```

#### Property Value

[JsonTypeInfo](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.metadata.jsontypeinfo-1)\<[ErrorDetail](/sdk/api/kun/kun-errors/errordetail.md)>

## Methods

### Deserialize(string) \{#Kun_Errors_ErrorDetailJson_Deserialize_System_String\_}

从 JSON 文本反序列化。

```csharp
public static ErrorDetail? Deserialize(string json)
```

#### Parameters

`json` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[ErrorDetail](/sdk/api/kun/kun-errors/errordetail.md)?

### Serialize(ErrorDetail) \{#Kun_Errors_ErrorDetailJson_Serialize_Kun_Errors_ErrorDetail\_}

序列化为 JSON 文本。

```csharp
public static string Serialize(ErrorDetail detail)
```

#### Parameters

`detail` [ErrorDetail](/sdk/api/kun/kun-errors/errordetail.md)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
