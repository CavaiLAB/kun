> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class CommandInvocation

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

一次已经绑定业务事务的调用；输入和身份不得从日志字符串化展开。

```csharp
public sealed class CommandInvocation
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[CommandInvocation](/sdk/api/kun/kun-plugin-sdk/commandinvocation.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Properties

### CancellationToken \{#Kun_Plugin_Sdk_CommandInvocation_CancellationToken}

请求取消及基础策略卸载共同控制的令牌。

```csharp
public CancellationToken CancellationToken { get; }
```

#### Property Value

[CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

### Channel \{#Kun_Plugin_Sdk_CommandInvocation_Channel}

宿主在注册能力时确定的调用通道。

```csharp
public string Channel { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Contract \{#Kun_Plugin_Sdk_CommandInvocation_Contract}

不可变代码契约。

```csharp
public CapabilityContract Contract { get; }
```

#### Property Value

[CapabilityContract](/sdk/api/kun/kun-plugin-sdk/capabilitycontract.md)

### Id \{#Kun_Plugin_Sdk_CommandInvocation_Id}

本次执行实例的审计标识。

```csharp
public Guid Id { get; }
```

#### Property Value

[Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### Input \{#Kun_Plugin_Sdk_CommandInvocation_Input}

实际请求的 JSON 投影，仅用于验证和脱敏；不得复制到普通日志。

```csharp
public JsonElement Input { get; }
```

#### Property Value

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

### Principal \{#Kun_Plugin_Sdk_CommandInvocation_Principal}

从已认证 Identity 端口复制的主体快照。

```csharp
public Principal Principal { get; }
```

#### Property Value

Principal

### Source \{#Kun_Plugin_Sdk_CommandInvocation_Source}

本次调用的真实插件与 scope 上下文。

```csharp
public IPluginContext Source { get; }
```

#### Property Value

IPluginContext

### StartedAt \{#Kun_Plugin_Sdk_CommandInvocation_StartedAt}

执行开始时刻。

```csharp
public DateTimeOffset StartedAt { get; }
```

#### Property Value

[DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

### TraceId \{#Kun_Plugin_Sdk_CommandInvocation_TraceId}

沿用实际追踪上下文，无上下文时创建 W3C 标识。

```csharp
public string TraceId { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Transaction \{#Kun_Plugin_Sdk_CommandInvocation_Transaction}

与业务、领域事件和 Outbox 同一连接上的外层事务。

```csharp
public DbTransaction Transaction { get; }
```

#### Property Value

[DbTransaction](https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction)

## Methods

### ToString() \{#Kun_Plugin_Sdk_CommandInvocation_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
