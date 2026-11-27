> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface ICapabilityRequest\<TResponse>

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

真实 Mediator 请求与生成契约的类型关联。

```csharp
public interface ICapabilityRequest<TResponse> : IRequest<TResponse>, IPermissionProtectedRequest
```

#### Type Parameters

`TResponse`

#### Implements

IRequest\<TResponse>,
IPermissionProtectedRequest

## Properties

### KunCapability \{#Kun_Plugin_Sdk_ICapabilityRequest_1_KunCapability}

由代码声明和 JsonTypeInfo 生成的契约。

```csharp
public static abstract CapabilityContract KunCapability { get; }
```

#### Property Value

[CapabilityContract](/sdk/api/kun/kun-plugin-sdk/capabilitycontract.md)
