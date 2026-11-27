> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class NativeAuditPlugin

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Postgres` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Postgres`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Postgres)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit.Postgres](/sdk/api/kun/kun-audit-postgres/index.md)\
Assembly: Kun.Audit.Postgres.dll

服务与审计策略同属 Apply 效果栈，卸载取消并排空在途调用。

```csharp
public sealed class NativeAuditPlugin : IKunPlugin
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[NativeAuditPlugin](/sdk/api/kun/kun-audit-postgres/nativeauditplugin.md)

#### Implements

IKunPlugin

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### NativeAuditPlugin() \{#Kun_Audit_Postgres_NativeAuditPlugin__ctor}

```csharp
public NativeAuditPlugin()
```

## Properties

### Inject \{#Kun_Audit_Postgres_NativeAuditPlugin_Inject}

Apply 前必须就绪的服务键。

```csharp
public IReadOnlyList<ServiceKey> Inject { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<ServiceKey>

## Methods

### ApplyAsync(IPluginContext, JsonElement, CancellationToken) \{#Kun_Audit_Postgres_NativeAuditPlugin_ApplyAsync_Kun_Runtime_IPluginContext_System_Text_Json_JsonElement_System_Threading_CancellationToken\_}

注册服务、监听器与效果；失败时全部逆序撤销。

```csharp
public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
```

#### Parameters

`ctx` IPluginContext

`config` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

`ct` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
