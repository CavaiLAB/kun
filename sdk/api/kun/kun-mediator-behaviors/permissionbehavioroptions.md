> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class PermissionBehaviorOptions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Mediator` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Mediator`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Mediator)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Mediator.Behaviors](/sdk/api/kun/kun-mediator-behaviors/index.md)\
Assembly: Kun.Mediator.dll

权限行为取权限判定器与当前主体的方式；缺省都从请求作用域的服务里取，取不到即拒绝。

```csharp
public sealed class PermissionBehaviorOptions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[PermissionBehaviorOptions](/sdk/api/kun/kun-mediator-behaviors/permissionbehavioroptions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### PermissionBehaviorOptions() \{#Kun_Mediator_Behaviors_PermissionBehaviorOptions__ctor}

```csharp
public PermissionBehaviorOptions()
```

## Properties

### CurrentUser \{#Kun_Mediator_Behaviors_PermissionBehaviorOptions_CurrentUser}

取当前主体；返回 null 或未认证时拒绝（[PlatformErrors.Unauthenticated](/sdk/api/kun/kun-errors/platformerrors.md#Kun_Errors_PlatformErrors_Unauthenticated)）。

```csharp
public Func<IServiceProvider, ICurrentUser?> CurrentUser { get; set; }
```

#### Property Value

[Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[IServiceProvider](https://learn.microsoft.com/dotnet/api/system.iserviceprovider), ICurrentUser?>

### PermissionChecker \{#Kun_Mediator_Behaviors_PermissionBehaviorOptions_PermissionChecker}

取权限判定器；返回 null 时一律拒绝（[PlatformErrors.AuthzDenied](/sdk/api/kun/kun-errors/platformerrors.md#Kun_Errors_PlatformErrors_AuthzDenied)）。

```csharp
public Func<IServiceProvider, IPermissionChecker?> PermissionChecker { get; set; }
```

#### Property Value

[Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[IServiceProvider](https://learn.microsoft.com/dotnet/api/system.iserviceprovider), IPermissionChecker?>
