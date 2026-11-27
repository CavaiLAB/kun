> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class FieldError

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors](/sdk/api/kun/kun-errors/index.md)\
Assembly: Kun.Errors.dll

`errors[]` 中的一项：字段级或业务错误码 + 出错位置。

```csharp
public sealed class FieldError
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[FieldError](/sdk/api/kun/kun-errors/fielderror.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### FieldError(ErrorCode, string, IReadOnlyDictionary\<string, string>?) \{#Kun_Errors_FieldError__ctor_Kun_Errors_ErrorCode_System_String_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String\_\_}

创建一项字段错误。

```csharp
public FieldError(ErrorCode code, string pointer, IReadOnlyDictionary<string, string>? args = null)
```

#### Parameters

`code` [ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

字段级（[ErrorKind.Field](/sdk/api/kun/kun-errors/errorkind.md#Kun_Errors_ErrorKind_Field)）或业务错误码。

`pointer` [string](https://learn.microsoft.com/dotnet/api/system.string)

出错位置（见 [ErrorPointer](/sdk/api/kun/kun-errors/errorpointer.md)）。

`args` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>?

文案占位符参数。

## Properties

### Allowed \{#Kun_Errors_FieldError_Allowed}

该位置允许的取值。

```csharp
public IReadOnlyList<JsonElement>? Allowed { get; init; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)>?

### Args \{#Kun_Errors_FieldError_Args}

文案参数。

```csharp
public IReadOnlyDictionary<string, string> Args { get; }
```

#### Property Value

[IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>

### Code \{#Kun_Errors_FieldError_Code}

错误码。

```csharp
public ErrorCode Code { get; }
```

#### Property Value

[ErrorCode](/sdk/api/kun/kun-errors/errorcode.md)

### Pointer \{#Kun_Errors_FieldError_Pointer}

出错位置。

```csharp
public string Pointer { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
