> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunEventsOptions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EventBus.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EventBus.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EventBus.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EventBus.EntityFrameworkCore](/sdk/api/kun/kun-eventbus-entityframeworkcore/index.md)\
Assembly: Kun.EventBus.EntityFrameworkCore.dll

Outbox 投递的宿主级设置；组件配置 `outbox.lock` 必须与 [KunEventsOptions.OutboxLock](/sdk/api/kun/kun-eventbus-entityframeworkcore/kuneventsoptions.md#Kun_EventBus_EntityFrameworkCore_KunEventsOptions_OutboxLock) 一致。

```csharp
public sealed class KunEventsOptions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunEventsOptions](/sdk/api/kun/kun-eventbus-entityframeworkcore/kuneventsoptions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### KunEventsOptions() \{#Kun_EventBus_EntityFrameworkCore_KunEventsOptions__ctor}

```csharp
public KunEventsOptions()
```

## Properties

### BatchSize \{#Kun_EventBus_EntityFrameworkCore_KunEventsOptions_BatchSize}

每次领取的最大行数。默认 100。

```csharp
public int BatchSize { get; set; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### LeaseDuration \{#Kun_EventBus_EntityFrameworkCore_KunEventsOptions_LeaseDuration}

领取后的租约；持有者崩溃时，其他副本在租约到期后接管。默认 30 秒，须明显长于一批的发布耗时。

```csharp
public TimeSpan LeaseDuration { get; set; }
```

#### Property Value

[TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

### MaxAttempts \{#Kun_EventBus_EntityFrameworkCore_KunEventsOptions_MaxAttempts}

发布失败的最大尝试次数，达到后行标为失败。默认 5。

```csharp
public int MaxAttempts { get; set; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### OutboxLock \{#Kun_EventBus_EntityFrameworkCore_KunEventsOptions_OutboxLock}

领取的互斥方式。默认 PostgreSQL 行锁。

```csharp
public OutboxLockMode OutboxLock { get; set; }
```

#### Property Value

[OutboxLockMode](/sdk/api/kun/kun-eventbus-entityframeworkcore/outboxlockmode.md)

### PollInterval \{#Kun_EventBus_EntityFrameworkCore_KunEventsOptions_PollInterval}

某轮没有领取到消息时的轮询间隔。默认 1 秒。

```csharp
public TimeSpan PollInterval { get; set; }
```

#### Property Value

[TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

### PublishTimeout \{#Kun_EventBus_EntityFrameworkCore_KunEventsOptions_PublishTimeout}

单条消息等待 broker 确认的上限。默认 10 秒。

```csharp
public TimeSpan PublishTimeout { get; set; }
```

#### Property Value

[TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

### RelayInstance \{#Kun_EventBus_EntityFrameworkCore_KunEventsOptions_RelayInstance}

本副本的标识，只用于指标标签与日志。默认机器名加进程号。

```csharp
public string RelayInstance { get; set; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### RetryBaseDelay \{#Kun_EventBus_EntityFrameworkCore_KunEventsOptions_RetryBaseDelay}

失败重试的起始间隔，按 2 的幂增长，封顶一分钟。默认 1 秒。

```csharp
public TimeSpan RetryBaseDelay { get; set; }
```

#### Property Value

[TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)
