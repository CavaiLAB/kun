> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface IAuditWriter

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit](/sdk/api/kun/kun-audit/index.md)\
Assembly: Kun.Audit.Abstractions.dll

可信组件生成记录后的追加端口；独立追加不替代业务同事务写入。

```csharp
public interface IAuditWriter
```

## Methods

### AppendAsync(AuditRecord, CancellationToken) \{#Kun_Audit_IAuditWriter_AppendAsync_Kun_Audit_AuditRecord_System_Threading_CancellationToken\_}

追加当前租户的记录；缺失租户拒绝。

```csharp
Task<AuditChainEntry> AppendAsync(AuditRecord record, CancellationToken cancellationToken = default)
```

#### Parameters

`record` [AuditRecord](/sdk/api/kun/kun-audit/auditrecord.md)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)>

### AppendEnlistedAsync(AuditRecord, DbTransaction, CancellationToken) \{#Kun_Audit_IAuditWriter_AppendEnlistedAsync_Kun_Audit_AuditRecord_System_Data_Common_DbTransaction_System_Threading_CancellationToken\_}

将租户审计加入现有 PostgreSQL 可串行化业务事务；返回节点只有在外层提交后才持久化，冲突须由调用方重试整个业务事务。

```csharp
Task<AuditChainEntry> AppendEnlistedAsync(AuditRecord record, DbTransaction transaction, CancellationToken cancellationToken = default)
```

#### Parameters

`record` [AuditRecord](/sdk/api/kun/kun-audit/auditrecord.md)

`transaction` [DbTransaction](https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)>

### AppendPlatformAsync(IPluginContext, AuditRecord, CancellationToken) \{#Kun_Audit_IAuditWriter_AppendPlatformAsync_Kun_Runtime_IPluginContext_Kun_Audit_AuditRecord_System_Threading_CancellationToken\_}

显式追加平台记录；需提供仍有效的 global 生产方上下文，并显式配置专用平台角色；租户调用拒绝。

```csharp
Task<AuditChainEntry> AppendPlatformAsync(IPluginContext producer, AuditRecord record, CancellationToken cancellationToken = default)
```

#### Parameters

`producer` IPluginContext

`record` [AuditRecord](/sdk/api/kun/kun-audit/auditrecord.md)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)>

### AppendPlatformEnlistedAsync(IPluginContext, AuditRecord, DbTransaction, CancellationToken) \{#Kun_Audit_IAuditWriter_AppendPlatformEnlistedAsync_Kun_Runtime_IPluginContext_Kun_Audit_AuditRecord_System_Data_Common_DbTransaction_System_Threading_CancellationToken\_}

将平台记录加入现有 PostgreSQL 可串行化业务事务（S1-AUD-009）：生产方检查与 AppendPlatformAsync 相同（global 生产方、同一 Audit 实例、已配置平台链），存在租户上下文时拒绝；
与 AppendEnlistedAsync 同一约束——不提交、不释放、不局部重试，返回节点只有在外层提交后才持久化，冲突须由调用方换会话重试整个业务事务。

```csharp
Task<AuditChainEntry> AppendPlatformEnlistedAsync(IPluginContext producer, AuditRecord record, DbTransaction transaction, CancellationToken cancellationToken = default)
```

#### Parameters

`producer` IPluginContext

`record` [AuditRecord](/sdk/api/kun/kun-audit/auditrecord.md)

`transaction` [DbTransaction](https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task-1)\<[AuditChainEntry](/sdk/api/kun/kun-audit/auditchainentry.md)>
