> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface ISecretResolverFactory

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

宿主根据已审批策略创建解析器；平台调用方身份取自真实插件上下文。

```csharp
public interface ISecretResolverFactory
```

## Methods

### ForHost(string, ITenantContext?) \{#Kun_Configuration_ISecretResolverFactory_ForHost_System_String_Kun_Core_MultiTenancy_ITenantContext\_}

独立宿主的显式入口，policyId 必须已在可信引导中登记。

```csharp
ISecretResolver ForHost(string policyId, ITenantContext? tenantContext = null)
```

#### Parameters

`policyId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`tenantContext` ITenantContext?

#### Returns

[ISecretResolver](/sdk/api/kun/kun-configuration/isecretresolver.md)

### ForPlugin(IPluginContext) \{#Kun_Configuration_ISecretResolverFactory_ForPlugin_Kun_Runtime_IPluginContext\_}

绑定插件身份和生命周期；声明及审批权限从宿主策略中取，不由请求传入。

```csharp
ISecretResolver ForPlugin(IPluginContext context)
```

#### Parameters

`context` IPluginContext

#### Returns

[ISecretResolver](/sdk/api/kun/kun-configuration/isecretresolver.md)
