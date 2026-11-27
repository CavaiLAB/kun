> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IGatewayService

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Gateway.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Gateway.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Gateway.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Gateway](/sdk/api/kun/kun-gateway/index.md)\
Assembly: Kun.Gateway.Abstractions.dll

网关只读状态；路由配置在构建宿主时固定，未挂载时禁止代理请求。

```csharp
public interface IGatewayService
```

## Properties

### IsReady \{#Kun_Gateway_IGatewayService_IsReady}

对应组件当前是否挂载。

```csharp
bool IsReady { get; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

## Methods

### AudienceFor(string) \{#Kun_Gateway_IGatewayService_AudienceFor_System_String\_}

取得静态下游的唯一受众；未登记的 cluster 拒绝。

```csharp
string AudienceFor(string clusterId)
```

#### Parameters

`clusterId` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
