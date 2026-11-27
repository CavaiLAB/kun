> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class EventActor

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

触发事件的主体；取值与 S2S principal_kind、审计 actor_type 一致。

```csharp
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EventActor
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[EventActor](/sdk/api/kun/kun-eventbus/eventactor.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### EventActor() \{#Kun_EventBus_EventActor__ctor}

```csharp
public EventActor()
```

## Properties

### Id \{#Kun_EventBus_EventActor_Id}

user 为 person_id，service / agent 为 client_id。

```csharp
[JsonPropertyName("id")]
public required string Id { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Kind \{#Kun_EventBus_EventActor_Kind}

主体类型。

```csharp
[JsonPropertyName("kind")]
public required EventActorKind Kind { get; init; }
```

#### Property Value

[EventActorKind](/sdk/api/kun/kun-eventbus/eventactorkind.md)

### OnBehalfOf \{#Kun_EventBus_EventActor_OnBehalfOf}

agent 或 service 代表用户行事时的委托人 person_id。

```csharp
[JsonPropertyName("on_behalf_of")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public string? OnBehalfOf { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

## Methods

### Agent(string, string?) \{#Kun_EventBus_EventActor_Agent_System_String_System_String\_}

以 Agent 身份触发。

```csharp
public static EventActor Agent(string clientId, string? onBehalfOf = null)
```

#### Parameters

`clientId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`onBehalfOf` [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Returns

[EventActor](/sdk/api/kun/kun-eventbus/eventactor.md)

### Service(string, string?) \{#Kun_EventBus_EventActor_Service_System_String_System_String\_}

以服务身份触发；定时作业等无人触发的事件也用执行它的服务主体。

```csharp
public static EventActor Service(string clientId, string? onBehalfOf = null)
```

#### Parameters

`clientId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`onBehalfOf` [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Returns

[EventActor](/sdk/api/kun/kun-eventbus/eventactor.md)

### ToString() \{#Kun_EventBus_EventActor_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### User(string) \{#Kun_EventBus_EventActor_User_System_String\_}

以用户身份触发。

```csharp
public static EventActor User(string personId)
```

#### Parameters

`personId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[EventActor](/sdk/api/kun/kun-eventbus/eventactor.md)
