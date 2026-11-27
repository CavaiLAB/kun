> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class FeatureFlagDraftResult

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

草稿写入回执，不含 Token 或规则内容。

```csharp
public sealed class FeatureFlagDraftResult
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[FeatureFlagDraftResult](/sdk/api/kun/kun-configuration/featureflagdraftresult.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### FeatureFlagDraftResult() \{#Kun_Configuration_FeatureFlagDraftResult__ctor}

```csharp
public FeatureFlagDraftResult()
```

## Properties

### Name \{#Kun_Configuration_FeatureFlagDraftResult_Name}

开关名称。

```csharp
public required string Name { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### RuleHash \{#Kun_Configuration_FeatureFlagDraftResult_RuleHash}

规范化规则的 SHA-256，供操作员核对。

```csharp
public required string RuleHash { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Status \{#Kun_Configuration_FeatureFlagDraftResult_Status}

固定 pending-publish；不得解释为运行时已生效。

```csharp
public string Status { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)
