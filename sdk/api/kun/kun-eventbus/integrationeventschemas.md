> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class IntegrationEventSchemas

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.dll

由登记时的源生成 JsonTypeInfo 导出 payload 的 JSON Schema（2020-12），与 Outbox 实际写出的 JSON 同一份契约；
不用反射序列化补位：没有 JsonTypeInfo 的事件不能发布，也不进目录。

```csharp
public static class IntegrationEventSchemas
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[IntegrationEventSchemas](/sdk/api/kun/kun-eventbus/integrationeventschemas.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### Describe(IEventTypeRegistry) \{#Kun_EventBus_IntegrationEventSchemas_Describe_Kun_EventBus_IEventTypeRegistry\_}

导出 Kun 事件目录中每个事件的描述，按事件 ID 排序；任一事件不合规即拒绝。

```csharp
public static IReadOnlyList<IntegrationEventDescriptor> Describe(IEventTypeRegistry registry)
```

#### Parameters

`registry` IEventTypeRegistry

#### Returns

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<IntegrationEventDescriptor>

### Export(JsonTypeInfo) \{#Kun_EventBus_IntegrationEventSchemas_Export_System_Text_Json_Serialization_Metadata_JsonTypeInfo\_}

从 JsonTypeInfo 导出 payload Schema；payload 必须序列化为 JSON 对象。

```csharp
public static JsonElement Export(JsonTypeInfo typeInfo)
```

#### Parameters

`typeInfo` [JsonTypeInfo](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.metadata.jsontypeinfo)

#### Returns

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)
