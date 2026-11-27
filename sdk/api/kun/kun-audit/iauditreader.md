> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IAuditReader

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit](/sdk/api/kun/kun-audit/index.md)\
Assembly: Kun.Audit.Abstractions.dll

组件内读取端口；面向用户的查询必须由宿主另行鉴权并审计。

```csharp
public interface IAuditReader
```

## Methods

### ReadAsync(long, int, string?, CancellationToken) \{#Kun_Audit_IAuditReader_ReadAsync_System_Int64_System_Int32_System_String_System_Threading_CancellationToken\_}

分页读取当前租户的审计记录。

```csharp
Task<IReadOnlyList<AuditChainEntry>> ReadAsync(long afterSequence, int take, string? onBehalfOf = null, CancellationToken cancellationToken = default)
```

#### Parameters

`afterSequence` [long](https://learn.microsoft.com/dotnet/api/system.int64)

`take` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`onBehalfOf` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)>>

### ReadPlatformAsync(IPluginContext, long, int, string?, CancellationToken) \{#Kun_Audit_IAuditReader_ReadPlatformAsync_Kun_Runtime_IPluginContext_System_Int64_System_Int32_System_String_System_Threading_CancellationToken\_}

读取平台链，不自动跨租户。

```csharp
Task<IReadOnlyList<AuditChainEntry>> ReadPlatformAsync(IPluginContext producer, long afterSequence, int take, string? onBehalfOf = null, CancellationToken cancellationToken = default)
```

#### Parameters

`producer` IPluginContext

`afterSequence` [long](https://learn.microsoft.com/dotnet/api/system.int64)

`take` [int](https://learn.microsoft.com/dotnet/api/system.int32)

`onBehalfOf` [string](https://learn.microsoft.com/dotnet/api/system.string)?

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[IReadOnlyList](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlylist-1)\<[AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)>>

### VerifyAsync(CancellationToken) \{#Kun_Audit_IAuditReader_VerifyAsync_System_Threading_CancellationToken\_}

验证当前租户的链。

```csharp
Task<AuditChainVerification> VerifyAsync(CancellationToken cancellationToken = default)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[AuditChainVerification](/sdk/api/kun/kun-audit/auditchainverification.md)>

### VerifyPlatformAsync(IPluginContext, CancellationToken) \{#Kun_Audit_IAuditReader_VerifyPlatformAsync_Kun_Runtime_IPluginContext_System_Threading_CancellationToken\_}

验证显式平台链。

```csharp
Task<AuditChainVerification> VerifyPlatformAsync(IPluginContext producer, CancellationToken cancellationToken = default)
```

#### Parameters

`producer` IPluginContext

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[AuditChainVerification](/sdk/api/kun/kun-audit/auditchainverification.md)>
