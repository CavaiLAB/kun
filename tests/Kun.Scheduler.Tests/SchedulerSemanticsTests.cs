using System.Text.Json;
using Kun.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using Kun.Errors;
using Qian.Scheduler.TestHost;
using static Qian.Scheduler.Tests.SchedulerScenario;

namespace Qian.Scheduler.Tests;

// 调度语义、准入与隔离：业务键幂等与取消、目录准入、迁移模型与 cron 解析、过期租约围栏、三表 RLS 与组合外键、
// worker 专用登录的列权限、租户冻结、超时重试与取消终态、misfire，以及停机窗口里上下文已关闭时 worker 的收尾。
public sealed class SchedulerSemanticsTests(SchedulerFixture fixture) : IClassFixture<SchedulerFixture>
{
    [Fact]
    public async Task 同业务键并发幂等_取消到点不执行_不同租户同键互不影响()
    {
        var config = fixture.LongLease; // 不测到期，用 30 秒租约
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(config, false, Ct);
        var due = DateTimeOffset.UtcNow.AddSeconds(4);
        var ids = await Task.WhenAll(Enumerable.Range(0, 20).Select(async _ =>
        {
            using var scope = producer.Tenant.BeginScope(Guid.ParseExact(TenantA, "D")); var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantA));
            return (await caller.Scheduler().ScheduleOnce(caller, "same", due, Target, Parameters, cancellationToken: Ct)).Id;
        }));
        Assert.Single(ids.Distinct());
        using (producer.Tenant.BeginScope(Guid.ParseExact(TenantA, "D")))
        {
            var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantA));
            var conflict = await Assert.ThrowsAsync<ErrorException>(() => caller.Scheduler().ScheduleOnce(caller, "same", due.AddMinutes(1), Target, Parameters, cancellationToken: Ct).AsTask());
            Assert.Equal("CONFLICT", conflict.Error.Code.Code);
            Assert.True(await caller.Scheduler().Cancel(caller, "same", Ct)); Assert.True(await caller.Scheduler().Cancel(caller, "same", Ct));
        }
        using (producer.Tenant.BeginScope(Guid.ParseExact(TenantB, "D")))
        {
            var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantB)); await caller.Scheduler().ScheduleOnce(caller, "same", due, Target, Parameters, cancellationToken: Ct);
        }
        await using var worker = await WorkerProcess.Start(config, TenantA, TenantB);
        await SchedulerFixture.WaitUntil(async () => await fixture.Scalar<long>("SELECT count(*) FROM public.sch_probe_effects") == 1, fixture.Leases(2, config));
        Assert.Equal(TenantB, await fixture.Scalar<string>("SELECT tenant_id FROM public.sch_probe_effects"));
        Assert.Equal(2, await fixture.Scalar<long>("SELECT count(*) FROM plugin_qian_scheduler.job_definitions"));
    }

    [Fact]
    public async Task 真实目录准入与执行_拒绝任意URL_未挂载与restrict不执行缓存处理器()
    {
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(fixture.Config, false, Ct);
        using var scope = producer.Tenant.BeginScope(Guid.ParseExact(TenantA, "D")); var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantA));
        foreach (var target in new[] { "https://example.com/job", "http://localhost/run", "https://127.0.0.1/run", "file:///tmp/job", "//example.com/run", "qian.missing/run" })
            await Assert.ThrowsAsync<ErrorException>(() => caller.Scheduler().ScheduleOnce(caller, "invalid", DateTimeOffset.UtcNow.AddMinutes(1), target, Parameters, cancellationToken: Ct).AsTask());
        var good = PostgresScheduler.Resolve(caller, Target); Assert.Equal("probe", good.Code);
        using (producer.Caller.Restrict(new ScopeKey("tenant", TenantA), registration => registration.Key != JobBindingRegistration.Key))
        {
            var error = await Assert.ThrowsAsync<ErrorException>(() => caller.Scheduler().ScheduleOnce(caller, "hidden", DateTimeOffset.UtcNow, Target, Parameters, cancellationToken: Ct).AsTask());
            Assert.Equal("NOT_FOUND", error.Error.Code.Code);
        }
        await producer.Runtime.UnmountAsync("probe", ScopeKey.Global, Ct);
        Assert.Throws<ObjectDisposedException>(() => PostgresScheduler.Resolve(caller, Target));
        Assert.Equal(0, await fixture.Scalar<long>("SELECT count(*) FROM plugin_qian_scheduler.job_definitions"));
    }

    [Fact]
    public void 生成迁移模型与三表强制RLS一致_分钟和七段年cron不混为每秒()
    {
        using var db = new SchedulerDesignTimeFactory().CreateDbContext([]); Assert.False(db.Database.HasPendingModelChanges());
        var script = db.GetService<IMigrator>().GenerateScript();
        foreach (var table in new[] { "job_definitions", "job_triggers", "job_runs" }) Assert.Contains("ALTER TABLE plugin_qian_scheduler." + table + " FORCE ROW LEVEL SECURITY", script, StringComparison.Ordinal);
        var now = new DateTimeOffset(2026, 10, 23, 12, 0, 12, TimeSpan.Zero);
        Assert.Equal(new DateTimeOffset(2026, 10, 23, 12, 1, 0, TimeSpan.Zero), JobSchedule.Next("0 * * * * ?", now));
        Assert.Equal(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero), JobSchedule.Next("0 0 0 1 1 ? 2027", now));
        Assert.Null(JobSchedule.Next("0 0 0 1 1 ? 2025", now));
        Assert.Throws<ErrorException>(() => JobSchedule.Validate("* * * * *"));
        Assert.Equal(now.AddSeconds(15), SchedulerWorker.NextOccurrence("interval:50000000", now, now.AddSeconds(12)));
    }

    [Fact]
    public async Task 过期租约拒绝旧执行者完成_新owner代次也拒绝旧续租和推进()
    {
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(fixture.Config, false, Ct);
        using (producer.Tenant.BeginScope(Guid.ParseExact(TenantA, "D")))
        {
            var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantA));
            await caller.Scheduler().ScheduleOnce(caller, "fencing", DateTimeOffset.UtcNow, Target, Parameters, cancellationToken: Ct);
        }
        var settings = new SchedulerSettings(fixture.Config.WorkerRole, 3, 1);
        await using var first = new SchedulerWorker(producer.Caller, producer.Tenant, settings);
        await using var second = new SchedulerWorker(producer.Caller, producer.Tenant, settings);
        var old = Assert.Single(await first.ClaimAsync(1, Ct));
        Assert.Empty(await second.ClaimAsync(1, Ct));
        await Task.Delay(TimeSpan.FromMilliseconds(3200), Ct);
        using (producer.Tenant.BeginScope(Guid.ParseExact(TenantA, "D"))) Assert.False(await first.CompleteAsync(old, "once", new JobOptions(), "succeeded", null, Ct));
        var current = Assert.Single(await second.ClaimAsync(1, Ct));
        Assert.Equal(old.RunId, current.RunId); Assert.Equal(old.Generation + 1, current.Generation); Assert.NotEqual(old.Worker, current.Worker);
        using (producer.Tenant.BeginScope(Guid.ParseExact(TenantA, "D")))
        {
            Assert.False(await first.CompleteAsync(old, "once", new JobOptions(), "failed", "old", Ct));
            await using var db = new NpgsqlConnection(fixture.Config.Application); await db.OpenAsync(Ct); await using var transaction = await db.BeginTransactionAsync(Ct);
            await using (var set = new NpgsqlCommand("SELECT set_config('app.current_tenant_id',@tenant,true)", db, transaction)) { set.Parameters.AddWithValue("tenant", TenantA); await set.ExecuteScalarAsync(Ct); }
            await using var renew = new NpgsqlCommand("UPDATE plugin_qian_scheduler.job_triggers SET lease_until=now()+interval '30 seconds' WHERE tenant_id=@tenant AND id=@id AND generation=@generation AND lease_owner=@owner AND lease_until>now()", db, transaction);
            renew.Parameters.AddWithValue("tenant", TenantA); renew.Parameters.AddWithValue("id", old.Id); renew.Parameters.AddWithValue("generation", old.Generation); renew.Parameters.AddWithValue("owner", old.Worker);
            Assert.Equal(0, await renew.ExecuteNonQueryAsync(Ct));
        }
        Assert.Equal(current.Worker, await fixture.Scalar<string>("SELECT lease_owner FROM plugin_qian_scheduler.job_triggers"));
        Assert.Equal(0, await fixture.Scalar<long>("SELECT run_count FROM plugin_qian_scheduler.job_triggers"));
    }

    [Fact]
    public async Task 普通应用原始SQL不能读改他租户三表_触发器组合外键拒绝跨租户伪造()
    {
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(fixture.Config, false, Ct);
        Guid b;
        using (producer.Tenant.BeginScope(Guid.ParseExact(TenantB, "D")))
        {
            var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantB));
            b = (await caller.Scheduler().ScheduleOnce(caller, "b", DateTimeOffset.UtcNow.AddMinutes(1), Target, Parameters, cancellationToken: Ct)).Id;
        }
        // 三张表都有 B 的真实行；不能以空运行表的零行结果冒充隔离证据。
        await fixture.Admin($"INSERT INTO plugin_qian_scheduler.job_runs(tenant_id,id,trigger_id,scheduled_at,started_at,status,attempts,worker,generation) VALUES ('{TenantB}','{Guid.NewGuid()}','{b}',now(),now(),'running',1,'fixture',1);");
        Assert.Equal(1, await fixture.Scalar<long>("SELECT count(*) FROM plugin_qian_scheduler.job_runs"));
        await using var db = new NpgsqlConnection(fixture.Config.Application); await db.OpenAsync(Ct); await using var transaction = await db.BeginTransactionAsync(Ct);
        await using (var set = new NpgsqlCommand("SELECT set_config('app.current_tenant_id',@tenant,true)", db, transaction)) { set.Parameters.AddWithValue("tenant", TenantA); await set.ExecuteScalarAsync(Ct); }
        foreach (var table in new[] { "job_definitions", "job_triggers", "job_runs" })
        {
            await using var read = new NpgsqlCommand("SELECT count(*) FROM plugin_qian_scheduler." + table, db, transaction);
            Assert.Equal(0L, await read.ExecuteScalarAsync(Ct));
        }
        foreach (var statement in new[] { "UPDATE plugin_qian_scheduler.job_definitions SET business_key='hijacked'", "UPDATE plugin_qian_scheduler.job_triggers SET state='archived'", "UPDATE plugin_qian_scheduler.job_runs SET status='failed'" })
        {
            await using var update = new NpgsqlCommand(statement + " WHERE tenant_id=@other", db, transaction); update.Parameters.AddWithValue("other", TenantB);
            Assert.Equal(0, await update.ExecuteNonQueryAsync(Ct));
        }
        await using (var alter = new NpgsqlCommand("UPDATE plugin_qian_scheduler.job_triggers SET state='archived' WHERE id=@id", db, transaction))
        { alter.Parameters.AddWithValue("id", b); Assert.Equal(0, await alter.ExecuteNonQueryAsync(Ct)); }
        await using var forge = new NpgsqlCommand("INSERT INTO plugin_qian_scheduler.job_triggers(tenant_id,id,state,next_run_at,available_at,run_count,generation) VALUES (@tenant,@id,'ready',now(),now(),0,0)", db, transaction);
        forge.Parameters.AddWithValue("tenant", TenantA); forge.Parameters.AddWithValue("id", b);
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, (await Assert.ThrowsAsync<PostgresException>(() => forge.ExecuteNonQueryAsync(Ct))).SqlState);
        await transaction.RollbackAsync(Ct);
        await using var crossTenant = new NpgsqlCommand("INSERT INTO plugin_qian_scheduler.job_triggers(tenant_id,id,state,next_run_at,available_at,run_count,generation) VALUES (@tenant,@id,'ready',now(),now(),0,0)", db);
        crossTenant.Parameters.AddWithValue("tenant", TenantB); crossTenant.Parameters.AddWithValue("id", Guid.NewGuid());
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, (await Assert.ThrowsAsync<PostgresException>(() => crossTenant.ExecuteNonQueryAsync(Ct))).SqlState);
    }

    [Fact]
    public async Task 专用worker只有领取列权限_SET_ROLE及成员关系不能冒充全局登录()
    {
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(fixture.Config, false, Ct);
        using (producer.Tenant.BeginScope(Guid.ParseExact(TenantA, "D")))
        {
            var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantA));
            await caller.Scheduler().ScheduleOnce(caller, "permission", DateTimeOffset.UtcNow.AddMinutes(1), Target, Parameters, cancellationToken: Ct);
        }
        await using (var worker = new NpgsqlConnection(fixture.Config.Worker))
        {
            await worker.OpenAsync(Ct); await SchedulerWorkerAccess.ValidateConnectionAsync(worker, fixture.Config.WorkerRole, Ct);
            foreach (var sql in new[] { "SELECT * FROM plugin_qian_scheduler.job_definitions", "SELECT * FROM plugin_qian_scheduler.job_runs", "UPDATE plugin_qian_scheduler.job_triggers SET tenant_id='other'", "DELETE FROM plugin_qian_scheduler.job_triggers" })
            { await using var command = new NpgsqlCommand(sql, worker); Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, (await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync(Ct))).SqlState); }
        }
        foreach (var permission in new[] { "UPDATE (tenant_id) ON plugin_qian_scheduler.job_triggers", "UPDATE (next_run_at) ON plugin_qian_scheduler.job_triggers", "INSERT ON plugin_qian_scheduler.job_definitions", "UPDATE (status) ON plugin_qian_scheduler.job_runs" })
        {
            await fixture.Admin($"GRANT {permission} TO \"{fixture.Config.WorkerRole}\";");
            try
            {
                await using var expanded = new NpgsqlConnection(fixture.Config.Worker); await expanded.OpenAsync(Ct);
                await Assert.ThrowsAsync<KunDataMisconfiguredException>(() => SchedulerWorkerAccess.ValidateConnectionAsync(expanded, fixture.Config.WorkerRole, Ct));
            }
            finally { await fixture.Admin($"REVOKE {permission} FROM \"{fixture.Config.WorkerRole}\";"); }
        }
        await fixture.Admin($"GRANT \"{fixture.Config.WorkerRole}\" TO qian_app;");
        try
        {
            await using var db = new NpgsqlConnection(fixture.Config.Application); await db.OpenAsync(Ct);
            await using (var set = new NpgsqlCommand($"SET ROLE \"{fixture.Config.WorkerRole}\"", db)) await set.ExecuteNonQueryAsync(Ct);
            await Assert.ThrowsAsync<KunDataMisconfiguredException>(() => SchedulerWorkerAccess.ValidateConnectionAsync(db, fixture.Config.WorkerRole, Ct));
            await using var read = new NpgsqlCommand("SELECT count(*) FROM plugin_qian_scheduler.job_triggers", db); Assert.Equal(0L, await read.ExecuteScalarAsync(Ct));
            await using var realWorker = new NpgsqlConnection(fixture.Config.Worker); await realWorker.OpenAsync(Ct);
            await Assert.ThrowsAsync<KunDataMisconfiguredException>(() => SchedulerWorkerAccess.ValidateConnectionAsync(realWorker, fixture.Config.WorkerRole, Ct));
        }
        finally { await fixture.Admin($"REVOKE \"{fixture.Config.WorkerRole}\" FROM qian_app;"); }
        var nonlogin = "sch_nonlogin";
        await fixture.Admin($"CREATE ROLE {nonlogin} NOLOGIN NOINHERIT NOSUPERUSER NOBYPASSRLS;");
        await Assert.ThrowsAsync<PostgresException>(() => fixture.Admin(SchedulerWorkerAccess.CreateMigrationSql(nonlogin)));
    }

    [Fact]
    public async Task 创建时有效但到期前被冻结的租户不执行处理器()
    {
        var config = fixture.LongLease; // 不测到期，用 30 秒租约
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(config, false, Ct);
        using (producer.Tenant.BeginScope(Guid.ParseExact(TenantA, "D")))
        {
            var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantA));
            await caller.Scheduler().ScheduleOnce(caller, "tenant-frozen", DateTimeOffset.UtcNow.AddSeconds(2), Target, Parameters, new JobOptions { MaxAttempts = 1 }, Ct);
        }
        await fixture.Admin($"INSERT INTO public.sch_probe_tenants(id,status) VALUES ('{TenantA}','suspended');");
        await using var worker = await WorkerProcess.Start(config, TenantA);
        var phases = new Dictionary<string, DateTimeOffset> { ["workerReady"] = DateTimeOffset.UtcNow };
        try
        {
            await SchedulerFixture.WaitUntil(async () => await fixture.Scalar<long>("SELECT count(*) FROM plugin_qian_scheduler.job_runs WHERE status='failed'") == 1, fixture.Leases(2, config));
            Assert.Equal(0, await fixture.Scalar<long>("SELECT count(*) FROM public.sch_probe_attempts"));
        }
        catch
        {
            // S1-QA-023 在 CI 上见过这条等待超时、却没有现场。
            phases["failed"] = DateTimeOffset.UtcNow;
            await fixture.CaptureFailure("tenant-frozen", phases, null, [worker]);
            throw;
        }
    }

    [Fact]
    public async Task 超时重试保留runId_重试上限和取消终态不会被旧执行者覆盖()
    {
        var config = fixture.LongLease;
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(config, false, Ct);
        await using var worker = await WorkerProcess.Start(config, TenantA, TenantB);
        // 超时要盖过处理器记下进入的抖动（打开连接、写进入行），否则 CPU 被挤时两次尝试都在记下进入前被取消、:215 无行可比；
        // 处理器仍延迟 5 秒，每次都超时（S1-QA-023）。每次尝试 = 一次领取与准备（不超过一份租约）+ 超时。
        var timeout = TimeSpan.FromSeconds(2);
        using (producer.Tenant.BeginScope(Guid.ParseExact(TenantA, "D")))
        {
            var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantA));
            await caller.Scheduler().ScheduleOnce(caller, "timeout", DateTimeOffset.UtcNow, Target, JsonSerializer.SerializeToElement(new { mode = "normal", delayMs = 5000 }),
                new JobOptions { Timeout = timeout, RetryDelay = TimeSpan.Zero, MaxAttempts = 2 }, Ct);
        }
        await SchedulerFixture.WaitUntil(async () => await fixture.Scalar<long>("SELECT count(*) FROM plugin_qian_scheduler.job_runs WHERE status='failed' AND error_class='timeout' AND attempts=2") == 1, 2 * (fixture.Leases(1, config) + timeout));
        Assert.Equal(0, await fixture.Scalar<long>("SELECT count(*) FROM public.sch_probe_effects"));
        Assert.Equal(1, await fixture.Scalar<long>("SELECT count(DISTINCT run_id) FROM public.sch_probe_attempts"));
        using (producer.Tenant.BeginScope(Guid.ParseExact(TenantB, "D")))
        {
            var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantB));
            await caller.Scheduler().ScheduleOnce(caller, "cancel-running", DateTimeOffset.UtcNow, Target, JsonSerializer.SerializeToElement(new { mode = "normal", delayMs = 15000 }), cancellationToken: Ct);
            await SchedulerFixture.WaitUntil(async () => await fixture.Scalar<long>($"SELECT count(*) FROM public.sch_probe_attempts WHERE tenant_id='{TenantB}'") == 1, fixture.Leases(2, config));
            Assert.True(await caller.Scheduler().Cancel(caller, "cancel-running", Ct));
        }
        // 给旧执行者至少一个半心跳周期去发现取消，再看终态没被覆盖。
        await Task.Delay(fixture.Leases(0.5, config), Ct);
        Assert.Equal(1, await fixture.Scalar<long>("SELECT count(*) FROM plugin_qian_scheduler.job_runs WHERE status='cancelled'"));
        Assert.Equal(0, await fixture.Scalar<long>("SELECT count(*) FROM public.sch_probe_effects"));
    }

    [Fact]
    public async Task misfire跳过不调用_补一次合并积压_补齐按计划发生点且同定义不重叠()
    {
        var config = fixture.LongLease; // 不测到期，用 30 秒租约
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(config, false, Ct);
        using (producer.Tenant.BeginScope(Guid.ParseExact(TenantA, "D")))
        {
            var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantA)); var past = DateTimeOffset.UtcNow.AddSeconds(-5);
            await caller.Scheduler().ScheduleOnce(caller, "skip", past, Target, Parameters, new JobOptions { Misfire = MisfirePolicy.Skip, MisfireGrace = TimeSpan.Zero }, Ct);
            await caller.Scheduler().ScheduleInterval(caller, "fire-once", past, TimeSpan.FromHours(1), Target, Parameters, new JobOptions { Misfire = MisfirePolicy.FireOnce }, Ct);
            await caller.Scheduler().ScheduleInterval(caller, "catch-up", past, TimeSpan.FromSeconds(1), Target, JsonSerializer.SerializeToElement(new { mode = "normal", delayMs = 50 }), new JobOptions { Misfire = MisfirePolicy.CatchUp }, Ct);
        }
        await using var a = await WorkerProcess.Start(config, TenantA); await using var b = await WorkerProcess.Start(config, TenantA);
        await SchedulerFixture.WaitUntil(async () => await fixture.Scalar<long>("SELECT count(*) FROM plugin_qian_scheduler.job_runs r JOIN plugin_qian_scheduler.job_definitions d ON r.tenant_id=d.tenant_id AND r.trigger_id=d.id WHERE d.business_key='catch-up' AND r.status='succeeded'") >= 4, fixture.Leases(3, config));
        using (producer.Tenant.BeginScope(Guid.ParseExact(TenantA, "D")))
        { var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantA)); await caller.Scheduler().Cancel(caller, "catch-up", Ct); }
        Assert.Equal(1, await fixture.Scalar<long>("SELECT count(*) FROM plugin_qian_scheduler.job_runs WHERE status='skipped'"));
        Assert.Equal(1, await fixture.Scalar<long>("SELECT count(*) FROM plugin_qian_scheduler.job_runs r JOIN plugin_qian_scheduler.job_definitions d ON r.tenant_id=d.tenant_id AND r.trigger_id=d.id WHERE d.business_key='fire-once'"));
        Assert.Equal(0, await fixture.Scalar<long>("SELECT count(*) FROM public.sch_probe_attempts a JOIN public.sch_probe_attempts b ON a.id<b.id JOIN plugin_qian_scheduler.job_runs ra ON ra.id=a.run_id JOIN plugin_qian_scheduler.job_runs rb ON rb.id=b.run_id WHERE ra.trigger_id=rb.trigger_id AND a.finished>b.started"));
    }

    // 停机窗口：Kun.Runtime 卸载时先关闭上下文、再逆序跑效果，worker 的 DisposeAsync 在效果里才取消 _stop。
    // 下面的 worker 在进程内起，上下文换成 ClosingPluginContext 按开关关闭，不靠竞争窗口；租约 3 秒，等待都按租约份数给上限。
    // 关闭的那个 worker 不得有 Warning 及以上的日志；只有「上下文开着、服务缺失」那条照旧按错误记。
    private const int StopLease = 3;
    private static TimeSpan StopLeases(double n) => TimeSpan.FromSeconds(StopLease * n);
    private SchedulerSettings StopSettings => new(fixture.Config.WorkerRole, StopLease, 1);
    private Task<long> Count(string from) => fixture.Scalar<long>("SELECT count(*) FROM " + from);
    private static async Task<Guid> ScheduleNow(ProbeHost producer, string key, JobOptions? options = null)
    {
        using var scope = producer.Tenant.BeginScope(Guid.ParseExact(TenantA, "D"));
        var caller = producer.Caller.Scoped(new ScopeKey("tenant", TenantA));
        return (await caller.Scheduler().ScheduleOnce(caller, key, DateTimeOffset.UtcNow, Target, Parameters, options, Ct)).Id;
    }
    // 等触发器被领到，返回这次执行的 run id（只用于执行会被放下、触发器仍指向它的用例）。
    private async Task<Guid> ClaimedRun(Guid trigger)
    {
        await SchedulerFixture.WaitUntil(async () => await Count($"plugin_qian_scheduler.job_triggers WHERE id='{trigger}' AND active_run_id IS NOT NULL") == 1, StopLeases(2));
        return await fixture.Scalar<Guid>($"SELECT active_run_id FROM plugin_qian_scheduler.job_triggers WHERE id='{trigger}'");
    }
    // 等这次执行在关闭的 worker 的日志里有了下文，再看日志级别：放下的执行只记 Debug「留给租约恢复」。
    private static Task Mentioned(ClosingPluginContext context, Guid run) => SchedulerFixture.WaitUntil(() => Task.FromResult(context.State.Log.Has(run.ToString())), StopLeases(2));
    private async Task TakeOver(ProbeHost producer, Guid run)
    {
        await using var takeover = new SchedulerWorker(producer.Caller, producer.Tenant, StopSettings);
        takeover.Start();
        await SchedulerFixture.WaitUntil(async () => await Count($"plugin_qian_scheduler.job_runs WHERE id='{run}' AND status='succeeded'") == 1, StopLeases(4));
    }

    [Fact]
    public async Task 停机_上下文关闭后领取循环安静退出_不再领新触发器()
    {
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(fixture.Config, false, Ct);
        var context = new ClosingPluginContext(producer.Caller, producer.Tenant);
        await using (var worker = new SchedulerWorker(context, producer.Tenant, StopSettings))
        {
            await ScheduleNow(producer, "stop-claim-in-flight");
            int closedAt;
            await using (var blocker = new NpgsqlConnection(fixture.Postgres.ConnectionString))
            {
                await blocker.OpenAsync(Ct);
                await using var transaction = await blocker.BeginTransactionAsync(Ct);
                await using var command = new NpgsqlCommand("LOCK TABLE plugin_qian_scheduler.job_triggers IN SHARE MODE", blocker, transaction);
                await command.ExecuteNonQueryAsync(Ct);
                worker.Start();
                // 领取 UPDATE 已经发到 PostgreSQL，正等这条连接的表锁；此刻关闭上下文再让 UPDATE 继续。
                await SchedulerFixture.WaitUntil(async () => await fixture.Scalar<bool>($"SELECT EXISTS (SELECT 1 FROM pg_stat_activity WHERE {blocker.ProcessID} = ANY(pg_blocking_pids(pid)))"), StopLeases(2));
                context.State.Close();
                closedAt = context.State.DataRequests;
                await transaction.CommitAsync(Ct);
            }
            await ScheduleNow(producer, "stop-claim");
            await Task.Delay(StopLeases(1), Ct); // 三个领取周期
            Assert.Equal(0, await Count("plugin_qian_scheduler.job_triggers WHERE generation>0"));
            Assert.Equal(0, await Count("plugin_qian_scheduler.job_runs"));
            // 关闭后至多再取一次数据服务；继续轮询会在这段时间内多次取用。
            Assert.InRange(context.State.DataRequests - closedAt, 0, 1);
        }
        Assert.Empty(context.State.Log.Warnings);
        // 等锁的和关闭后新增的两个触发器仍可由正常 worker 领取。
        await using (var normal = new SchedulerWorker(producer.Caller, producer.Tenant, StopSettings))
        {
            normal.Start();
            await SchedulerFixture.WaitUntil(async () => await Count("plugin_qian_scheduler.job_runs WHERE status='succeeded'") == 2, StopLeases(3));
        }
    }

    [Fact]
    public async Task 停机_领到后BeginAsync前碰到关闭_不建run不记attempt_接管后attempts为1()
    {
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(fixture.Config, false, Ct);
        var id = await ScheduleNow(producer, "stop-begin");
        var context = new ClosingPluginContext(producer.Caller, producer.Tenant) { State = { CloseOnBegin = true } };
        Guid run;
        await using (var worker = new SchedulerWorker(context, producer.Tenant, StopSettings))
        {
            worker.Start();
            run = await ClaimedRun(id);
            await Mentioned(context, run);
        }
        Assert.Empty(context.State.Log.Warnings);
        Assert.True(context.State.Log.Has("留给租约恢复"));
        // 领到了（代次 1），但 run 没建、attempts 没记，处理器没进。
        Assert.Equal(1, await Count($"plugin_qian_scheduler.job_triggers WHERE id='{id}' AND generation=1"));
        Assert.Equal(0, await Count("plugin_qian_scheduler.job_runs"));
        // 租约到期后正常 worker 接管同一 run：这是它的第一次执行，attempts 1。
        await TakeOver(producer, run);
        Assert.Equal(1, await Count($"plugin_qian_scheduler.job_runs WHERE id='{run}' AND attempts=1 AND generation=2"));
        Assert.Equal(1, await Count("public.sch_probe_attempts"));
    }

    [Fact]
    public async Task 停机_BeginAsync已提交后解析时碰到关闭_不写终态_留给接管()
    {
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(fixture.Config, false, Ct);
        var id = await ScheduleNow(producer, "stop-resolve");
        var context = new ClosingPluginContext(producer.Caller, producer.Tenant) { State = { CloseOnFirstScoped = true } };
        Guid run;
        await using (var worker = new SchedulerWorker(context, producer.Tenant, StopSettings))
        {
            worker.Start();
            run = await ClaimedRun(id);
            await Mentioned(context, run);
        }
        Assert.Empty(context.State.Log.Warnings);
        Assert.True(context.State.Log.Has("留给租约恢复"));
        // BeginAsync 已提交（run 已建、attempts 1），终态与错误类都没写；处理器没进。
        Assert.Equal(1, await Count($"plugin_qian_scheduler.job_runs WHERE id='{run}' AND status='running' AND attempts=1 AND error_class IS NULL"));
        Assert.Equal(0, await Count("public.sch_probe_attempts"));
        // 租约到期后正常 worker 接管同一 run：attempts 2、成功、处理器进一次。
        await TakeOver(producer, run);
        Assert.Equal(1, await Count($"plugin_qian_scheduler.job_runs WHERE id='{run}' AND attempts=2"));
        Assert.Equal(1, await Count("public.sch_probe_attempts"));
    }

    [Fact]
    public async Task 停机_调用处理器时上下文已关闭_能力绑定报NOT_FOUND也不写failed_留给接管()
    {
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(fixture.Config, false, Ct);
        var id = await ScheduleNow(producer, "stop-handler");
        var context = new ClosingPluginContext(producer.Caller, producer.Tenant);
        // 解析之后、调处理器之前关闭：真实的能力绑定经已关闭的调用方解析自己，把 ObjectDisposedException 换成 NOT_FOUND 抛出。
        // 停机分支靠探上下文判断，不看异常类型。
        context.State.Handler = async (real, caller, input, ct) => { context.State.Close(); return await real.InvokeAsync(caller, input, ct); };
        Guid run;
        await using (var worker = new SchedulerWorker(context, producer.Tenant, StopSettings))
        {
            worker.Start();
            run = await ClaimedRun(id);
            await Mentioned(context, run);
        }
        Assert.Empty(context.State.Log.Warnings);
        Assert.True(context.State.Log.Has("留给租约恢复"));
        Assert.Equal(1, await Count($"plugin_qian_scheduler.job_runs WHERE id='{run}' AND status='running' AND attempts=1 AND error_class IS NULL"));
        Assert.Equal(0, await Count("public.sch_probe_attempts"));
    }

    [Fact]
    public async Task 停机_处理器正常返回时上下文已关闭_照常提交成功()
    {
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(fixture.Config, false, Ct);
        var id = await ScheduleNow(producer, "stop-complete");
        var context = new ClosingPluginContext(producer.Caller, producer.Tenant);
        context.State.Handler = async (real, caller, input, ct) => { var result = await real.InvokeAsync(ClosingPluginContext.Unwrap(caller), input, ct); context.State.Close(); return result; };
        await using (var worker = new SchedulerWorker(context, producer.Tenant, StopSettings))
        {
            worker.Start();
            // 提交成功，或者 worker 记下了 Warning 以上的日志，就往下看。
            await SchedulerFixture.WaitUntil(async () => await Count("plugin_qian_scheduler.job_runs WHERE status='succeeded'") == 1 || context.State.Log.Warnings.Count > 0, StopLeases(2));
        }
        Assert.Empty(context.State.Log.Warnings);
        Assert.Equal(1, await Count("plugin_qian_scheduler.job_runs WHERE status='succeeded' AND attempts=1"));
        Assert.Equal(1, await Count($"plugin_qian_scheduler.job_triggers WHERE id='{id}' AND state='archived' AND active_run_id IS NULL"));
        Assert.Equal(1, await Count("public.sch_probe_attempts"));
    }

    [Fact]
    public async Task 停机_处理器关闭上下文后跨过整份租约才返回_续租不断_不被接管_照常提交()
    {
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(fixture.Config, false, Ct);
        var id = await ScheduleNow(producer, "stop-renew");
        var context = new ClosingPluginContext(producer.Caller, producer.Tenant);
        // 关闭后再等 1.5 份租约（四个续租周期）：续租若跟着关闭断掉，租约早已到期，旁边的正常 worker 会接管。
        context.State.Handler = async (real, caller, input, ct) =>
        {
            var result = await real.InvokeAsync(ClosingPluginContext.Unwrap(caller), input, ct);
            context.State.Close();
            await Task.Delay(StopLeases(1.5), ct);
            return result;
        };
        await using var rival = new SchedulerWorker(producer.Caller, producer.Tenant, StopSettings);
        await using (var worker = new SchedulerWorker(context, producer.Tenant, StopSettings))
        {
            worker.Start();
            await SchedulerFixture.WaitUntil(async () => await Count("public.sch_probe_attempts") == 1, StopLeases(2));
            rival.Start();
            await SchedulerFixture.WaitUntil(async () => await Count("plugin_qian_scheduler.job_runs WHERE status='succeeded'") == 1, StopLeases(3));
        }
        Assert.Empty(context.State.Log.Warnings);
        Assert.Equal(1, await Count("plugin_qian_scheduler.job_runs WHERE status='succeeded' AND attempts=1 AND generation=1"));
        Assert.Equal(1, await Count($"plugin_qian_scheduler.job_triggers WHERE id='{id}' AND state='archived' AND generation=1"));
        Assert.Equal(1, await Count("public.sch_probe_attempts"));
    }

    [Fact]
    public async Task 停机分支只认关闭_上下文开着而数据服务缺失时照旧按错误处理()
    {
        await fixture.Reset(); await using var producer = await ProbeHost.CreateAsync(fixture.Config, false, Ct);
        var id = await ScheduleNow(producer, "missing-data", new JobOptions { MaxAttempts = 1 });
        var context = new ClosingPluginContext(producer.Caller, producer.Tenant);
        // 处理器执行时数据组件的注册没了（条目回到 Pending 的那段时间）：上下文开着，TryInject 返回 null、不抛，这不是停机。
        context.State.Handler = (_, _, _, _) => { context.State.DataMissing = true; throw new ServiceNotReadyException(); };
        await using (var worker = new SchedulerWorker(context, producer.Tenant, StopSettings))
        {
            worker.Start();
            // 执行记 failed、照常收尾（收尾用 worker 创建时取得的数据服务）；领取循环记错误后继续下一轮，不当停机退出。
            await SchedulerFixture.WaitUntil(async () => await Count("plugin_qian_scheduler.job_runs WHERE status='failed'") == 1 && context.State.Log.Has("调度领取失败"), StopLeases(2));
            var requests = context.State.DataRequests;
            await SchedulerFixture.WaitUntil(() => Task.FromResult(context.State.DataRequests >= requests + 2), StopLeases(1));
        }
        Assert.Equal(1, await Count("plugin_qian_scheduler.job_runs WHERE status='failed' AND attempts=1 AND error_class='ServiceNotReadyException'"));
        Assert.Equal(1, await Count($"plugin_qian_scheduler.job_triggers WHERE id='{id}' AND state='archived'"));
        Assert.False(context.State.Log.Has("留给租约恢复"));
    }
}
