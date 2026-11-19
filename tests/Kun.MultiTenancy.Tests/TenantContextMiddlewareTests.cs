using System.Net;
using System.Security.Claims;
using Kun.Core.MultiTenancy;
using Kun.MultiTenancy.DependencyInjection;
using Kun.MultiTenancy.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Kun.MultiTenancy.Tests;

public sealed class TenantContextMiddlewareTests
{
    private const string TenantId = "01900000-0000-7000-8000-000000000001";

    [Theory]
    [InlineData(TenantRequestMode.TenantRequired, TenantId, HttpStatusCode.OK)]
    [InlineData(TenantRequestMode.PlatformPrincipalAllowed, TenantId, HttpStatusCode.OK)]
    [InlineData(TenantRequestMode.TenantRequired, null, HttpStatusCode.Unauthorized)]
    [InlineData(TenantRequestMode.PlatformPrincipalAllowed, null, HttpStatusCode.OK)]
    public async Task AuthenticatedRequest_UsesConfiguredScope(TenantRequestMode mode, string? tenantId, HttpStatusCode status)
    {
        var claim = tenantId is null ? new Claim("tenant_id", "", "JSON_NULL") : new Claim("tenant_id", tenantId);
        var user = new ClaimsPrincipal(new ClaimsIdentity([claim], "test"));
        using var host = await BuildHost(user, mode);
        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/", TestContext.Current.CancellationToken);
        Assert.Equal(status, response.StatusCode);
        if (status == HttpStatusCode.OK)
            Assert.Equal(tenantId ?? "platform", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Null(host.Services.GetRequiredService<ITenantContext>().TenantId);
    }

    [Theory]
    [InlineData(TenantRequestMode.TenantRequired, HttpStatusCode.Unauthorized)]
    [InlineData(TenantRequestMode.PlatformPrincipalAllowed, HttpStatusCode.OK)]
    public async Task AnonymousRequest_UsesHostPolicy(TenantRequestMode mode, HttpStatusCode status)
    {
        using var host = await BuildHost(new ClaimsPrincipal(new ClaimsIdentity()), mode);
        using var client = host.GetTestClient();
        using var response = await client.GetAsync("/", TestContext.Current.CancellationToken);
        Assert.Equal(status, response.StatusCode);
    }

    private static Task<IHost> BuildHost(ClaimsPrincipal user, TenantRequestMode mode) =>
        new HostBuilder().ConfigureWebHost(web => web.UseTestServer()
            .ConfigureServices(services => services.AddKunMultiTenancy())
            .Configure(app =>
            {
                app.Use(async (context, next) =>
                {
                    context.User = user;
                    await next(context);
                });
                app.UseKunMultiTenancy(mode);
                app.Run(context => context.Response.WriteAsync(
                    context.RequestServices.GetRequiredService<ITenantContext>().TenantId ?? "platform", context.RequestAborted));
            })).StartAsync(TestContext.Current.CancellationToken);
}
