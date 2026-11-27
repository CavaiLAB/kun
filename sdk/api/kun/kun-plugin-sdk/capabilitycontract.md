> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class CapabilityContract

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

代码生成的能力声明；只有元数据，不承担请求认证、授权或安装状态判定。

```csharp
public sealed class CapabilityContract
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[CapabilityContract](/sdk/api/kun/kun-plugin-sdk/capabilitycontract.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### CapabilityContract(string, string, JsonTypeInfo, JsonTypeInfo, IReadOnlyList\<string>?) \{#Kun_Plugin_Sdk_CapabilityContract__ctor_System_String_System_String_System_Text_Json_Serialization_Metadata_JsonTypeInfo_System_Text_Json_Serialization_Metadata_JsonTypeInfo_System_Collections_Generic_IReadOnlyList_System_String\_\_}

从已编译的代码声明和源生成序列化契约建立只读快照。

```csharp
public CapabilityContract(string componentId, string metadataJson, JsonTypeInfo request, JsonTypeInfo response, IReadOnlyList<string>? sensitiveProperties = null)
```

#### Parameters

`componentId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`metadataJson` [string](https://learn.microsoft.com/dotnet/api/system.string)

`request` [JsonTypeInfo](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.metadata.jsontypeinfo)

`response` [JsonTypeInfo](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.metadata.jsontypeinfo)

`sensitiveProperties` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>?

## Properties

### ComponentId \{#Kun_Plugin_Sdk_CapabilityContract_ComponentId}

拥有代码的组件。

```csharp
public string ComponentId { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Id \{#Kun_Plugin_Sdk_CapabilityContract_Id}

全局能力标识，component/name。

```csharp
public string Id { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### InputSchema \{#Kun_Plugin_Sdk_CapabilityContract_InputSchema}

由实际序列化器导出的输入 Schema 独立副本。

```csharp
public JsonNode InputSchema { get; }
```

#### Property Value

[JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

### Metadata \{#Kun_Plugin_Sdk_CapabilityContract_Metadata}

不可变声明的独立 JSON 副本。

```csharp
public JsonElement Metadata { get; }
```

#### Property Value

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

### Name \{#Kun_Plugin_Sdk_CapabilityContract_Name}

组件内短名。

```csharp
public string Name { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### OutputSchema \{#Kun_Plugin_Sdk_CapabilityContract_OutputSchema}

由实际序列化器导出的输出 Schema 独立副本。

```csharp
public JsonNode OutputSchema { get; }
```

#### Property Value

[JsonNode](https://learn.microsoft.com/dotnet/api/system.text.json.nodes.jsonnode)

### RequestTypeInfo \{#Kun_Plugin_Sdk_CapabilityContract_RequestTypeInfo}

请求的源生成元数据。

```csharp
public JsonTypeInfo RequestTypeInfo { get; }
```

#### Property Value

[JsonTypeInfo](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.metadata.jsontypeinfo)

### ResponseTypeInfo \{#Kun_Plugin_Sdk_CapabilityContract_ResponseTypeInfo}

响应的源生成元数据。

```csharp
public JsonTypeInfo ResponseTypeInfo { get; }
```

#### Property Value

[JsonTypeInfo](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.metadata.jsontypeinfo)
