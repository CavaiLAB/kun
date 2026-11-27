> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class EventsErrors

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

事件组件的稳定错误码定义。

```csharp
public static class EventsErrors
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[EventsErrors](/sdk/api/kun/kun-eventbus/eventserrors.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### ConsumeRejected \{#Kun_EventBus_EventsErrors_ConsumeRejected}

消费端判定一条消息不可处理（信封、租户、类型或载荷不满足契约，或 handler 以确定性错误拒绝）：不重投、不记幂等，直接进订阅的死信队列。
handler 抛出携带本错误码的 [KunEventsException](/sdk/api/kun/kun-eventbus/kuneventsexception.md) 即表示「重试也不会成功」。

```csharp
public static readonly ErrorCode ConsumeRejected
```

#### Field Value

ErrorCode

### Misconfigured \{#Kun_EventBus_EventsErrors_Misconfigured}

宿主或组件配置不满足事件发布的装配要求。

```csharp
public static readonly ErrorCode Misconfigured
```

#### Field Value

ErrorCode

### PublishRejected \{#Kun_EventBus_EventsErrors_PublishRejected}

无法为集成事件生成合法信封，事件不会写入 Outbox。

```csharp
public static readonly ErrorCode PublishRejected
```

#### Field Value

ErrorCode
