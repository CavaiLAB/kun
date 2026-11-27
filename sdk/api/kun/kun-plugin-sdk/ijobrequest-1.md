> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IJobRequest\<TResponse>

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

源生成器为 KunJob 请求实现的静态声明，不使用反射寻找处理器。

```csharp
public interface IJobRequest<TResponse> : ICapabilityRequest<TResponse>, IRequest<TResponse>, IPermissionProtectedRequest
```

#### Type Parameters

`TResponse`

#### Implements

[ICapabilityRequest\<TResponse>](/sdk/api/kun/kun-plugin-sdk/icapabilityrequest-1.md),
IRequest\<TResponse>,
IPermissionProtectedRequest

## Properties

### KunJobCode \{#Kun_Plugin_Sdk_IJobRequest_1_KunJobCode}

声明的作业代码。

```csharp
public static abstract string KunJobCode { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
