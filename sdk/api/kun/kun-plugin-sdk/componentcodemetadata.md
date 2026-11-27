> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ComponentCodeMetadata

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

代码派生的组件字段；手写版本、展示、策略和路径仍由唯一 plugin.json 保存。

```csharp
public sealed class ComponentCodeMetadata
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ComponentCodeMetadata](/sdk/api/kun/kun-plugin-sdk/componentcodemetadata.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### ComponentCodeMetadata(string, string, IReadOnlyList\<ServiceKey>, IReadOnlyList\<ServiceKey>, IReadOnlyList\<CapabilityContract>) \{#Kun_Plugin_Sdk_ComponentCodeMetadata__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyList_Kun_Runtime_ServiceKey__System_Collections_Generic_IReadOnlyList_Kun_Runtime_ServiceKey__System_Collections_Generic_IReadOnlyList_Kun_Plugin_Sdk_CapabilityContract\_\_}

复制生成的服务键与能力集合。

```csharp
public ComponentCodeMetadata(string componentId, string runtimeEntry, IReadOnlyList<ServiceKey> inject, IReadOnlyList<ServiceKey> provides, IReadOnlyList<CapabilityContract> capabilities)
```

#### Parameters

`componentId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`runtimeEntry` [string](https://learn.microsoft.com/dotnet/api/system.string)

`inject` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<ServiceKey>

`provides` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<ServiceKey>

`capabilities` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[CapabilityContract](/sdk/api/kun/kun-plugin-sdk/capabilitycontract.md)>

### ComponentCodeMetadata(string, string, IReadOnlyList\<ServiceKey>, IReadOnlyList\<string>, IReadOnlyList\<CapabilityContract>, string, string, string?) \{#Kun_Plugin_Sdk_ComponentCodeMetadata__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyList_Kun_Runtime_ServiceKey__System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyList_Kun_Plugin_Sdk_CapabilityContract__System_String_System_String_System_String\_}

复制完整代码声明；权限与事件使用既有 metadata 表面，配置使用独立 Schema。

```csharp
public ComponentCodeMetadata(string componentId, string runtimeEntry, IReadOnlyList<ServiceKey> inject, IReadOnlyList<string> provides, IReadOnlyList<CapabilityContract> capabilities, string permissionsJson, string eventsJson, string? configSchemaJson)
```

#### Parameters

`componentId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`runtimeEntry` [string](https://learn.microsoft.com/dotnet/api/system.string)

`inject` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<ServiceKey>

`provides` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

`capabilities` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[CapabilityContract](/sdk/api/kun/kun-plugin-sdk/capabilitycontract.md)>

`permissionsJson` [string](https://learn.microsoft.com/dotnet/api/system.string)

`eventsJson` [string](https://learn.microsoft.com/dotnet/api/system.string)

`configSchemaJson` [string](https://learn.microsoft.com/dotnet/api/system.string)?

### ComponentCodeMetadata(string, string, IReadOnlyList\<ServiceKey>, IReadOnlyList\<string>, IReadOnlyList\<CapabilityContract>, string, string, string?, IReadOnlyList\<JobContract>) \{#Kun_Plugin_Sdk_ComponentCodeMetadata__ctor_System_String_System_String_System_Collections_Generic_IReadOnlyList_Kun_Runtime_ServiceKey__System_Collections_Generic_IReadOnlyList_System_String__System_Collections_Generic_IReadOnlyList_Kun_Plugin_Sdk_CapabilityContract__System_String_System_String_System_String_System_Collections_Generic_IReadOnlyList_Kun_Plugin_Sdk_JobContract\_\_}

复制完整声明与可调度能力；既有构造入口保持兼容。

```csharp
public ComponentCodeMetadata(string componentId, string runtimeEntry, IReadOnlyList<ServiceKey> inject, IReadOnlyList<string> provides, IReadOnlyList<CapabilityContract> capabilities, string permissionsJson, string eventsJson, string? configSchemaJson, IReadOnlyList<JobContract> jobs)
```

#### Parameters

`componentId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`runtimeEntry` [string](https://learn.microsoft.com/dotnet/api/system.string)

`inject` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<ServiceKey>

`provides` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

`capabilities` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[CapabilityContract](/sdk/api/kun/kun-plugin-sdk/capabilitycontract.md)>

`permissionsJson` [string](https://learn.microsoft.com/dotnet/api/system.string)

`eventsJson` [string](https://learn.microsoft.com/dotnet/api/system.string)

`configSchemaJson` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`jobs` [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[JobContract](/sdk/api/kun/kun-plugin-sdk/jobcontract.md)>

## Properties

### Capabilities \{#Kun_Plugin_Sdk_ComponentCodeMetadata_Capabilities}

代码中仍存在的能力。

```csharp
public IReadOnlyList<CapabilityContract> Capabilities { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[CapabilityContract](/sdk/api/kun/kun-plugin-sdk/capabilitycontract.md)>

### ComponentId \{#Kun_Plugin_Sdk_ComponentCodeMetadata_ComponentId}

组件稳定 ID。

```csharp
public string ComponentId { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ConfigSchema \{#Kun_Plugin_Sdk_ComponentCodeMetadata_ConfigSchema}

代码声明的配置 Schema；null 表示尚未声明。

```csharp
public JsonElement? ConfigSchema { get; }
```

#### Property Value

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)?

### Events \{#Kun_Plugin_Sdk_ComponentCodeMetadata_Events}

metadata/events.json 的事件声明数组。

```csharp
public JsonElement Events { get; }
```

#### Property Value

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

### Inject \{#Kun_Plugin_Sdk_ComponentCodeMetadata_Inject}

从 Inject 属性派生的必需服务。

```csharp
public IReadOnlyList<string> Inject { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

### Jobs \{#Kun_Plugin_Sdk_ComponentCodeMetadata_Jobs}

代码派生的 local 作业声明。

```csharp
public IReadOnlyList<JobContract> Jobs { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[JobContract](/sdk/api/kun/kun-plugin-sdk/jobcontract.md)>

### Permissions \{#Kun_Plugin_Sdk_ComponentCodeMetadata_Permissions}

metadata/permissions.json 的权限资源数组。

```csharp
public JsonElement Permissions { get; }
```

#### Property Value

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

### Provides \{#Kun_Plugin_Sdk_ComponentCodeMetadata_Provides}

从 Apply 的 Provide 调用派生的服务。

```csharp
public IReadOnlyList<string> Provides { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[string](https://learn.microsoft.com/dotnet/api/system.string)>

### RuntimeEntry \{#Kun_Plugin_Sdk_ComponentCodeMetadata_RuntimeEntry}

实现类型及程序集。

```csharp
public string RuntimeEntry { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
