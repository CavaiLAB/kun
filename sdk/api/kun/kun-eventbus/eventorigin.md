> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class EventOrigin

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

集成事件的来源：谁、以哪个组件的名义、因哪一次调用发布。信封的 producer、actor 与因果链取自这里。

```csharp
public sealed record EventOrigin : IEquatable<EventOrigin>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[EventOrigin](/sdk/api/kun/kun-eventbus/eventorigin.md)

#### Implements

[IEquatable\<EventOrigin>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### EventOrigin(string, EventActor) \{#Kun_EventBus_EventOrigin__ctor_System_String_Kun_EventBus_EventActor\_}

集成事件的来源：谁、以哪个组件的名义、因哪一次调用发布。信封的 producer、actor 与因果链取自这里。

```csharp
public EventOrigin(string Producer, EventActor Actor)
```

#### Parameters

`Producer` [string](https://learn.microsoft.com/dotnet/api/system.string)

发布方组件 id，如 `qian.sample.crm`。

`Actor` [EventActor](/sdk/api/kun/kun-eventbus/eventactor.md)

触发事件的主体。

## Properties

### Actor \{#Kun_EventBus_EventOrigin_Actor}

触发事件的主体。

```csharp
public EventActor Actor { get; init; }
```

#### Property Value

[EventActor](/sdk/api/kun/kun-eventbus/eventactor.md)

### CausationId \{#Kun_EventBus_EventOrigin_CausationId}

直接引起事件的命令或事件 id；SDK 命令管道填本次调用 id。

```csharp
public Guid? CausationId { get; init; }
```

#### Property Value

[Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

### CorrelationId \{#Kun_EventBus_EventOrigin_CorrelationId}

业务关联 id；跨多个事件的同一业务流程沿用同一个值。

```csharp
public Guid? CorrelationId { get; init; }
```

#### Property Value

[Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

### Producer \{#Kun_EventBus_EventOrigin_Producer}

发布方组件 id，如 `qian.sample.crm`。

```csharp
public string Producer { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TraceParent \{#Kun_EventBus_EventOrigin_TraceParent}

W3C traceparent；为空时按当前 Activity 或新 trace 生成。

```csharp
public string? TraceParent { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

## Methods

### \<Clone>$() \{#Kun_EventBus_EventOrigin__Clone\_\_}

```csharp
public EventOrigin <Clone>$()
```

#### Returns

[EventOrigin](/sdk/api/kun/kun-eventbus/eventorigin.md)

### Deconstruct(out string, out EventActor) \{#Kun_EventBus_EventOrigin_Deconstruct_System_String__Kun_EventBus_EventActor\_\_}

```csharp
public void Deconstruct(out string Producer, out EventActor Actor)
```

#### Parameters

`Producer` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Actor` [EventActor](/sdk/api/kun/kun-eventbus/eventactor.md)

### Equals(object?) \{#Kun_EventBus_EventOrigin_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(EventOrigin?) \{#Kun_EventBus_EventOrigin_Equals_Kun_EventBus_EventOrigin\_}

```csharp
public bool Equals(EventOrigin? other)
```

#### Parameters

`other` [EventOrigin](/sdk/api/kun/kun-eventbus/eventorigin.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_EventBus_EventOrigin_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_EventBus_EventOrigin_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Validate() \{#Kun_EventBus_EventOrigin_Validate}

按信封规则校验来源字段。

```csharp
public void Validate()
```

## Operators

### operator ==(EventOrigin?, EventOrigin?) \{#Kun_EventBus_EventOrigin_op_Equality_Kun_EventBus_EventOrigin_Kun_EventBus_EventOrigin\_}

```csharp
public static bool operator ==(EventOrigin? left, EventOrigin? right)
```

#### Parameters

`left` [EventOrigin](/sdk/api/kun/kun-eventbus/eventorigin.md)?

`right` [EventOrigin](/sdk/api/kun/kun-eventbus/eventorigin.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(EventOrigin?, EventOrigin?) \{#Kun_EventBus_EventOrigin_op_Inequality_Kun_EventBus_EventOrigin_Kun_EventBus_EventOrigin\_}

```csharp
public static bool operator !=(EventOrigin? left, EventOrigin? right)
```

#### Parameters

`left` [EventOrigin](/sdk/api/kun/kun-eventbus/eventorigin.md)?

`right` [EventOrigin](/sdk/api/kun/kun-eventbus/eventorigin.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
