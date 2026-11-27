> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunEventEnvelope

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

跨进程集成事件信封 v1，与 contracts/v0/event-envelope.schema.json 同形：Outbox 行与 RabbitMQ 消息体都是这份 JSON。
payload 由事件目录里该 [KunEventEnvelope.Type](/sdk/api/kun/kun-eventbus/kuneventenvelope.md#Kun_EventBus_KunEventEnvelope_Type) 的 Schema 约束，信封不约束其内容；进程内运行时事件不用本信封。

```csharp
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class KunEventEnvelope
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunEventEnvelope](/sdk/api/kun/kun-eventbus/kuneventenvelope.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### KunEventEnvelope() \{#Kun_EventBus_KunEventEnvelope__ctor}

```csharp
public KunEventEnvelope()
```

## Fields

### CurrentVersion \{#Kun_EventBus_KunEventEnvelope_CurrentVersion}

信封版本常量；破坏性变更升 v2。

```csharp
public const string CurrentVersion = "v1"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Actor \{#Kun_EventBus_KunEventEnvelope_Actor}

触发事件的主体。

```csharp
[JsonPropertyName("actor")]
public required EventActor Actor { get; init; }
```

#### Property Value

[EventActor](/sdk/api/kun/kun-eventbus/eventactor.md)

### CausationId \{#Kun_EventBus_KunEventEnvelope_CausationId}

直接引起本事件的命令或事件 id。

```csharp
[JsonPropertyName("causation_id")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
[JsonConverter(typeof(CanonicalUuidConverter))]
public Guid? CausationId { get; init; }
```

#### Property Value

[Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

### CorrelationId \{#Kun_EventBus_KunEventEnvelope_CorrelationId}

业务关联 id。

```csharp
[JsonPropertyName("correlation_id")]
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
[JsonConverter(typeof(CanonicalUuidConverter))]
public Guid? CorrelationId { get; init; }
```

#### Property Value

[Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

### EventId \{#Kun_EventBus_KunEventEnvelope_EventId}

事件实例 id（UUID v7），消费端幂等键。

```csharp
[JsonPropertyName("event_id")]
[JsonConverter(typeof(CanonicalUuidConverter))]
public required Guid EventId { get; init; }
```

#### Property Value

[Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### OccurredAt \{#Kun_EventBus_KunEventEnvelope_OccurredAt}

业务发生时间，带时区。

```csharp
[JsonPropertyName("occurred_at")]
[JsonConverter(typeof(OffsetTimestampConverter))]
public required DateTimeOffset OccurredAt { get; init; }
```

#### Property Value

[DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

### Payload \{#Kun_EventBus_KunEventEnvelope_Payload}

事件数据，必须是 JSON 对象。

```csharp
[JsonPropertyName("payload")]
public required JsonElement Payload { get; init; }
```

#### Property Value

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

### Producer \{#Kun_EventBus_KunEventEnvelope_Producer}

发布方组件 id。

```csharp
[JsonPropertyName("producer")]
public required string Producer { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Subject \{#Kun_EventBus_KunEventEnvelope_Subject}

`<domain>.<entity>.<verb>`，等于 [KunEventEnvelope.Type](/sdk/api/kun/kun-eventbus/kuneventenvelope.md#Kun_EventBus_KunEventEnvelope_Type) 去掉版本后缀，也是路由键。

```csharp
[JsonPropertyName("subject")]
public required string Subject { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TenantId \{#Kun_EventBus_KunEventEnvelope_TenantId}

租户；null 表示平台级事件，必须显式写出。

```csharp
[JsonPropertyName("tenant_id")]
[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
[JsonConverter(typeof(CanonicalUuidConverter))]
public required Guid? TenantId { get; init; }
```

#### Property Value

[Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

### TraceParent \{#Kun_EventBus_KunEventEnvelope_TraceParent}

W3C traceparent。

```csharp
[JsonPropertyName("trace_parent")]
public required string TraceParent { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Type \{#Kun_EventBus_KunEventEnvelope_Type}

稳定事件 ID，即 `[IntegrationEventId]` 的值。

```csharp
[JsonPropertyName("type")]
public required string Type { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Version \{#Kun_EventBus_KunEventEnvelope_Version}

信封版本。

```csharp
[JsonPropertyName("version")]
public required string Version { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### IsComponentId(string?) \{#Kun_EventBus_KunEventEnvelope_IsComponentId_System_String\_}

组件 id 规则：小写点分、至少两段，允许连字符。

```csharp
public static bool IsComponentId(string? value)
```

#### Parameters

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### IsTraceParent(string?) \{#Kun_EventBus_KunEventEnvelope_IsTraceParent_System_String\_}

W3C traceparent 规则（版本 00）。

```csharp
public static bool IsTraceParent(string? value)
```

#### Parameters

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Parse(string) \{#Kun_EventBus_KunEventEnvelope_Parse_System_String\_}

解析并校验信封 JSON；不合法时以 [EventsErrors.PublishRejected](/sdk/api/kun/kun-eventbus/eventserrors.md#Kun_EventBus_EventsErrors_PublishRejected) 拒绝。

```csharp
public static KunEventEnvelope Parse(string json)
```

#### Parameters

`json` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[KunEventEnvelope](/sdk/api/kun/kun-eventbus/kuneventenvelope.md)

### ToJson() \{#Kun_EventBus_KunEventEnvelope_ToJson}

序列化为信封 JSON；先做与 Schema 相同的语义校验。

```csharp
public string ToJson()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Validate() \{#Kun_EventBus_KunEventEnvelope_Validate}

按 contracts/v0 的规则校验字段，包括 Schema 表达不了的 subject 与 type 对应关系。

```csharp
public void Validate()
```
