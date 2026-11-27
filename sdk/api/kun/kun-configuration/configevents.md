> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ConfigEvents

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

配置组件的事件与审计词汇；只含稳定标识，不含任何配置值。

```csharp
public static class ConfigEvents
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ConfigEvents](/sdk/api/kun/kun-configuration/configevents.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### ReleaseAppliedCapabilityId \{#Kun_Configuration_ConfigEvents_ReleaseAppliedCapabilityId}

Apollo 快照被实例接受并生效时写入平台审计链的能力 ID（R23 component/name 写法）。

```csharp
public const string ReleaseAppliedCapabilityId = "qian.config/release.applied"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ReleaseRejectedCapabilityId \{#Kun_Configuration_ConfigEvents_ReleaseRejectedCapabilityId}

Apollo 快照被实例校验拒绝、保留上一份时写入平台审计链的能力 ID。

```csharp
public const string ReleaseRejectedCapabilityId = "qian.config/release.rejected"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ReloadFailedType \{#Kun_Configuration_ConfigEvents_ReloadFailedType}

重载失败告警的跨进程事件 ID（PRD 7.8）。配置重载不在数据事务里，Outbox 写入端契约发布（S1-EVT-003）前只以
[ConfigRuntimeEvents.ReloadFailed](/sdk/api/kun/kun-configuration/configruntimeevents.md#Kun_Configuration_ConfigRuntimeEvents_ReloadFailed) 在进程内发出，载荷形状与此 ID 一致。

```csharp
public const string ReloadFailedType = "config.reload.failed.v1"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
