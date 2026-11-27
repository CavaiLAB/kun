> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class EventSubscriptionDescriptor

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

订阅目录中的一项，与 [IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md) 按事件 ID 对齐，是事件目录「订阅者」的来源。

```csharp
public sealed record EventSubscriptionDescriptor : IEquatable<EventSubscriptionDescriptor>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[EventSubscriptionDescriptor](/sdk/api/kun/kun-eventbus/eventsubscriptiondescriptor.md)

#### Implements

[IEquatable\<EventSubscriptionDescriptor>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### EventSubscriptionDescriptor(string, string, EventSubscriptionMode, string, string, int, string, bool) \{#Kun_EventBus_EventSubscriptionDescriptor__ctor_System_String_System_String_Kun_EventBus_EventSubscriptionMode_System_String_System_String_System_Int32_System_String_System_Boolean\_}

订阅目录中的一项，与 [IntegrationEventDescriptor](/sdk/api/kun/kun-eventbus/integrationeventdescriptor.md) 按事件 ID 对齐，是事件目录「订阅者」的来源。

```csharp
public EventSubscriptionDescriptor(string Service, string Subscription, EventSubscriptionMode Mode, string Type, string Subject, int Version, string Handler, bool AcceptPlatformEvents)
```

#### Parameters

`Service` [string](https://learn.microsoft.com/dotnet/api/system.string)

订阅方服务名，队列名的第二段。

`Subscription` [string](https://learn.microsoft.com/dotnet/api/system.string)

订阅名；同一订阅可以覆盖多个事件。

`Mode` [EventSubscriptionMode](/sdk/api/kun/kun-eventbus/eventsubscriptionmode.md)

竞争消费或广播。

`Type` [string](https://learn.microsoft.com/dotnet/api/system.string)

订阅的稳定事件 ID。

`Subject` [string](https://learn.microsoft.com/dotnet/api/system.string)

信封 subject，也是队列的绑定键。

`Version` [int](https://learn.microsoft.com/dotnet/api/system.int32)

payload 版本。

`Handler` [string](https://learn.microsoft.com/dotnet/api/system.string)

处理该事件的 handler 类型全名。

`AcceptPlatformEvents` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

是否派发 tenant_id 为 null 的平台级事件；为 false 时这类消息直接拒绝。

## Properties

### AcceptPlatformEvents \{#Kun_EventBus_EventSubscriptionDescriptor_AcceptPlatformEvents}

是否派发 tenant_id 为 null 的平台级事件；为 false 时这类消息直接拒绝。

```csharp
public bool AcceptPlatformEvents { get; init; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Handler \{#Kun_EventBus_EventSubscriptionDescriptor_Handler}

处理该事件的 handler 类型全名。

```csharp
public string Handler { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Mode \{#Kun_EventBus_EventSubscriptionDescriptor_Mode}

竞争消费或广播。

```csharp
public EventSubscriptionMode Mode { get; init; }
```

#### Property Value

[EventSubscriptionMode](/sdk/api/kun/kun-eventbus/eventsubscriptionmode.md)

### Service \{#Kun_EventBus_EventSubscriptionDescriptor_Service}

订阅方服务名，队列名的第二段。

```csharp
public string Service { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Subject \{#Kun_EventBus_EventSubscriptionDescriptor_Subject}

信封 subject，也是队列的绑定键。

```csharp
public string Subject { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Subscription \{#Kun_EventBus_EventSubscriptionDescriptor_Subscription}

订阅名；同一订阅可以覆盖多个事件。

```csharp
public string Subscription { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Type \{#Kun_EventBus_EventSubscriptionDescriptor_Type}

订阅的稳定事件 ID。

```csharp
public string Type { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Version \{#Kun_EventBus_EventSubscriptionDescriptor_Version}

payload 版本。

```csharp
public int Version { get; init; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

## Methods

### \<Clone>$() \{#Kun_EventBus_EventSubscriptionDescriptor__Clone\_\_}

```csharp
public EventSubscriptionDescriptor <Clone>$()
```

#### Returns

[EventSubscriptionDescriptor](/sdk/api/kun/kun-eventbus/eventsubscriptiondescriptor.md)

### Deconstruct(out string, out string, out EventSubscriptionMode, out string, out string, out int, out string, out bool) \{#Kun_EventBus_EventSubscriptionDescriptor_Deconstruct_System_String__System_String__Kun_EventBus_EventSubscriptionMode__System_String__System_String__System_Int32__System_String__System_Boolean\_\_}

```csharp
public void Deconstruct(out string Service, out string Subscription, out EventSubscriptionMode Mode, out string Type, out string Subject, out int Version, out string Handler, out bool AcceptPlatformEvents)
```

#### Parameters

`Service` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Subscription` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Mode` [EventSubscriptionMode](/sdk/api/kun/kun-eventbus/eventsubscriptionmode.md)

`Type` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Subject` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Version` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`Handler` [string](https://learn.microsoft.com/dotnet/api/system.string)

`AcceptPlatformEvents` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(object?) \{#Kun_EventBus_EventSubscriptionDescriptor_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(EventSubscriptionDescriptor?) \{#Kun_EventBus_EventSubscriptionDescriptor_Equals_Kun_EventBus_EventSubscriptionDescriptor\_}

```csharp
public bool Equals(EventSubscriptionDescriptor? other)
```

#### Parameters

`other` [EventSubscriptionDescriptor](/sdk/api/kun/kun-eventbus/eventsubscriptiondescriptor.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_EventBus_EventSubscriptionDescriptor_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_EventBus_EventSubscriptionDescriptor_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(EventSubscriptionDescriptor?, EventSubscriptionDescriptor?) \{#Kun_EventBus_EventSubscriptionDescriptor_op_Equality_Kun_EventBus_EventSubscriptionDescriptor_Kun_EventBus_EventSubscriptionDescriptor\_}

```csharp
public static bool operator ==(EventSubscriptionDescriptor? left, EventSubscriptionDescriptor? right)
```

#### Parameters

`left` [EventSubscriptionDescriptor](/sdk/api/kun/kun-eventbus/eventsubscriptiondescriptor.md)?

`right` [EventSubscriptionDescriptor](/sdk/api/kun/kun-eventbus/eventsubscriptiondescriptor.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(EventSubscriptionDescriptor?, EventSubscriptionDescriptor?) \{#Kun_EventBus_EventSubscriptionDescriptor_op_Inequality_Kun_EventBus_EventSubscriptionDescriptor_Kun_EventBus_EventSubscriptionDescriptor\_}

```csharp
public static bool operator !=(EventSubscriptionDescriptor? left, EventSubscriptionDescriptor? right)
```

#### Parameters

`left` [EventSubscriptionDescriptor](/sdk/api/kun/kun-eventbus/eventsubscriptiondescriptor.md)?

`right` [EventSubscriptionDescriptor](/sdk/api/kun/kun-eventbus/eventsubscriptiondescriptor.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
