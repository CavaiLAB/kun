> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class CapabilityBindingRegistration

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

SDK 层的可逆能力注册，不依赖 platform 的 Capabilities Provider。

```csharp
public static class CapabilityBindingRegistration
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[CapabilityBindingRegistration](/sdk/api/kun/kun-plugin-sdk/capabilitybindingregistration.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### Key \{#Kun_Plugin_Sdk_CapabilityBindingRegistration_Key}

供目录、提示词和执行共同读取的具名注册集合。

```csharp
public static readonly ServiceKey Key
```

#### Field Value

ServiceKey

## Methods

### ProvideCapability\<TRequest, TResponse>(IPluginContext) \{#Kun_Plugin_Sdk_CapabilityBindingRegistration_ProvideCapability__2_Kun_Runtime_IPluginContext\_}

在所属组件 Apply 中登记已经实际注册的 Mediator 请求；注册自动纳入 Effect。

```csharp
public static IDisposable ProvideCapability<TRequest, TResponse>(this IPluginContext context) where TRequest : ICapabilityRequest<TResponse>
```

#### Parameters

`context` IPluginContext

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

#### Type Parameters

`TRequest`

`TResponse`
