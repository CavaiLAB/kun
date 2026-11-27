> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class EventsPlugin

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus.EntityFrameworkCore](/sdk/api/kun/kun-eventbus-entityframeworkcore/index.md)\
Assembly: Kun.EventBus.EntityFrameworkCore.dll

qian.events 的真实 Provider：挂载时核对组件配置与宿主装配一致，提供 Events 服务键；卸载撤销服务与错误码。

```csharp
[KunPlugin("qian.events")]
[KunConfigSchema("{\n  \"$schema\": \"https://json-schema.org/draft/2020-12/schema\",\n  \"type\": \"object\",\n  \"properties\": {\n    \"broker\": {\n      \"type\": \"string\",\n      \"description\": \"RabbitMQ 的 AMQP 地址引用（secretRef），必须与宿主 AddKunEventsBroker 登记的引用一致；凭据只在连接时读取，不写进清单。省略时不核对 broker，是否投递由宿主装配决定。\",\n      \"pattern\": \"^secretRef://(platform|dev)/[^\\\\s?#]+$\",\n      \"x-qian-sensitive\": true\n    },\n    \"outbox\": {\n      \"type\": \"object\",\n      \"properties\": {\n        \"lock\": {\n          \"type\": \"string\",\n          \"description\": \"Outbox 领取的互斥方式：skip-locked 为各副本以 FOR UPDATE SKIP LOCKED 加租约分摊；redis 在此之外按表取 Redis 租约，同一时刻一个副本投递。须与宿主装配一致。\",\n          \"enum\": [\n            \"skip-locked\",\n            \"redis\"\n          ],\n          \"default\": \"skip-locked\"\n        }\n      },\n      \"additionalProperties\": false\n    }\n  },\n  \"additionalProperties\": false\n}")]
public sealed class EventsPlugin : IKunPlugin
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[EventsPlugin](/sdk/api/kun/kun-eventbus-entityframeworkcore/eventsplugin.md)

#### Implements

IKunPlugin

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### EventsPlugin() \{#Kun_EventBus_EntityFrameworkCore_EventsPlugin__ctor}

```csharp
public EventsPlugin()
```

## Properties

### Inject \{#Kun_EventBus_EntityFrameworkCore_EventsPlugin_Inject}

Apply 前必须就绪的服务键。

```csharp
public IReadOnlyList<ServiceKey> Inject { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<ServiceKey>

## Methods

### ApplyAsync(IPluginContext, JsonElement, CancellationToken) \{#Kun_EventBus_EntityFrameworkCore_EventsPlugin_ApplyAsync_Kun_Runtime_IPluginContext_System_Text_Json_JsonElement_System_Threading_CancellationToken\_}

注册服务、监听器与效果；失败时全部逆序撤销。

```csharp
public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
```

#### Parameters

`ctx` IPluginContext

`config` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
