using BenchmarkDotNet.Attributes;
using Kun.Core.Domain;
using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace Kun.Benchmarks;

/// <summary>
/// Measures the overhead the Kun multi-tenant query filter adds to a
/// simple <c>ToListAsync</c> read path. Two sibling <see cref="DbContext"/>s
/// are seeded with <see cref="RowCount"/> rows across two tenants; the
/// filtered side applies <see cref="KunMultiTenancyModelBuilderExtensions.ApplyKunMultiTenantFilters"/>
/// under a fixed <c>ITenantContext.TenantId</c>. The filter is an
/// <c>Expression</c> tree translated by EF Core into a SQL predicate
/// (or, here, an in-memory expression evaluator); we're quantifying the
/// extra Expression evaluation + allocation per query, not a SQL plan
/// difference.
/// </summary>
[MemoryDiagnoser]
public class MultiTenancyQueryFilterBenchmarks : IDisposable
{
    private const string TenantA = "01900000-0000-7000-8000-000000000001";
    private const string TenantB = "01900000-0000-7000-8000-000000000002";
    private TenantlessDbContext _unfiltered = null!;
    private TenantFilteredDbContext _filtered = null!;

    /// <summary>Rows per tenant (so total rows = 2 × RowCount).</summary>
    [Params(100, 1000)]
    public int RowCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _unfiltered = new TenantlessDbContext(
            new DbContextOptionsBuilder<TenantlessDbContext>()
                .UseInMemoryDatabase($"kun-bench-tenant-none-{Guid.NewGuid():N}")
                .Options);

        _filtered = new TenantFilteredDbContext(
            new DbContextOptionsBuilder<TenantFilteredDbContext>()
                .UseInMemoryDatabase($"kun-bench-tenant-filtered-{Guid.NewGuid():N}")
                .Options,
            tenantContext: new FixedTenantContext(new Guid(TenantA)));

        for (int i = 0; i < RowCount; i++)
        {
            _unfiltered.Items.Add(new TenantItem { Id = Guid.NewGuid(), Name = $"u{i}", TenantId = TenantA });
            _unfiltered.Items.Add(new TenantItem { Id = Guid.NewGuid(), Name = $"u{i}", TenantId = TenantB });
            _filtered.Items.Add(new TenantItem { Id = Guid.NewGuid(), Name = $"f{i}", TenantId = TenantA });
            _filtered.Items.Add(new TenantItem { Id = Guid.NewGuid(), Name = $"f{i}", TenantId = TenantB });
        }

        _unfiltered.SaveChanges();
        _filtered.SaveChanges();
    }

    [GlobalCleanup]
    public void Cleanup() => Dispose();

    /// <summary>Disposes both DbContexts.</summary>
    public void Dispose()
    {
        _unfiltered?.Dispose();
        _filtered?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Benchmark(Baseline = true, Description = "Query · no tenant filter (manual Where)")]
    public async Task<int> NoFilter_ManualWhere()
    {
        var rows = await _unfiltered.Items.Where(x => x.TenantId == TenantA).AsNoTracking().ToListAsync().ConfigureAwait(false);
        return rows.Count;
    }

    [Benchmark(Description = "Query · Kun tenant filter (HasQueryFilter)")]
    public async Task<int> WithFilter()
    {
        var rows = await _filtered.Items.AsNoTracking().ToListAsync().ConfigureAwait(false);
        return rows.Count;
    }
}

internal sealed class FixedTenantContext : ITenantContext
{
    public FixedTenantContext(Guid tenantId) { TenantId = tenantId; }

    public Guid? TenantId { get; }

    public bool IsMultiTenant => TenantId.HasValue;
}

internal sealed class TenantlessDbContext : DbContext
{
    public TenantlessDbContext(DbContextOptions<TenantlessDbContext> options) : base(options) { }

    public DbSet<TenantItem> Items => Set<TenantItem>();
}

internal sealed class TenantFilteredDbContext : DbContext, IKunMultiTenantDbContext
{
    private readonly ITenantContext _tenantContext;

    public TenantFilteredDbContext(DbContextOptions<TenantFilteredDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<TenantItem> Items => Set<TenantItem>();

    public string? TenantIdForModelCacheKey => _tenantContext.TenantId?.ToString("D");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TenantItem>();
        modelBuilder.ApplyKunMultiTenantFilters(this);
    }
}

/// <summary>Benchmark-only entity implementing <c>IMultiTenant</c> so the Kun tenant filter attaches.</summary>
internal sealed class TenantItem : IMultiTenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TenantId { get; set; }
}
