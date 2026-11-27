> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class Error

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors](/sdk/api/kun/kun-errors/index.md)\
Assembly: Kun.Errors.dll

一次出错：哪个错误码、文案参数、出错位置与允许值。进程内由 `Kun.Core.Results.Result` 携带或随
[ErrorException](/sdk/api/kun/kun-errors/errorexception.md) 抛出；对外由 [ErrorCatalog.CreateDetail](/sdk/api/kun/kun-errors/errorcatalog.md#Kun_Errors_ErrorCatalog_CreateDetail_Kun_Errors_Error_System_String_System_Globalization_CultureInfo_System_String_) 渲染为 [ErrorDetail](/sdk/api/kun/kun-errors/errordetail.md)。

```csharp
public sealed class Error
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[Error](/sdk/api/kun/kun-errors/error.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Remarks

字段错误有多个时放进 [Error.Errors](/sdk/api/kun/kun-errors/error.md#Kun_Errors_Error_Errors)，此时顶层 [Error.Pointer](/sdk/api/kun/kun-errors/error.md#Kun_Errors_Error_Pointer) / [Error.Allowed](/sdk/api/kun/kun-errors/error.md#Kun_Errors_Error_Allowed) 留空，
由渲染取第一项；只有一个出错位置的业务错误直接设 [Error.Pointer](/sdk/api/kun/kun-errors/error.md#Kun_Errors_Error_Pointer)。

## Constructors

### Error(ErrorCode, IReadOnlyDictionary\<string, string>?) \{#Kun_Errors_Error__ctor_Kun_Errors_ErrorCode_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String\_\_}

创建一次出错。

```csharp
public Error(ErrorCode code, IReadOnlyDictionary<string, string>? args = null)
```

#### Parameters

`code` [ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

协议或业务错误码；字段级错误码只能放进 [Error.Errors](/sdk/api/kun/kun-errors/error.md#Kun_Errors_Error_Errors)。

`args` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>?

文案占位符参数，必须覆盖 [ErrorCode.Parameters](/sdk/api/kun/kun-errors/errorcode.md#Kun_Errors_ErrorCode_Parameters)。

## Properties

### Allowed \{#Kun_Errors_Error_Allowed}

该位置允许的取值（见 [AllowedValues](/sdk/api/kun/kun-errors/allowedvalues.md)）。

```csharp
public IReadOnlyList<JsonElement>? Allowed { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)>?

### Args \{#Kun_Errors_Error_Args}

文案参数。

```csharp
public IReadOnlyDictionary<string, string> Args { get; }
```

#### Property Value

[IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

### Code \{#Kun_Errors_Error_Code}

错误码。

```csharp
public ErrorCode Code { get; }
```

#### Property Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### Errors \{#Kun_Errors_Error_Errors}

逐项的字段错误，最多 100 项。

```csharp
public IReadOnlyList<FieldError> Errors { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[FieldError](/sdk/api/kun/kun-errors/fielderror.md)>

### Message \{#Kun_Errors_Error_Message}

按当前界面语言（[`CurrentUICulture`](https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo.currentuiculture)）渲染的文案，找不到时回退 zh-CN。
供日志、gRPC 状态说明这类进程内用途；HTTP 响应体由错误码目录渲染（[ErrorCatalog.CreateDetail](/sdk/api/kun/kun-errors/errorcatalog.md#Kun_Errors_ErrorCatalog_CreateDetail_Kun_Errors_Error_System_String_System_Globalization_CultureInfo_System_String_)）。

```csharp
public string Message { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Pointer \{#Kun_Errors_Error_Pointer}

出错的输入位置（见 [ErrorPointer](/sdk/api/kun/kun-errors/errorpointer.md)）。

```csharp
public string? Pointer { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?
