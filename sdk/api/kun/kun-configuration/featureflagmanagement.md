> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class FeatureFlagManagement

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

可选平台管理组件的声明；只保存 Apollo 草稿，发布仍由 Portal 操作员完成。

```csharp
public static class FeatureFlagManagement
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[FeatureFlagManagement](/sdk/api/kun/kun-configuration/featureflagmanagement.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### Key \{#Kun_Configuration_FeatureFlagManagement_Key}

平台草稿管理端口，与 Config 只读端口分开。

```csharp
public static readonly ServiceKey Key
```

#### Field Value

ServiceKey

### SetCapabilityId \{#Kun_Configuration_FeatureFlagManagement_SetCapabilityId}

草稿编辑能力标识。

```csharp
public const string SetCapabilityId = "qian.config.flags/flags.set"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### SetPermission \{#Kun_Configuration_FeatureFlagManagement_SetPermission}

草稿编辑权限；声明不等于授予。

```csharp
public const string SetPermission = "qian.config.flags:flag:set"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
