> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class CommandEvents

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

SDK 生产执行端拥有的事件 Definition，供基础组件注册强制监听器。

```csharp
public static class CommandEvents
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[CommandEvents](/sdk/api/kun/kun-plugin-sdk/commandevents.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### Dispatching \{#Kun_Plugin_Sdk_CommandEvents_Dispatching}

围绕实际 Mediator handler 的命令或查询执行。

```csharp
public static readonly KunEvent<CommandInvocation, object?> Dispatching
```

#### Field Value

KunEvent\<[CommandInvocation](/sdk/api/kun/kun-plugin-sdk/commandinvocation.md), [object](https://learn.microsoft.com/dotnet/api/system.object)?>
