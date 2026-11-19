using Kun.EntityFrameworkCore.MultiTenancy;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kun.EntityFrameworkCore.Tests.MultiTenancy;

public sealed class OptionsBasedTenantConnectionResolverTests
{
    [Fact]
    public void Ctor_NullOptions_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new OptionsBasedTenantConnectionResolver(null!));
    }

    [Fact]
    public void Resolve_RejectsEmptyTenantId()
    {
        var resolver = BuildResolver(new TenantsConnectionOptions());
        Assert.Throws<ArgumentException>(() => resolver.ResolveConnectionString(Guid.Empty));
    }

    [Fact]
    public void Resolve_MissingTenant_Throws_WithRemediation()
    {
        var resolver = BuildResolver(new TenantsConnectionOptions());
        var ex = Assert.Throws<InvalidOperationException>(() => resolver.ResolveConnectionString(Tenants.X));
        Assert.Contains(Tenants.XText, ex.Message, StringComparison.Ordinal);
        Assert.Contains("Kun:MultiTenancy:Tenants", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Resolve_Returns_KnownConnectionString()
    {
        var opts = new TenantsConnectionOptions();
        opts.ConnectionStrings[Tenants.A] = "Host=db-a;Database=app;Username=u;Password=p";
        var resolver = BuildResolver(opts);

        Assert.Equal("Host=db-a;Database=app;Username=u;Password=p", resolver.ResolveConnectionString(Tenants.A));
    }

    private static OptionsBasedTenantConnectionResolver BuildResolver(TenantsConnectionOptions opts)
        => new(new FakeMonitor(opts));

    private sealed class FakeMonitor : IOptionsMonitor<TenantsConnectionOptions>
    {
        public FakeMonitor(TenantsConnectionOptions opts) { CurrentValue = opts; }
        public TenantsConnectionOptions CurrentValue { get; }
        public TenantsConnectionOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<TenantsConnectionOptions, string?> listener) => null;
    }
}
