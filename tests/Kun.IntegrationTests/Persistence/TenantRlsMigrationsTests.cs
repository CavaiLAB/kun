using Kun.Core.Domain;
using Kun.Core.MultiTenancy;
using Kun.Core.Security;
using Kun.EntityFrameworkCore;
using Kun.EntityFrameworkCore.Extensions;
using Kun.EntityFrameworkCore.Interceptors;
using Kun.EntityFrameworkCore.MultiTenancy;
using Kun.EntityFrameworkCore.PostgreSQL;
using Kun.Testing.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit;

namespace Kun.IntegrationTests.Persistence;

// Kun.EntityFrameworkCore.PostgreSQL 在真库上：只用内核的严格上下文与迁移生成器（不经 Kun.Data）。
// 生成的建表脚本给租户表打开并强制行级安全、建一条宽松一条限制策略；在应用角色下两个租户互相看不见、也写不进对方的行。
public sealed class TenantRlsMigrationsTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    private const string Schema = "rls_migrations";

    [Fact]
    public async Task GeneratedScript_IsolatesTenantsUnderTheApplicationRole()
    {
        var ct = TestContext.Current.CancellationToken;
        string tenantTable, lookupTable;
        var nobody = new FixedTenant(null);
        await using (var design = new RlsContext(Options(postgres.ConnectionString, nobody), nobody))
        {
            tenantTable = Table(design, typeof(TenantRow));
            lookupTable = Table(design, typeof(LookupRow));
            // 建表脚本只靠模型生成，不连库；以超级用户执行，再授权给应用角色。
            var script = design.Database.GenerateCreateScript();
            await using var admin = new NpgsqlConnection(postgres.ConnectionString);
            await admin.OpenAsync(ct);
            await using (var ddl = new NpgsqlCommand(script, admin)) await ddl.ExecuteNonQueryAsync(ct);
            await using (var grant = new NpgsqlCommand(
                $"GRANT USAGE ON SCHEMA {Schema} TO {PostgresFixture.AppRoleName}; GRANT SELECT, INSERT ON ALL TABLES IN SCHEMA {Schema} TO {PostgresFixture.AppRoleName};", admin))
                await grant.ExecuteNonQueryAsync(ct);

            Assert.Equal((true, true), await RowSecurityAsync(admin, tenantTable, ct));
            Assert.Equal((false, false), await RowSecurityAsync(admin, lookupTable, ct));
            Assert.Equal("PERMISSIVE,RESTRICTIVE", string.Join(",", await PoliciesAsync(admin, tenantTable, ct)));
            Assert.Empty(await PoliciesAsync(admin, lookupTable, ct));
        }

        var a = new FixedTenant(Tenants.A);
        await using (var tenantA = new RlsContext(Options(postgres.AppConnectionString, a), a))
        {
            tenantA.Add(new TenantRow { Name = "a1" });
            await tenantA.SaveChangesAsync(ct);
            Assert.Equal(1, await tenantA.Set<TenantRow>().IgnoreQueryFilters().CountAsync(ct));
        }

        var b = new FixedTenant(Tenants.B);
        await using (var tenantB = new RlsContext(Options(postgres.AppConnectionString, b), b))
        {
            // 绕过 EF 的查询过滤，只剩库里的策略：租户 b 看不见租户 a 的行。
            Assert.Equal(0, await tenantB.Set<TenantRow>().IgnoreQueryFilters().CountAsync(ct));
            // 显式写别的租户号：限制策略的 WITH CHECK 拒绝。
            tenantB.Add(new TenantRow { Name = "forged", TenantId = Tenants.AText });
            var error = await Assert.ThrowsAsync<DbUpdateException>(() => tenantB.SaveChangesAsync(ct));
            Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, Assert.IsType<PostgresException>(error.InnerException).SqlState);
        }
    }

    private static string Table(DbContext context, Type entity)
    {
        var type = context.Model.FindEntityType(entity)!;
        return type.GetSchema() + "." + type.GetTableName();
    }

    private static async Task<(bool Enabled, bool Forced)> RowSecurityAsync(NpgsqlConnection admin, string table, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand("SELECT relrowsecurity, relforcerowsecurity FROM pg_class WHERE oid = @table::regclass", admin);
        command.Parameters.AddWithValue("table", table);
        await using var reader = await command.ExecuteReaderAsync(ct);
        Assert.True(await reader.ReadAsync(ct));
        return (reader.GetBoolean(0), reader.GetBoolean(1));
    }

    private static async Task<string[]> PoliciesAsync(NpgsqlConnection admin, string table, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(
            "SELECT permissive FROM pg_policies WHERE schemaname || '.' || tablename = @table ORDER BY permissive", admin);
        command.Parameters.AddWithValue("table", table);
        var result = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) result.Add(reader.GetString(0));
        return [.. result];
    }

    private static DbContextOptions Options(string connectionString, ITenantContext tenant)
    {
        var user = AnonymousCurrentUser.Instance;
        var builder = new DbContextOptionsBuilder();
        builder.UseNpgsql(connectionString).UseKunSchema(Schema);
        builder.UseKunDataModel()
            .UseKunTenantRlsMigrations()
            .ReplaceService<IModelCacheKeyFactory, KunMultiTenantModelCacheKeyFactory>()
            .AddInterceptors(
                new TenantRlsConnectionInterceptor(tenant, NullLogger<TenantRlsConnectionInterceptor>.Instance),
                new TenantRlsTransactionInterceptor(tenant),
                new KunSoftDeleteInterceptor(user, TimeProvider.System),
                new AuditFieldsSaveChangesInterceptor(user, TimeProvider.System),
                new KunTenantSaveChangesInterceptor(tenant, NullLogger<KunTenantSaveChangesInterceptor>.Instance),
                new KunApplicationRoleInterceptor());
        return builder.Options;
    }

    private sealed class FixedTenant(Guid? tenantId) : ITenantContext
    {
        public Guid? TenantId => tenantId;

        public bool IsMultiTenant => true;
    }

    private sealed class TenantRow : IMultiTenant
    {
        public Guid Id { get; set; }
        public string? TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class LookupRow
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    private sealed class RlsContext(DbContextOptions options, ITenantContext tenant) : KunStrictDbContext(options, tenant)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TenantRow>().ToTable("tenant_rows");
            modelBuilder.Entity<LookupRow>().ToTable("lookup_rows");
        }
    }
}
