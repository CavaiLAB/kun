> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class TenantContextMiddleware

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.MultiTenancy` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.MultiTenancy`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.MultiTenancy)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.MultiTenancy.Middleware](/sdk/api/kun/kun-multitenancy-middleware/index.md)\
Assembly: Kun.MultiTenancy.dll

为 HTTP 请求打开租户作用域。

```csharp
public sealed class TenantContextMiddleware
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[TenantContextMiddleware](/sdk/api/kun/kun-multitenancy-middleware/tenantcontextmiddleware.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### TenantContextMiddleware(RequestDelegate, TenantRequestMode) \{#Kun_MultiTenancy_Middleware_TenantContextMiddleware__ctor_Microsoft_AspNetCore_Http_RequestDelegate_Kun_MultiTenancy_TenantRequestMode\_}

为 HTTP 请求打开租户作用域。

```csharp
public TenantContextMiddleware(RequestDelegate next, TenantRequestMode mode)
```

#### Parameters

`next` [RequestDelegate](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.requestdelegate)

`mode` [TenantRequestMode](/sdk/api/kun/kun-multitenancy/tenantrequestmode.md)

## Methods

### InvokeAsync(HttpContext, ITenantContext) \{#Kun_MultiTenancy_Middleware_TenantContextMiddleware_InvokeAsync_Microsoft_AspNetCore_Http_HttpContext_Kun_Core_MultiTenancy_ITenantContext\_}

建立请求上下文后进入后续管道。

```csharp
public Task InvokeAsync(HttpContext httpContext, ITenantContext tenantContext)
```

#### Parameters

`httpContext` [HttpContext](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.httpcontext)

`tenantContext` ITenantContext

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)
