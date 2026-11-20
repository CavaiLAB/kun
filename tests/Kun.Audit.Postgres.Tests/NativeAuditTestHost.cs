using System.Text.Json;
using Kun.Core.MultiTenancy;
using Kun.Data.Native;
using Kun.Data.Native.Postgres;
using Kun.Errors;
using Kun.MultiTenancy;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using Kun.Testing.NativeAudit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kun.Audit.Postgres.Tests;

internal sealed class NativeAuditTestHost(IHost host, PluginLoader loader, AsyncLocalTenantContext tenant, NativeAuditDatabase database, ScopeKey auditScope) : IAsyncDisposable
{
    private static readonly string[] BothBindings = ["audit_tenant", "audit_platform"];
    private static readonly string[] TenantBindings = ["audit_tenant"];
    internal IHost Host => host;
    internal PluginLoader Loader => loader;
    internal AsyncLocalTenantContext Tenant => tenant;
    internal IPluginContext Caller => loader.GetContext("consumer", auditScope);
    internal IAuditService Audit => Caller.Audit();
    internal INativeDataService Data => Caller.NativeData();
    internal static async Task<NativeAuditTestHost> Create(NativeAuditDatabase database, CancellationToken ct, string? scopedTenant = null,
        bool platformBinding = true, int maxAttempts = 6, bool ready = true)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
        var tenant = new AsyncLocalTenantContext(); builder.Services.AddSingleton<ITenantContext>(tenant);
        builder.Services.AddSingleton(ErrorCatalog.CreateDefault());
        var guard = NativeAuditStorage.ExpectedGuard(database.Roles);
        var tables = new[] { new NativeDataTable(NativeAuditStorage.Schema, "audit_entries", NativeAuditStorage.GuardPolicy, guard, guard),
            new NativeDataTable(NativeAuditStorage.Schema, "audit_chain_state", NativeAuditStorage.GuardPolicy, guard, guard) };
        builder.Services.AddKunNativeData(new("audit_tenant", database.Roles.TenantLogin, NativeDataAccess.Tenant, _ => ValueTask.FromResult(database.TenantConnection), tables));
        if (platformBinding) builder.Services.AddKunNativeData(new("audit_platform", database.Roles.PlatformLogin!, NativeDataAccess.Platform, _ => ValueTask.FromResult(database.PlatformConnection), tables));
        var host = builder.Build(); await host.StartAsync(ct); var loader = new PluginLoader(host.Services);
        var scope = scopedTenant is null ? ScopeKey.Global : new ScopeKey("tenant", scopedTenant);
        var running = new NativeAuditTestHost(host, loader, tenant, database, scope);
        try
        {
            await loader.MountAsync([Define("data", "qian.data.native", new NativeDataComponent(), [NativeDataDefinition.Key], ScopeKey.Global,
                JsonSerializer.SerializeToElement(new { connections = platformBinding ? BothBindings : TenantBindings }))], ct);
            Assert.True(loader.GetState("data", ScopeKey.Global) == PluginState.Ready, loader.GetFailure("data", ScopeKey.Global)?.ToString());
            using (scopedTenant is null ? null : tenant.BeginScope(Guid.ParseExact(scopedTenant, "D")))
            {
                await running.MountAudit(ct, platformBinding, maxAttempts);
                if (ready) Assert.True(loader.GetState("audit", scope) == PluginState.Ready, loader.GetFailure("audit", scope)?.ToString());
                await loader.MountAsync([Define("consumer", "native.audit.test", new Consumer(), [], scope, JsonSerializer.SerializeToElement(new { }))], ct);
            }
            return running;
        }
        catch { await running.DisposeAsync(); throw; }
    }
    internal ValueTask MountAudit(CancellationToken ct, bool platformBinding = true, int maxAttempts = 6) => loader.MountAsync([
        Define("audit", "qian.audit.native", new NativeAuditComponent(), [AuditDefinition.Key, CommandPolicies.AuditKey], auditScope,
            JsonSerializer.SerializeToElement(new { tenantBinding = "audit_tenant", platformBinding = platformBinding ? "audit_platform" : null,
                tenantRole = database.Roles.TenantLogin, platformRole = database.Roles.PlatformLogin, functionOwner = database.Roles.FunctionOwner, maxAttempts }))], ct);
    internal static PluginDefinition Define(string id, string pluginId, IKunPlugin plugin, IReadOnlyList<ServiceKey> provides, ScopeKey scope, JsonElement config) =>
        new(new PluginEntry(id, pluginId, "1.0.0", scope, 0, config), plugin, provides);
    public async ValueTask DisposeAsync() { await loader.DisposeAsync(); await host.StopAsync(); host.Dispose(); }
    private sealed class Consumer : IKunPlugin
    {
        public IReadOnlyList<ServiceKey> Inject => [AuditDefinition.Key, NativeDataDefinition.Key];
        public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct) => ValueTask.CompletedTask;
    }
}
