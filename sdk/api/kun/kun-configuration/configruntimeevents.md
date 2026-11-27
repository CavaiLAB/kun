> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ConfigRuntimeEvents

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

Config seam 的进程内扩展事件（组件模型规格 §6.4）。

```csharp
public static class ConfigRuntimeEvents
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ConfigRuntimeEvents](/sdk/api/kun/kun-configuration/configruntimeevents.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### ReloadFailed \{#Kun_Configuration_ConfigRuntimeEvents_ReloadFailed}

强类型配置重载被校验拒绝、已保留上一份有效值；由挂载中的配置组件发出，监听器异常被隔离。

```csharp
[KunEvent(Mode = DispatchMode.Emit)]
public static readonly KunEvent<ConfigReloadFailed, object?> ReloadFailed
```

#### Field Value

KunEvent\<[ConfigReloadFailed](/sdk/api/kun/kun-configuration/configreloadfailed.md), [object](https://learn.microsoft.com/dotnet/api/system.object)?>
