> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class NativeAuditComponent

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Postgres` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Postgres`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Postgres)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit.Postgres](/sdk/api/kun/kun-audit-postgres/index.md)\
Assembly: Kun.Audit.Postgres.dll

原生 PostgreSQL 审计的唯一组件入口。

```csharp
[KunPlugin("qian.audit.native")]
[KunConfigSchema("{\"$schema\":\"https://json-schema.org/draft/2020-12/schema\",\"type\":\"object\",\"additionalProperties\":false,\n\"required\":[\"tenantBinding\",\"tenantRole\",\"functionOwner\"],\"description\":\"global挂载须显式提供独立platformBinding/platformRole用于只读启动校验；tenant挂载须有真实可信租户，可只声明已有platformRole而不绑定其凭据。\",\n\"properties\":{\"tenantBinding\":{\"type\":\"string\",\"minLength\":1,\"maxLength\":100},\"platformBinding\":{\"type\":[\"string\",\"null\"],\"minLength\":1,\"maxLength\":100},\n\"tenantRole\":{\"type\":\"string\",\"pattern\":\"^[a-z][a-z0-9_]{0,62}$\"},\"functionOwner\":{\"type\":\"string\",\"pattern\":\"^[a-z][a-z0-9_]{0,62}$\"},\n\"platformRole\":{\"type\":[\"string\",\"null\"],\"pattern\":\"^[a-z][a-z0-9_]{0,62}$\"},\"maxAttempts\":{\"type\":\"integer\",\"minimum\":1,\"maximum\":10,\"default\":6}},\n\"allOf\":[{\"if\":{\"required\":[\"platformBinding\"],\"properties\":{\"platformBinding\":{\"type\":\"string\"}}},\n\"then\":{\"required\":[\"platformRole\"],\"properties\":{\"platformRole\":{\"type\":\"string\"}}}}]}")]
[DelegatePlugin(typeof(NativeAuditPlugin))]
[Provides(typeof(AuditDefinition), "Key")]
[Provides(typeof(CommandPolicies), "AuditKey")]
public sealed class NativeAuditComponent : IKunPlugin
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[NativeAuditComponent](/sdk/api/kun/kun-audit-postgres/nativeauditcomponent.md)

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

### NativeAuditComponent() \{#Kun_Audit_Postgres_NativeAuditComponent__ctor}

```csharp
public NativeAuditComponent()
```

## Properties

### Inject \{#Kun_Audit_Postgres_NativeAuditComponent_Inject}

Apply 前必须就绪的服务键。

```csharp
public IReadOnlyList<ServiceKey> Inject { get; }
```

#### Property Value

[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<ServiceKey>

## Methods

### ApplyAsync(IPluginContext, JsonElement, CancellationToken) \{#Kun_Audit_Postgres_NativeAuditComponent_ApplyAsync_Kun_Runtime_IPluginContext_System_Text_Json_JsonElement_System_Threading_CancellationToken\_}

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
