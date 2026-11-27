> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunDataMisconfiguredException

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore](/sdk/api/kun/kun-entityframeworkcore/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

在构造上下文或启动宿主时拒绝不安全的数据层配置。

```csharp
public sealed class KunDataMisconfiguredException : InvalidOperationException, ISerializable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[Exception](https://learn.microsoft.com/dotnet/api/system.exception) ←
[SystemException](https://learn.microsoft.com/dotnet/api/system.systemexception) ←
[InvalidOperationException](https://learn.microsoft.com/dotnet/api/system.invalidoperationexception) ←
[KunDataMisconfiguredException](/sdk/api/kun/kun-entityframeworkcore/kundatamisconfiguredexception.md)

#### Implements

[ISerializable](https://learn.microsoft.com/dotnet/api/system.runtime.serialization.iserializable)

#### Inherited Members

[Exception.GetBaseException()](https://learn.microsoft.com/dotnet/api/system.exception.getbaseexception),
[Exception.ToString()](https://learn.microsoft.com/dotnet/api/system.exception.tostring),
[Exception.GetType()](https://learn.microsoft.com/dotnet/api/system.exception.gettype),
[Exception.TargetSite](https://learn.microsoft.com/dotnet/api/system.exception.targetsite),
[Exception.Message](https://learn.microsoft.com/dotnet/api/system.exception.message),
[Exception.Data](https://learn.microsoft.com/dotnet/api/system.exception.data),
[Exception.InnerException](https://learn.microsoft.com/dotnet/api/system.exception.innerexception),
[Exception.HelpLink](https://learn.microsoft.com/dotnet/api/system.exception.helplink),
[Exception.Source](https://learn.microsoft.com/dotnet/api/system.exception.source),
[Exception.HResult](https://learn.microsoft.com/dotnet/api/system.exception.hresult),
[Exception.StackTrace](https://learn.microsoft.com/dotnet/api/system.exception.stacktrace),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### KunDataMisconfiguredException(string) \{#Kun_EntityFrameworkCore_KunDataMisconfiguredException__ctor_System_String\_}

指定缺失或被覆盖的安全配置。

```csharp
public KunDataMisconfiguredException(string reason)
```

#### Parameters

`reason` [string](https://learn.microsoft.com/dotnet/api/system.string)

## Fields

### Code \{#Kun_EntityFrameworkCore_KunDataMisconfiguredException_Code}

这个异常对应的稳定错误码（`DATA_MISCONFIGURED`，归属 `qian.data`）。
由异常类型自己公开，不依赖平台的错误码类；装配数据组件的宿主（`AddKunData`）把它登记进错误码目录。

```csharp
public static readonly ErrorCode Code
```

#### Field Value

ErrorCode

## Properties

### Error \{#Kun_EntityFrameworkCore_KunDataMisconfiguredException_Error}

供宿主记录的统一错误详情。

```csharp
public Error Error { get; }
```

#### Property Value

Error
