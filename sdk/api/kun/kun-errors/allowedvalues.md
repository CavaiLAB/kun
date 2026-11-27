> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class AllowedValues

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors](/sdk/api/kun/kun-errors/index.md)\
Assembly: Kun.Errors.dll

构造 `allowed`（允许的取值，1–100 个 JSON 值）。

```csharp
public static class AllowedValues
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[AllowedValues](/sdk/api/kun/kun-errors/allowedvalues.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### MaxCount \{#Kun_Errors_AllowedValues_MaxCount}

上限，与 contracts/v0 的 `Allowed.maxItems` 一致。

```csharp
public const int MaxCount = 100
```

#### Field Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

## Methods

### Of(params ReadOnlySpan\<string>) \{#Kun_Errors_AllowedValues_Of_System_ReadOnlySpan_System_String\_\_}

字符串取值。

```csharp
public static IReadOnlyList<JsonElement> Of(params ReadOnlySpan<string> values)
```

#### Parameters

`values` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

#### Returns

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)>

### Of(params ReadOnlySpan\<long>) \{#Kun_Errors_AllowedValues_Of_System_ReadOnlySpan_System_Int64\_\_}

整数取值。

```csharp
public static IReadOnlyList<JsonElement> Of(params ReadOnlySpan<long> values)
```

#### Parameters

`values` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan-1)\<[long](https://learn.microsoft.com/dotnet/api/system.int64)>

#### Returns

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)>
