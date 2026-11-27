> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IExecutableCapability

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

已挂载代码能力的真实执行端口；目录读取契约，调用仍经实际 Mediator 和基础策略。

```csharp
public interface IExecutableCapability
```

## Properties

### Contract \{#Kun_Plugin_Sdk_IExecutableCapability_Contract}

已编译的不可变输入/输出及 Agent 契约。

```csharp
CapabilityContract Contract { get; }
```

#### Property Value

[CapabilityContract](/sdk/api/kun/kun-plugin-sdk/capabilitycontract.md)

## Methods

### InvokeAsync(IPluginContext, JsonElement, CancellationToken) \{#Kun_Plugin_Sdk_IExecutableCapability_InvokeAsync_Kun_Runtime_IPluginContext_System_Text_Json_JsonElement_System_Threading_CancellationToken\_}

调用者必须提供当前租户的实际可见视图；缓存的旧绑定不能绕过卸载或 restrict。

```csharp
ValueTask<JsonElement> InvokeAsync(IPluginContext caller, JsonElement input, CancellationToken cancellationToken = default)
```

#### Parameters

`caller` IPluginContext

`input` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)>
