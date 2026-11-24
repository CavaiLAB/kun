using System.Text;
using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace Kun.EntityFrameworkCore.Tests.MultiTenancy;

// Database 策略按 docs/guides/saas-multitenancy.md 的写法装配：Configure<TenantsConnectionOptions>(配置节)，
// JSON 里以租户 UUID 为键；再调 AddKunMultiTenancy<TContext>(Database)。宿主不加默认配置源，只读这里给的 JSON。
public sealed class TenantsConnectionOptionsBindingTests
{
    private const string Acme = "01900000-0000-7000-8000-000000000001";
    private const string Fabrikam = "01900000-0000-7000-8000-000000000002";

    private const string DocumentedJson = """
        {
          "Kun": {
            "MultiTenancy": {
              "Tenants": {
                "ConnectionStrings": {
                  "01900000-0000-7000-8000-000000000001": "Host=db-acme;Database=catalog",
                  "01900000-0000-7000-8000-000000000002": "Host=db-fabrikam;Database=catalog"
                }
              }
            }
          }
        }
        """;

    [Fact]
    public async Task DocumentedJson_BindsThroughConfigure_AndResolvesEachTenant()
    {
        using var host = Build(DocumentedJson);
        await host.StartAsync(TestContext.Current.CancellationToken);

        var resolver = host.Services.GetRequiredService<IDbConnectionResolver>();
        Assert.Equal("Host=db-acme;Database=catalog", resolver.ResolveConnectionString(Guid.Parse(Acme)));
        Assert.Equal("Host=db-fabrikam;Database=catalog", resolver.ResolveConnectionString(Guid.Parse(Fabrikam)));
        var options = host.Services.GetRequiredService<IOptions<TenantsConnectionOptions>>().Value;
        Assert.Equal([Guid.Parse(Acme), Guid.Parse(Fabrikam)], options.ConnectionStrings.Keys.Order());

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CodeAndConfiguration_CoverDifferentTenants_BothResolve()
    {
        using var host = Build(Json(Acme, "Host=db-acme;Database=catalog"),
            services => services.Configure<TenantsConnectionOptions>(
                options => options.ConnectionStrings[Guid.Parse(Fabrikam)] = "Host=db-fabrikam;Database=catalog"));
        await host.StartAsync(TestContext.Current.CancellationToken);

        var resolver = host.Services.GetRequiredService<IDbConnectionResolver>();
        Assert.Equal("Host=db-acme;Database=catalog", resolver.ResolveConnectionString(Guid.Parse(Acme)));
        Assert.Equal("Host=db-fabrikam;Database=catalog", resolver.ResolveConnectionString(Guid.Parse(Fabrikam)));

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("tenant-a")]
    [InlineData("0190F3A2-7C41-7B3E-9D2A-5C8E1F4A6B20")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("{01900000-0000-7000-8000-000000000001}")]
    [InlineData("01900000000070008000000000000001")]
    public async Task MalformedKey_FailsHostStart_NamingTheKey(string key)
    {
        using var host = Build(Json(key, "Host=db;Database=catalog"));

        var error = await Assert.ThrowsAsync<OptionsValidationException>(
            () => host.StartAsync(TestContext.Current.CancellationToken));
        Assert.Contains($"key '{key}' must be a non-empty UUID in lowercase D format", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CodeAndConfigurationDisagree_FailsHostStart_NamingTheKey()
    {
        using var host = Build(Json(Acme, "Host=db-acme;Database=catalog"),
            services => services.Configure<TenantsConnectionOptions>(
                options => options.ConnectionStrings[Guid.Parse(Acme)] = "Host=db-other;Database=catalog"));

        var error = await Assert.ThrowsAsync<OptionsValidationException>(
            () => host.StartAsync(TestContext.Current.CancellationToken));
        Assert.Contains($"key '{Acme}' has a different connection string in code and in configuration", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CodeAndConfigurationAgree_Starts()
    {
        using var host = Build(Json(Acme, "Host=db-acme;Database=catalog"),
            services => services.Configure<TenantsConnectionOptions>(
                options => options.ConnectionStrings[Guid.Parse(Acme)] = "Host=db-acme;Database=catalog"));
        await host.StartAsync(TestContext.Current.CancellationToken);

        Assert.Equal("Host=db-acme;Database=catalog",
            host.Services.GetRequiredService<IDbConnectionResolver>().ResolveConnectionString(Guid.Parse(Acme)));

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    private static IHost Build(string json, Action<IServiceCollection>? configure = null)
    {
        var builder = new HostApplicationBuilder(new HostApplicationBuilderSettings { DisableDefaults = true });
        builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        builder.Services.Configure<TenantsConnectionOptions>(
            builder.Configuration.GetSection(TenantsConnectionOptions.SectionName));
        builder.Services.AddKunMultiTenancy<CatalogContext>(options => options.Strategy = TenantIsolation.Database);
        configure?.Invoke(builder.Services);
        return builder.Build();
    }

    private static string Json(string key, string connectionString) =>
        System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["Kun"] = new Dictionary<string, object>
            {
                ["MultiTenancy"] = new Dictionary<string, object>
                {
                    ["Tenants"] = new Dictionary<string, object>
                    {
                        ["ConnectionStrings"] = new Dictionary<string, string> { [key] = connectionString },
                    },
                },
            },
        });

    private sealed class CatalogContext(DbContextOptions<CatalogContext> options) : DbContext(options);
}
