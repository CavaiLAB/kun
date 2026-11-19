using Kun.EntityFrameworkCore;
using Kun.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Kun.Errors;

namespace Kun.Authorization.Rbac.Tests;

public sealed partial class PermissionCatalogPersistenceTests
{
    [Fact]
    public async Task 组件默认策略安装后已有Owner获得三项授权_使用者只读_另一租户不继承()
    {
        var tenant = new AsyncLocalTenantContext(); using var host = Host(tenant); await host.StartAsync(Ct); await Prepare(host);
        await Store(host).PutAsync(Catalog(), Ct);
        var roles = new PostgresTenantRoleStore(host.Services.GetRequiredService<IDataService>(), tenant);
        var anonymous = await Assert.ThrowsAsync<ErrorException>(() => roles.EnsureBuiltinsAsync("operator", Ct)); Assert.Equal("UNAUTHENTICATED", anonymous.Error.Code.Code);
        using (tenant.BeginScope(Tenants.A))
        {
            await roles.EnsureBuiltinsAsync("operator", Ct);
            await roles.AssignAsync("tenant.owner", "user", "owner-a", "operator", Ct);
            await roles.AssignAsync("tenant.user", "user", "user-a", "operator", Ct);
            Assert.Empty(await roles.ReadDirectUserPermissionsAsync("owner-a", Ct));
            await roles.InstallDefaultsAsync("qian.sample.crm", "1.0.0", null, "operator", Ct);
            Assert.Equal(["qian.sample.crm:customer:export", "qian.sample.crm:customer:read", "qian.sample.crm:customer:write"], await roles.ReadDirectUserPermissionsAsync("owner-a", Ct));
            Assert.Equal(["qian.sample.crm:customer:read"], await roles.ReadDirectUserPermissionsAsync("user-a", Ct));
            await roles.InstallDefaultsAsync("qian.sample.crm", "1.0.0", null, "operator", Ct);
            await using var session = await host.Services.GetRequiredService<IDataService>().OpenAsync<AuthzDbContext>(Ct);
            Assert.Equal(4, await session.Context.Roles.CountAsync(Ct)); Assert.Equal(2, await session.Context.RoleAssignments.CountAsync(Ct));
            Assert.Equal(4, await session.Context.Grants.CountAsync(Ct)); Assert.Equal(1, (await session.Context.PolicyInstalls.SingleAsync(Ct)).Revision);
        }
        using (tenant.BeginScope(Tenants.B))
        {
            await roles.EnsureBuiltinsAsync("operator", Ct);
            await roles.AssignAsync("tenant.owner", "user", "owner-a", "operator", Ct);
            Assert.Empty(await roles.ReadDirectUserPermissionsAsync("owner-a", Ct));
        }
        await using var app = new NpgsqlConnection(postgres.AppConnectionString); await app.OpenAsync(Ct);
        await using (var none = new NpgsqlCommand("SELECT count(*) FROM plugin_qian_authz.roles", app)) Assert.Equal(0L, await none.ExecuteScalarAsync(Ct));
        await using (var set = new NpgsqlCommand("SELECT set_config('app.current_tenant_id', @tenant, false)", app)) { set.Parameters.AddWithValue("tenant", Tenants.AText); await set.ExecuteNonQueryAsync(Ct); }
        await using (var own = new NpgsqlCommand("SELECT array_agg(DISTINCT tenant_id) FROM plugin_qian_authz.roles", app)) Assert.Equal(Tenants.AText, Assert.Single((string[])(await own.ExecuteScalarAsync(Ct))!));
        await using var cross = new NpgsqlCommand("UPDATE plugin_qian_authz.roles SET tenant_id = @other WHERE tenant_id = @own", app);
        cross.Parameters.AddWithValue("other", Tenants.BText);
        cross.Parameters.AddWithValue("own", Tenants.AText);
        var denied = await Assert.ThrowsAsync<PostgresException>(() => cross.ExecuteNonQueryAsync(Ct)); Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, denied.SqlState);
    }

    [Fact]
    public async Task 同一主体在两个租户的直接角色不同_有效期在服务端真实查询中生效()
    {
        var tenant = new AsyncLocalTenantContext(); using var host = Host(tenant); await host.StartAsync(Ct); await Prepare(host); await Store(host).PutAsync(Catalog(), Ct);
        var roles = new PostgresTenantRoleStore(host.Services.GetRequiredService<IDataService>(), tenant);
        foreach (var id in new[] { Tenants.AText, Tenants.BText })
        {
            using var scope = tenant.BeginScope(Guid.ParseExact(id, "D")); await roles.EnsureBuiltinsAsync("operator", Ct);
            await roles.AssignAsync(id == Tenants.AText ? "tenant.owner" : "tenant.user", "user", "same-person", "operator", Ct);
            await roles.InstallDefaultsAsync("qian.sample.crm", "1.0.0", null, "operator", Ct);
        }
        using (tenant.BeginScope(Tenants.A)) Assert.Equal(3, (await roles.ReadDirectUserPermissionsAsync("same-person", Ct)).Count);
        using (tenant.BeginScope(Tenants.B))
        {
            Assert.Equal(["qian.sample.crm:customer:read"], await roles.ReadDirectUserPermissionsAsync("same-person", Ct));
            await roles.AssignAsync("tenant.user", "user", "same-person", "operator", Ct, validFrom: DateTimeOffset.UtcNow.AddHours(1));
            Assert.Empty(await roles.ReadDirectUserPermissionsAsync("same-person", Ct));
            await roles.AssignAsync("tenant.user", "user", "same-person", "operator", Ct, validTo: DateTimeOffset.UtcNow.AddHours(-1));
            Assert.Empty(await roles.ReadDirectUserPermissionsAsync("same-person", Ct));
            await roles.AssignAsync("tenant.user", "user", "same-person", "operator", Ct);
            Assert.Single(await roles.ReadDirectUserPermissionsAsync("same-person", Ct));
        }
    }

    [Fact]
    public async Task 升级授权写入失败会恢复旧授权与版本_并发安装同版本不重复授予()
    {
        var firstTenant = new AsyncLocalTenantContext(); var secondTenant = new AsyncLocalTenantContext();
        using var first = Host(firstTenant); using var second = Host(secondTenant); await first.StartAsync(Ct); await second.StartAsync(Ct); await Prepare(first);
        await Store(first).PutAsync(Catalog(), Ct); await Store(first).PutAsync(Catalog("2.0.0", "critical"), Ct);
        var one = new PostgresTenantRoleStore(first.Services.GetRequiredService<IDataService>(), firstTenant);
        var two = new PostgresTenantRoleStore(second.Services.GetRequiredService<IDataService>(), secondTenant);
        using var firstScope = firstTenant.BeginScope(Tenants.A); using var secondScope = secondTenant.BeginScope(Tenants.A);
        await one.EnsureBuiltinsAsync("operator", Ct); await one.AssignAsync("tenant.owner", "user", "owner", "operator", Ct);
        await Task.WhenAll(one.InstallDefaultsAsync("qian.sample.crm", "1.0.0", null, "operator", Ct), two.InstallDefaultsAsync("qian.sample.crm", "1.0.0", null, "operator", Ct));
        await using var admin = new NpgsqlConnection(postgres.ConnectionString); await admin.OpenAsync(Ct);
        await using (var trigger = new NpgsqlCommand("""
            CREATE FUNCTION plugin_qian_authz.reject_grant() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN RAISE EXCEPTION 'fixture reject grant'; END $$;
            CREATE TRIGGER reject_grant BEFORE INSERT ON plugin_qian_authz.grants FOR EACH ROW EXECUTE FUNCTION plugin_qian_authz.reject_grant();
            """, admin)) await trigger.ExecuteNonQueryAsync(Ct);
        await Assert.ThrowsAsync<DbUpdateException>(() => one.InstallDefaultsAsync("qian.sample.crm", "2.0.0", "1.0.0", "operator", Ct));
        await using var session = await first.Services.GetRequiredService<IDataService>().OpenAsync<AuthzDbContext>(Ct);
        var active = await session.Context.PolicyInstalls.SingleAsync(Ct); Assert.Equal("1.0.0", active.Version); Assert.Equal(1, active.Revision);
        Assert.Equal(4, await session.Context.Grants.CountAsync(Ct)); Assert.Equal(3, (await one.ReadDirectUserPermissionsAsync("owner", Ct)).Count);
        var stale = await Assert.ThrowsAsync<ErrorException>(() => one.InstallDefaultsAsync("qian.sample.crm", "2.0.0", "0.9.0", "operator", Ct)); Assert.Equal("CONFLICT", stale.Error.Code.Code);
    }

    [Fact]
    public async Task 首个宿主租户作用域退出后_另一个宿主仍使用自己的上下文查询角色()
    {
        var firstTenant = new AsyncLocalTenantContext(); var secondTenant = new AsyncLocalTenantContext();
        using var first = Host(firstTenant); using var second = Host(secondTenant); await first.StartAsync(Ct); await second.StartAsync(Ct); await Prepare(first);
        await Store(first).PutAsync(Catalog(), Ct);
        var one = new PostgresTenantRoleStore(first.Services.GetRequiredService<IDataService>(), firstTenant);
        var two = new PostgresTenantRoleStore(second.Services.GetRequiredService<IDataService>(), secondTenant);
        using (firstTenant.BeginScope(Tenants.A))
        {
            await one.EnsureBuiltinsAsync("operator", Ct); await one.AssignAsync("tenant.owner", "user", "owner", "operator", Ct);
            await one.InstallDefaultsAsync("qian.sample.crm", "1.0.0", null, "operator", Ct);
            Assert.Equal(3, (await one.ReadDirectUserPermissionsAsync("owner", Ct)).Count);
        }
        using (secondTenant.BeginScope(Tenants.A)) Assert.Equal(3, (await two.ReadDirectUserPermissionsAsync("owner", Ct)).Count);
        using (firstTenant.BeginScope(Tenants.B))
        using (secondTenant.BeginScope(Tenants.A))
        {
            Assert.Empty(await one.ReadDirectUserPermissionsAsync("owner", Ct));
            Assert.Equal(3, (await two.ReadDirectUserPermissionsAsync("owner", Ct)).Count);
        }
    }

    [Fact]
    public async Task 保留可空平台角色列_普通租户与缺省上下文均不能读取平台行()
    {
        var tenant = new AsyncLocalTenantContext(); using var host = Host(tenant); await host.StartAsync(Ct); await Prepare(host);
        await using var admin = new NpgsqlConnection(postgres.ConnectionString); await admin.OpenAsync(Ct);
        await using (var seed = new NpgsqlCommand("INSERT INTO plugin_qian_authz.roles(id, tenant_id, code, builtin) VALUES (@id, NULL, 'platform.superadmin', true)", admin))
        { seed.Parameters.AddWithValue("id", Guid.CreateVersion7()); await seed.ExecuteNonQueryAsync(Ct); }
        await using var app = new NpgsqlConnection(postgres.AppConnectionString); await app.OpenAsync(Ct);
        await using (var read = new NpgsqlCommand("SELECT count(*) FROM plugin_qian_authz.roles", app)) Assert.Equal(0L, await read.ExecuteScalarAsync(Ct));
        using (tenant.BeginScope(Tenants.A))
        {
            await using var session = await host.Services.GetRequiredService<IDataService>().OpenAsync<AuthzDbContext>(Ct);
            Assert.Empty(await session.Context.Roles.ToArrayAsync(Ct));
        }
    }
}
