> For AI agents: the complete documentation index is available at /llms.txt, the full documentation bundle is available at /llms-full.txt.

# Class KunTenantRlsMigrationsExtensions

:::info 生成页
本页由 DocFX 从 nuget.org 上 `Kun.EntityFrameworkCore.PostgreSQL` 2026.11.24 的 XML 注释生成，源码在 kun 仓库 [`src/Kun.EntityFrameworkCore.PostgreSQL`](https://github.com/CavaiLAB/kun/tree/v2026.11.24/src/Kun.EntityFrameworkCore.PostgreSQL)（`v2026.11.24`）。Kun 文档与代码不一致处以代码为准（[ADR-001 D1.8](/design/adr/adr-001-kun-relationship.md)）；平台集成方式见[内核与平台组件](/sdk/kun/with-qian.md)。
:::

Namespace: [Kun.EntityFrameworkCore.PostgreSQL](/sdk/api/kun/kun-entityframeworkcore-postgresql/index.md)\
Assembly: Kun.EntityFrameworkCore.PostgreSQL.dll

装配与判定 Kun 的 PostgreSQL 迁移 SQL 生成器：建租户表时紧接着打开行级安全并建租户策略。

```csharp
public static class KunTenantRlsMigrationsExtensions
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ←
[KunTenantRlsMigrationsExtensions](/sdk/api/kun/kun-entityframeworkcore-postgresql/kuntenantrlsmigrationsextensions.md)

#### Inherited Members

[object.GetType()](https://learn.microsoft.com/dotnet/api/system.object.gettype),
[object.MemberwiseClone()](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone),
[object.ToString()](https://learn.microsoft.com/dotnet/api/system.object.tostring),
[object.Equals(object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object\)),
[object.Equals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.equals#system-object-equals\(system-object-system-object\)),
[object.ReferenceEquals(object?, object?)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals),
[object.GetHashCode()](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)

## Methods

### UseKunTenantRlsMigrations(DbContextOptionsBuilder) \{#Kun_EntityFrameworkCore_PostgreSQL_KunTenantRlsMigrationsExtensions_UseKunTenantRlsMigrations_Microsoft_EntityFrameworkCore_DbContextOptionsBuilder\_}

把 Npgsql 的迁移 SQL 生成器换成 Kun 的：目标模型里带租户标记的表（严格上下文给实现 `IMultiTenant` 的实体自动标记），
每条 `CREATE TABLE` 之后紧接着 `ENABLE` / `FORCE ROW LEVEL SECURITY`，再建一条宽松策略与一条限制策略；
没有标记却带非空 `tenant_id` 列的表拒绝生成。只用于 Npgsql 提供程序。

```csharp
public static DbContextOptionsBuilder UseKunTenantRlsMigrations(this DbContextOptionsBuilder options)
```

#### Parameters

`options` [DbContextOptionsBuilder](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder)

数据上下文的配置。

#### Returns

[DbContextOptionsBuilder](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontextoptionsbuilder)

### UsesKunTenantRlsMigrations(DbContext) \{#Kun_EntityFrameworkCore_PostgreSQL_KunTenantRlsMigrationsExtensions_UsesKunTenantRlsMigrations_Microsoft_EntityFrameworkCore_DbContext\_}

上下文实际生效的迁移 SQL 生成器是不是 Kun 的这一个；启动检查用它确认装配没被替换。

```csharp
public static bool UsesKunTenantRlsMigrations(this DbContext context)
```

#### Parameters

`context` [DbContext](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.dbcontext)

要检查的数据上下文。

#### Returns

[bool](https://learn.microsoft.com/dotnet/api/system.boolean)
