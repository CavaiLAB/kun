using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore.MultiTenancy;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kun.EntityFrameworkCore.Tests.MultiTenancy;

public sealed class SchemaPerTenantProvisionerTests
{
    [Fact]
    public void Ctor_NullFactory_Throws()
    {
        var options = Options.Create(new KunMultiTenancyOptions());
        Assert.Throws<ArgumentNullException>(() =>
            new SchemaPerTenantProvisioner<TestDbContext>(factory: null!, options));
    }

    [Fact]
    public void Ctor_NullOptions_Throws()
    {
        var factory = new FakeFactory();
        Assert.Throws<ArgumentNullException>(() =>
            new SchemaPerTenantProvisioner<TestDbContext>(factory, options: null!));
    }

    [Theory]
    [InlineData("bad prefix")]
    [InlineData("")]
    [InlineData("123prefix")]
    public async Task ProvisionAsync_RejectsInvalidSchemaPrefix(string prefix)
    {
        var provisioner = BuildProvisioner(new KunMultiTenancyOptions { SchemaPrefix = prefix });

        await Assert.ThrowsAsync<InvalidOperationException>(() => provisioner.ProvisionAsync(
            new TenantInfo(Tenants.A), TestContext.Current.CancellationToken));
    }

    [Fact]
    public void GetSchemaName_UsesCanonicalUuidWithoutSeparators()
    {
        var options = new KunMultiTenancyOptions();
        Assert.Equal("tenant_" + Tenants.A.ToString("N"), options.GetSchemaName(Tenants.A));
    }

    [Fact]
    public async Task DeprovisionAsync_ThrowsWhenDestructiveOptInDisabled()
    {
        var provisioner = BuildProvisioner(new KunMultiTenancyOptions { AllowDestructiveDeprovision = false });

        Task act() => provisioner.DeprovisionAsync(Tenants.A, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<NotSupportedException>(act);
    }

    [Fact]
    public async Task DeprovisionAsync_RejectsEmptyTenant_EvenWhenOptInEnabled()
    {
        var provisioner = BuildProvisioner(new KunMultiTenancyOptions { AllowDestructiveDeprovision = true });

        Task act() => provisioner.DeprovisionAsync(Guid.Empty, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    private static SchemaPerTenantProvisioner<TestDbContext> BuildProvisioner(KunMultiTenancyOptions opts)
    {
        var factory = new FakeFactory();
        return new SchemaPerTenantProvisioner<TestDbContext>(factory, Options.Create(opts));
    }

    private sealed class FakeFactory : ITenantDbContextFactory<TestDbContext>
    {
        public TestDbContext CreateForTenant(Guid tenantId)
        {
            // Test never reaches CreateForTenant because all current test cases assert on
            // argument validation BEFORE the factory is called.
            throw new InvalidOperationException("Fake factory should not be called in unit tests.");
        }
    }
}
