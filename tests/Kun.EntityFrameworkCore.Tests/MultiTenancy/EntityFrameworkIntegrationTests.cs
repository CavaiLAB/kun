using FluentAssertions;
using Kun.Core.Domain;
using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore.MultiTenancy;
using Kun.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kun.EntityFrameworkCore.Tests.MultiTenancy;

public sealed class EntityFrameworkIntegrationTests
{
    [Fact]
    public async Task GlobalFilter_HidesOtherTenants()
    {
        var ctx = new AsyncLocalTenantContext();
        var ct = TestContext.Current.CancellationToken;

        // Seed with tenant-a active
        using (ctx.BeginScope(Tenants.A))
        {
            using var db = CreateDb(ctx);
            db.Widgets.Add(new Widget { Id = Guid.NewGuid(), Name = "a-one" });
            db.Widgets.Add(new Widget { Id = Guid.NewGuid(), Name = "a-two" });
            await db.SaveChangesAsync(ct);
        }

        // Add a tenant-b row on the same in-memory database name.
        using (ctx.BeginScope(Tenants.B))
        {
            using var db = CreateDb(ctx);
            db.Widgets.Add(new Widget { Id = Guid.NewGuid(), Name = "b-one" });
            await db.SaveChangesAsync(ct);
        }

        // Tenant A sees only A's rows
        using (ctx.BeginScope(Tenants.A))
        {
            using var db = CreateDb(ctx);
            var names = await db.Widgets.Select(w => w.Name).ToListAsync(ct);
            names.Should().BeEquivalentTo(["a-one", "a-two"]);
        }

        // Tenant B sees only B's rows
        using (ctx.BeginScope(Tenants.B))
        {
            using var db = CreateDb(ctx);
            var names = await db.Widgets.Select(w => w.Name).ToListAsync(ct);
            names.Should().BeEquivalentTo(["b-one"]);
        }
    }

    [Fact]
    public async Task Interceptor_AutoFillsTenantIdOnInsert()
    {
        var ctx = new AsyncLocalTenantContext();
        var ct = TestContext.Current.CancellationToken;

        using (ctx.BeginScope(Tenants.X))
        {
            using var db = CreateDb(ctx);
            // Consumer forgot to set TenantId — the interceptor fills it in.
            db.Widgets.Add(new Widget { Id = Guid.NewGuid(), Name = "auto-filled" });
            await db.SaveChangesAsync(ct);
        }

        // Verify by escaping the filter on a fresh db context.
        using (ctx.BeginScope(Tenants.X))
        {
            using var db = CreateDb(ctx);
            var widget = await db.Widgets.IgnoreQueryFilters()
                .SingleAsync(w => w.Name == "auto-filled", ct);
            widget.TenantId.Should().Be(Tenants.XText);
        }
    }

    [Fact]
    public async Task Interceptor_PreservesExplicitTenantId()
    {
        var ctx = new AsyncLocalTenantContext();
        var ct = TestContext.Current.CancellationToken;

        using (ctx.BeginScope(Tenants.Y))
        {
            using var db = CreateDb(ctx);
            db.Widgets.Add(new Widget
            {
                Id = Guid.NewGuid(),
                Name = "explicit",
                TenantId = Tenants.OverrideText,
            });
            await db.SaveChangesAsync(ct);
        }

        using (ctx.BeginScope(Tenants.Override))
        {
            using var db = CreateDb(ctx);
            var widget = await db.Widgets.SingleAsync(w => w.Name == "explicit", ct);
            widget.TenantId.Should().Be(Tenants.OverrideText);
        }
    }

    [Theory]
    [InlineData("tenant-a")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("0190F3A2-7C41-7B3E-9D2A-5C8E1F4A6B20")]
    public async Task Interceptor_RejectsNonCanonicalExplicitTenantId(string value)
    {
        var context = new AsyncLocalTenantContext();
        using var scope = context.BeginScope(Tenants.A);
        using var db = CreateDb(context);
        db.Widgets.Add(new Widget { Id = Guid.NewGuid(), Name = "explicit", TenantId = value });

        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    private readonly string _dbName = $"mt-tests-{Guid.NewGuid():N}";

    private TestDbContext CreateDb(ITenantContext tenantContext)
    {
        var interceptor = new KunTenantSaveChangesInterceptor(
            tenantContext,
            NullLogger<KunTenantSaveChangesInterceptor>.Instance);

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(_dbName)
            .AddInterceptors(interceptor)
            .ReplaceService<IModelCacheKeyFactory, KunMultiTenantModelCacheKeyFactory>()
            .Options;

        return new TestDbContext(options, tenantContext);
    }

    private sealed class Widget : IMultiTenant
    {
        public Guid Id { get; init; }
        public string Name { get; set; } = string.Empty;
        public string? TenantId { get; set; }
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options, ITenantContext tenantContext)
        : DbContext(options), IKunMultiTenantDbContext
    {
        private readonly ITenantContext _tenantContext = tenantContext;

        public DbSet<Widget> Widgets => Set<Widget>();

        public string? TenantIdForModelCacheKey => _tenantContext.TenantId?.ToString("D");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Widget>().HasKey(w => w.Id);
            modelBuilder.ApplyKunMultiTenantFilters(this);
        }
    }
}
