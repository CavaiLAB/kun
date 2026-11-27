> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class NativeDataComponent

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Data.Native.Postgres` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Data.Native.Postgres`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Data.Native.Postgres)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Data.Native.Postgres](/sdk/api/kun/kun-data-native-postgres/index.md)\
Assembly: Kun.Data.Native.Postgres.dll

qian.data.native 唯一入口；仅挂载于 global，提供独立 NativeData 键。

```csharp
[KunPlugin("qian.data.native")]
[KunConfigSchema("{\"$schema\":\"https://json-schema.org/draft/2020-12/schema\",\"type\":\"object\",\"additionalProperties\":false,\n \"required\":[\"connections\"],\"properties\":{\"connections\":{\"type\":\"array\",\"minItems\":1,\"uniqueItems\":true,\n   \"items\":{\"type\":\"string\",\"pattern\":\"^[a-z][a-z0-9_-]{0,62}$\"},\"description\":\"受信任宿主登记的连接绑定名，不是连接串。\"}}}")]
public sealed class NativeDataComponent : IKunPlugin
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[NativeDataComponent](/sdk/api/kun/kun-data-native-postgres/nativedatacomponent.md)

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

### NativeDataComponent() \{#Kun_Data_Native_Postgres_NativeDataComponent__ctor}

```csharp
public NativeDataComponent()
```

## Properties

### Inject \{#Kun_Data_Native_Postgres_NativeDataComponent_Inject}

Apply 前必须就绪的服务键。

```csharp
public IReadOnlyList<ServiceKey> Inject { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<ServiceKey>

## Methods

### ApplyAsync(IPluginContext, JsonElement, CancellationToken) \{#Kun_Data_Native_Postgres_NativeDataComponent_ApplyAsync_Kun_Runtime_IPluginContext_System_Text_Json_JsonElement_System_Threading_CancellationToken\_}

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
