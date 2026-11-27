> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ConfigErrors

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

配置组件的稳定错误词汇。

```csharp
public static class ConfigErrors
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[ConfigErrors](/sdk/api/kun/kun-configuration/configerrors.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### BootstrapInvalid \{#Kun_Configuration_ConfigErrors_BootstrapInvalid}

引导项不合法。

```csharp
public static readonly ErrorCode BootstrapInvalid
```

#### Field Value

ErrorCode

### ReloadRejected \{#Kun_Configuration_ConfigErrors_ReloadRejected}

候选配置未通过校验：强类型配置重载或 Apollo 合并快照的自定义校验被拒绝，已保留上一份有效值。

```csharp
public static readonly ErrorCode ReloadRejected
```

#### Field Value

ErrorCode

### RequiredMissing \{#Kun_Configuration_ConfigErrors_RequiredMissing}

运行所需的配置缺失。

```csharp
public static readonly ErrorCode RequiredMissing
```

#### Field Value

ErrorCode

## Methods

### Register(ErrorCatalog) \{#Kun_Configuration_ConfigErrors_Register_Kun_Errors_ErrorCatalog\_}

登记错误词汇；释放时只撤销本次登记。

```csharp
public static IDisposable Register(ErrorCatalog catalog)
```

#### Parameters

`catalog` ErrorCatalog

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)
