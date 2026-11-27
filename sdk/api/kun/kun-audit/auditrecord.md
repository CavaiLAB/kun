> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class AuditRecord

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit](/sdk/api/kun/kun-audit/index.md)\
Assembly: Kun.Audit.Abstractions.dll

进入审计链的不可变记录；输入只保留脱敏后的哈希，Payload 须由可信调用方按审计契约构造。

```csharp
public sealed class AuditRecord
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[AuditRecord](/sdk/api/kun/kun-audit/auditrecord.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### AuditRecord(Guid, string?, string, string, string?, string, string, string, string, string?, string, string, DateTimeOffset, JsonElement) \{#Kun_Audit_AuditRecord__ctor_System_Guid_System_String_System_String_System_String_System_String_System_String_System_String_System_String_System_String_System_String_System_String_System_String_System_DateTimeOffset_System_Text_Json_JsonElement\_}

构造已校验的记录；时间规整到 PostgreSQL 微秒精度。平台记录可表示，但不因此获得平台写权限。

```csharp
public AuditRecord(Guid id, string? tenantId, string actorType, string actorId, string? onBehalfOf, string capabilityId, string contractVersion, string inputHash, string channel, string? resource, string result, string traceId, DateTimeOffset occurredAt, JsonElement payload)
```

#### Parameters

`id` [Guid](https://learn.microsoft.com/dotnet/api/system.guid)

`tenantId` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`actorType` [string](https://learn.microsoft.com/dotnet/api/system.string)

`actorId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`onBehalfOf` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`capabilityId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`contractVersion` [string](https://learn.microsoft.com/dotnet/api/system.string)

`inputHash` [string](https://learn.microsoft.com/dotnet/api/system.string)

`channel` [string](https://learn.microsoft.com/dotnet/api/system.string)

`resource` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`result` [string](https://learn.microsoft.com/dotnet/api/system.string)

`traceId` [string](https://learn.microsoft.com/dotnet/api/system.string)

`occurredAt` [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

`payload` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

## Properties

### ActorId \{#Kun_Audit_AuditRecord_ActorId}

执行主体 ID。

```csharp
public string ActorId { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ActorType \{#Kun_Audit_AuditRecord_ActorType}

user / service / agent。

```csharp
public string ActorType { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### CapabilityId \{#Kun_Audit_AuditRecord_CapabilityId}

稳定能力 ID。

```csharp
public string CapabilityId { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Channel \{#Kun_Audit_AuditRecord_Channel}

调用通道。

```csharp
public string Channel { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ContractVersion \{#Kun_Audit_AuditRecord_ContractVersion}

能力契约版本。

```csharp
public string ContractVersion { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Id \{#Kun_Audit_AuditRecord_Id}

全局事件实例 ID，写入重试复用。

```csharp
public Guid Id { get; }
```

#### Property Value

[Guid](https://learn.microsoft.com/dotnet/api/system.guid)

### InputHash \{#Kun_Audit_AuditRecord_InputHash}

脱敏后的输入哈希。

```csharp
public string InputHash { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### OccurredAt \{#Kun_Audit_AuditRecord_OccurredAt}

已规整为 UTC 微秒精度的发生时间。

```csharp
public DateTimeOffset OccurredAt { get; }
```

#### Property Value

[DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

### OnBehalfOf \{#Kun_Audit_AuditRecord_OnBehalfOf}

代行的用户 ID。

```csharp
public string? OnBehalfOf { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### Payload \{#Kun_Audit_AuditRecord_Payload}

按能力审计契约构造的补充元数据，不能直接传原始输入。

```csharp
public JsonElement Payload { get; }
```

#### Property Value

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

### Resource \{#Kun_Audit_AuditRecord_Resource}

资源引用。

```csharp
public string? Resource { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### Result \{#Kun_Audit_AuditRecord_Result}

执行结果。

```csharp
public string Result { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### TenantId \{#Kun_Audit_AuditRecord_TenantId}

租户 UUID；平台级为 null。

```csharp
public string? TenantId { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### TraceId \{#Kun_Audit_AuditRecord_TraceId}

W3C 追踪标识。

```csharp
public string TraceId { get; }
```

#### Property Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### ToString() \{#Kun_Audit_AuditRecord_ToString}

```csharp
public override string ToString()
```

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)
