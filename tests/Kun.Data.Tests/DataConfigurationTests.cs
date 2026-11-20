using System.Text.Json;
using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore;
using Kun.EntityFrameworkCore.Extensions;
using Kun.EventBus.DependencyInjection;
using Kun.MultiTenancy;
using Kun.Runtime;
using Kun.Testing.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kun.Data.Tests;

public sealed class DataConfigurationTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    private const string Reference = "secretRef://platform/pg/first";

    [Fact]
    public async Task 引用选择真实上下文_凭据按创建读取_不会静默换连接()
    {
        var ct = TestContext.Current.CancellationToken;
        var calls = 0;
        var currentConnection = postgres.AppConnectionString;
        var tenant = new AsyncLocalTenantContext();
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<ITenantContext>(tenant);
        builder.Services.AddKunEventBus();
        builder.Services.AddKunData<FirstContext>(Reference, _ => { calls++; return currentConnection; },
            (_, options) => options.UseKunSchema("plugin_bind_first"));
        builder.Services.AddKunData<SecondContext>("secretRef://platform/pg/second", _ => postgres.AppConnectionString,
            (_, options) => options.UseKunSchema("plugin_bind_second"));
        using var host = builder.Build();
        await using var runtime = new PluginLoader(host.Services);
        var config = JsonSerializer.SerializeToElement(new { dialect = "pg", connection = Reference, rls = new { enforce = true } });
        await runtime.MountAsync([Definition(config, ScopeKey.Global)], ct);
        Assert.Equal(PluginState.Ready, runtime.GetState("data", ScopeKey.Global));
        Assert.Equal(0, calls);
        await host.StartAsync(ct);
        var before = calls;
        using var tenantScope = tenant.BeginScope(Tenants.A);
        var data = runtime.GetContext("data", ScopeKey.Global).Data();
        await using (var session = await data.OpenAsync<FirstContext>(ct))
        {
            Assert.Equal(before + 1, calls);
            Assert.Equal(postgres.AppConnectionString, session.Context.Database.GetConnectionString());
            await session.Context.Database.OpenConnectionAsync(ct);
        }
        await Assert.ThrowsAsync<KunDataMisconfiguredException>(async () => await data.OpenAsync<SecondContext>(ct));
        currentConnection = postgres.ConnectionString;
        await using (var session = await data.OpenAsync<FirstContext>(ct))
            await Assert.ThrowsAsync<KunDataMisconfiguredException>(() => session.Context.Database.OpenConnectionAsync(ct));
        var visible = Assert.Single(runtime.DumpConfig()).Config.GetRawText();
        Assert.Contains(Reference, visible, StringComparison.Ordinal);
        Assert.DoesNotContain(postgres.AppConnectionString, visible, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("{\"connection\":\"Host=localhost;Password=not-a-reference\"}")]
    [InlineData("{\"connection\":\"secretRef://platform/pg/unknown\"}")]
    [InlineData("{\"connection\":\"secretRef://tenant/a/pg\"}")]
    [InlineData("{\"dialect\":\"mysql\"}")]
    [InlineData("{\"rls\":{\"enforce\":false}}")]
    [InlineData("{\"dialect\":\"pg\",\"dialect\":\"pg\"}")]
    public async Task 无效或未绑定的配置拒绝挂载(string json)
    {
        using var host = BuildHost();
        await using var runtime = new PluginLoader(host.Services);
        using var parsed = JsonDocument.Parse(json);
        await runtime.MountAsync([Definition(parsed.RootElement.Clone(), ScopeKey.Global)], TestContext.Current.CancellationToken);
        Assert.Equal(PluginState.Failed, runtime.GetState("data", ScopeKey.Global));
        Assert.IsType<KunDataMisconfiguredException>(runtime.GetFailure("data", ScopeKey.Global));
    }

    [Fact]
    public async Task 独立宿主直接使用数据服务_租户挂载不接受其他租户()
    {
        var ct = TestContext.Current.CancellationToken;
        using var host = BuildHost();
        await host.StartAsync(ct);
        var tenant = host.Services.GetRequiredService<ITenantContext>();
        using var tenantA = tenant.BeginScope(Tenants.A);
        await using (var standalone = await host.Services.GetRequiredService<IDataService>().OpenAsync<FirstContext>(ct))
            await standalone.Context.Database.OpenConnectionAsync(ct);
        await using var runtime = new PluginLoader(host.Services);
        var owner = new ScopeKey("tenant", Tenants.BText);
        await runtime.MountAsync([Definition(JsonSerializer.SerializeToElement(new { connection = Reference }), owner)], ct);
        var data = runtime.GetContext("data", owner).Data();
        await Assert.ThrowsAsync<KunDataMisconfiguredException>(async () => await data.OpenAsync<FirstContext>(ct));
        using (tenant.BeginScope(Tenants.B))
        await using (var session = await data.OpenAsync<FirstContext>(ct))
            await session.Context.Database.OpenConnectionAsync(ct);
    }

    private IHost BuildHost()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<ITenantContext>(new AsyncLocalTenantContext());
        builder.Services.AddKunEventBus();
        builder.Services.AddKunData<FirstContext>(Reference, _ => postgres.AppConnectionString,
            (_, options) => options.UseKunSchema("plugin_bind_first"));
        return builder.Build();
    }
    private static PluginDefinition Definition(JsonElement config, ScopeKey scope) => new(
        new PluginEntry("data", "qian.data", "1.0.0", scope, 0, config), new DataPlugin(), [DataDefinition.Key]);
    public sealed class FirstContext(DbContextOptions<FirstContext> options, ITenantContext tenant) : KunStrictDbContext(options, tenant);
    public sealed class SecondContext(DbContextOptions<SecondContext> options, ITenantContext tenant) : KunStrictDbContext(options, tenant);
}
