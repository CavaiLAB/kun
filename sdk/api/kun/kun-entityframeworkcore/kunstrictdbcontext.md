> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunStrictDbContext

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore](/sdk/api/kun/kun-entityframeworkcore/index.md)\
Assembly: Kun.EntityFrameworkCore.dll

带安全装配门禁的数据上下文。租户过滤与 Outbox 继承 Kun 的行为。

```csharp
public abstract class KunStrictDbContext : KunDbContext, IInfrastructure<IServiceProvider>, IDbContextDependencies, IDbSetCache, IDbContextPoolable, IResettableService, IDisposable, IAsyncDisposable, IKunMultiTenantDbContext
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[DbContext](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext) ←
[KunDbContext](/sdk/api/kun/kun-entityframeworkcore/kundbcontext.md) ←
[KunStrictDbContext](/sdk/api/kun/kun-entityframeworkcore/kunstrictdbcontext.md)

#### Implements

[IInfrastructure\<IServiceProvider>](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.infrastructure.iinfrastructure-1),
[IDbContextDependencies](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.internal.idbcontextdependencies),
[IDbSetCache](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.internal.idbsetcache),
[IDbContextPoolable](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.internal.idbcontextpoolable),
[IResettableService](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.infrastructure.iresettableservice),
[IDisposable](https://learn.microsoft.com/dotnet/api/system.idisposable),
[IAsyncDisposable](https://learn.microsoft.com/dotnet/api/system.iasyncdisposable),
[IKunMultiTenantDbContext](/sdk/api/kun/kun-entityframeworkcore-multitenancy/ikunmultitenantdbcontext.md)

#### Inherited Members

[KunDbContext.OnConfiguring(DbContextOptionsBuilder)](/sdk/api/kun/kun-entityframeworkcore/kundbcontext.md#Kun_EntityFrameworkCore_KunDbContext_OnConfiguring_Microsoft_EntityFrameworkCore_DbContextOptionsBuilder_),
[KunDbContext.ConfigureConventions(ModelConfigurationBuilder)](/sdk/api/kun/kun-entityframeworkcore/kundbcontext.md#Kun_EntityFrameworkCore_KunDbContext_ConfigureConventions_Microsoft_EntityFrameworkCore_ModelConfigurationBuilder_),
[KunDbContext.OnModelCreating(ModelBuilder)](/sdk/api/kun/kun-entityframeworkcore/kundbcontext.md#Kun_EntityFrameworkCore_KunDbContext_OnModelCreating_Microsoft_EntityFrameworkCore_ModelBuilder_),
[KunDbContext.OutboxMessages](/sdk/api/kun/kun-entityframeworkcore/kundbcontext.md#Kun_EntityFrameworkCore_KunDbContext_OutboxMessages),
[KunDbContext.DefaultSchema](/sdk/api/kun/kun-entityframeworkcore/kundbcontext.md#Kun_EntityFrameworkCore_KunDbContext_DefaultSchema),
[KunDbContext.TenantIdForModelCacheKey](/sdk/api/kun/kun-entityframeworkcore/kundbcontext.md#Kun_EntityFrameworkCore_KunDbContext_TenantIdForModelCacheKey),
[DbContext.Set\<TEntity>()](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.set#microsoft-entityframeworkcore-dbcontext-set-1),
[DbContext.Set\<TEntity>(string)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.set#microsoft-entityframeworkcore-dbcontext-set-1\(system-string\)),
[DbContext.OnConfiguring(DbContextOptionsBuilder)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.onconfiguring),
[DbContext.ConfigureConventions(ModelConfigurationBuilder)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.configureconventions),
[DbContext.OnModelCreating(ModelBuilder)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.onmodelcreating),
[DbContext.SaveChanges()](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.savechanges#microsoft-entityframeworkcore-dbcontext-savechanges),
[DbContext.SaveChanges(bool)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.savechanges#microsoft-entityframeworkcore-dbcontext-savechanges\(system-boolean\)),
[DbContext.SaveChangesAsync(CancellationToken)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.savechangesasync#microsoft-entityframeworkcore-dbcontext-savechangesasync\(system-threading-cancellationtoken\)),
[DbContext.SaveChangesAsync(bool, CancellationToken)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.savechangesasync#microsoft-entityframeworkcore-dbcontext-savechangesasync\(system-boolean-system-threading-cancellationtoken\)),
[DbContext.Dispose()](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.dispose),
[DbContext.DisposeAsync()](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.disposeasync),
[DbContext.Entry\<TEntity>(TEntity)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.entry#microsoft-entityframeworkcore-dbcontext-entry-1\(-0\)),
[DbContext.Entry(object)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.entry#microsoft-entityframeworkcore-dbcontext-entry\(system-object\)),
[DbContext.Add\<TEntity>(TEntity)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.add#microsoft-entityframeworkcore-dbcontext-add-1\(-0\)),
[DbContext.AddAsync\<TEntity>(TEntity, CancellationToken)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.addasync#microsoft-entityframeworkcore-dbcontext-addasync-1\(-0-system-threading-cancellationtoken\)),
[DbContext.Attach\<TEntity>(TEntity)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.attach#microsoft-entityframeworkcore-dbcontext-attach-1\(-0\)),
[DbContext.Update\<TEntity>(TEntity)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.update#microsoft-entityframeworkcore-dbcontext-update-1\(-0\)),
[DbContext.Remove\<TEntity>(TEntity)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.remove#microsoft-entityframeworkcore-dbcontext-remove-1\(-0\)),
[DbContext.Add(object)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.add#microsoft-entityframeworkcore-dbcontext-add\(system-object\)),
[DbContext.AddAsync(object, CancellationToken)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.addasync#microsoft-entityframeworkcore-dbcontext-addasync\(system-object-system-threading-cancellationtoken\)),
[DbContext.Attach(object)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.attach#microsoft-entityframeworkcore-dbcontext-attach\(system-object\)),
[DbContext.Update(object)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.update#microsoft-entityframeworkcore-dbcontext-update\(system-object\)),
[DbContext.Remove(object)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.remove#microsoft-entityframeworkcore-dbcontext-remove\(system-object\)),
[DbContext.AddRange(params object\[\])](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.addrange#microsoft-entityframeworkcore-dbcontext-addrange\(system-object\(\)\)),
[DbContext.AddRangeAsync(params object\[\])](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.addrangeasync#microsoft-entityframeworkcore-dbcontext-addrangeasync\(system-object\(\)\)),
[DbContext.AttachRange(params object\[\])](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.attachrange#microsoft-entityframeworkcore-dbcontext-attachrange\(system-object\(\)\)),
[DbContext.UpdateRange(params object\[\])](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.updaterange#microsoft-entityframeworkcore-dbcontext-updaterange\(system-object\(\)\)),
[DbContext.RemoveRange(params object\[\])](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.removerange#microsoft-entityframeworkcore-dbcontext-removerange\(system-object\(\)\)),
[DbContext.AddRange(IEnumerable\<object>)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.addrange#microsoft-entityframeworkcore-dbcontext-addrange\(system-collections-generic-ienumerable\(\(system-object\)\)\)),
[DbContext.AddRangeAsync(IEnumerable\<object>, CancellationToken)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.addrangeasync#microsoft-entityframeworkcore-dbcontext-addrangeasync\(system-collections-generic-ienumerable\(\(system-object\)\)-system-threading-cancellationtoken\)),
[DbContext.AttachRange(IEnumerable\<object>)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.attachrange#microsoft-entityframeworkcore-dbcontext-attachrange\(system-collections-generic-ienumerable\(\(system-object\)\)\)),
[DbContext.UpdateRange(IEnumerable\<object>)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.updaterange#microsoft-entityframeworkcore-dbcontext-updaterange\(system-collections-generic-ienumerable\(\(system-object\)\)\)),
[DbContext.RemoveRange(IEnumerable\<object>)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.removerange#microsoft-entityframeworkcore-dbcontext-removerange\(system-collections-generic-ienumerable\(\(system-object\)\)\)),
[DbContext.Find(Type, params object?\[\]?)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.find#microsoft-entityframeworkcore-dbcontext-find\(system-type-system-object\(\)\)),
[DbContext.FindAsync(Type, params object?\[\]?)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.findasync#microsoft-entityframeworkcore-dbcontext-findasync\(system-type-system-object\(\)\)),
[DbContext.FindAsync(Type, object?\[\]?, CancellationToken)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.findasync#microsoft-entityframeworkcore-dbcontext-findasync\(system-type-system-object\(\)-system-threading-cancellationtoken\)),
[DbContext.Find\<TEntity>(params object?\[\]?)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.find#microsoft-entityframeworkcore-dbcontext-find-1\(system-object\(\)\)),
[DbContext.FindAsync\<TEntity>(params object?\[\]?)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.findasync#microsoft-entityframeworkcore-dbcontext-findasync-1\(system-object\(\)\)),
[DbContext.FindAsync\<TEntity>(object?\[\]?, CancellationToken)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.findasync#microsoft-entityframeworkcore-dbcontext-findasync-1\(system-object\(\)-system-threading-cancellationtoken\)),
[DbContext.FromExpression\<TResult>(Expression\<Func\<IQueryable\<TResult>>>)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.fromexpression),
[DbContext.Database](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.database),
[DbContext.ChangeTracker](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.changetracker),
[DbContext.Model](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.model),
[DbContext.ContextId](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.contextid),
[DbContext.SavingChanges](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.savingchanges),
[DbContext.SavedChanges](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.savedchanges),
[DbContext.SaveChangesFailed](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext.savechangesfailed),
[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

#### Extension Methods

[KunSaveChangesResultExtensions.SaveChangesResultAsync(DbContext, CancellationToken)](/sdk/api/kun/kun-entityframeworkcore-extensions/kunsavechangesresultextensions.md#Kun_EntityFrameworkCore_Extensions_KunSaveChangesResultExtensions_SaveChangesResultAsync_Microsoft_EntityFrameworkCore_DbContext_System_Threading_CancellationToken_)

## Constructors

### KunStrictDbContext(DbContextOptions, ITenantContext) \{#Kun_EntityFrameworkCore_KunStrictDbContext__ctor_Microsoft_EntityFrameworkCore_DbContextOptions_Kun_Core_MultiTenancy_ITenantContext\_}

检查双拦截器与租户模型缓存键，缺少任一项即拒绝构造。

```csharp
protected KunStrictDbContext(DbContextOptions options, ITenantContext tenantContext)
```

#### Parameters

`options` [DbContextOptions](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontextoptions)

`tenantContext` ITenantContext

## Methods

### OnConfiguring(DbContextOptionsBuilder) \{#Kun_EntityFrameworkCore_KunStrictDbContext_OnConfiguring_Microsoft_EntityFrameworkCore_DbContextOptionsBuilder\_}

配置只通过 AddKunData 的 options 回调传入，防止构造后移除安全装配。

```csharp
protected override sealed void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
```

#### Parameters

`optionsBuilder` [DbContextOptionsBuilder](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder)
