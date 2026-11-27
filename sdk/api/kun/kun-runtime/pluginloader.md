> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class PluginLoader

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Runtime` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Runtime`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Runtime)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Runtime](/sdk/api/kun/kun-runtime/index.md)\
Assembly: Kun.Runtime.dll

按服务依赖装载插件。代码实例由宿主提供，不加载动态程序集、不读数据库。

```csharp
public sealed class PluginLoader : IAsyncDisposable
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[PluginLoader](/sdk/api/kun/kun-runtime/pluginloader.md)

#### Implements

[IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### PluginLoader(IServiceProvider, ILogger?) \{#Kun_Runtime_PluginLoader__ctor_System_IServiceProvider_Microsoft_Extensions_Logging_ILogger\_}

使用宿主 DI 与日志器创建装载器；装载器不拥有宿主 DI 的生命周期。

```csharp
public PluginLoader(IServiceProvider services, ILogger? logger = null)
```

#### Parameters

`services` [IServiceProvider](https://learn.microsoft.com/dotnet/api/system.iserviceprovider)

`logger` [ILogger](https://learn.microsoft.com/dotnet/api/microsoft.extensions.logging.ilogger)?

## Methods

### DisposeAsync() \{#Kun_Runtime_PluginLoader_DisposeAsync}

按 Apply 顺序逆序卸载整棵树；清理失败记录在日志，继续清理其它条目。

```csharp
public ValueTask DisposeAsync()
```

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### DumpConfig() \{#Kun_Runtime_PluginLoader_DumpConfig}

取得当前挂载树的配置快照；已卸载或正在移除的条目不进入管理面 dump-config。

```csharp
public IReadOnlyList<PluginEntry> DumpConfig()
```

#### Returns

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[PluginEntry](/sdk/api/kun/kun-runtime/pluginentry.md)>

### GetContext(string, ScopeKey) \{#Kun_Runtime_PluginLoader_GetContext_System_String_Kun_Runtime_ScopeKey\_}

取得当前 Apply 的上下文；重新 Apply 后旧上下文作废。

```csharp
public IPluginContext GetContext(string id, ScopeKey scope)
```

#### Parameters

`id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`scope` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

#### Returns

[IPluginContext](/sdk/api/kun/kun-runtime/iplugincontext.md)

### GetFailure(string, ScopeKey) \{#Kun_Runtime_PluginLoader_GetFailure_System_String_Kun_Runtime_ScopeKey\_}

查询 Apply 或清理失败；仅用于管理诊断，不作为业务响应原文。

```csharp
public Exception? GetFailure(string id, ScopeKey scope)
```

#### Parameters

`id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`scope` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

#### Returns

[Exception](https://learn.microsoft.com/dotnet/api/system.exception)?

### GetPendingDependencies(string, ScopeKey) \{#Kun_Runtime_PluginLoader_GetPendingDependencies_System_String_Kun_Runtime_ScopeKey\_}

管理面诊断当前条目等待的服务键。

```csharp
public IReadOnlyList<ServiceKey> GetPendingDependencies(string id, ScopeKey scope)
```

#### Parameters

`id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`scope` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

#### Returns

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[ServiceKey](/sdk/api/kun/kun-runtime/servicekey.md)>

### GetState(string, ScopeKey) \{#Kun_Runtime_PluginLoader_GetState_System_String_Kun_Runtime_ScopeKey\_}

查询条目的当前状态。

```csharp
public PluginState GetState(string id, ScopeKey scope)
```

#### Parameters

`id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`scope` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

#### Returns

[PluginState](/sdk/api/kun/kun-runtime/pluginstate.md)

### MountAsync(IEnumerable\<PluginDefinition>, CancellationToken) \{#Kun_Runtime_PluginLoader_MountAsync_System_Collections_Generic_IEnumerable_Kun_Runtime_PluginDefinition__System_Threading_CancellationToken\_}

一次提交完整依赖集合；缺失键与环路在任何 Apply 之前失败。

```csharp
public ValueTask MountAsync(IEnumerable<PluginDefinition> definitions, CancellationToken ct = default)
```

#### Parameters

`definitions` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[PluginDefinition](/sdk/api/kun/kun-runtime/plugindefinition.md)>

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### ReloadAsync(PluginDefinition, CancellationToken) \{#Kun_Runtime_PluginLoader_ReloadAsync_Kun_Runtime_PluginDefinition_System_Threading_CancellationToken\_}

配置重载：先卸载旧条目，再以新配置 Apply；不支持替换已加载的代码程序集。

```csharp
public ValueTask ReloadAsync(PluginDefinition definition, CancellationToken ct = default)
```

#### Parameters

`definition` [PluginDefinition](/sdk/api/kun/kun-runtime/plugindefinition.md)

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### UnmountAsync(string, ScopeKey, CancellationToken) \{#Kun_Runtime_PluginLoader_UnmountAsync_System_String_Kun_Runtime_ScopeKey_System_Threading_CancellationToken\_}

先撤销依赖者，再逆序撤销条目效果；可用其它 Provider 恢复的依赖者随后重新 Apply。

```csharp
public ValueTask UnmountAsync(string id, ScopeKey scope, CancellationToken ct = default)
```

#### Parameters

`id` [string](https://learn.microsoft.com/dotnet/api/system.string)

`scope` [ScopeKey](/sdk/api/kun/kun-runtime/scopekey.md)

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### WhenIdleAsync(CancellationToken) \{#Kun_Runtime_PluginLoader_WhenIdleAsync_System_Threading_CancellationToken\_}

等待通过 IDisposable 撤销注册后触发的异步级联卸载与恢复。

```csharp
public ValueTask WhenIdleAsync(CancellationToken ct = default)
```

#### Parameters

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
