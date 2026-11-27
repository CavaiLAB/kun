> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunErrorResult

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors.AspNetCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors.AspNetCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors.AspNetCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors.AspNetCore](/sdk/api/kun/kun-errors-aspnetcore/index.md)\
Assembly: Kun.Errors.AspNetCore.dll

写出 ErrorDetail 的 [`IResult`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.iresult)。

```csharp
public sealed class KunErrorResult : IResult, IStatusCodeHttpResult, IContentTypeHttpResult
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunErrorResult](/sdk/api/kun/kun-errors-aspnetcore/kunerrorresult.md)

#### Implements

[IResult](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.iresult),
[IStatusCodeHttpResult](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.istatuscodehttpresult),
[IContentTypeHttpResult](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.icontenttypehttpresult)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Properties

### ContentType \{#Kun_Errors_AspNetCore_KunErrorResult_ContentType}

```csharp
public string ContentType { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Error \{#Kun_Errors_AspNetCore_KunErrorResult_Error}

出错内容。

```csharp
public Error Error { get; }
```

#### Property Value

Error

### StatusCode \{#Kun_Errors_AspNetCore_KunErrorResult_StatusCode}

```csharp
public int? StatusCode { get; }
```

#### Property Value

[int](https://learn.microsoft.com/dotnet/api/system.int32)?

## Methods

### ExecuteAsync(HttpContext) \{#Kun_Errors_AspNetCore_KunErrorResult_ExecuteAsync_Microsoft_AspNetCore_Http_HttpContext\_}

```csharp
public Task ExecuteAsync(HttpContext httpContext)
```

#### Parameters

`httpContext` [HttpContext](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.httpcontext)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)
