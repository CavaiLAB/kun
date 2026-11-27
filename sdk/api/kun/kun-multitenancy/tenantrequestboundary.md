> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class TenantRequestBoundary

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.MultiTenancy` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.MultiTenancy`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.MultiTenancy)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.MultiTenancy](/sdk/api/kun/kun-multitenancy/index.md)\
Assembly: Kun.MultiTenancy.dll

按已认证主身份的 tenant_id 建立本次请求的租户作用域。

```csharp
public sealed class TenantRequestBoundary
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[TenantRequestBoundary](/sdk/api/kun/kun-multitenancy/tenantrequestboundary.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### TenantRequestBoundary(ITenantContext) \{#Kun_MultiTenancy_TenantRequestBoundary__ctor_Kun_Core_MultiTenancy_ITenantContext\_}

按已认证主身份的 tenant_id 建立本次请求的租户作用域。

```csharp
public TenantRequestBoundary(ITenantContext tenant)
```

#### Parameters

`tenant` ITenantContext

## Methods

### InvokeAsync(HttpContext, RequestDelegate) \{#Kun_MultiTenancy_TenantRequestBoundary_InvokeAsync_Microsoft_AspNetCore_Http_HttpContext_Microsoft_AspNetCore_Http_RequestDelegate\_}

在租户作用域内执行请求，完成、异常或取消后还原上下文。

```csharp
public Task InvokeAsync(HttpContext context, RequestDelegate next)
```

#### Parameters

`context` [HttpContext](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.httpcontext)

`next` [RequestDelegate](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.requestdelegate)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

### InvokeAsync(HttpContext, TenantRequestMode, RequestDelegate) \{#Kun_MultiTenancy_TenantRequestBoundary_InvokeAsync_Microsoft_AspNetCore_Http_HttpContext_Kun_MultiTenancy_TenantRequestMode_Microsoft_AspNetCore_Http_RequestDelegate\_}

按指定入口模式在当前请求的租户范围内执行委托。

```csharp
public Task InvokeAsync(HttpContext context, TenantRequestMode mode, RequestDelegate next)
```

#### Parameters

`context` [HttpContext](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.httpcontext)

`mode` [TenantRequestMode](/sdk/api/kun/kun-multitenancy/tenantrequestmode.md)

`next` [RequestDelegate](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.requestdelegate)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)
