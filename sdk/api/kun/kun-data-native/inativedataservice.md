> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Interface INativeDataService

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.Data.Native.Abstractions` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.Data.Native.Abstractions`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.Data.Native.Abstractions)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.Data.Native](/sdk/api/kun/kun-data-native/index.md)\
Assembly: Kun.Data.Native.Abstractions.dll

仅访问受信任宿主登记的连接绑定；不接受连接串或任意角色名。

```csharp
public interface INativeDataService
```

## Methods

### OpenPlatformAsync(string, IPluginContext, CancellationToken) \{#Kun_Data_Native_INativeDataService_OpenPlatformAsync_System_String_Kun_Runtime_IPluginContext_System_Threading_CancellationToken\_}

显式打开专用平台登录连接；调用方必须仍有效、处于 global，且当前没有租户。不能以租户调用或角色成员关系冒充。

```csharp
ValueTask<INativeDataSession> OpenPlatformAsync(string binding, IPluginContext caller, CancellationToken cancellationToken = default)
```

#### Parameters

`binding` [string](https://learn.microsoft.com/dotnet/api/system.string)

`caller` IPluginContext

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[INativeDataSession](/sdk/api/kun/kun-data-native/inativedatasession.md)>

### OpenTenantAsync(string, CancellationToken) \{#Kun_Data_Native_INativeDataService_OpenTenantAsync_System_String_System_Threading_CancellationToken\_}

按当前已验证租户打开连接；缺租户、错误绑定或服务已卸载时拒绝。

```csharp
ValueTask<INativeDataSession> OpenTenantAsync(string binding, CancellationToken cancellationToken = default)
```

#### Parameters

`binding` [string](https://learn.microsoft.com/dotnet/api/system.string)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask-1)\<[INativeDataSession](/sdk/api/kun/kun-data-native/inativedatasession.md)>

### ValidateTenantTransactionAsync(string, DbTransaction, CancellationToken) \{#Kun_Data_Native_INativeDataService_ValidateTenantTransactionAsync_System_String_System_Data_Common_DbTransaction_System_Threading_CancellationToken\_}

校验受管或外部真实租户事务的同库绑定、登录角色、GUC、当前租户与 SERIALIZABLE 隔离级别。既不提交、回滚，也不重试外部事务。

```csharp
ValueTask ValidateTenantTransactionAsync(string binding, DbTransaction transaction, CancellationToken cancellationToken = default)
```

#### Parameters

`binding` [string](https://learn.microsoft.com/dotnet/api/system.string)

`transaction` [DbTransaction](https://learn.microsoft.com/dotnet/api/system.data.common.dbtransaction)

`cancellationToken` [CancellationToken](https://learn.microsoft.com/dotnet/api/system.threading.cancellationtoken)

#### Returns

[ValueTask](https://learn.microsoft.com/dotnet/api/system.threading.tasks.valuetask)
