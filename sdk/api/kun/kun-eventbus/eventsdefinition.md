> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class EventsDefinition

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

事件 seam 的稳定服务键；业务组件只依赖 Definition。

```csharp
public static class EventsDefinition
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[EventsDefinition](/sdk/api/kun/kun-eventbus/eventsdefinition.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### Key \{#Kun_EventBus_EventsDefinition_Key}

当前 scope 可见的事件 Provider。

```csharp
public static readonly ServiceKey Key
```

#### Field Value

ServiceKey

## Methods

### Events(IPluginContext) \{#Kun_EventBus_EventsDefinition_Events_Kun_Runtime_IPluginContext\_}

解析已挂载的事件 Provider。

```csharp
public static IEventsService Events(this IPluginContext context)
```

#### Parameters

`context` IPluginContext

#### Returns

[IEventsService](/sdk/api/kun/kun-eventbus/ieventsservice.md)
