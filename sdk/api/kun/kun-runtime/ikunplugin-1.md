> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IKunPlugin\<TConfig>

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

类型化配置接口；Schema 校验由清单装配器在调用前完成。

```csharp
public interface IKunPlugin<TConfig> : IKunPlugin
```

#### Type Parameters

`TConfig`

#### Implements

[IKunPlugin](/sdk/api/kun/kun-runtime/ikunplugin.md)

## Properties

### ConfigTypeInfo \{#Kun_Runtime_IKunPlugin_1_ConfigTypeInfo}

插件为配置类型生成的 JSON 契约。上下文使用 JsonSerializerDefaults.Web。

```csharp
JsonTypeInfo<TConfig> ConfigTypeInfo { get; }
```

#### Property Value

[JsonTypeInfo](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.metadata.jsontypeinfo-1)\<TConfig>

## Methods

### ApplyAsync(IPluginContext, TConfig, CancellationToken) \{#Kun_Runtime_IKunPlugin_1_ApplyAsync_Kun_Runtime_IPluginContext__0_System_Threading_CancellationToken\_}

以类型化配置执行 Apply。

```csharp
ValueTask ApplyAsync(IPluginContext ctx, TConfig config, CancellationToken ct)
```

#### Parameters

`ctx` [IPluginContext](/sdk/api/kun/kun-runtime/iplugincontext.md)

`config` TConfig

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
