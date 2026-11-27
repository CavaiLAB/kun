> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunEventRelayExtensions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus.EntityFrameworkCore](/sdk/api/kun/kun-eventbus-entityframeworkcore/index.md)\
Assembly: Kun.EventBus.EntityFrameworkCore.dll

Registers the EF-backed event relay and its host settings.

```csharp
public static class KunEventRelayExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunEventRelayExtensions](/sdk/api/kun/kun-eventbus-entityframeworkcore/kuneventrelayextensions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### AddKunEvents(IServiceCollection, Action\<KunEventsOptions>) \{#Kun_EventBus_EntityFrameworkCore_KunEventRelayExtensions_AddKunEvents_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Kun_EventBus_EntityFrameworkCore_KunEventsOptions\_\_}

Registers the event writer and validates the configured relay options when the host starts.

```csharp
public static IServiceCollection AddKunEvents(this IServiceCollection services, Action<KunEventsOptions> configure)
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

`configure` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[KunEventsOptions](/sdk/api/kun/kun-eventbus-entityframeworkcore/kuneventsoptions.md)>

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

### AddKunOutboxRelay\<TContext>(IServiceCollection) \{#Kun_EventBus_EntityFrameworkCore_KunEventRelayExtensions_AddKunOutboxRelay__1_Microsoft_Extensions_DependencyInjection_IServiceCollection\_}

Adds the mapped outbox of a regular Kun context to the host relay.

```csharp
public static IServiceCollection AddKunOutboxRelay<TContext>(this IServiceCollection services) where TContext : KunDbContext
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

#### Type Parameters

`TContext`

### AddKunOutboxRelay(IServiceCollection) \{#Kun_EventBus_EntityFrameworkCore_KunEventRelayExtensions_AddKunOutboxRelay_Microsoft_Extensions_DependencyInjection_IServiceCollection\_}

Publishes the outboxes of registered data contexts through the configured broker.

```csharp
public static IServiceCollection AddKunOutboxRelay(this IServiceCollection services)
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)
