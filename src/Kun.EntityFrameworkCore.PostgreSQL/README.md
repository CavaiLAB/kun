# Kun.EntityFrameworkCore.PostgreSQL

PostgreSQL 的 EF 迁移 SQL 生成器：建租户表时，紧跟每条 `CREATE TABLE` 打开并强制行级安全，再建两条策略——一条宽松、一条限制（`AS RESTRICTIVE`）。限制策略按会话里的当前租户号核对 `USING` 与 `WITH CHECK`，缺失和空租户都不匹配；后加的宽松策略不能通过 OR 放宽这条边界。

单独成包，是为了不把 Npgsql 带进提供程序中立的 `Kun.EntityFrameworkCore`。内核档提供者，不是 AOT 兼容的（生成器继承 Npgsql 的 EF 迁移 SQL 生成器）。

## 用法

```csharp
options.UseNpgsql(connectionString)
    .UseKunTenantRlsMigrations();
```

- `UseKunTenantRlsMigrations()` 把 Npgsql 的迁移 SQL 生成器换成这一个。带租户标记的表（严格上下文 `KunStrictDbContext` 给实现 `IMultiTenant` 的实体自动标记）才加行级安全；没有标记却带非空 `tenant_id` 列的表拒绝生成，不建裸表。
- `context.UsesKunTenantRlsMigrations()` 判断上下文实际生效的生成器是不是这一个，启动检查用它确认装配没被替换。
- 平台的 `AddKunData<T>` 已经装上它，平台组件不用自己调。

行级安全语句只在迁移**执行期**由生成器写出，不在迁移文件里。迁移要经装了它的上下文执行；`dotnet ef` 的设计时入口没有装它时用的是 Npgsql 默认生成器，建出的租户表没有行级安全。
