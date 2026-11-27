> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunApplicationRoleInterceptor

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore](/sdk/api/kun/kun-entityframeworkcore/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

在线应用连接只能使用受 RLS 约束的 PostgreSQL 角色；迁移管理员通过独立连接执行 DDL。

```csharp
public sealed class KunApplicationRoleInterceptor : DbConnectionInterceptor, IDbConnectionInterceptor, IInterceptor
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[DbConnectionInterceptor](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor) ←
[KunApplicationRoleInterceptor](/sdk/api/kun/kun-entityframeworkcore/kunapplicationroleinterceptor.md)

#### Implements

[IDbConnectionInterceptor](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.idbconnectioninterceptor),
[IInterceptor](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.iinterceptor)

#### Inherited Members

[DbConnectionInterceptor.ConnectionCreating(ConnectionCreatingEventData, InterceptionResult\<DbConnection>)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectioncreating),
[DbConnectionInterceptor.ConnectionCreated(ConnectionCreatedEventData, DbConnection)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectioncreated),
[DbConnectionInterceptor.ConnectionOpening(DbConnection, ConnectionEventData, InterceptionResult)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectionopening),
[DbConnectionInterceptor.ConnectionOpeningAsync(DbConnection, ConnectionEventData, InterceptionResult, CancellationToken)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectionopeningasync),
[DbConnectionInterceptor.ConnectionOpened(DbConnection, ConnectionEndEventData)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectionopened),
[DbConnectionInterceptor.ConnectionOpenedAsync(DbConnection, ConnectionEndEventData, CancellationToken)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectionopenedasync),
[DbConnectionInterceptor.ConnectionClosing(DbConnection, ConnectionEventData, InterceptionResult)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectionclosing),
[DbConnectionInterceptor.ConnectionClosingAsync(DbConnection, ConnectionEventData, InterceptionResult)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectionclosingasync),
[DbConnectionInterceptor.ConnectionClosed(DbConnection, ConnectionEndEventData)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectionclosed),
[DbConnectionInterceptor.ConnectionClosedAsync(DbConnection, ConnectionEndEventData)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectionclosedasync),
[DbConnectionInterceptor.ConnectionDisposing(DbConnection, ConnectionEventData, InterceptionResult)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectiondisposing),
[DbConnectionInterceptor.ConnectionDisposingAsync(DbConnection, ConnectionEventData, InterceptionResult)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectiondisposingasync),
[DbConnectionInterceptor.ConnectionDisposed(DbConnection, ConnectionEndEventData)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectiondisposed),
[DbConnectionInterceptor.ConnectionDisposedAsync(DbConnection, ConnectionEndEventData)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectiondisposedasync),
[DbConnectionInterceptor.ConnectionFailed(DbConnection, ConnectionErrorEventData)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectionfailed),
[DbConnectionInterceptor.ConnectionFailedAsync(DbConnection, ConnectionErrorEventData, CancellationToken)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectionfailedasync),
[DbConnectionInterceptor.ConnectionCanceled(DbConnection, ConnectionEndEventData)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectioncanceled),
[DbConnectionInterceptor.ConnectionCanceledAsync(DbConnection, ConnectionEndEventData, CancellationToken)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbconnectioninterceptor.connectioncanceledasync),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Constructors

### KunApplicationRoleInterceptor() \{#Kun_EntityFrameworkCore_KunApplicationRoleInterceptor__ctor}

```csharp
public KunApplicationRoleInterceptor()
```

## Methods

### ConnectionOpened(DbConnection, ConnectionEndEventData) \{#Kun_EntityFrameworkCore_KunApplicationRoleInterceptor_ConnectionOpened_System_Data_Common_DbConnection_Microsoft_EntityFrameworkCore_Diagnostics_ConnectionEndEventData\_}

Called just after EF has called [`Open`](https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.open).

```csharp
public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
```

#### Parameters

`connection` [DbConnection](https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection)

The connection.

`eventData` [ConnectionEndEventData](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.connectionendeventdata)

Contextual information about the connection.

### ConnectionOpenedAsync(DbConnection, ConnectionEndEventData, CancellationToken) \{#Kun_EntityFrameworkCore_KunApplicationRoleInterceptor_ConnectionOpenedAsync_System_Data_Common_DbConnection_Microsoft_EntityFrameworkCore_Diagnostics_ConnectionEndEventData_System_Threading_CancellationToken\_}

Called just after EF has called [`OpenAsync`](https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection.openasync).

```csharp
public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
```

#### Parameters

`connection` [DbConnection](https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection)

The connection.

`eventData` [ConnectionEndEventData](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.diagnostics.connectionendeventdata)

Contextual information about the connection.

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

A [`CancellationToken`](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) to observe while waiting for the task to complete.

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

A [`Task`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task) representing the asynchronous operation.

#### Exceptions

[OperationCanceledException](https://learn.microsoft.com/dotnet/api/system.operationcanceledexception)

If the [`CancellationToken`](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken) is canceled.

### Validate(DbConnection) \{#Kun_EntityFrameworkCore_KunApplicationRoleInterceptor_Validate_System_Data_Common_DbConnection\_}

检查已经打开的连接，包括宿主提供的预打开连接；同时校验登录角色和当前角色。

```csharp
public static void Validate(DbConnection connection)
```

#### Parameters

`connection` [DbConnection](https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection)

### ValidateAsync(DbConnection, CancellationToken) \{#Kun_EntityFrameworkCore_KunApplicationRoleInterceptor_ValidateAsync_System_Data_Common_DbConnection_System_Threading_CancellationToken\_}

异步检查已经打开的连接；不创建表、不迁移、不修改角色或租户会话。

```csharp
public static Task ValidateAsync(DbConnection connection, CancellationToken cancellationToken = default)
```

#### Parameters

`connection` [DbConnection](https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)
