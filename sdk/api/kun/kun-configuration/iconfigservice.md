> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IConfigService

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

供受信任代码读取配置；不提供写入入口，也不自动公开为模型工具。

```csharp
public interface IConfigService
```

## Properties

### Flags \{#Kun_Configuration_IConfigService_Flags}

平台规则的只读租户灰度求值端口；与授权及组件挂载无关。

```csharp
IFeatureFlags Flags { get; }
```

#### Property Value

[IFeatureFlags](/sdk/api/kun/kun-configuration/ifeatureflags.md)

### Secrets \{#Kun_Configuration_IConfigService_Secrets}

可选秘密解析能力；未装配时显式不可用，不退回读取明文配置。

```csharp
ISecretResolverFactory? Secrets { get; }
```

#### Property Value

[ISecretResolverFactory](/sdk/api/kun/kun-configuration/isecretresolverfactory.md)?

## Methods

### GetReloadToken() \{#Kun_Configuration_IConfigService_GetReloadToken}

取得下一次变更令牌，监听句柄应交给 ctx.Effect。

```csharp
IChangeToken GetReloadToken()
```

#### Returns

[IChangeToken](https://learn.microsoft.com/dotnet/api/microsoft.extensions.primitives.ichangetoken)

### GetRequiredValue(string) \{#Kun_Configuration_IConfigService_GetRequiredValue_System_String\_}

读取必需值，缺失时报告统一配置错误，不打印值。

```csharp
string GetRequiredValue(string key)
```

#### Parameters

`key` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### GetValue(string) \{#Kun_Configuration_IConfigService_GetValue_System_String\_}

按标准 IConfiguration 层次键读取值。

```csharp
string? GetValue(string key)
```

#### Parameters

`key` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### ReadSection(string) \{#Kun_Configuration_IConfigService_ReadSection_System_String\_}

复制指定非空节的相对键和值；返回只读快照。

```csharp
IReadOnlyDictionary<string, string?> ReadSection(string path)
```

#### Parameters

`path` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

[IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)?>
