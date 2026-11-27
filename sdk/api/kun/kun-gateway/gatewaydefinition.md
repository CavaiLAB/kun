> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class GatewayDefinition

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Gateway.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Gateway.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Gateway.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Gateway](/sdk/api/kun/kun-gateway/index.md)\
Assembly: Kun.Gateway.Abstractions.dll

网关 seam；Provider 安装到 gateway profile，业务组件只消费本契约。

```csharp
public static class GatewayDefinition
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[GatewayDefinition](/sdk/api/kun/kun-gateway/gatewaydefinition.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### Key \{#Kun_Gateway_GatewayDefinition_Key}

网关服务键。

```csharp
public static readonly ServiceKey Key
```

#### Field Value

ServiceKey

## Methods

### Gateway(IPluginContext) \{#Kun_Gateway_GatewayDefinition_Gateway_Kun_Runtime_IPluginContext\_}

取得当前已挂载网关。

```csharp
public static IGatewayService Gateway(this IPluginContext context)
```

#### Parameters

`context` IPluginContext

#### Returns

[IGatewayService](/sdk/api/kun/kun-gateway/igatewayservice.md)
