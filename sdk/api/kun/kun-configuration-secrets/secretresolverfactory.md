> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretResolverFactory

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Secrets` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Secrets`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Secrets)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration.Secrets](/sdk/api/kun/kun-configuration-secrets/index.md)\
Assembly: Kun.Configuration.Secrets.dll

不可变策略与按需文件读取的组合；不缓存已解析值。

```csharp
public sealed class SecretResolverFactory : ISecretResolverFactory
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SecretResolverFactory](/sdk/api/kun/kun-configuration-secrets/secretresolverfactory.md)

#### Implements

ISecretResolverFactory

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### SecretResolverFactory(SecretResolverOptions, ISecretAccessObserver) \{#Kun_Configuration_Secrets_SecretResolverFactory__ctor_Kun_Configuration_Secrets_SecretResolverOptions_Kun_Configuration_ISecretAccessObserver\_}

构造解析器工厂，复制绑定和策略，避免调用方事后扩大授权。

```csharp
public SecretResolverFactory(SecretResolverOptions options, ISecretAccessObserver observer)
```

#### Parameters

`options` [SecretResolverOptions](/sdk/api/kun/kun-configuration-secrets/secretresolveroptions.md)

`observer` ISecretAccessObserver

## Methods

### ForHost(string, ITenantContext?) \{#Kun_Configuration_Secrets_SecretResolverFactory_ForHost_System_String_Kun_Core_MultiTenancy_ITenantContext\_}

独立宿主的显式入口，policyId 必须已在可信引导中登记。

```csharp
public ISecretResolver ForHost(string policyId, ITenantContext? tenantContext = null)
```

#### Parameters

`policyId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`tenantContext` ITenantContext?

#### Returns

ISecretResolver

### ForPlugin(IPluginContext) \{#Kun_Configuration_Secrets_SecretResolverFactory_ForPlugin_Kun_Runtime_IPluginContext\_}

绑定插件身份和生命周期；声明及审批权限从宿主策略中取，不由请求传入。

```csharp
public ISecretResolver ForPlugin(IPluginContext context)
```

#### Parameters

`context` IPluginContext

#### Returns

ISecretResolver
