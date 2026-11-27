> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class NativeAuditStorage

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Audit.Postgres` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Audit.Postgres`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Audit.Postgres)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Audit.Postgres](/sdk/api/kun/kun-audit-postgres/index.md)\
Assembly: Kun.Audit.Postgres.dll

原生审计的离线迁移和固定安全声明。在线 Provider 只校验，不调用迁移。

```csharp
public static class NativeAuditStorage
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[NativeAuditStorage](/sdk/api/kun/kun-audit-postgres/nativeauditstorage.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Fields

### GuardPolicy \{#Kun_Audit_Postgres_NativeAuditStorage_GuardPolicy}

仅收窄 Native 角色的独立 restrictive 策略。

```csharp
public const string GuardPolicy = "qian_audit_native_guard"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

### Schema \{#Kun_Audit_Postgres_NativeAuditStorage_Schema}

与既有 EF Provider 共用的 v1 审计 schema。

```csharp
public const string Schema = "plugin_qian_audit"
```

#### Field Value

[string](https://learn.microsoft.com/dotnet/api/system.string)

## Methods

### CreateMigrationSql(NativeAuditRoles) \{#Kun_Audit_Postgres_NativeAuditStorage_CreateMigrationSql_Kun_Audit_Postgres_NativeAuditRoles\_}

生成原生专用授权和函数；必须在离线迁移事务中运行。既有 EF 角色的授权不被改变。

```csharp
public static string CreateMigrationSql(NativeAuditRoles roles)
```

#### Parameters

`roles` [NativeAuditRoles](/sdk/api/kun/kun-audit-postgres/nativeauditroles.md)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### ExpectedGuard(NativeAuditRoles) \{#Kun_Audit_Postgres_NativeAuditStorage_ExpectedGuard_Kun_Audit_Postgres_NativeAuditRoles\_}

由受信迁移模板冻结的 PostgreSQL 16 规范表达式，供 NativeDataTable 的两项期待值使用。

```csharp
public static string ExpectedGuard(NativeAuditRoles roles)
```

#### Parameters

`roles` [NativeAuditRoles](/sdk/api/kun/kun-audit-postgres/nativeauditroles.md)

#### Returns

[string](https://learn.microsoft.com/dotnet/api/system.string)

### MigrateAsync(DbConnection, NativeAuditRoles, CancellationToken) \{#Kun_Audit_Postgres_NativeAuditStorage_MigrateAsync_System_Data_Common_DbConnection_Kun_Audit_Postgres_NativeAuditRoles_System_Threading_CancellationToken\_}

在独立迁移连接上以事务和 schema 锁执行；连接不应来自在线 NativeData。

```csharp
public static Task MigrateAsync(DbConnection migrationConnection, NativeAuditRoles roles, CancellationToken cancellationToken = default)
```

#### Parameters

`migrationConnection` [DbConnection](https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection)

`roles` [NativeAuditRoles](/sdk/api/kun/kun-audit-postgres/nativeauditroles.md)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)
