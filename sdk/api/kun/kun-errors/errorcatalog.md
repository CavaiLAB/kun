> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ErrorCatalog

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors](/sdk/api/kun/kun-errors/index.md)\
Assembly: Kun.Errors.dll

错误码目录：全局唯一的错误码登记处，渲染 [ErrorDetail](/sdk/api/kun/kun-errors/errordetail.md)，并导出
contracts/v0/error-catalog.schema.json 格式的 JSON（错误码与多语言文案）。

```csharp
public sealed class ErrorCatalog
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ErrorCatalog](/sdk/api/kun/kun-errors/errorcatalog.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Remarks

同一错误码只能有一份定义：再次登记完全相同的定义按引用计数共享，定义不同即失败。
[ErrorCatalog.Register](/sdk/api/kun/kun-errors/errorcatalog.md#Kun_Errors_ErrorCatalog_Register_Kun_Errors_ErrorCode_) 返回的释放函数撤销本次登记，供插件卸载时逆序撤销（ctx.Effect）。线程安全。

## Constructors

### ErrorCatalog() \{#Kun_Errors_ErrorCatalog__ctor}

```csharp
public ErrorCatalog()
```

## Properties

### Codes \{#Kun_Errors_ErrorCatalog_Codes}

当前登记的全部错误码，按 code 排序。

```csharp
public IReadOnlyList<ErrorCode> Codes { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)>

## Methods

### Contains(ErrorCode) \{#Kun_Errors_ErrorCatalog_Contains_Kun_Errors_ErrorCode\_}

该定义是否已登记。

```csharp
public bool Contains(ErrorCode code)
```

#### Parameters

`code` [ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### CreateDefault() \{#Kun_Errors_ErrorCatalog_CreateDefault}

含平台内置错误码（[PlatformErrors.All](/sdk/api/kun/kun-errors/platformerrors.md#Kun_Errors_PlatformErrors_All)）的目录。

```csharp
public static ErrorCatalog CreateDefault()
```

#### Returns

[ErrorCatalog](/sdk/api/kun/kun-errors/errorcatalog.md)

### CreateDetail(Error, string, CultureInfo?, string?) \{#Kun_Errors_ErrorCatalog_CreateDetail_Kun_Errors_Error_System_String_System_Globalization_CultureInfo_System_String\_}

把一次出错渲染为错误响应体。

```csharp
public ErrorDetail CreateDetail(Error error, string traceId, CultureInfo? culture = null, string? instance = null)
```

#### Parameters

`error` [Error](/sdk/api/kun/kun-errors/error.md)

出错内容；其错误码与字段错误码都必须已登记。

`traceId` [string](https://learn.microsoft.com/dotnet/api/system.string)

W3C trace-id（32 位小写十六进制）。

`culture` [CultureInfo](https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo)?

文案语言；默认 [`CurrentUICulture`](https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo.currentuiculture)，找不到时回退 zh-CN。

`instance` [string](https://learn.microsoft.com/dotnet/api/system.string)?

出错的请求路径。

#### Returns

[ErrorDetail](/sdk/api/kun/kun-errors/errordetail.md)

#### Exceptions

[InvalidOperationException](https://learn.microsoft.com/dotnet/api/system.invalidoperationexception)

错误码未登记。

### ExportJson() \{#Kun_Errors_ErrorCatalog_ExportJson}

导出全部错误码与多语言文案（error-catalog.schema.json 格式，缩进两格、按 code 排序、以换行结尾）。

```csharp
public string ExportJson()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Register(ErrorCode) \{#Kun_Errors_ErrorCatalog_Register_Kun_Errors_ErrorCode\_}

登记一个错误码。

```csharp
public IDisposable Register(ErrorCode code)
```

#### Parameters

`code` [ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

撤销本次登记的释放函数。

#### Exceptions

[InvalidOperationException](https://learn.microsoft.com/dotnet/api/system.invalidoperationexception)

同名错误码已有不同定义。

### TryGet(string, out ErrorCode?) \{#Kun_Errors_ErrorCatalog_TryGet_System_String_Kun_Errors_ErrorCode\_\_}

按 code 查找。

```csharp
public bool TryGet(string code, out ErrorCode? errorCode)
```

#### Parameters

`code` [string](https://learn.microsoft.com/dotnet/api/system.string)

`errorCode` [ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
