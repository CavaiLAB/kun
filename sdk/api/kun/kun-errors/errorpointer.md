> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ErrorPointer

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors](/sdk/api/kun/kun-errors/index.md)\
Assembly: Kun.Errors.dll

构造 `pointer`：RFC 6901 JSON Pointer 的 URI 片段形式，可带文档前缀。
请求输入写 `#/items/0/qty`；文件写 `.qian-plugin/plugin.json#/runtime/nuget/0`。

```csharp
public static class ErrorPointer
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ErrorPointer](/sdk/api/kun/kun-errors/errorpointer.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### InFile(string, params ReadOnlySpan\<object>) \{#Kun_Errors_ErrorPointer_InFile_System_String_System_ReadOnlySpan_System_Object\_\_}

指向某个文件内位置的指针，如清单与表面校验错误。

```csharp
public static string InFile(string relativePath, params ReadOnlySpan<object> segments)
```

#### Parameters

`relativePath` [string](https://learn.microsoft.com/dotnet/api/system.string)

以 `/` 分隔的相对路径，不含空白与 `#`。

`segments` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan-1)\<[object](https://learn.microsoft.com/dotnet/api/system.object)>

逐级的属性名或数组下标。

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Input(params ReadOnlySpan\<object>) \{#Kun_Errors_ErrorPointer_Input_System_ReadOnlySpan_System_Object\_\_}

指向请求输入（能力的输入文档）的指针；无参数时指向整个输入（`#`）。

```csharp
public static string Input(params ReadOnlySpan<object> segments)
```

#### Parameters

`segments` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan-1)\<[object](https://learn.microsoft.com/dotnet/api/system.object)>

逐级的属性名或数组下标。

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### IsValid(string?) \{#Kun_Errors_ErrorPointer_IsValid_System_String\_}

是否符合 contracts/v0 的 `Pointer` 形式。

```csharp
public static bool IsValid(string? pointer)
```

#### Parameters

`pointer` [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
