> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class OptionsReloadFailure

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.dll

一次被拒绝的候选配置，仅包含登记信息和失败条数。

```csharp
public sealed class OptionsReloadFailure
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[OptionsReloadFailure](/sdk/api/kun/kun-configuration/optionsreloadfailure.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Properties

### Failures \{#Kun_Configuration_OptionsReloadFailure_Failures}

未通过的校验条数，绑定失败记一条。

```csharp
public int Failures { get; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### OptionsName \{#Kun_Configuration_OptionsReloadFailure_OptionsName}

命名选项的名称。

```csharp
public string OptionsName { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### OptionsType \{#Kun_Configuration_OptionsReloadFailure_OptionsType}

选项类型全名。

```csharp
public string OptionsType { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Plugin \{#Kun_Configuration_OptionsReloadFailure_Plugin}

登记选项的组件标识。

```csharp
public string? Plugin { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### Section \{#Kun_Configuration_OptionsReloadFailure_Section}

配置节路径。

```csharp
public string Section { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
