> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class AuditProtocol

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit](/sdk/api/kun/kun-audit/index.md)\
Assembly: Kun.Audit.Abstractions.dll

审计 v1 的确定性 JSON：键按 ordinal 排序，精确十进制数规整，数组顺序保留，重复键拒绝。

```csharp
public static class AuditProtocol
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[AuditProtocol](/sdk/api/kun/kun-audit/auditprotocol.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### Canonicalize(JsonElement) \{#Kun_Audit_AuditProtocol_Canonicalize_System_Text_Json_JsonElement\_}

确定性 JSON 编码；不是将有损浮点数转换作为规范化手段。

```csharp
public static string Canonicalize(JsonElement value)
```

#### Parameters

`value` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Content(AuditRecord, long) \{#Kun_Audit_AuditProtocol_Content_Kun_Audit_AuditRecord_System_Int64\_}

审计 v1 的规范化正文；显式写入全部字段，兼容历史字节且不依赖反射序列化。

```csharp
public static string Content(AuditRecord record, long sequence)
```

#### Parameters

`record` [AuditRecord](/sdk/api/kun/kun-audit/auditrecord.md)

`sequence` [long](https://learn.microsoft.com/dotnet/api/system.int64)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Hash(AuditRecord, long, string?) \{#Kun_Audit_AuditProtocol_Hash_Kun_Audit_AuditRecord_System_Int64_System_String\_}

计算 SHA256(prev_hash 的 UTF-8 文本 || canonical_json(entry))，包含审计记录的全部字段和序号。

```csharp
public static string Hash(AuditRecord record, long sequence, string? previousHash)
```

#### Parameters

`record` [AuditRecord](/sdk/api/kun/kun-audit/auditrecord.md)

`sequence` [long](https://learn.microsoft.com/dotnet/api/system.int64)

`previousHash` [string](https://learn.microsoft.com/dotnet/api/system.string)?

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ReadEntry(JsonElement) \{#Kun_Audit_AuditProtocol_ReadEntry_System_Text_Json_JsonElement\_}

读取既有 PostgreSQL v1 审计行的 JSON 投影；不以 DTO 反射或 Provider 类型定义另一套协议。

```csharp
public static AuditChainEntry ReadEntry(JsonElement row)
```

#### Parameters

`row` [JsonElement](https://learn.microsoft.com/dotnet/api/system.text.json.jsonelement)

#### Returns

[AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)
