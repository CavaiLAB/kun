using Kun.Core.Domain;
using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kun.EntityFrameworkCore.Tests.PostgreSQL;

// Kun.EntityFrameworkCore.PostgreSQL：装配入口、判定，以及生成的建表 SQL（不连库）。策略在真库上生效见 Kun.IntegrationTests。
public sealed class TenantRlsMigrationsTests
{
    private const string Schema = "rls_script";

    [Fact]
    public void UseKunTenantRlsMigrations_ReplacesTheMigrationsSqlGenerator()
    {
        using var plain = new PlainContext(PlainOptions(rls: false));
        using var replaced = new PlainContext(PlainOptions(rls: true));
        Assert.False(plain.UsesKunTenantRlsMigrations());
        Assert.True(replaced.UsesKunTenantRlsMigrations());
    }

    [Fact]
    public void CreateScript_EnablesRowLevelSecurityRightAfterEachTenantTable()
    {
        using var context = new StrictContext(StrictContextOptions.Builder(Schema).UseKunTenantRlsMigrations().Options);
        var tenantTable = Table(context, typeof(TenantRow));
        var lookupTable = Table(context, typeof(LookupRow));
        var statements = context.Database.GenerateCreateScript()
            .Split(';').Select(statement => statement.Trim()).Where(statement => statement.Length > 0).ToArray();

        var create = Array.FindIndex(statements, statement => statement.StartsWith($"CREATE TABLE {tenantTable} (", StringComparison.Ordinal));
        Assert.True(create >= 0);
        Assert.Equal($"ALTER TABLE {tenantTable} ENABLE ROW LEVEL SECURITY", statements[create + 1]);
        Assert.Equal($"ALTER TABLE {tenantTable} FORCE ROW LEVEL SECURITY", statements[create + 2]);
        Assert.StartsWith("CREATE POLICY ", statements[create + 3], StringComparison.Ordinal);
        Assert.EndsWith($" ON {tenantTable} FOR ALL USING (true) WITH CHECK (true)", statements[create + 3], StringComparison.Ordinal);
        Assert.StartsWith("CREATE POLICY ", statements[create + 4], StringComparison.Ordinal);
        Assert.Contains($" ON {tenantTable} AS RESTRICTIVE FOR ALL USING (tenant_id = NULLIF(current_setting(", statements[create + 4], StringComparison.Ordinal);
        Assert.Equal(2, statements.Count(statement => statement.Contains($" ON {tenantTable} ", StringComparison.Ordinal)
            && statement.StartsWith("CREATE POLICY ", StringComparison.Ordinal)));

        // 不带租户标记的表照 Npgsql 原样生成，不打开行级安全。
        Assert.Contains(statements, statement => statement.StartsWith($"CREATE TABLE {lookupTable} (", StringComparison.Ordinal));
        Assert.DoesNotContain(statements, statement => statement.Contains($"TABLE {lookupTable} ENABLE", StringComparison.Ordinal)
            || statement.Contains($" ON {lookupTable} ", StringComparison.Ordinal));
    }

    [Fact]
    public void CreateScript_RejectsNonNullTenantColumnWithoutTenantMarker()
    {
        // 普通上下文不给实体加租户标记：表里有非空 tenant_id 列却没有标记，生成器拒绝生成裸表。
        using var context = new PlainContext(PlainOptions(rls: true));
        Assert.Throws<KunDataMisconfiguredException>(() => context.Database.GenerateCreateScript());
    }

    private static string Table(DbContext context, Type entity)
    {
        var type = context.Model.FindEntityType(entity)!;
        return type.GetSchema() + "." + type.GetTableName();
    }

    private static DbContextOptions PlainOptions(bool rls)
    {
        var builder = new DbContextOptionsBuilder().UseNpgsql("Host=localhost;Database=unused");
        if (rls) builder.UseKunTenantRlsMigrations();
        return builder.Options;
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

    private sealed class UnmarkedRow
    {
        public Guid Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
    }

    private sealed class StrictContext(DbContextOptions options) : KunStrictDbContext(options, NullTenantContext.Instance)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TenantRow>().ToTable("tenant_rows");
            modelBuilder.Entity<LookupRow>().ToTable("lookup_rows");
        }
    }

    private sealed class PlainContext(DbContextOptions options) : KunDbContext(options, NullTenantContext.Instance)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UnmarkedRow>(entity =>
            {
                entity.ToTable("unmarked_rows");
                entity.Property(row => row.TenantId).HasColumnName("tenant_id").IsRequired();
            });
        }
    }
}
