> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunEventsExtensions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.dll

Registers the event envelope writer and event catalog.

```csharp
public static class KunEventsExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunEventsExtensions](/sdk/api/kun/kun-eventbus/kuneventsextensions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### AddKunEvents(IServiceCollection) \{#Kun_EventBus_KunEventsExtensions_AddKunEvents_Microsoft_Extensions_DependencyInjection_IServiceCollection\_}

登记 v1 信封写入端与事件目录。与 AddKunData 的调用顺序无关。
此入口登记写入端。持有 Outbox 的宿主单独登记投递后台服务。

```csharp
public static IServiceCollection AddKunEvents(this IServiceCollection services)
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)
