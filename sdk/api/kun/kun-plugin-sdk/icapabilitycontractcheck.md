> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface ICapabilityContractCheck

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Plugin.Sdk` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Plugin.Sdk`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Plugin.Sdk)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Plugin.Sdk](/sdk/api/kun/kun-plugin-sdk/index.md)\
Assembly: Kun.Plugin.Sdk.dll

能力登记期的契约检查端口。平台宿主登记实现后，[CapabilityBindingRegistration.ProvideCapability](/sdk/api/kun/kun-plugin-sdk/capabilitybindingregistration.md#Kun_Plugin_Sdk_CapabilityBindingRegistration_ProvideCapability__2_Kun_Runtime_IPluginContext_)
在登记前调用它：生成目录条目并预编译输入校验器，不合格的契约在登记阶段拒绝，不留到首次调用。platform profile 宿主必须登记
（`AddKunCapabilities()`）；未登记时不做此检查，独立宿主没有通用执行入口，不需要它。

```csharp
public interface ICapabilityContractCheck
```

## Methods

### Check(CapabilityContract) \{#Kun_Plugin_Sdk_ICapabilityContractCheck_Check_Kun_Plugin_Sdk_CapabilityContract\_}

检查并缓存一个能力契约；不合格时抛出 [CapabilityContractException](/sdk/api/kun/kun-plugin-sdk/capabilitycontractexception.md)。

```csharp
void Check(CapabilityContract contract)
```

#### Parameters

`contract` [CapabilityContract](/sdk/api/kun/kun-plugin-sdk/capabilitycontract.md)
