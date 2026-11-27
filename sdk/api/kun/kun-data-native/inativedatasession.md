> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface INativeDataSession

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Data.Native.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Data.Native.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Data.Native.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Data.Native](/sdk/api/kun/kun-data-native/index.md)\
Assembly: Kun.Data.Native.Abstractions.dll

拥有连接与事务的会话。命令只能来自可信组件；释放回滚未完成事务、清理会话并释放连接，组件卸载同样释放活动会话。

```csharp
public interface INativeDataSession : IAsyncDisposable
```

#### Implements

[IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable)

## Properties

### Connection \{#Kun_Data_Native_INativeDataSession_Connection}

带生命周期、角色与租户守卫的已打开连接；可以先获取 session advisory lock，再开启事务。

```csharp
DbConnection Connection { get; }
```

#### Property Value

[DbConnection](https://learn.microsoft.com/dotnet/api/system.data.common.dbconnection)

### TenantId \{#Kun_Data_Native_INativeDataSession_TenantId}

会话捕获的租户 UUID；显式平台会话为 null，不表示自动取得平台权限。

```csharp
Guid? TenantId { get; }
```

#### Property Value

[Guid](https://learn.microsoft.com/dotnet/api/system.guid)?

### Transaction \{#Kun_Data_Native_INativeDataSession_Transaction}

当前未完成事务；没有事务时为 null。

```csharp
DbTransaction? Transaction { get; }
```

#### Property Value

[DbTransaction](https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction)?

## Methods

### BeginTransactionAsync(IsolationLevel, CancellationToken) \{#Kun_Data_Native_INativeDataSession_BeginTransactionAsync_System_Data_IsolationLevel_System_Threading_CancellationToken\_}

开始事务时再次校验调用上下文，并设置事务级租户 GUC；嵌套事务拒绝。

```csharp
ValueTask<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
```

#### Parameters

`isolationLevel` [IsolationLevel](https://learn.microsoft.com/dotnet/api/system.data.isolationlevel)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[DbTransaction](https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction)>

### CommitAsync(CancellationToken) \{#Kun_Data_Native_INativeDataSession_CommitAsync_System_Threading_CancellationToken\_}

提交当前事务；取消或提交失败时会话不得继续执行。

```csharp
ValueTask CommitAsync(CancellationToken cancellationToken = default)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### RollbackAsync(CancellationToken) \{#Kun_Data_Native_INativeDataSession_RollbackAsync_System_Threading_CancellationToken\_}

回滚当前事务；没有事务时为空操作。

```csharp
ValueTask RollbackAsync(CancellationToken cancellationToken = default)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)

### ValidateAsync(CancellationToken) \{#Kun_Data_Native_INativeDataSession_ValidateAsync_System_Threading_CancellationToken\_}

验证会话仍有效，当前上下文、数据库登录与 GUC 仍符合绑定。消费者不需要也不得自行改租户 GUC。

```csharp
ValueTask ValidateAsync(CancellationToken cancellationToken = default)
```

#### Parameters

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
