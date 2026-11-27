> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunErrorsExtensions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Errors.AspNetCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Errors.AspNetCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Errors.AspNetCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Errors.AspNetCore](/sdk/api/kun/kun-errors-aspnetcore/index.md)\
Assembly: Kun.Errors.AspNetCore.dll

装配 Kun 结构化错误（ErrorDetail）。

```csharp
public static class KunErrorsExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunErrorsExtensions](/sdk/api/kun/kun-errors-aspnetcore/kunerrorsextensions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### AddKunErrors(IServiceCollection, Action\<ErrorCatalog>?) \{#Kun_Errors_AspNetCore_KunErrorsExtensions_AddKunErrors_Microsoft_Extensions_DependencyInjection_IServiceCollection_System_Action_Kun_Errors_ErrorCatalog\_\_}

注册错误码目录（含平台内置错误码）、[ErrorException](/sdk/api/kun/kun-errors/errorexception.md) 的异常处理，
并把经 [`IProblemDetailsService`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.iproblemdetailsservice) 写出的一切 ProblemDetails 规范为 ErrorDetail 形状。
可重复调用：目录只建一次，`configure` 每次都作用于同一目录。

```csharp
public static IServiceCollection AddKunErrors(this IServiceCollection services, Action<ErrorCatalog>? configure = null)
```

#### Parameters

`services` [IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

服务集合。

`configure` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<ErrorCatalog>?

登记组件自己的错误码。

#### Returns

[IServiceCollection](https://learn.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)

### UseKunErrors(IApplicationBuilder) \{#Kun_Errors_AspNetCore_KunErrorsExtensions_UseKunErrors_Microsoft_AspNetCore_Builder_IApplicationBuilder\_}

在管道最前面启用异常处理与状态码兜底：未处理异常、空响应体的 4xx / 5xx（未认证、404、请求体无法解析等）
都写成 ErrorDetail。等价于 `UseExceptionHandler()` + `UseStatusCodePages()`。

```csharp
public static IApplicationBuilder UseKunErrors(this IApplicationBuilder app)
```

#### Parameters

`app` [IApplicationBuilder](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.builder.iapplicationbuilder)

#### Returns

[IApplicationBuilder](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.builder.iapplicationbuilder)
