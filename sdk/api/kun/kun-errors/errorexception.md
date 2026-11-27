> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class ErrorException

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors](/sdk/api/kun/kun-errors/index.md)\
Assembly: Kun.Errors.dll

携带 [ErrorException.Error](/sdk/api/kun/kun-errors/errorexception.md#Kun_Errors_ErrorException_Error) 的异常，Kun 只有这一个带错误码的异常：业务代码直接抛出，或由
`Result<T>.EnsureSuccess()` 在失败时抛出；HTTP 宿主由 Kun.Errors.AspNetCore 写成 problem+json，
gRPC 宿主由 Kun.Grpc 的拦截器转成对应的状态码。

```csharp
public sealed class ErrorException : Exception, ISerializable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[Exception](https://learn.microsoft.com/dotnet/api/system.exception) ←
[ErrorException](/sdk/api/kun/kun-errors/errorexception.md)

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

### ErrorException(Error) \{#Kun_Errors_ErrorException__ctor_Kun_Errors_Error\_}

以一次出错创建异常；异常消息为错误码与 zh-CN 文案，只用于日志。

```csharp
public ErrorException(Error error)
```

#### Parameters

`error` [Error](/sdk/api/kun/kun-errors/error.md)

### ErrorException(Error, Exception?) \{#Kun_Errors_ErrorException__ctor_Kun_Errors_Error_System_Exception\_}

以一次出错与内部异常创建异常。

```csharp
public ErrorException(Error error, Exception? innerException)
```

#### Parameters

`error` [Error](/sdk/api/kun/kun-errors/error.md)

`innerException` [Exception](https://learn.microsoft.com/dotnet/api/system.exception)?

## Properties

### Error \{#Kun_Errors_ErrorException_Error}

出错内容。

```csharp
public Error Error { get; }
```

#### Property Value

[Error](/sdk/api/kun/kun-errors/error.md)
