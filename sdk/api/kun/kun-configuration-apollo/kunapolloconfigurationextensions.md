> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunApolloConfigurationExtensions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Apollo` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Apollo`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Apollo)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration.Apollo](/sdk/api/kun/kun-configuration-apollo/index.md)\
Assembly: Kun.Configuration.Apollo.dll

固定配置源顺序，复用 Kun 的协议、缓存、签名与重载实现。

```csharp
public static class KunApolloConfigurationExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunApolloConfigurationExtensions](/sdk/api/kun/kun-configuration-apollo/kunapolloconfigurationextensions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### AddKunConfigurationSources(IConfigurationBuilder, KunConfigurationOptions) \{#Kun_Configuration_Apollo_KunApolloConfigurationExtensions_AddKunConfigurationSources_Microsoft_Extensions_Configuration_IConfigurationBuilder_Kun_Configuration_Apollo_KunConfigurationOptions\_}

向空构建器加入 appsettings、Apollo、Secret 文件、环境变量和命令行。

```csharp
public static IConfigurationBuilder AddKunConfigurationSources(this IConfigurationBuilder builder, KunConfigurationOptions options)
```

#### Parameters

`builder` [IConfigurationBuilder](https://learn.microsoft.com/dotnet/api/microsoft.extensions.configuration.iconfigurationbuilder)

`options` [KunConfigurationOptions](/sdk/api/kun/kun-configuration-apollo/kunconfigurationoptions.md)

#### Returns

[IConfigurationBuilder](https://learn.microsoft.com/dotnet/api/microsoft.extensions.configuration.iconfigurationbuilder)
