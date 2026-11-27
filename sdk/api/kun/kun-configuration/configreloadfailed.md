> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ConfigReloadFailed

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

重载失败告警（[ConfigEvents.ReloadFailedType](/sdk/api/kun/kun-configuration/configevents.md#Kun_Configuration_ConfigEvents_ReloadFailedType)）：options 类型与名称、配置节、失败条数与错误码；
不含任何配置值，也不含校验消息原文（自定义校验器的消息可能带值）。

```csharp
public sealed record ConfigReloadFailed : IEquatable<ConfigReloadFailed>
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ConfigReloadFailed](/sdk/api/kun/kun-configuration/configreloadfailed.md)

#### Implements

[IEquatable\<ConfigReloadFailed>](https://learn.microsoft.com/dotnet/api/system.iequatable-1)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### ConfigReloadFailed(string, string, string, int, string) \{#Kun_Configuration_ConfigReloadFailed__ctor_System_String_System_String_System_String_System_Int32_System_String\_}

重载失败告警（[ConfigEvents.ReloadFailedType](/sdk/api/kun/kun-configuration/configevents.md#Kun_Configuration_ConfigEvents_ReloadFailedType)）：options 类型与名称、配置节、失败条数与错误码；
不含任何配置值，也不含校验消息原文（自定义校验器的消息可能带值）。

```csharp
public ConfigReloadFailed(string OptionsType, string OptionsName, string Section, int Failures, string Code)
```

#### Parameters

`OptionsType` [string](https://learn.microsoft.com/dotnet/api/system.string)

`OptionsName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Section` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Failures` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`Code` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### Code \{#Kun_Configuration_ConfigReloadFailed_Code}

```csharp
public string Code { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Failures \{#Kun_Configuration_ConfigReloadFailed_Failures}

```csharp
public int Failures { get; init; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### OptionsName \{#Kun_Configuration_ConfigReloadFailed_OptionsName}

```csharp
public string OptionsName { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### OptionsType \{#Kun_Configuration_ConfigReloadFailed_OptionsType}

```csharp
public string OptionsType { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Plugin \{#Kun_Configuration_ConfigReloadFailed_Plugin}

登记该配置的组件 ID；宿主未指明时为 null。

```csharp
public string? Plugin { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### Section \{#Kun_Configuration_ConfigReloadFailed_Section}

```csharp
public string Section { get; init; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### \<Clone>$() \{#Kun_Configuration_ConfigReloadFailed__Clone\_\_}

```csharp
public ConfigReloadFailed <Clone>$()
```

#### Returns

[ConfigReloadFailed](/sdk/api/kun/kun-configuration/configreloadfailed.md)

### Deconstruct(out string, out string, out string, out int, out string) \{#Kun_Configuration_ConfigReloadFailed_Deconstruct_System_String__System_String__System_String__System_Int32__System_String\_\_}

```csharp
public void Deconstruct(out string OptionsType, out string OptionsName, out string Section, out int Failures, out string Code)
```

#### Parameters

`OptionsType` [string](https://learn.microsoft.com/dotnet/api/system.string)

`OptionsName` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Section` [string](https://learn.microsoft.com/dotnet/api/system.string)

`Failures` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`Code` [string](https://learn.microsoft.com/dotnet/api/system.string)

### Equals(object?) \{#Kun_Configuration_ConfigReloadFailed_Equals_System_Object\_}

```csharp
public override bool Equals(object? obj)
```

#### Parameters

`obj` [object](https://learn.microsoft.com/dotnet/api/system.object)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### Equals(ConfigReloadFailed?) \{#Kun_Configuration_ConfigReloadFailed_Equals_Kun_Configuration_ConfigReloadFailed\_}

```csharp
public bool Equals(ConfigReloadFailed? other)
```

#### Parameters

`other` [ConfigReloadFailed](/sdk/api/kun/kun-configuration/configreloadfailed.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### GetHashCode() \{#Kun_Configuration_ConfigReloadFailed_GetHashCode}

```csharp
public override int GetHashCode()
```

#### Returns

[int](https://learn.microsoft.com/dotnet/api/system.int32)

### ToString() \{#Kun_Configuration_ConfigReloadFailed_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Operators

### operator ==(ConfigReloadFailed?, ConfigReloadFailed?) \{#Kun_Configuration_ConfigReloadFailed_op_Equality_Kun_Configuration_ConfigReloadFailed_Kun_Configuration_ConfigReloadFailed\_}

```csharp
public static bool operator ==(ConfigReloadFailed? left, ConfigReloadFailed? right)
```

#### Parameters

`left` [ConfigReloadFailed](/sdk/api/kun/kun-configuration/configreloadfailed.md)?

`right` [ConfigReloadFailed](/sdk/api/kun/kun-configuration/configreloadfailed.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### operator !=(ConfigReloadFailed?, ConfigReloadFailed?) \{#Kun_Configuration_ConfigReloadFailed_op_Inequality_Kun_Configuration_ConfigReloadFailed_Kun_Configuration_ConfigReloadFailed\_}

```csharp
public static bool operator !=(ConfigReloadFailed? left, ConfigReloadFailed? right)
```

#### Parameters

`left` [ConfigReloadFailed](/sdk/api/kun/kun-configuration/configreloadfailed.md)?

`right` [ConfigReloadFailed](/sdk/api/kun/kun-configuration/configreloadfailed.md)?

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
