> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class JsonSchemaReferences

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

源生成与实际 HTTP 描述共用的 JSON Schema 引用处理。

```csharp
public static class JsonSchemaReferences
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[JsonSchemaReferences](/sdk/api/kun/kun-plugin-sdk/jsonschemareferences.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### RebaseToComponent(JsonElement, string) \{#Kun_Plugin_Sdk_JsonSchemaReferences_RebaseToComponent_System_Text_Json_JsonElement_System_String\_}

嵌入 OpenAPI components 时重定位本地引用并等价展开布尔 Schema，保持实例 examples/default 原样。

```csharp
public static JsonNode RebaseToComponent(JsonElement schema, string key)
```

#### Parameters

`schema` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

`key` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)
