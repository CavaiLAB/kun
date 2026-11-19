using System.Text.Json;
using System.Text.Json.Serialization;
using Kun.Authorization;
using Kun.Core.Cqrs;
using Kun.Core.MultiTenancy;
using Kun.Core.Security;
using Kun.EntityFrameworkCore.Extensions;
using Kun.EventBus.DependencyInjection;
using Kun.Mediator.DependencyInjection;
using Kun.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Qian.Authz.Abstractions;
using Kun.Data;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using Kun.Errors;
using Kun.Tenancy;

namespace Qian.Scheduler.TestHost;

public sealed record HostConfig(string Application, string Worker, string WorkerRole, int LeaseSeconds = 6, IReadOnlyList<string>? WarmupTenants = null);

public sealed class ProbeHost : IAsyncDisposable
{
    private readonly IHost _host;
    public PluginLoader Runtime { get; }
    public ITenantContext Tenant { get; }
    public IPluginContext Caller => Runtime.GetContext("probe", ScopeKey.Global);
    public IServiceProvider Services => _host.Services;
    private ProbeHost(IHost host, PluginLoader runtime, ITenantContext tenant) { _host = host; Runtime = runtime; Tenant = tenant; }
    public static async Task<ProbeHost> CreateAsync(HostConfig config, bool worker, CancellationToken ct = default)
    {
        var builder = Host.CreateApplicationBuilder(); builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Error).AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Error);
        var tenant = new AsyncLocalTenantContext(); builder.Services.AddSingleton<ITenantContext>(tenant);
        builder.Services.AddSingleton(config); builder.Services.AddSingleton<ICurrentUser>(new ServiceUser());
        builder.Services.AddSingleton<IPermissionChecker>(new ProbePermissions());
        builder.Services.AddKunEventBus();
        builder.Services.AddKunData<SchedulerDbContext>((_, options) => options.UseNpgsql(config.Application).UseKunSchema(SchedulerDbContext.Schema));
        builder.Services.AddKunData<SchedulerWorkerDbContext>((_, options) => options.UseNpgsql(config.Worker).UseKunSchema(SchedulerDbContext.Schema));
        builder.Services.AddKunMediator(registry => registry.AddAuthorizedHandler<ProbeRequest, ProbeResponse, ProbeHandler>(_ => throw new ErrorException(new Error(PlatformErrors.AuthzDenied))));
        var host = builder.Build(); await host.StartAsync(ct);
        var runtime = new PluginLoader(host.Services, host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("SchedulerProbe"));
        var result = new ProbeHost(host, runtime, tenant);
        await runtime.MountAsync([
            Define("data", "qian.data", new DataPlugin(), [DataDefinition.Key]),
            Define("tenancy", "qian.tenancy", new DirectoryPlugin(config), [TenancyDefinition.Key]),
            Define("probe", "qian.scheduler.probe", new ProbePlugin(), [])], ct);
        if (worker) await WarmUpAsync(runtime, tenant, config, ct);
        await runtime.MountAsync([
            Define("scheduler", "qian.scheduler", worker ? new SchedulerComponent() : new ProducerPlugin(), [SchedulerDefinition.Key],
                new { workerRole = config.WorkerRole, leaseSeconds = config.LeaseSeconds, maxParallel = 16 })], ct);
        if (runtime.GetState("scheduler", ScopeKey.Global) != PluginState.Ready) throw new InvalidOperationException("调度宿主未就绪", runtime.GetFailure("scheduler", ScopeKey.Global));
        if (runtime.GetState("probe", ScopeKey.Global) != PluginState.Ready) throw new InvalidOperationException("探针未就绪", runtime.GetFailure("probe", ScopeKey.Global));
        return result;
    }
    // READY 表示这个进程的第一次执行不再付冷启动：真实租户的 EF 模型、领取，以及 BeginAsync、初始续租、收尾用到的查询与保存管线都已跑过。
    // 只建模型不够：冷进程的准备阶段（领取 → 初始续租）在 CPU 争用下比热路径慢 5–8 倍，会跨过 6 秒测试租约，
    // 首次执行在准备阶段丢租约、多记一次 attempt（S1-QA-023）。预热发生在领取循环之前；不造作业、不留持久状态
    // （写入都在回滚的事务里，只写定义表，续租语句命中 0 行，不碰用例自建的触发器）、不隐藏重试。
    // Kun 模型缓存按 tenant 分区，必须预热真实被测租户，随机虚构租户不能替代。
    private static async Task WarmUpAsync(PluginLoader runtime, ITenantContext tenant, HostConfig config, CancellationToken ct)
    {
        var data = runtime.GetContext("data", ScopeKey.Global);
        await using var worker = new SchedulerWorker(data, tenant, new SchedulerSettings(config.WorkerRole, config.LeaseSeconds, 1));
        _ = await worker.ClaimAsync(0, ct); // 真实领取语句与 worker 登录校验；LIMIT 0，不领任何触发器
        foreach (var tenantId in config.WarmupTenants ?? [])
        {
            using var warmupTenant = tenant.BeginScope(tenantId);
            var claim = new JobClaim(tenantId, Guid.NewGuid(), Guid.NewGuid(), 0, DateTimeOffset.UtcNow, "warmup");
            // 真实收尾入口：对不存在的触发器在所有权校验处返回 false、事务回滚；跑过与 BeginAsync 同一条锁行查询和数据库时钟。
            _ = await worker.CompleteAsync(claim, "once", new JobOptions(), "succeeded", null, ct);
            // 下面是复刻：只留真实入口跑不到、又占着租约的几步，逐条对应 SchedulerWorker 的方法，整个事务最后回滚。
            // 产品改了对应的查询、保存或续租语句，这里要跟着改：漏改不报错，只会让首次执行变冷，失败现场的 firstExecutions 会显出来。
            await using var session = await data.Data().OpenAsync<SchedulerDbContext>(ct);
            var db = session.Context;
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            // BeginAsync：按主键取定义与 run。谓词同样取自名为 claim 的 JobClaim，EF 才复用编译好的查询；定义不存在，SingleAsync 抛出即可。
            try { _ = await db.Definitions.SingleAsync(x => x.Id == claim.Id, ct); } catch (InvalidOperationException) { }
            _ = await db.Runs.SingleOrDefaultAsync(x => x.Id == claim.RunId, ct);
            // BeginAsync：解作业选项。
            _ = JsonSerializer.Deserialize<JobOptions>(JsonSerializer.Serialize(new JobOptions(), PostgresScheduler.Json), PostgresScheduler.Json);
            // BeginAsync：SaveChangesAsync 的插入与更新（首次建 run、接管时更新 run）。借定义表跑，不碰 run 表上用例注入的触发器。
            var id = Guid.NewGuid();
            var row = new JobDefinitionRow { TenantId = tenantId, Id = id, BusinessKey = "warmup-" + id.ToString("N"), Target = "warmup", Schedule = "once", CreatedBy = "warmup" };
            db.Definitions.Add(row); await session.SaveChangesAsync(ct);
            row.Parameters = """{"warmup":true}"""; await session.SaveChangesAsync(ct);
            // RenewAsync：同一条续租语句，对不存在的触发器命中 0 行。
            _ = await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE plugin_qian_scheduler.job_triggers SET lease_until=now()+{TimeSpan.FromSeconds(config.LeaseSeconds)} WHERE tenant_id={claim.TenantId} AND id={claim.Id} AND lease_owner={claim.Worker} AND generation={claim.Generation} AND lease_until>now() AND state='ready'", ct);
            await transaction.RollbackAsync(ct);
        }
    }
    public static PluginDefinition Define(string id, string pluginId, IKunPlugin plugin, IReadOnlyList<ServiceKey> provides, object? config = null)
        => new(new PluginEntry(id, pluginId, "1.0.0", ScopeKey.Global, 0, JsonSerializer.SerializeToElement(config ?? new { })), plugin, provides);
    public async ValueTask DisposeAsync() { await Runtime.DisposeAsync(); await _host.StopAsync(); _host.Dispose(); }
    private sealed class ProducerPlugin : IKunPlugin
    {
        public IReadOnlyList<ServiceKey> Inject => [DataDefinition.Key];
        public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
        { ctx.Provide<IScheduler>(SchedulerDefinition.Key, new PostgresScheduler(ctx, ctx.Services.GetRequiredService<ITenantContext>())); return ValueTask.CompletedTask; }
    }
    private sealed class DirectoryPlugin(HostConfig config) : IKunPlugin, ITenantDirectory
    {
        public IReadOnlyList<ServiceKey> Inject => [];
        public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct) { ctx.Provide<ITenantDirectory>(TenancyDefinition.Key, this); return ValueTask.CompletedTask; }
        public ValueTask<TenantDescriptor?> GetAsync(string code, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public async ValueTask<TenantDescriptor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await using var db = new NpgsqlConnection(config.Application); await db.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT status FROM public.sch_probe_tenants WHERE id=@id", db); command.Parameters.AddWithValue("id", id);
            var status = await command.ExecuteScalarAsync(cancellationToken) as string;
            return new TenantDescriptor(id, "fixture", "测试租户", TenantIsolation.Row, status == "suspended" ? TenantStatus.Suspended : TenantStatus.Active);
        }
    }
    private sealed class ServiceUser : ICurrentUser
    { public string UserId => "qian.scheduler"; public IReadOnlyList<string> Roles => []; public bool IsAuthenticated => true; }
    // 权限端口是调度测试依赖替身；真实能力经 Kun 的权限行为执行，不替代 Authz 的集成验收。
    private sealed class ProbePermissions : IPermissionChecker
    {
        public ValueTask<PermissionDecision> CheckAsync(PermissionRequest request, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new PermissionDecision(request.Subject.Id == "qian.scheduler" && Guid.TryParse(request.Subject.TenantId, out _) && request.Permission == "qian.scheduler.probe:job:run", "fixture", []));
        public ValueTask<IReadOnlyList<PermissionDecision>> BatchCheckAsync(IReadOnlyList<PermissionRequest> requests, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<IReadOnlyList<string>> ListAllowedActionsAsync(AuthorizationSubject subject, string resourceType, string? resourceId = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<IReadOnlyList<string>> ListPermissionsAsync(AuthorizationSubject subject, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<PermissionDecision> ExplainAsync(PermissionRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}

[KunPlugin("qian.scheduler.probe")]
public sealed class ProbePlugin : IKunPlugin
{
    public IReadOnlyList<ServiceKey> Inject => [];
    public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
    { ctx.ProvideJob<ProbeRequest, ProbeResponse>(); return ValueTask.CompletedTask; }
}

[KunJob("probe")]
[Capability("run", typeof(ProbeResponse), typeof(ProbeJson), Description = "集群调度验收探针：持久记录每次处理器进入与幂等业务效果。", SideEffect = "write", Idempotent = true, RequiredPermission = "qian.scheduler.probe:job:run")]
public sealed partial record ProbeRequest(Guid IdempotencyKey, string Mode = "normal", int DelayMs = 0);
public sealed record ProbeResponse(bool Applied);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ProbeRequest))][JsonSerializable(typeof(ProbeResponse))]
public sealed partial class ProbeJson : JsonSerializerContext;

public sealed class ProbeHandler(HostConfig config, ITenantContext tenant) : IRequestHandler<ProbeRequest, ProbeResponse>
{
    public async ValueTask<ProbeResponse> Handle(ProbeRequest request, CancellationToken cancellationToken)
    {
        await using var db = new NpgsqlConnection(config.Application); await db.OpenAsync(cancellationToken);
        await using var enter = new NpgsqlCommand("INSERT INTO public.sch_probe_attempts(run_id,tenant_id,process_id,started) VALUES (@run,@tenant,@pid,clock_timestamp()) RETURNING id", db);
        enter.Parameters.AddWithValue("run", request.IdempotencyKey); enter.Parameters.AddWithValue("tenant", tenant.TenantId!); enter.Parameters.AddWithValue("pid", Environment.ProcessId);
        var attempt = (long)(await enter.ExecuteScalarAsync(cancellationToken))!;
        await using var count = new NpgsqlCommand("SELECT count(*) FROM public.sch_probe_attempts WHERE run_id=@run", db); count.Parameters.AddWithValue("run", request.IdempotencyKey);
        var first = (long)(await count.ExecuteScalarAsync(cancellationToken))! == 1;
        if (request.Mode == "fail") throw new InvalidOperationException("预期失败探针");
        if (request.Mode == "hold-first" && first) await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
        if (request.DelayMs > 0) await Task.Delay(request.DelayMs, cancellationToken);
        await using var effect = new NpgsqlCommand("INSERT INTO public.sch_probe_effects(run_id,tenant_id,process_id) VALUES (@run,@tenant,@pid) ON CONFLICT (run_id) DO NOTHING", db);
        effect.Parameters.AddWithValue("run", request.IdempotencyKey); effect.Parameters.AddWithValue("tenant", tenant.TenantId!); effect.Parameters.AddWithValue("pid", Environment.ProcessId);
        var applied = await effect.ExecuteNonQueryAsync(cancellationToken) == 1;
        if (request.Mode == "effect-then-hold" && first) await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
        await using var finish = new NpgsqlCommand("UPDATE public.sch_probe_attempts SET finished=clock_timestamp() WHERE id=@id", db); finish.Parameters.AddWithValue("id", attempt); await finish.ExecuteNonQueryAsync(cancellationToken);
        return new ProbeResponse(applied);
    }
}
