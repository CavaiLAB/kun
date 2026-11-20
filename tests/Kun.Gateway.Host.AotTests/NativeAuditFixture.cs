using System.Text.Json;
using Kun.Core.MultiTenancy;
using Kun.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Kun.Audit;
using Kun.Audit.Postgres;
using Kun.Data.Native;
using Kun.Data.Native.Postgres;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using Kun.Errors;
using Qian.Testing.NativeAudit;

namespace Kun.Gateway.Host.AotTests;

// 用第二个真实 Provider 组合读回原生生产宿主写入的审计链；它不替生产宿主写成功记录。
internal sealed class NativeAuditReader(ServiceProvider provider, PluginLoader loader, AuditConsumer consumer) : IAsyncDisposable
{
    private static readonly string[] s_bindings = ["audit_tenant", "audit_platform"];
    // 平台链上的全部记录：分页读到末尾（每页 100）。只读第一页时，读回之后的「TenantId 全为 null」「审计链不能含凭据」看不到第 100 条之后的记录（S1-QA-026）。
    internal async Task<IReadOnlyList<AuditChainEntry>> Read(CancellationToken ct)
    {
        var entries = new List<AuditChainEntry>(); long after = 0;
        while (true)
        {
            var page = await consumer.Context!.Audit().ReadPlatformAsync(consumer.Context!, after, 100, cancellationToken: ct);
            entries.AddRange(page);
            if (page.Count < 100) return entries;
            after = page[^1].Sequence;
        }
    }
    internal Task<AuditChainVerification> Verify(CancellationToken ct) => consumer.Context!.Audit().VerifyPlatformAsync(consumer.Context!, ct);
    internal static async Task<NativeAuditReader> Start(NativeAuditDatabase database, CancellationToken ct)
    {
        var services = new ServiceCollection(); services.AddLogging(); services.AddSingleton(ErrorCatalog.CreateDefault());
        services.AddSingleton<ITenantContext>(new AsyncLocalTenantContext());
        var guard = NativeAuditStorage.ExpectedGuard(database.Roles);
        NativeDataTable[] tables = [new(NativeAuditStorage.Schema, "audit_entries", NativeAuditStorage.GuardPolicy, guard, guard),
            new(NativeAuditStorage.Schema, "audit_chain_state", NativeAuditStorage.GuardPolicy, guard, guard)];
        services.AddKunNativeData(new("audit_tenant", database.Roles.TenantLogin, NativeDataAccess.Tenant, _ => ValueTask.FromResult(database.TenantConnection), tables));
        services.AddKunNativeData(new("audit_platform", database.Roles.PlatformLogin!, NativeDataAccess.Platform, _ => ValueTask.FromResult(database.PlatformConnection), tables));
        var provider = services.BuildServiceProvider(); var loader = new PluginLoader(provider); var consumer = new AuditConsumer();
        try
        {
            await loader.MountAsync([
                Define("data", "qian.data.native", new NativeDataComponent(), new { connections = s_bindings }, NativeDataDefinition.Key),
                Define("audit", "qian.audit.native", new NativeAuditComponent(), new
                {
                    tenantBinding = "audit_tenant", platformBinding = "audit_platform", tenantRole = database.Roles.TenantLogin,
                    platformRole = database.Roles.PlatformLogin, functionOwner = database.Roles.FunctionOwner, maxAttempts = 6
                }, AuditDefinition.Key, CommandPolicies.AuditKey),
                Define("reader", "test.gateway-audit-reader", consumer, new { })], ct);
            foreach (var id in new[] { "data", "audit", "reader" })
                Assert.True(loader.GetState(id, ScopeKey.Global) == PluginState.Ready, "审计回读组合未就绪：" + id);
            return new(provider, loader, consumer);
        }
        catch { await loader.DisposeAsync(); await provider.DisposeAsync(); throw; }
    }
    private static PluginDefinition Define(string id, string plugin, IKunPlugin implementation, object config, params ServiceKey[] provides) =>
        new(new PluginEntry(id, plugin, "1.0.0", ScopeKey.Global, 0, JsonSerializer.SerializeToElement(config)), implementation, provides);
    public async ValueTask DisposeAsync() { await loader.DisposeAsync(); await provider.DisposeAsync(); }
}

internal sealed class AuditConsumer : IKunPlugin
{
    internal IPluginContext? Context { get; private set; }
    public IReadOnlyList<ServiceKey> Inject => [AuditDefinition.Key];
    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct) { Context = ctx; return ValueTask.CompletedTask; }
}
