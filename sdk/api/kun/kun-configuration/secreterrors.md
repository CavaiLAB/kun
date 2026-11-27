> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class SecretErrors

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Configuration.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Configuration.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Configuration.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Configuration](/sdk/api/kun/kun-configuration/index.md)\
Assembly: Kun.Configuration.Abstractions.dll

秘密解析的统一错误，不把输入值或物理路径放进错误文案。

```csharp
public static class SecretErrors
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[SecretErrors](/sdk/api/kun/kun-configuration/secreterrors.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### AccessDenied \{#Kun_Configuration_SecretErrors_AccessDenied}

引用未获声明和审批授权，或租户不匹配。

```csharp
public static readonly ErrorCode AccessDenied
```

#### Field Value

ErrorCode

### Exposed \{#Kun_Configuration_SecretErrors_Exposed}

配置中出现非引用敏感数据。

```csharp
public static readonly ErrorCode Exposed
```

#### Field Value

ErrorCode

### InvalidReference \{#Kun_Configuration_SecretErrors_InvalidReference}

引用格式不合法。

```csharp
public static readonly ErrorCode InvalidReference
```

#### Field Value

ErrorCode

### NotFound \{#Kun_Configuration_SecretErrors_NotFound}

允许访问的引用没有可用文件。

```csharp
public static readonly ErrorCode NotFound
```

#### Field Value

ErrorCode

### SchemaInvalid \{#Kun_Configuration_SecretErrors_SchemaInvalid}

敏感字段规则无法安全解释。

```csharp
public static readonly ErrorCode SchemaInvalid
```

#### Field Value

ErrorCode

### Unavailable \{#Kun_Configuration_SecretErrors_Unavailable}

秘密读取或审计设施暂不可用。

```csharp
public static readonly ErrorCode Unavailable
```

#### Field Value

ErrorCode

## Methods

### Register(ErrorCatalog) \{#Kun_Configuration_SecretErrors_Register_Kun_Errors_ErrorCatalog\_}

向宿主目录登记本组件错误。

```csharp
public static IDisposable Register(ErrorCatalog catalog)
```

#### Parameters

`catalog` ErrorCatalog

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)
