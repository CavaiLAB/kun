> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class EventEnvelopeScope

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus](/sdk/api/kun/kun-eventbus/index.md)\
Assembly: Kun.EventBus.Abstractions.dll

当前异步流的事件来源。集成事件只能在命令 handler 的同一作用域内发出（ADR-008 D8.2）：
SDK 命令管道在执行 handler 与保存期间打开它，Outbox 写入端缺少来源时拒绝发布。
定时作业、生命周期控制器等不经命令管道的平台发布方显式以服务主体打开。

```csharp
public static class EventEnvelopeScope
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[EventEnvelopeScope](/sdk/api/kun/kun-eventbus/eventenvelopescope.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Properties

### Current \{#Kun_EventBus_EventEnvelopeScope_Current}

当前生效的来源；不在任何事件作用域内时为 null。

```csharp
public static EventOrigin? Current { get; }
```

#### Property Value

[EventOrigin](/sdk/api/kun/kun-eventbus/eventorigin.md)?

## Methods

### Begin(EventOrigin) \{#Kun_EventBus_EventEnvelopeScope_Begin_Kun_EventBus_EventOrigin\_}

在当前异步流打开事件作用域，释放后恢复外层来源；必须按打开的逆序释放。

```csharp
public static IDisposable Begin(EventOrigin origin)
```

#### Parameters

`origin` [EventOrigin](/sdk/api/kun/kun-eventbus/eventorigin.md)

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)
