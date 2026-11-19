using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Kun.EntityFrameworkCore.PostgreSQL;

/// <summary>装配与判定 Kun 的 PostgreSQL 迁移 SQL 生成器：建租户表时紧接着打开行级安全并建租户策略。</summary>
public static class KunTenantRlsMigrationsExtensions
{
    /// <summary>
    /// 把 Npgsql 的迁移 SQL 生成器换成 Kun 的：目标模型里带租户标记的表（严格上下文给实现 <c>IMultiTenant</c> 的实体自动标记），
    /// 每条 <c>CREATE TABLE</c> 之后紧接着 <c>ENABLE</c> / <c>FORCE ROW LEVEL SECURITY</c>，再建一条宽松策略与一条限制策略；
    /// 没有标记却带非空 <c>tenant_id</c> 列的表拒绝生成。只用于 Npgsql 提供程序。
    /// </summary>
    /// <param name="options">数据上下文的配置。</param>
    public static DbContextOptionsBuilder UseKunTenantRlsMigrations(this DbContextOptionsBuilder options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.ReplaceService<IMigrationsSqlGenerator, KunTenantRlsMigrationsSqlGenerator>();
    }

    /// <summary>上下文实际生效的迁移 SQL 生成器是不是 Kun 的这一个；启动检查用它确认装配没被替换。</summary>
    /// <param name="context">要检查的数据上下文。</param>
    public static bool UsesKunTenantRlsMigrations(this DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.GetService<IMigrationsSqlGenerator>() is KunTenantRlsMigrationsSqlGenerator;
    }
}
