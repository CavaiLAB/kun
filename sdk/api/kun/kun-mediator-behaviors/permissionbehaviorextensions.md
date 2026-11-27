> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class PermissionBehaviorExtensions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Mediator` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Mediator`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Mediator)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Mediator.Behaviors](/sdk/api/kun/kun-mediator-behaviors/index.md)\
Assembly: Kun.Mediator.dll

为静态声明权限的命令装配判定行为，缺失引擎时拒绝执行。

```csharp
public static class PermissionBehaviorExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[PermissionBehaviorExtensions](/sdk/api/kun/kun-mediator-behaviors/permissionbehaviorextensions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### AddPermissionBehavior\<TRequest, TResponse>(KunMediatorBuilder, Func\<Error, TResponse>) \{#Kun_Mediator_Behaviors_PermissionBehaviorExtensions_AddPermissionBehavior__2_Kun_Mediator_KunMediatorBuilder_System_Func_Kun_Errors_Error___1\_\_}

拒绝响应由调用方的 Result 工厂提供，不以异常表达正常权限失败；未认证给 [PlatformErrors.Unauthenticated](/sdk/api/kun/kun-errors/platformerrors.md#Kun_Errors_PlatformErrors_Unauthenticated)，
其余拒绝（引擎或权限声明缺失、主体类型无效、判定不允许）一律给 [PlatformErrors.AuthzDenied](/sdk/api/kun/kun-errors/platformerrors.md#Kun_Errors_PlatformErrors_AuthzDenied)。
权限判定器与当前主体从请求作用域的服务里取。

```csharp
public static KunMediatorBuilder AddPermissionBehavior<TRequest, TResponse>(this KunMediatorBuilder builder, Func<Error, TResponse> denied) where TRequest : IRequest<TResponse>, IPermissionProtectedRequest
```

#### Parameters

`builder` [KunMediatorBuilder](/sdk/api/kun/kun-mediator/kunmediatorbuilder.md)

`denied` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<Error, TResponse>

#### Returns

[KunMediatorBuilder](/sdk/api/kun/kun-mediator/kunmediatorbuilder.md)

#### Type Parameters

`TRequest`

`TResponse`

### AddPermissionBehavior\<TRequest, TResponse>(KunMediatorBuilder, Func\<Error, TResponse>, Action\<PermissionBehaviorOptions>) \{#Kun_Mediator_Behaviors_PermissionBehaviorExtensions_AddPermissionBehavior__2_Kun_Mediator_KunMediatorBuilder_System_Func_Kun_Errors_Error___1__System_Action_Kun_Mediator_Behaviors_PermissionBehaviorOptions\_\_}

同 [PermissionBehaviorExtensions.AddPermissionBehavior](/sdk/api/kun/kun-mediator-behaviors/permissionbehaviorextensions.md#Kun_Mediator_Behaviors_PermissionBehaviorExtensions_AddPermissionBehavior__2_Kun_Mediator_KunMediatorBuilder_System_Func_Kun_Errors_Error___1__)，
另由 `configure` 改写这一个请求取权限判定器与当前主体的方式（[PermissionBehaviorOptions](/sdk/api/kun/kun-mediator-behaviors/permissionbehavioroptions.md)），
比如在特定流程里包一层判定器。判定算法与拒绝映射不变。

```csharp
public static KunMediatorBuilder AddPermissionBehavior<TRequest, TResponse>(this KunMediatorBuilder builder, Func<Error, TResponse> denied, Action<PermissionBehaviorOptions> configure) where TRequest : IRequest<TResponse>, IPermissionProtectedRequest
```

#### Parameters

`builder` [KunMediatorBuilder](/sdk/api/kun/kun-mediator/kunmediatorbuilder.md)

`denied` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<Error, TResponse>

`configure` [Action](https://learn.microsoft.com/dotnet/api/system.action-1)\<[PermissionBehaviorOptions](/sdk/api/kun/kun-mediator-behaviors/permissionbehavioroptions.md)>

#### Returns

[KunMediatorBuilder](/sdk/api/kun/kun-mediator/kunmediatorbuilder.md)

#### Type Parameters

`TRequest`

`TResponse`
