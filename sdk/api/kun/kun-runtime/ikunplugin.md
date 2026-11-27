> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IKunPlugin

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

插件的最小接口。

```csharp
public interface IKunPlugin
```

## Properties

### Inject \{#Kun_Runtime_IKunPlugin_Inject}

Apply 前必须就绪的服务键。

```csharp
IReadOnlyList<ServiceKey> Inject { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)>

## Methods

### ApplyAsync(IPluginContext, JsonElement, CancellationToken) \{#Kun_Runtime_IKunPlugin_ApplyAsync_Kun_Runtime_IPluginContext_System_Text_Json_JsonElement_System_Threading_CancellationToken\_}

注册服务、监听器与效果；失败时全部逆序撤销。

```csharp
ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
```

#### Parameters

`ctx` [IPluginContext](/sdk/api/kun/kun-runtime/iplugincontext.md)

`config` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
