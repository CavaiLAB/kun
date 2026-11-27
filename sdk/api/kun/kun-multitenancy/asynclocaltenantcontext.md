> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class AsyncLocalTenantContext

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.MultiTenancy` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.MultiTenancy`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.MultiTenancy)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.MultiTenancy](/sdk/api/kun/kun-multitenancy/index.md)\
Assembly: Kun.MultiTenancy.dll

[ITenantContext](/sdk/api/kun/kun-core-multitenancy/itenantcontext.md) implementation backed by an
[`AsyncLocal`](https://learn.microsoft.com/dotnet/api/system.threading.asynclocal-1) so the tenant id flows with the async
call graph across thread hops, task schedulers, and concurrent
parallel tasks. Pair with `TenantContextMiddleware` (or a
background-worker scope that opens a tenant scope explicitly) to
populate the value.

```csharp
public sealed class AsyncLocalTenantContext : ITenantContext
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[AsyncLocalTenantContext](/sdk/api/kun/kun-multitenancy/asynclocaltenantcontext.md)

#### Implements

ITenantContext

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### AsyncLocalTenantContext() \{#Kun_MultiTenancy_AsyncLocalTenantContext__ctor}

```csharp
public AsyncLocalTenantContext()
```

## Properties

### IsMultiTenant \{#Kun_MultiTenancy_AsyncLocalTenantContext_IsMultiTenant}

当前应用是否启用了多租户。

```csharp
public bool IsMultiTenant { get; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

### TenantId \{#Kun_MultiTenancy_AsyncLocalTenantContext_TenantId}

当前请求的租户 ID；后台任务中可能为 [null](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null)。

```csharp
public Guid? TenantId { get; }
```

#### Property Value

[Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

## Methods

### BeginScope(Guid?) \{#Kun_MultiTenancy_AsyncLocalTenantContext_BeginScope_System_Nullable_System_Guid\_\_}

Sets [AsyncLocalTenantContext.TenantId](/sdk/api/kun/kun-multitenancy/asynclocaltenantcontext.md#Kun_MultiTenancy_AsyncLocalTenantContext_TenantId) to `tenantId` for the
returned scope. The previous value is restored on dispose so
nested scopes compose predictably.

```csharp
public IDisposable BeginScope(Guid? tenantId)
```

#### Parameters

`tenantId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)
