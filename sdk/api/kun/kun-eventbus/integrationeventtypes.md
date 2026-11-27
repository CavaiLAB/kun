> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class IntegrationEventTypes

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

稳定事件 ID（`[IntegrationEventId]` 的值）与信封 subject 的换算规则。

```csharp
public static class IntegrationEventTypes
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[IntegrationEventTypes](/sdk/api/kun/kun-eventbus/integrationeventtypes.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Remarks

事件 ID 形如 `crm.customer.created.v1`：subject 为去掉末尾 `.v<N>` 的部分，也是 RabbitMQ 路由键。

## Fields

### MaxSubjectLength \{#Kun_EventBus_IntegrationEventTypes_MaxSubjectLength}

subject 的最大长度，与信封 Schema 一致。

```csharp
public const int MaxSubjectLength = 200
```

#### Field Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### MaxTypeLength \{#Kun_EventBus_IntegrationEventTypes_MaxTypeLength}

事件 ID 的最大长度，与信封 Schema 的 `type` 一致。

```csharp
public const int MaxTypeLength = 210
```

#### Field Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

## Methods

### IsSubject(string?) \{#Kun_EventBus_IntegrationEventTypes_IsSubject_System_String\_}

subject 是否符合 `<domain>.<entity>.<verb>` 命名（至少两段）。

```csharp
public static bool IsSubject(string? subject)
```

#### Parameters

`subject` [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### SubjectOf(string) \{#Kun_EventBus_IntegrationEventTypes_SubjectOf_System_String\_}

返回事件 ID 对应的 subject；不合法时以 [EventsErrors.PublishRejected](/sdk/api/kun/kun-eventbus/eventserrors.md#Kun_EventBus_EventsErrors_PublishRejected) 拒绝。

```csharp
public static string SubjectOf(string type)
```

#### Parameters

`type` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TryParse(string?, out string?, out int) \{#Kun_EventBus_IntegrationEventTypes_TryParse_System_String_System_String__System_Int32\_\_}

解析事件 ID；不合法时返回 false，不抛异常。

```csharp
public static bool TryParse(string? type, out string? subject, out int version)
```

#### Parameters

`type` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`subject` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`version` [int](https://learn.microsoft.com/dotnet/api/system.int32)

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
