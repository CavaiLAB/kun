using System.Security.Claims;
using Kun.Core.MultiTenancy;
using Kun.MultiTenancy;
using Kun.MultiTenancy.DependencyInjection;
using Kun.Security.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kun.Security.Tests;

public sealed class TenantContextRegistrationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Registration_EitherOrder_UsesOneAmbientContext(bool securityFirst)
    {
        var services = new ServiceCollection();
        if (securityFirst) services.AddKunSecurity();
        services.AddKunMultiTenancy();
        services.AddKunSecurity();
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        var tenant = Assert.Single(provider.GetServices<ITenantContext>());
        Assert.Same(provider.GetRequiredService<AsyncLocalTenantContext>(), tenant);
        var accessor = provider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("tenant_id", "http-tenant")], "test"))
        };
        Assert.Null(tenant.TenantId);
        using (tenant.BeginScope("message-tenant"))
        {
            await Task.Yield();
            Assert.Equal("message-tenant", tenant.TenantId);
            using (tenant.BeginScope(null)) Assert.Null(tenant.TenantId);
            Assert.Equal("message-tenant", tenant.TenantId);
        }
        Assert.Null(tenant.TenantId);
        accessor.HttpContext = null;
        Assert.Null(tenant.TenantId);
    }

    [Fact]
    public void SecurityOnly_RegistersIdentityServices()
    {
        using var provider = new ServiceCollection().AddKunSecurity().BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<IHttpContextAccessor>());
        Assert.Null(provider.GetService<ITenantContext>());
    }
}
