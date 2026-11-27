> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class OptionsSchema

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.dll

由源生成 JSON 元数据导出的配置契约，不包含任何运行时配置值。

```csharp
public sealed class OptionsSchema
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[OptionsSchema](/sdk/api/kun/kun-configuration/optionsschema.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### OptionsSchema(string, JsonTypeInfo, string?) \{#Kun_Configuration_OptionsSchema__ctor_System_String_System_Text_Json_Serialization_Metadata_JsonTypeInfo_System_String\_}

由源生成 JSON 元数据导出的配置契约，不包含任何运行时配置值。

```csharp
public OptionsSchema(string section, JsonTypeInfo typeInfo, string? plugin = null)
```

#### Parameters

`section` [string](https://learn.microsoft.com/dotnet/api/system.string)

`typeInfo` [JsonTypeInfo](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.metadata.jsontypeinfo)

`plugin` [string](https://learn.microsoft.com/dotnet/api/system.string)?

## Properties

### OptionsType \{#Kun_Configuration_OptionsSchema_OptionsType}

配置类型全名。

```csharp
public string OptionsType { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Plugin \{#Kun_Configuration_OptionsSchema_Plugin}

登记该契约的组件标识。

```csharp
public string? Plugin { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### Section \{#Kun_Configuration_OptionsSchema_Section}

该配置绑定的 section。

```csharp
public string Section { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### Export() \{#Kun_Configuration_OptionsSchema_Export}

每次返回独立的 JSON Schema 2020-12 节点，调用方修改不会污染缓存。

```csharp
public JsonObject Export()
```

#### Returns

[JsonObject](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonobject)
