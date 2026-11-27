> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class PluginEntry

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

已由清单与配置装配器解析的插件条目。

```csharp
public sealed record PluginEntry : IEquatable<PluginEntry>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)

#### Implements

[IEquatable\<PluginEntry>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### PluginEntry(string, string, string, ScopeKey, int, JsonElement) \{#Kun_Runtime_PluginEntry__ctor_System_String_System_String_System_String_Kun_Runtime_ScopeKey_System_Int32_System_Text_Json_JsonElement\_}

已由清单与配置装配器解析的插件条目。

```csharp
public PluginEntry(string Id, string Plugin, string Version, ScopeKey Scope, int Layer, JsonElement Config)
```

#### Parameters

`Id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Plugin` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Version` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Scope` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

`Layer` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`Config` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

## Properties

### Config \{#Kun_Runtime_PluginEntry_Config}

```csharp
public JsonElement Config { get; init; }
```

#### Property Value

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

### Id \{#Kun_Runtime_PluginEntry_Id}

```csharp
public string Id { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Layer \{#Kun_Runtime_PluginEntry_Layer}

```csharp
public int Layer { get; init; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### Plugin \{#Kun_Runtime_PluginEntry_Plugin}

```csharp
public string Plugin { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Scope \{#Kun_Runtime_PluginEntry_Scope}

```csharp
public ScopeKey Scope { get; init; }
```

#### Property Value

[ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

### Version \{#Kun_Runtime_PluginEntry_Version}

```csharp
public string Version { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### \<Clone>$() \{#Kun_Runtime_PluginEntry__Clone\_\_}

```csharp
public PluginEntry <Clone>$()
```

#### Returns

[PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)

### Deconstruct(out string, out string, out string, out ScopeKey, out int, out JsonElement) \{#Kun_Runtime_PluginEntry_Deconstruct_System_String__System_String__System_String__Kun_Runtime_ScopeKey__System_Int32__System_Text_Json_JsonElement\_\_}

```csharp
public void Deconstruct(out string Id, out string Plugin, out string Version, out ScopeKey Scope, out int Layer, out JsonElement Config)
```

#### Parameters

`Id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Plugin` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Version` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Scope` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

`Layer` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`Config` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

### Equals(object?) \{#Kun_Runtime_PluginEntry_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(PluginEntry?) \{#Kun_Runtime_PluginEntry_Equals_Kun_Runtime_PluginEntry\_}

```csharp
public bool Equals(PluginEntry? other)
```

#### Parameters

`other` [PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Runtime_PluginEntry_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Runtime_PluginEntry_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(PluginEntry?, PluginEntry?) \{#Kun_Runtime_PluginEntry_op_Equality_Kun_Runtime_PluginEntry_Kun_Runtime_PluginEntry\_}

```csharp
public static bool operator ==(PluginEntry? left, PluginEntry? right)
```

#### Parameters

`left` [PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)?

`right` [PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(PluginEntry?, PluginEntry?) \{#Kun_Runtime_PluginEntry_op_Inequality_Kun_Runtime_PluginEntry_Kun_Runtime_PluginEntry\_}

```csharp
public static bool operator !=(PluginEntry? left, PluginEntry? right)
```

#### Parameters

`left` [PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)?

`right` [PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
