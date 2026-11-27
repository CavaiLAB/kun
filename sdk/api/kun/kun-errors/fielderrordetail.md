> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class FieldErrorDetail

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors](/sdk/api/kun/kun-errors/index.md)\
Assembly: Kun.Errors.dll

[ErrorDetail.Errors](/sdk/api/kun/kun-errors/errordetail.md#Kun_Errors_ErrorDetail_Errors) 中的一项。

```csharp
public sealed class FieldErrorDetail
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[FieldErrorDetail](/sdk/api/kun/kun-errors/fielderrordetail.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### FieldErrorDetail() \{#Kun_Errors_FieldErrorDetail__ctor}

```csharp
public FieldErrorDetail()
```

## Properties

### Allowed \{#Kun_Errors_FieldErrorDetail_Allowed}

该位置允许的取值。

```csharp
[JsonPropertyName("allowed")]
[JsonPropertyOrder(3)]
public IReadOnlyList<JsonElement>? Allowed { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)>?

### Code \{#Kun_Errors_FieldErrorDetail_Code}

错误码。

```csharp
[JsonPropertyName("code")]
[JsonPropertyOrder(0)]
public required string Code { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Message \{#Kun_Errors_FieldErrorDetail_Message}

说明（已本地化）。

```csharp
[JsonPropertyName("message")]
[JsonPropertyOrder(1)]
public required string Message { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Pointer \{#Kun_Errors_FieldErrorDetail_Pointer}

出错位置。

```csharp
[JsonPropertyName("pointer")]
[JsonPropertyOrder(2)]
public required string Pointer { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
