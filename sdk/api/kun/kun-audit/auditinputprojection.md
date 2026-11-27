> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class AuditInputProjection

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit](/sdk/api/kun/kun-audit/index.md)\
Assembly: Kun.Audit.Abstractions.dll

由可信能力契约构造的输入审计策略；元数据不能来自调用请求。脱敏后才计算输入哈希。

```csharp
public sealed class AuditInputProjection
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[AuditInputProjection](/sdk/api/kun/kun-audit/auditinputprojection.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### AuditInputProjection(JsonElement, bool, bool?) \{#Kun_Audit_AuditInputProjection__ctor_System_Text_Json_JsonElement_System_Boolean_System_Nullable_System_Boolean\_\_}

复制并校验可信 Schema。命令默认开启、查询默认关闭；audit 是可信契约的显式覆盖。

```csharp
public AuditInputProjection(JsonElement schema, bool isCommand, bool? audit = null)
```

#### Parameters

`schema` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

`isCommand` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

`audit` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)?

## Fields

### Redacted \{#Kun_Audit_AuditInputProjection_Redacted}

所有敏感值使用同一标记，不保存原值的哈希、长度或类型。

```csharp
public const string Redacted = "[REDACTED]"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Properties

### ShouldRecord \{#Kun_Audit_AuditInputProjection_ShouldRecord}

是否记录此能力调用；它不替代权限判定或业务输入验证。

```csharp
public bool ShouldRecord { get; }
```

#### Property Value

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)

## Methods

### InputHash(JsonElement) \{#Kun_Audit_AuditInputProjection_InputHash_System_Text_Json_JsonElement\_}

关闭审计时为 null，否则返回脱敏投影的规范化 SHA-256；不保留原始输入。

```csharp
public string? InputHash(JsonElement input)
```

#### Parameters

`input` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)?

### Redact(JsonElement) \{#Kun_Audit_AuditInputProjection_Redact_System_Text_Json_JsonElement\_}

生成脱敏投影；未知字段值保守遮盖，原输入不会被修改。只支持离线本地 JSON Pointer 引用。

```csharp
public JsonElement Redact(JsonElement input)
```

#### Parameters

`input` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

#### Returns

[JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)
