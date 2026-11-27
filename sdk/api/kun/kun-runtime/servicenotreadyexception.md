> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ServiceNotReadyException

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

当前服务尚不可见。不存在与被过滤的服务返回相同错误。

```csharp
public sealed class ServiceNotReadyException : InvalidOperationException, ISerializable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[Exception](https://learn.microsoft.com/dotnet/api/system.exception) ←
[SystemException](https://learn.microsoft.com/dotnet/api/system.systemexception) ←
[InvalidOperationException](https://learn.microsoft.com/dotnet/api/system.invalidoperationexception) ←
[ServiceNotReadyException](/sdk/api/kun/kun-runtime/servicenotreadyexception.md)

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

### ServiceNotReadyException() \{#Kun_Runtime_ServiceNotReadyException__ctor}

构造不暴露不可见原因的异常。

```csharp
public ServiceNotReadyException()
```

## Properties

### Error \{#Kun_Runtime_ServiceNotReadyException_Error}

供宿主转换的统一 NOT_FOUND 错误。

```csharp
public Error Error { get; }
```

#### Property Value

Error
