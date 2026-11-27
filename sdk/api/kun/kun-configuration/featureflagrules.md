> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class FeatureFlagRules

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

解析并校验配置快照中的特性开关规则。

```csharp
public static class FeatureFlagRules
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[FeatureFlagRules](/sdk/api/kun/kun-configuration/featureflagrules.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### MaximumTenants \{#Kun_Configuration_FeatureFlagRules_MaximumTenants}

每份规则最多指定的租户数。

```csharp
public const int MaximumTenants = 256
```

#### Field Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### Namespace \{#Kun_Configuration_FeatureFlagRules_Namespace}

约定的开关命名空间。

```csharp
public const string Namespace = "qian.common.flags"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Prefix \{#Kun_Configuration_FeatureFlagRules_Prefix}

规则键前缀。

```csharp
public const string Prefix = "flag."
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### Read(IEnumerable\<KeyValuePair\<string, string?>>) \{#Kun_Configuration_FeatureFlagRules_Read_System_Collections_Generic_IEnumerable_System_Collections_Generic_KeyValuePair_System_String_System_String\_\_\_}

读取不可变规则集，规则无效时抛出 InvalidDataException。

```csharp
public static FrozenDictionary<string, FeatureFlagRule> Read(IEnumerable<KeyValuePair<string, string?>> values)
```

#### Parameters

`values` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[KeyValuePair](https://learn.microsoft.com/dotnet/api/system.collections.generic.keyvaluepair-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)?>>

#### Returns

[FrozenDictionary](https://learn.microsoft.com/dotnet/api/system.collections.frozen.frozendictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [FeatureFlagRule](/sdk/api/kun/kun-configuration/featureflagrule.md)>

### ValidName(string?) \{#Kun_Configuration_FeatureFlagRules_ValidName_System_String\_}

检查开关名称的格式。

```csharp
public static bool ValidName(string? name)
```

#### Parameters

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Validate(IReadOnlyDictionary\<string, string?>) \{#Kun_Configuration_FeatureFlagRules_Validate_System_Collections_Generic_IReadOnlyDictionary_System_String_System_String\_\_}

检查快照中的规则是否都能解析。

```csharp
public static bool Validate(IReadOnlyDictionary<string, string?> values)
```

#### Parameters

`values` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)?>

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
