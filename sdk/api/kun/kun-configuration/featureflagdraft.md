> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class FeatureFlagDraft

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

草稿操作输入；目标环境、应用、namespace、操作员与秘密引用均由可信宿主固定。

```csharp
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class FeatureFlagDraft
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[FeatureFlagDraft](/sdk/api/kun/kun-configuration/featureflagdraft.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### FeatureFlagDraft() \{#Kun_Configuration_FeatureFlagDraft__ctor}

```csharp
public FeatureFlagDraft()
```

## Properties

### Confirmed \{#Kun_Configuration_FeatureFlagDraft_Confirmed}

调用者明确确认保存草稿；不代替身份或权限检查，也不是 S3 审批票据。

```csharp
public required bool Confirmed { get; init; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### IdempotencyKey \{#Kun_Configuration_FeatureFlagDraft_IdempotencyKey}

必填且非空的持久幂等键；同一目标和键仅执行一次，相同键不同主体或输入拒绝。

```csharp
public required Guid IdempotencyKey { get; init; }
```

#### Property Value

[Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### Name \{#Kun_Configuration_FeatureFlagDraft_Name}

小写开关名称，实际 Apollo 键为 flag 加点号再加此名称。

```csharp
public required string Name { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Rule \{#Kun_Configuration_FeatureFlagDraft_Rule}

经过固定 Schema 校验的灰度规则。

```csharp
public required FeatureFlagPolicy Rule { get; init; }
```

#### Property Value

[FeatureFlagPolicy](/sdk/api/kun/kun-configuration/featureflagpolicy.md)
