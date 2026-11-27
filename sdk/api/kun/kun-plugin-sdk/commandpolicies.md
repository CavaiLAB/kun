> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class CommandPolicies

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

基础 Provider 的监听登记入口；执行器同时核验服务、实际进入与卸载取消。

```csharp
public static class CommandPolicies
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[CommandPolicies](/sdk/api/kun/kun-plugin-sdk/commandpolicies.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### AuditKey \{#Kun_Plugin_Sdk_CommandPolicies_AuditKey}

审计强制监听器的存活键。

```csharp
public static readonly ServiceKey AuditKey
```

#### Field Value

ServiceKey

### AuthorizationKey \{#Kun_Plugin_Sdk_CommandPolicies_AuthorizationKey}

授权强制监听器的存活键。

```csharp
public static readonly ServiceKey AuthorizationKey
```

#### Field Value

ServiceKey

### TenancyKey \{#Kun_Plugin_Sdk_CommandPolicies_TenancyKey}

租户强制监听器的存活键。

```csharp
public static readonly ServiceKey TenancyKey
```

#### Field Value

ServiceKey

## Methods

### Register(IPluginContext, CommandPolicyKind, Func\<CommandInvocation, Func\<ValueTask\<object?>>, ValueTask\<object?>>) \{#Kun_Plugin_Sdk_CommandPolicies_Register_Kun_Runtime_IPluginContext_Kun_Plugin_Sdk_CommandPolicyKind_System_Func_Kun_Plugin_Sdk_CommandInvocation_System_Func_System_Threading_Tasks_ValueTask_System_Object___System_Threading_Tasks_ValueTask_System_Object\_\_\_}

由 qian.tenancy、qian.authz、qian.audit 或其原生审计 Provider 在 Apply 中登记并随 Effect 撤销。

```csharp
public static void Register(IPluginContext context, CommandPolicyKind kind, Func<CommandInvocation, Func<ValueTask<object?>>, ValueTask<object?>> handler)
```

#### Parameters

`context` IPluginContext

`kind` [CommandPolicyKind](/sdk/api/kun/kun-plugin-sdk/commandpolicykind.md)

`handler` [Func](https://learn.microsoft.com/dotnet/api/system.func-3)\<[CommandInvocation](/sdk/api/kun/kun-plugin-sdk/commandinvocation.md), [Func](https://learn.microsoft.com/dotnet/api/system.func-1)\<[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[object](https://learn.microsoft.com/dotnet/api/system.object)?>>, [ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[object](https://learn.microsoft.com/dotnet/api/system.object)?>>
