> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IPluginContext

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

插件的服务、效果与作用域入口。

```csharp
public interface IPluginContext
```

## Properties

### Entry \{#Kun_Runtime_IPluginContext_Entry}

拥有当前效果栈的条目；Scoped 不改变条目身份。

```csharp
PluginEntry Entry { get; }
```

#### Property Value

[PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)

### Logger \{#Kun_Runtime_IPluginContext_Logger}

宿主提供的日志器。

```csharp
ILogger Logger { get; }
```

#### Property Value

[ILogger](https://learn.microsoft.com/dotnet/api/microsoft.extensions.logging.ilogger)

### Scope \{#Kun_Runtime_IPluginContext_Scope}

当前解析与注册所在的作用域。

```csharp
ScopeKey Scope { get; }
```

#### Property Value

[ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

### Services \{#Kun_Runtime_IPluginContext_Services}

只读宿主服务入口；以 ServiceKey 解析的 keyed service 经过当前 scope 过滤。

```csharp
IServiceProvider Services { get; }
```

#### Property Value

[IServiceProvider](https://learn.microsoft.com/dotnet/api/system.iserviceprovider)

## Methods

### DispatchAsync\<TPayload, TResult>(KunEvent\<TPayload, TResult>, TPayload, CancellationToken) \{#Kun_Runtime_IPluginContext_DispatchAsync__2_Kun_Runtime_KunEvent___0___1____0_System_Threading_CancellationToken\_}

发起一次 emit、waterfall 或 bail 分发。

```csharp
ValueTask<TResult> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, TResult> evt, TPayload payload, CancellationToken ct)
```

#### Parameters

`evt` [KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, TResult>

`payload` TPayload

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<TResult>

#### Type Parameters

`TPayload`

`TResult`

### DispatchAsync\<TPayload, TResult>(KunEvent\<TPayload, IReadOnlyList\<TResult>>, TPayload, CancellationToken) \{#Kun_Runtime_IPluginContext_DispatchAsync__2_Kun_Runtime_KunEvent___0_System_Collections_Generic_IReadOnlyList___1_____0_System_Threading_CancellationToken\_}

发起 parallel 或 serial 分发，返回监听器顺序的结果集合。

```csharp
ValueTask<IReadOnlyList<TResult>> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, IReadOnlyList<TResult>> evt, TPayload payload, CancellationToken ct)
```

#### Parameters

`evt` [KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<TResult>>

`payload` TPayload

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<TResult>>

#### Type Parameters

`TPayload`

`TResult`

### DispatchAsync\<TPayload, TResult>(KunEvent\<TPayload, TResult>, TPayload, Func\<ValueTask\<TResult>>, CancellationToken) \{#Kun_Runtime_IPluginContext_DispatchAsync__2_Kun_Runtime_KunEvent___0___1____0_System_Func_System_Threading_Tasks_ValueTask___1___System_Threading_CancellationToken\_}

发起 waterfall 分发并指定链尾。

```csharp
ValueTask<TResult> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, TResult> evt, TPayload payload, Func<ValueTask<TResult>> terminal, CancellationToken ct)
```

#### Parameters

`evt` [KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, TResult>

`payload` TPayload

`terminal` [Func](https://learn.microsoft.com/dotnet/api/system.func-1)\<[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<TResult>>

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<TResult>

#### Type Parameters

`TPayload`

`TResult`

### Effect(IDisposable) \{#Kun_Runtime_IPluginContext_Effect_System_IDisposable\_}

将第三方资源加入当前 Apply 的效果栈。

```csharp
void Effect(IDisposable disposer)
```

#### Parameters

`disposer` [IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

### Effect(Action) \{#Kun_Runtime_IPluginContext_Effect_System_Action\_}

加入同步撤销动作；卸载时逆序执行。

```csharp
void Effect(Action dispose)
```

#### Parameters

`dispose` [Action](https://learn.microsoft.com/dotnet/api/system.action)

### Effect(Func\<ValueTask>) \{#Kun_Runtime_IPluginContext_Effect_System_Func_System_Threading_Tasks_ValueTask\_\_}

加入异步撤销动作；逐项 await 后才继续撤销。

```csharp
void Effect(Func<ValueTask> disposeAsync)
```

#### Parameters

`disposeAsync` [Func](https://learn.microsoft.com/dotnet/api/system.func-1)\<[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)>

### Inject\<TService>(ServiceKey) \{#Kun_Runtime_IPluginContext_Inject__1_Kun_Runtime_ServiceKey\_}

解析当前可见的服务；未就绪时抛出 ServiceNotReadyException。

```csharp
TService Inject<TService>(ServiceKey key) where TService : class
```

#### Parameters

`key` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

#### Returns

TService

#### Type Parameters

`TService`

### Inject\<TService>(ServiceKey, string) \{#Kun_Runtime_IPluginContext_Inject__1_Kun_Runtime_ServiceKey_System_String\_}

解析具名 seam 实现；不存在与被过滤时返回相同 NOT_FOUND。

```csharp
TService Inject<TService>(ServiceKey key, string name) where TService : class
```

#### Parameters

`key` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

#### Returns

TService

#### Type Parameters

`TService`

### List(ServiceKey) \{#Kun_Runtime_IPluginContext_List_Kun_Runtime_ServiceKey\_}

返回已应用遮蔽与 restrict 的注册快照。

```csharp
IReadOnlyList<Registration> List(ServiceKey key)
```

#### Parameters

`key` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

#### Returns

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[Registration](/sdk/api/kun/kun-runtime/registration.md)>

### On\<TPayload, TResult>(KunEvent\<TPayload, TResult>, DispatchMode, Delegate, int) \{#Kun_Runtime_IPluginContext_On__2_Kun_Runtime_KunEvent___0___1__Kun_Runtime_DispatchMode_System_Delegate_System_Int32\_}

注册 emit、waterfall 或 bail 监听器，模式必须与 Definition 一致。

```csharp
IDisposable On<TPayload, TResult>(KunEvent<TPayload, TResult> evt, DispatchMode mode, Delegate handler, int priority)
```

#### Parameters

`evt` [KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, TResult>

`mode` [DispatchMode](/sdk/api/kun/kun-runtime/dispatchmode.md)

`handler` [Delegate](https://learn.microsoft.com/dotnet/api/system.delegate)

`priority` [int](https://learn.microsoft.com/dotnet/api/system.int32)

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

#### Type Parameters

`TPayload`

`TResult`

### On\<TPayload, TResult>(KunEvent\<TPayload, TResult>, DispatchMode, Delegate) \{#Kun_Runtime_IPluginContext_On__2_Kun_Runtime_KunEvent___0___1__Kun_Runtime_DispatchMode_System_Delegate\_}

以默认优先级 0 注册监听器。

```csharp
IDisposable On<TPayload, TResult>(KunEvent<TPayload, TResult> evt, DispatchMode mode, Delegate handler)
```

#### Parameters

`evt` [KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, TResult>

`mode` [DispatchMode](/sdk/api/kun/kun-runtime/dispatchmode.md)

`handler` [Delegate](https://learn.microsoft.com/dotnet/api/system.delegate)

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

#### Type Parameters

`TPayload`

`TResult`

### On\<TPayload, TResult>(KunEvent\<TPayload, IReadOnlyList\<TResult>>, DispatchMode, Delegate, int) \{#Kun_Runtime_IPluginContext_On__2_Kun_Runtime_KunEvent___0_System_Collections_Generic_IReadOnlyList___1___Kun_Runtime_DispatchMode_System_Delegate_System_Int32\_}

注册 parallel 或 serial 监听器；单个监听器返回 TResult。

```csharp
IDisposable On<TPayload, TResult>(KunEvent<TPayload, IReadOnlyList<TResult>> evt, DispatchMode mode, Delegate handler, int priority)
```

#### Parameters

`evt` [KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<TResult>>

`mode` [DispatchMode](/sdk/api/kun/kun-runtime/dispatchmode.md)

`handler` [Delegate](https://learn.microsoft.com/dotnet/api/system.delegate)

`priority` [int](https://learn.microsoft.com/dotnet/api/system.int32)

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

#### Type Parameters

`TPayload`

`TResult`

### On\<TPayload, TResult>(KunEvent\<TPayload, IReadOnlyList\<TResult>>, DispatchMode, Delegate) \{#Kun_Runtime_IPluginContext_On__2_Kun_Runtime_KunEvent___0_System_Collections_Generic_IReadOnlyList___1___Kun_Runtime_DispatchMode_System_Delegate\_}

以默认优先级 0 注册集合分发监听器。

```csharp
IDisposable On<TPayload, TResult>(KunEvent<TPayload, IReadOnlyList<TResult>> evt, DispatchMode mode, Delegate handler)
```

#### Parameters

`evt` [KunEvent](/sdk/api/kun/kun-runtime/kunevent-2.md)\<TPayload, [IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<TResult>>

`mode` [DispatchMode](/sdk/api/kun/kun-runtime/dispatchmode.md)

`handler` [Delegate](https://learn.microsoft.com/dotnet/api/system.delegate)

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

#### Type Parameters

`TPayload`

`TResult`

### Provide\<TService>(ServiceKey, TService) \{#Kun_Runtime_IPluginContext_Provide__1_Kun_Runtime_ServiceKey___0\_}

独占注册服务键，返回可重复撤销、可原子替换实例的注册句柄；不自动 Dispose 服务对象。

```csharp
IRegistrationHandle<TService> Provide<TService>(ServiceKey key, TService instance) where TService : class
```

#### Parameters

`key` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

`instance` TService

#### Returns

[IRegistrationHandle](/sdk/api/kun/kun-runtime/iregistrationhandle-1.md)\<TService>

#### Type Parameters

`TService`

### Provide\<TService>(ServiceKey, string, TService) \{#Kun_Runtime_IPluginContext_Provide__1_Kun_Runtime_ServiceKey_System_String___0\_}

注册具名 seam 实现，等价于提供 Key:name 服务键。

```csharp
IRegistrationHandle<TService> Provide<TService>(ServiceKey key, string name, TService instance) where TService : class
```

#### Parameters

`key` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

`name` [string](https://learn.microsoft.com/dotnet/api/system.string)

`instance` TService

#### Returns

[IRegistrationHandle](/sdk/api/kun/kun-runtime/iregistrationhandle-1.md)\<TService>

#### Type Parameters

`TService`

### Restrict(ScopeKey, Func\<Registration, bool>) \{#Kun_Runtime_IPluginContext_Restrict_Kun_Runtime_ScopeKey_System_Func_Kun_Runtime_Registration_System_Boolean\_\_}

过滤该 scope 可见的全局注册；多个纯函数谓词取交集。

```csharp
IDisposable Restrict(ScopeKey scope, Func<Registration, bool> keep)
```

#### Parameters

`scope` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

`keep` [Func](https://learn.microsoft.com/dotnet/api/system.func-2)\<[Registration](/sdk/api/kun/kun-runtime/registration.md), [bool](https://learn.microsoft.com/dotnet/api/system.boolean)>

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

### Scoped(ScopeKey) \{#Kun_Runtime_IPluginContext_Scoped_Kun_Runtime_ScopeKey\_}

返回指定 scope 的上下文，与当前 Apply 共享效果栈，不继承当前 scope。

```csharp
IPluginContext Scoped(ScopeKey scope)
```

#### Parameters

`scope` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

#### Returns

[IPluginContext](/sdk/api/kun/kun-runtime/iplugincontext.md)

### TryInject\<TService>(ServiceKey) \{#Kun_Runtime_IPluginContext_TryInject__1_Kun_Runtime_ServiceKey\_}

解析可选服务；缺失与不可见均返回 null。

```csharp
TService? TryInject<TService>(ServiceKey key) where TService : class
```

#### Parameters

`key` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

#### Returns

TService?

#### Type Parameters

`TService`

### WhenReadyAsync\<TService>(ServiceKey, CancellationToken) \{#Kun_Runtime_IPluginContext_WhenReadyAsync__1_Kun_Runtime_ServiceKey_System_Threading_CancellationToken\_}

等待当前 scope 的服务就绪，可由 ct 取消；上下文卸载后结束等待。

```csharp
ValueTask<TService> WhenReadyAsync<TService>(ServiceKey key, CancellationToken ct) where TService : class
```

#### Parameters

`key` [ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<TService>

#### Type Parameters

`TService`
