> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ConfigDefinition

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

平台/服务级只读配置 seam；租户设置由 Tenancy 管理。

```csharp
public static class ConfigDefinition
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ConfigDefinition](/sdk/api/kun/kun-configuration/configdefinition.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### Key \{#Kun_Configuration_ConfigDefinition_Key}

当前 scope 可见的配置服务键。

```csharp
public static readonly ServiceKey Key
```

#### Field Value

ServiceKey

### ValuesReadPermission \{#Kun_Configuration_ConfigDefinition_ValuesReadPermission}

读取平台配置当前值的权限点（PRD 7.8）；Schema 对已认证调用者可读，值须经授权执行端口判定，敏感键只回引用名。

```csharp
public const string ValuesReadPermission = "qian.config:values:read"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### Config(IPluginContext) \{#Kun_Configuration_ConfigDefinition_Config_Kun_Runtime_IPluginContext\_}

解析当前已挂载的配置 Provider。

```csharp
public static IConfigService Config(this IPluginContext context)
```

#### Parameters

`context` IPluginContext

#### Returns

[IConfigService](/sdk/api/kun/kun-configuration/iconfigservice.md)

### ResolveSecretAsync(IPluginContext, string, CancellationToken) \{#Kun_Configuration_ConfigDefinition_ResolveSecretAsync_Kun_Runtime_IPluginContext_System_String_System_Threading_CancellationToken\_}

按当前插件的声明、审批与租户上下文解析，已解析值不能进入模型消息。

```csharp
public static ValueTask<SecretValue> ResolveSecretAsync(this IPluginContext context, string reference, CancellationToken cancellationToken = default)
```

#### Parameters

`context` IPluginContext

`reference` [string](https://learn.microsoft.com/dotnet/api/system.string)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[SecretValue](/sdk/api/kun/kun-configuration/secretvalue.md)>
