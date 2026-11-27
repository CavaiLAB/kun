> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class JobBindingRegistration

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

local 作业的可逆注册；独立宿主与平台目录消费同一能力绑定。

```csharp
public static class JobBindingRegistration
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[JobBindingRegistration](/sdk/api/kun/kun-plugin-sdk/jobbindingregistration.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### Key \{#Kun_Plugin_Sdk_JobBindingRegistration_Key}

仅包含显式 KunJob 声明的目录，名称为能力 URI。

```csharp
public static readonly ServiceKey Key
```

#### Field Value

ServiceKey

## Methods

### ProvideJob\<TRequest, TResponse>(IPluginContext) \{#Kun_Plugin_Sdk_JobBindingRegistration_ProvideJob__2_Kun_Runtime_IPluginContext\_}

同时登记真实能力及其可调度标记；两者均随当前 Apply 撤销。

```csharp
public static IDisposable ProvideJob<TRequest, TResponse>(this IPluginContext context) where TRequest : IJobRequest<TResponse>
```

#### Parameters

`context` IPluginContext

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

#### Type Parameters

`TRequest`

`TResponse`
