> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class TenantContextPropagator

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.MultiTenancy` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.MultiTenancy`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.MultiTenancy)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.MultiTenancy](/sdk/api/kun/kun-multitenancy/index.md)\
Assembly: Kun.MultiTenancy.dll

复用唯一 ITenantContext；传播字段只携带租户，不授予调用权限。

```csharp
public sealed class TenantContextPropagator : ITenantContextPropagator
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[TenantContextPropagator](/sdk/api/kun/kun-multitenancy/tenantcontextpropagator.md)

#### Implements

[ITenantContextPropagator](/sdk/api/kun/kun-multitenancy-abstractions/itenantcontextpropagator.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### TenantContextPropagator(ITenantContext) \{#Kun_MultiTenancy_TenantContextPropagator__ctor_Kun_Core_MultiTenancy_ITenantContext\_}

复用唯一 ITenantContext；传播字段只携带租户，不授予调用权限。

```csharp
public TenantContextPropagator(ITenantContext tenant)
```

#### Parameters

`tenant` ITenantContext

## Methods

### BeginScope(JsonElement, Guid) \{#Kun_MultiTenancy_TenantContextPropagator_BeginScope_System_Text_Json_JsonElement_System_Guid\_}

消费已认证来源的 JSON；authorizedTenantId 必须来自令牌或受保护调度记录，不能取自同一输入。
例外是集成事件消费端（R122）：没有独立于消息的令牌，carrier 为信封原文，authorizedTenantId 为已与 AMQP 头核对一致的信封租户，
这里只做缺失、重复、不一致与租户冲突的 fail-closed 判定，不提供独立授权；信任根是 broker 的连接认证与发布端 Outbox 写入端。

```csharp
public IDisposable BeginScope(JsonElement carrier, Guid authorizedTenantId)
```

#### Parameters

`carrier` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

`authorizedTenantId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

### BeginScope(IEnumerable\<KeyValuePair\<string, string>>, Guid) \{#Kun_MultiTenancy_TenantContextPropagator_BeginScope_System_Collections_Generic_IEnumerable_System_Collections_Generic_KeyValuePair_System_String_System_String___System_Guid\_}

消费已认证来源的元数据；保留重复项以便拒绝重复 tenant_id。

```csharp
public IDisposable BeginScope(IEnumerable<KeyValuePair<string, string>> headers, Guid authorizedTenantId)
```

#### Parameters

`headers` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable-1)\<[KeyValuePair](https://learn.microsoft.com/dotnet/api/system.collections.generic.keyvaluepair-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>>

`authorizedTenantId` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

#### Returns

[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable)

### Write(JsonElement) \{#Kun_MultiTenancy_TenantContextPropagator_Write_System_Text_Json_JsonElement\_}

从当前可信上下文写入 tenant_id；已有不同值、重复字段或无租户时拒绝。

```csharp
public JsonElement Write(JsonElement carrier)
```

#### Parameters

`carrier` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

#### Returns

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

### WriteHeaders() \{#Kun_MultiTenancy_TenantContextPropagator_WriteHeaders}

从当前可信上下文生成只包含 tenant_id 的传输头。

```csharp
public IReadOnlyDictionary<string, string> WriteHeaders()
```

#### Returns

[IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary-2)\<[string](https://learn.microsoft.com/dotnet/api/system.string), [string](https://learn.microsoft.com/dotnet/api/system.string)>
