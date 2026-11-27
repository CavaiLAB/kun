> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ErrorCode

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors](/sdk/api/kun/kun-errors/index.md)\
Assembly: Kun.Errors.dll

一个登记在错误码目录里的稳定错误码：类别、状态码、归属组件与多语言文案。
组件在自己的 Definition 包里以 `static readonly` 字段定义，挂载时登记到 [ErrorCatalog](/sdk/api/kun/kun-errors/errorcatalog.md)。

```csharp
public sealed class ErrorCode : IEquatable<ErrorCode>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

#### Implements

[IEquatable\<ErrorCode>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Remarks

文案以 BCP 47 语言标签为键，`zh-CN` 必填；文案模板里的 `{name}` 是占位符，
各语言的占位符集合必须相同，出错方按名字提供参数（[Error](/sdk/api/kun/kun-errors/error.md)）。

## Fields

### DefaultLocale \{#Kun_Errors_ErrorCode_DefaultLocale}

必填的默认语言。

```csharp
public const string DefaultLocale = "zh-CN"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TypeBaseUri \{#Kun_Errors_ErrorCode_TypeBaseUri}

问题类型 URI 的前缀：`https://qian.dev/problems/<kind>/<code>`。

```csharp
public const string TypeBaseUri = "https://qian.dev/problems/"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Code \{#Kun_Errors_ErrorCode_Code}

稳定错误码（大写下划线），全局唯一。

```csharp
public string Code { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Kind \{#Kun_Errors_ErrorCode_Kind}

类别。

```csharp
public ErrorKind Kind { get; }
```

#### Property Value

[ErrorKind](/sdk/api/kun/kun-errors/errorkind.md)

### Message \{#Kun_Errors_ErrorCode_Message}

文案模板：语言标签 → 模板（按语言标签排序）。

```csharp
public IReadOnlyDictionary<string, string> Message { get; }
```

#### Property Value

[IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

### Owner \{#Kun_Errors_ErrorCode_Owner}

登记该错误码的组件 id。

```csharp
public string Owner { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Parameters \{#Kun_Errors_ErrorCode_Parameters}

文案模板里的占位符名（按名称排序）。

```csharp
public IReadOnlyList<string> Parameters { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

### Retryable \{#Kun_Errors_ErrorCode_Retryable}

以同一幂等键原样重试是否可能成功。

```csharp
public bool Retryable { get; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Status \{#Kun_Errors_ErrorCode_Status}

HTTP 状态码；[ErrorKind.Field](/sdk/api/kun/kun-errors/errorkind.md#Kun_Errors_ErrorKind_Field) 没有状态码。

```csharp
public int? Status { get; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)?

### Title \{#Kun_Errors_ErrorCode_Title}

标题：语言标签 → 文本（按语言标签排序）。

```csharp
public IReadOnlyDictionary<string, string> Title { get; }
```

#### Property Value

[IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

### Type \{#Kun_Errors_ErrorCode_Type}

问题类型 URI；[ErrorKind.Field](/sdk/api/kun/kun-errors/errorkind.md#Kun_Errors_ErrorKind_Field) 为 null。

```csharp
public string? Type { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

## Methods

### Business(string, int, string, IReadOnlyDictionary\<string, string>, IReadOnlyDictionary\<string, string>, bool) \{#Kun_Errors_ErrorCode_Business_System_String_System_Int32_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Boolean\_}

定义业务错误码。状态码只能是 409（状态冲突）或 422（业务规则不满足）。

```csharp
public static ErrorCode Business(string code, int status, string owner, IReadOnlyDictionary<string, string> title, IReadOnlyDictionary<string, string> message, bool retryable = false)
```

#### Parameters

`code` [string](https://learn.microsoft.com/dotnet/api/system.string)

`status` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`owner` [string](https://learn.microsoft.com/dotnet/api/system.string)

`title` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

`message` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

`retryable` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Returns

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### EnsureArguments(IReadOnlyDictionary\<string, string>?) \{#Kun_Errors_ErrorCode_EnsureArguments_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String\_\_}

检查参数是否覆盖全部占位符。

```csharp
public void EnsureArguments(IReadOnlyDictionary<string, string>? args)
```

#### Parameters

`args` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>?

#### Exceptions

[ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

缺少占位符参数。

### Equals(ErrorCode?) \{#Kun_Errors_ErrorCode_Equals_Kun_Errors_ErrorCode\_}

```csharp
public bool Equals(ErrorCode? other)
```

#### Parameters

`other` [ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(object?) \{#Kun_Errors_ErrorCode_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Field(string, string, IReadOnlyDictionary\<string, string>, IReadOnlyDictionary\<string, string>) \{#Kun_Errors_ErrorCode_Field_System_String_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String\_\_}

定义字段级错误码（只用于 `errors[]`）。

```csharp
public static ErrorCode Field(string code, string owner, IReadOnlyDictionary<string, string> title, IReadOnlyDictionary<string, string> message)
```

#### Parameters

`code` [string](https://learn.microsoft.com/dotnet/api/system.string)

`owner` [string](https://learn.microsoft.com/dotnet/api/system.string)

`title` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

`message` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

#### Returns

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### FormatMessage(CultureInfo?, IReadOnlyDictionary\<string, string>?) \{#Kun_Errors_ErrorCode_FormatMessage_System_Globalization_CultureInfo_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String\_\_}

按语言渲染文案；`args` 必须覆盖 [ErrorCode.Parameters](/sdk/api/kun/kun-errors/errorcode.md#Kun_Errors_ErrorCode_Parameters)。

```csharp
public string FormatMessage(CultureInfo? culture = null, IReadOnlyDictionary<string, string>? args = null)
```

#### Parameters

`culture` [CultureInfo](https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo)?

`args` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>?

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

#### Exceptions

[ArgumentException](https://learn.microsoft.com/dotnet/api/system.argumentexception)

缺少占位符参数。

### GetHashCode() \{#Kun_Errors_ErrorCode_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### GetTitle(CultureInfo?) \{#Kun_Errors_ErrorCode_GetTitle_System_Globalization_CultureInfo\_}

按语言取标题：精确匹配 → 逐级父语言（en-US → en）→ zh-CN。

```csharp
public string GetTitle(CultureInfo? culture = null)
```

#### Parameters

`culture` [CultureInfo](https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo)?

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### IsBusinessStatus(int) \{#Kun_Errors_ErrorCode_IsBusinessStatus_System_Int32\_}

业务错误专用的状态码：409、422。

```csharp
public static bool IsBusinessStatus(int status)
```

#### Parameters

`status` [int](https://learn.microsoft.com/dotnet/api/system.int32)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Protocol(string, int, string, IReadOnlyDictionary\<string, string>, IReadOnlyDictionary\<string, string>, bool) \{#Kun_Errors_ErrorCode_Protocol_System_String_System_Int32_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Collections_Generic_IReadOnlyDictionary_System_String_System_String__System_Boolean\_}

定义协议错误码。状态码为 4xx / 5xx，且不是 409 / 422。

```csharp
public static ErrorCode Protocol(string code, int status, string owner, IReadOnlyDictionary<string, string> title, IReadOnlyDictionary<string, string> message, bool retryable = false)
```

#### Parameters

`code` [string](https://learn.microsoft.com/dotnet/api/system.string)

`status` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`owner` [string](https://learn.microsoft.com/dotnet/api/system.string)

`title` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

`message` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

`retryable` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

#### Returns

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### ResolveLocale(CultureInfo?) \{#Kun_Errors_ErrorCode_ResolveLocale_System_Globalization_CultureInfo\_}

该语言实际取用的文案语言标签（用于 `Content-Language`），规则同 [ErrorCode.GetTitle](/sdk/api/kun/kun-errors/errorcode.md#Kun_Errors_ErrorCode_GetTitle_System_Globalization_CultureInfo_)。

```csharp
public string ResolveLocale(CultureInfo? culture = null)
```

#### Parameters

`culture` [CultureInfo](https://learn.microsoft.com/dotnet/api/system.globalization.cultureinfo)?

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ToString() \{#Kun_Errors_ErrorCode_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
