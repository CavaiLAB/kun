using System.Net;
using System.Text.Json;
using Qian.Events;
using Qian.Identity.Abstractions;
using Kun.Tenancy;
using Qian.Testing.Containers;
using static Qian.Identity.Server.Tests.AccountSupport;

namespace Qian.Identity.Server.Tests;

// 租户管理员新增成员（S1-IDP-017，R163）：HTTP 入口 POST /api/directory/members 在真实宿主（受限应用角色 + 强制 RLS、同事务 Outbox）上的验收。
// 本宿主不装授权与审计组件，所以 granted_roles 为空、不写审计；初始角色 tenant.user（R158）与审计在 CRM 参考宿主的
// IdentityAuthzTests「新增成员」用例里经真实授权与审计组件验。
public sealed class DirectoryMemberAddTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    internal const string ContractPath = "contracts/identity/v1/directory-members.openapi.json";
    private const string Initial = "Initial-Pass-2026";
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static string Code(JsonElement body) => body.GetProperty("code").GetString()!;
    private static string E164(string phone) => "+86" + phone;
    private static (string Code, string? Pointer) Field(JsonElement body)
    {
        var error = Assert.Single(body.GetProperty("errors").EnumerateArray());
        return (error.GetProperty("code").GetString()!, error.GetProperty("pointer").GetString());
    }

    private static Task<(HttpStatusCode Status, JsonElement Body, HttpResponseMessage Raw)> Add(AccountsHost.Browser admin, object body, bool csrf = true) =>
        admin.Json(HttpMethod.Post, "api/directory/members", body, csrf);

    // 一个租户管理员成员，并以他登录该租户。
    private static async Task<(AccountsHost.Browser Browser, Person Person)> Administrator(AccountsHost.Running host, TenantDescriptor tenant, string name = "管理员")
    {
        var (person, phone) = await CreatePerson(host, name);
        await Join(host, tenant, person.Id, name, admin: true);
        var browser = host.NewBrowser();
        var login = await browser.Login(phone, Password, tenant: tenant.Id);
        Assert.Equal("authenticated", login.Body.GetProperty("status").GetString());
        return (browser, person);
    }

    // 库内事实（以超级用户绕过 RLS，只作断言）：自然人数、持某标识的自然人数、某租户的成员数与目录修订。
    private static async Task<(string Persons, string Identifiers, string Members, string Revision, int Events)> Facts(AccountsHost.Running host, TenantDescriptor tenant, string identifier) => (
        await host.Text("SELECT count(*) FROM plugin_qian_identity.persons"),
        await host.Text("SELECT count(*) FROM plugin_qian_identity.person_identifiers WHERE value = @v", ("v", identifier)),
        await host.Text("SELECT count(*) FROM plugin_qian_identity.memberships WHERE tenant_id = @t", ("t", tenant.Id.ToString("D"))),
        await host.Text("SELECT coalesce(max(revision), 0)::text FROM plugin_qian_identity.directory_revisions WHERE tenant_id = @t", ("t", tenant.Id.ToString("D"))),
        (await DirectoryEventTests.DirectoryEvents(host, tenant.Id)).Count);

    [Fact]
    public async Task 管理员新增成员_新号码新建自然人_首登强制改密后能登录本租户_响应日志与事件不含明文密码_同事务发出member_created()
    {
        await using var host = await AccountsHost.Start(postgres); var tenant = host.Tenants.Add("member-add");
        var (admin, adminPerson) = await Administrator(host, tenant);
        var phone = NextPhone();
        var added = await Add(admin, new { identifier = phone, display_name = "新成员", initial_password = Initial, account = "Newbie" });
        Assert.Equal(HttpStatusCode.OK, added.Status);
        Assert.Contains("no-store", added.Raw.Headers.CacheControl!.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(Initial, added.Body.GetRawText(), StringComparison.Ordinal);
        var membership = added.Body.GetProperty("membership");
        var memberId = membership.GetProperty("id").GetGuid(); var personId = membership.GetProperty("person_id").GetGuid();
        Assert.Equal((tenant.Id, "新成员", "active", false, "newbie"), (membership.GetProperty("tenant_id").GetGuid(), membership.GetProperty("display_name").GetString(),
            membership.GetProperty("status").GetString(), membership.GetProperty("is_tenant_administrator").GetBoolean(), membership.GetProperty("account").GetString()));
        Assert.True(added.Body.GetProperty("person_created").GetBoolean()); Assert.False(added.Body.GetProperty("restored").GetBoolean());
        Assert.Empty(added.Body.GetProperty("granted_roles").EnumerateArray());
        // 真实响应符合发布的成员管理契约。
        Assert.Empty(DirectoryAgentContract.Load(ContractPath).ValidateComponent("AddMemberResult", added.Body));
        // 库内：管理员录入的手机号按未验证保存，首次登录须改密，凭据只有 Argon2id 哈希；登录索引含本租户。
        Assert.Equal("", await host.Text("SELECT coalesce(verified_at::text, '') FROM plugin_qian_identity.person_identifiers WHERE person_id = @p", ("p", personId)));
        Assert.Equal("True", await host.Text("SELECT must_change_password FROM plugin_qian_identity.persons WHERE id = @p", ("p", personId)));
        Assert.StartsWith("$argon2id$", await host.Text("SELECT password_hash FROM plugin_qian_identity.persons WHERE id = @p", ("p", personId)), StringComparison.Ordinal);
        Assert.Equal(tenant.Id.ToString("D"), await host.Text("SELECT array_to_string(tenant_ids, ',') FROM plugin_qian_identity.persons WHERE id = @p", ("p", personId)));
        // 初始密码进密码历史（与开户同口径），之后几次改密都不能改回它。
        Assert.Equal("1", await host.Text("SELECT count(*) FROM plugin_qian_identity.password_history h JOIN plugin_qian_identity.persons p ON p.id = h.person_id " +
            "WHERE h.person_id = @p AND h.hash = p.password_hash", ("p", personId)));
        // 新成员登录：先被引导修改初始密码，此前没有会话；不能「改」成初始密码本身（票据不因此作废），改密后建立本租户会话，授权端点签发授权码。
        var member = host.NewBrowser();
        var login = await member.Login(phone, Initial);
        Assert.Equal("password_change_required", login.Body.GetProperty("status").GetString());
        Assert.False(member.HasAccountCookie); Assert.Null(await member.Code());
        var kept = await member.Json(HttpMethod.Post, "api/identity/login/password", new { ticket = login.Body.GetProperty("ticket").GetString(), new_password = Initial });
        Assert.Equal("PASSWORD_REUSED", Code(kept.Body)); Assert.False(member.HasAccountCookie);
        var changed = await member.Json(HttpMethod.Post, "api/identity/login/password", new { ticket = login.Body.GetProperty("ticket").GetString(), new_password = "Brand-New-Pass-7" });
        Assert.Equal("authenticated", changed.Body.GetProperty("status").GetString());
        var me = await member.Json(HttpMethod.Get, "api/identity/me");
        Assert.Equal((tenant.Id, memberId, personId), (me.Body.GetProperty("tenant_id").GetGuid(), me.Body.GetProperty("membership_id").GetGuid(), me.Body.GetProperty("person_id").GetGuid()));
        Assert.NotNull(await member.Code());
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.NewBrowser().Login(phone, Initial)).Status);
        // 事件：恰一条 directory.member.created.v1，载荷符合 Schema，操作人是发起新增的管理员；Outbox 与日志都不含明文密码。
        var created = Assert.Single(await DirectoryEventTests.DirectoryEvents(host, tenant.Id), envelope => envelope.Payload.GetProperty("membership_id").GetGuid() == memberId);
        Assert.Equal(DirectoryEventIds.MemberCreated, created.Type); DirectoryEventTests.AssertSchema(created);
        Assert.Equal((personId, "active"), (created.Payload.GetProperty("person_id").GetGuid(), created.Payload.GetProperty("status").GetString()));
        Assert.Equal((EventActorKind.User, adminPerson.Id.ToString("D")), (created.Actor.Kind, created.Actor.Id));
        Assert.Equal("0", await host.Text("SELECT count(*) FROM plugin_qian_identity.outbox_messages WHERE payload LIKE @p OR payload LIKE @q", ("p", "%" + Initial + "%"), ("q", "%Brand-New-Pass-7%")));
        Assert.DoesNotContain(host.Logs.Messages, message => message.Contains(Initial, StringComparison.Ordinal) || message.Contains("Brand-New-Pass-7", StringComparison.Ordinal));
    }

    // 比较错误体时去掉每次请求都不同的 trace_id。
    private static string Shape(JsonElement body)
    {
        var node = System.Text.Json.Nodes.JsonNode.Parse(body.GetRawText())!.AsObject();
        node.Remove("trace_id");
        return node.ToJsonString();
    }

    [Fact]
    public async Task 只新建自然人_标识已被占用返回IDENTIFIER_IN_USE且无写入()
    {
        await using var host = await AccountsHost.Start(postgres); var a = host.Tenants.Add("member-taken"); var b = host.Tenants.Add("member-taken-other");
        var (admin, _) = await Administrator(host, a, "甲管理员"); var (otherAdmin, _) = await Administrator(host, b, "乙管理员");
        // 已占用标识的几种来源。
        var (other, otherPhone) = await CreatePerson(host, "乙方老员工", extra: [new IdentifierInput(IdentifierKind.Username, "private.handle", true)]);
        await Join(host, b, other.Id, "乙方老员工", account: "b-side-account");
        var created = NextPhone();
        Assert.Equal(HttpStatusCode.OK, (await Add(otherAdmin, new { identifier = created, display_name = "乙方新人", initial_password = "Other-Side-Pass-1" })).Status);
        var createdPerson = Guid.Parse(await host.Text("SELECT person_id::text FROM plugin_qian_identity.person_identifiers WHERE value = @v", ("v", E164(created))));
        var (regular, regularPhone) = await CreatePerson(host, "甲方在册"); var regularMember = await Join(host, a, regular.Id, "甲方在册");
        var (paused, pausedPhone) = await CreatePerson(host, "甲方停用"); var pausedMember = await Join(host, a, paused.Id, "甲方停用");
        Assert.Equal(HttpStatusCode.OK, (await admin.Json(HttpMethod.Post, $"api/directory/members/{pausedMember.Id:D}/status", new { status = "disabled", reason = "test" })).Status);
        var (_, platformPhone) = await CreatePerson(host, "平台运营", platform: true);
        var (disabled, disabledPhone) = await CreatePerson(host, "已停用"); await host.Accounts.SetPersonStatusAsync(disabled.Id, AccountStatuses.Disabled, "test", Ct);
        var (locked, lockedPhone) = await CreatePerson(host, "已锁定"); await host.Accounts.SetPersonStatusAsync(locked.Id, AccountStatuses.Locked, "test", Ct);
        var occupied = new (string Identifier, string Facts)[]
        {
            (E164(otherPhone), E164(otherPhone)), (otherPhone, E164(otherPhone)), ("Private.Handle", "private.handle"), (created, E164(created)),
            (regularPhone, E164(regularPhone)), (pausedPhone, E164(pausedPhone)), (platformPhone, E164(platformPhone)), (disabledPhone, E164(disabledPhone)), (lockedPhone, E164(lockedPhone))
        };
        var shapes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (identifier, facts) in occupied)
        {
            var before = await Facts(host, a, facts); var beforeB = await Facts(host, b, facts);
            var refused = await Add(admin, new { identifier, display_name = "甲方新员工", initial_password = Initial, account = "jia-new" });
            Assert.Equal((HttpStatusCode.Conflict, "IDENTIFIER_IN_USE", "#/identifier"), (refused.Status, Code(refused.Body), refused.Body.GetProperty("pointer").GetString()));
            shapes.Add(Shape(refused.Body));
            // 错误体只含错误码与位置。
            foreach (var foreign in new[] { "b-side-account", "private.handle", "乙方", "甲方在册", "甲方停用", other.Id.ToString("D"), createdPerson.ToString("D"), regular.Id.ToString("D") })
                Assert.DoesNotContain(foreign, refused.Body.GetRawText(), StringComparison.Ordinal);
            Assert.Equal(before, await Facts(host, a, facts)); Assert.Equal(beforeB, await Facts(host, b, facts));
        }
        Assert.Single(shapes);
        // 已有的自然人保持原样。
        Assert.Equal("0", await host.Text("SELECT count(*) FROM plugin_qian_identity.memberships WHERE tenant_id = @t AND person_id = ANY(@p)",
            ("t", a.Id.ToString("D")), ("p", new[] { other.Id, createdPerson, disabled.Id, locked.Id })));
        Assert.Equal("false|true", await host.Text("SELECT (SELECT must_change_password::text FROM plugin_qian_identity.persons WHERE id = @o) || '|' || (SELECT must_change_password::text FROM plugin_qian_identity.persons WHERE id = @c)",
            ("o", other.Id), ("c", createdPerson)));
        Assert.Equal("authenticated", (await host.NewBrowser().Login(otherPhone, Password, tenant: b.Id)).Body.GetProperty("status").GetString());
        foreach (var identifier in new[] { otherPhone, created, "private.handle" }) Assert.Equal(HttpStatusCode.Unauthorized, (await host.NewBrowser().Login(identifier, Initial)).Status);
        Assert.Equal("disabled", await host.Text("SELECT status FROM plugin_qian_identity.memberships WHERE id = @id", ("id", pausedMember.Id)));
        Assert.Equal(regularMember.Id, Guid.Parse(await host.Text("SELECT id::text FROM plugin_qian_identity.memberships WHERE tenant_id = @t AND person_id = @p", ("t", a.Id.ToString("D")), ("p", regular.Id))));
        // 新用户名照常新建自然人，账号缺省取用户名。
        var fresh = await Add(admin, new { identifier = "QianQi", display_name = "钱七", initial_password = Initial });
        Assert.Equal((HttpStatusCode.OK, true, false, "qianqi"), (fresh.Status, fresh.Body.GetProperty("person_created").GetBoolean(), fresh.Body.GetProperty("restored").GetBoolean(),
            fresh.Body.GetProperty("membership").GetProperty("account").GetString()));
        Assert.NotEqual("", await host.Text("SELECT coalesce(verified_at::text, '') FROM plugin_qian_identity.person_identifiers WHERE value = 'qianqi'"));
    }

    [Fact]
    public async Task 字段错误都带位置且不写入_显示名控制字符与超长_账号格式_初始密码必填与策略_指定账号被占用()
    {
        await using var host = await AccountsHost.Start(postgres); var a = host.Tenants.Add("member-fields");
        var (admin, _) = await Administrator(host, a);
        var (named, _) = await CreatePerson(host, "赵六"); await Join(host, a, named.Id, "赵六", account: "zhaoliu");
        var (_, takenPhone) = await CreatePerson(host, "别处的人");
        var newPhone = NextPhone(); var untouched = await Facts(host, a, E164(newPhone));
        // 显示名：去掉首尾空白后为空、含控制字符、超过 200 字都拒绝；正好 200 字可以（见末尾）。
        foreach (var name in new[] { "   ", "a\nb", "制\u0007表", new string('名', 201) })
        {
            var invalid = await Add(admin, new { identifier = newPhone, display_name = name, initial_password = Initial });
            Assert.Equal((HttpStatusCode.BadRequest, "#/display_name"), (invalid.Status, Field(invalid.Body).Pointer));
        }
        // 标识：邮箱形似、过短与非法字符都按格式不符拒绝。
        foreach (var identifier in new[] { "someone@example.test", "1", "张三" })
        {
            var invalid = await Add(admin, new { identifier, display_name = "非法标识", initial_password = Initial });
            Assert.Equal((HttpStatusCode.BadRequest, "#/identifier"), (invalid.Status, Field(invalid.Body).Pointer));
        }
        // 指定账号：含空格、空串、以符号开头、超长都拒绝。
        foreach (var account in new[] { "bad account", "", "-lead", new string('a', 65) })
        {
            var invalid = await Add(admin, new { identifier = newPhone, display_name = "账号非法", initial_password = Initial, account });
            Assert.Equal((HttpStatusCode.BadRequest, "#/account"), (invalid.Status, Field(invalid.Body).Pointer));
        }
        // 初始密码必填。
        var missing = await Add(admin, new { identifier = newPhone, display_name = "缺密码" });
        Assert.Equal((HttpStatusCode.BadRequest, "INPUT_INVALID", ("REQUIRED", (string?)"#/initial_password")), (missing.Status, Code(missing.Body), Field(missing.Body)));
        var missingTaken = await Add(admin, new { identifier = takenPhone, display_name = "缺密码" });
        Assert.Equal(Shape(missing.Body), Shape(missingTaken.Body));
        var weak = await Add(admin, new { identifier = newPhone, display_name = "弱密码", initial_password = "short" });
        Assert.Equal(((HttpStatusCode)422, "PASSWORD_POLICY_VIOLATION", "#/initial_password"), (weak.Status, Code(weak.Body), weak.Body.GetProperty("pointer").GetString()));
        var taken = await Add(admin, new { identifier = newPhone, display_name = "账号重复", initial_password = Initial, account = "ZhaoLiu" });
        Assert.Equal((HttpStatusCode.Conflict, "ACCOUNT_IN_USE", "#/account"), (taken.Status, Code(taken.Body), taken.Body.GetProperty("pointer").GetString()));
        Assert.Equal(untouched, await Facts(host, a, E164(newPhone)));
        // 指定的账号已被本租户别的成员占用时先报账号：标识没被占用、已被别处占用、是平台身份，响应都相同。
        var (_, platformPhone) = await CreatePerson(host, "平台运营", platform: true);
        foreach (var identifier in new[] { takenPhone, platformPhone, "QianBa" })
        {
            var same = await Add(admin, new { identifier, display_name = "账号重复", initial_password = Initial, account = "ZhaoLiu" });
            Assert.Equal((HttpStatusCode.Conflict, Shape(taken.Body)), (same.Status, Shape(same.Body)));
        }
        Assert.Equal("0", await host.Text("SELECT count(*) FROM plugin_qian_identity.person_identifiers WHERE value = 'qianba'"));
        // 边界：200 字的显示名、首尾空白去掉后入库，账号转小写。
        var longest = new string('名', 200);
        var ok = await Add(admin, new { identifier = newPhone, display_name = "  " + longest + "  ", initial_password = Initial, account = "Mixed.Case" });
        Assert.Equal((HttpStatusCode.OK, longest, "mixed.case"), (ok.Status, ok.Body.GetProperty("membership").GetProperty("display_name").GetString(), ok.Body.GetProperty("membership").GetProperty("account").GetString()));
    }

    [Fact]
    public async Task 达到用户配额返回CARDINALITY_USER_LIMIT且无写入_并发与账号端口建成员都不越过上限()
    {
        await using var host = await AccountsHost.Start(postgres); var tenant = host.Tenants.Add("member-quota", limit: 2); var other = host.Tenants.Add("member-quota-other");
        var (admin, _) = await Administrator(host, tenant);
        // 满额时本租户还有一个已删除的成员：删一个、补一个，名额回到 2/2。
        var (gone, gonePhone) = await CreatePerson(host, "已删除"); var goneMember = await Join(host, tenant, gone.Id, "已删除");
        Assert.Equal(HttpStatusCode.OK, (await admin.Json(HttpMethod.Delete, $"api/directory/members/{goneMember.Id:D}")).Status);
        var (present, presentPhone) = await CreatePerson(host, "已有成员"); await Join(host, tenant, present.Id, "已有成员");
        var (elsewhere, elsewherePhone) = await CreatePerson(host, "别处的用户"); await Join(host, other, elsewhere.Id, "别处的用户");
        var (_, platformPhone) = await CreatePerson(host, "平台运营", platform: true);
        var (disabled, disabledPhone) = await CreatePerson(host, "已停用"); await host.Accounts.SetPersonStatusAsync(disabled.Id, AccountStatuses.Disabled, "test", Ct);
        // 满额返回配额错误，什么都不写。
        var shapes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var phone in new[] { NextPhone(), elsewherePhone, platformPhone, disabledPhone, presentPhone, gonePhone })
        {
            var before = await Facts(host, tenant, E164(phone));
            var full = await Add(admin, new { identifier = phone, display_name = "超额", initial_password = Initial });
            Assert.Equal(((HttpStatusCode)422, "CARDINALITY_USER_LIMIT"), (full.Status, Code(full.Body)));
            shapes.Add(Shape(full.Body));
            Assert.Equal(before, await Facts(host, tenant, E164(phone)));
        }
        Assert.Single(shapes);
        Assert.Equal("disabled|true", await host.Text("SELECT status || '|' || directory_deleted FROM plugin_qian_identity.memberships WHERE id = @id", ("id", goneMember.Id)));
        // 并发：只剩一个名额时三个请求同时到达，恰好一个成功，其余 422；成员计数锁串行判定，不会一起越过上限。
        var roomy = host.Tenants.Add("member-race", limit: 2); var (raceAdmin, _) = await Administrator(host, roomy);
        var racers = await Task.WhenAll(Enumerable.Range(0, 3).Select(index => Add(raceAdmin, new { identifier = NextPhone(), display_name = "并发" + index, initial_password = Initial })));
        Assert.Equal(1, racers.Count(result => result.Status == HttpStatusCode.OK));
        Assert.All(racers.Where(result => result.Status != HttpStatusCode.OK), result => Assert.Equal("CARDINALITY_USER_LIMIT", Code(result.Body)));
        Assert.Equal("2", await host.Text("SELECT count(*) FROM plugin_qian_identity.memberships WHERE tenant_id = @t", ("t", roomy.Id.ToString("D"))));
        // 与账号端口建成员对撞（开户、替换管理员走的端口）：每轮只剩一个名额，新增与端口同时到达，恰好一个成功，人数始终不超过上限。
        for (var round = 0; round < 6; round++)
        {
            var edge = host.Tenants.Add("member-edge-" + round, limit: 2); var (edgeAdmin, _) = await Administrator(host, edge);
            var (joiner, _) = await CreatePerson(host, "端口加入" + round);
            async Task<string> ViaPort()
            {
                try { await Join(host, edge, joiner.Id, "端口加入"); return "OK"; }
                catch (Kun.Errors.ErrorException error) { return error.Error.Code.Code; }
            }
            async Task<string> ViaAdd()
            {
                var result = await Add(edgeAdmin, new { identifier = NextPhone(), display_name = "新增", initial_password = Initial });
                return result.Status == HttpStatusCode.OK ? "OK" : Code(result.Body);
            }
            var outcomes = await Task.WhenAll(ViaAdd(), ViaPort());
            Assert.Equal(["CARDINALITY_USER_LIMIT", "OK"], outcomes.Order(StringComparer.Ordinal));
            Assert.Equal("2", await host.Text("SELECT count(*) FROM plugin_qian_identity.memberships WHERE tenant_id = @t", ("t", edge.Id.ToString("D"))));
        }
    }

    // 加锁顺序（与成员建立相同）：新增成员先取成员计数锁 members:<tenant>，再取目录锁。别的事务持有成员计数锁时，新增等它结束才判定配额。
    [Fact]
    public async Task 新增成员与账号端口共用成员计数锁_锁被占用时等待_释放后照常完成()
    {
        await using var host = await AccountsHost.Start(postgres); var tenant = host.Tenants.Add("member-lock");
        var (admin, _) = await Administrator(host, tenant);
        await using var holder = new Npgsql.NpgsqlConnection(postgres.ConnectionString); await holder.OpenAsync(Ct);
        await using var transaction = await holder.BeginTransactionAsync(Ct);
        await using (var command = new Npgsql.NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtextextended(@key, 0))", holder, transaction))
        {
            command.Parameters.AddWithValue("key", "qian.identity:members:" + tenant.Id.ToString("D"));
            await command.ExecuteNonQueryAsync(Ct);
        }
        var pending = Add(admin, new { identifier = NextPhone(), display_name = "等锁", initial_password = Initial });
        Assert.NotSame(pending, await Task.WhenAny(pending, Task.Delay(TimeSpan.FromSeconds(2), Ct)));
        Assert.Equal("1", await host.Text("SELECT count(*) FROM plugin_qian_identity.memberships WHERE tenant_id = @t", ("t", tenant.Id.ToString("D"))));
        await transaction.CommitAsync(Ct);
        Assert.Equal(HttpStatusCode.OK, (await pending).Status);
    }

    [Fact]
    public async Task 从目录删除即释放名额_被删的人重新新增即恢复原成员_成员id账号与凭据不变_停用的成员仍占名额()
    {
        await using var host = await AccountsHost.Start(postgres); var tenant = host.Tenants.Add("member-restore", limit: 2);
        var (admin, adminPerson) = await Administrator(host, tenant);
        async Task<int> Counted() { using var scope = host.Tenant.BeginScope(tenant.Id.ToString("D")); return await host.Service.CountUsersAsync(Ct); }
        var (person, phone) = await CreatePerson(host, "老员工", extra: [new IdentifierInput(IdentifierKind.Username, "laoyuangong", true)]);
        var original = await Join(host, tenant, person.Id, "老员工", account: "lao");
        Assert.Equal(2, await Counted());
        // 停用不释放名额；删除释放。
        Assert.Equal(HttpStatusCode.OK, (await admin.Json(HttpMethod.Post, $"api/directory/members/{original.Id:D}/status", new { status = "disabled", reason = "test" })).Status);
        Assert.Equal(2, await Counted());
        Assert.Equal("CARDINALITY_USER_LIMIT", Code((await Add(admin, new { identifier = NextPhone(), display_name = "补位", initial_password = Initial })).Body));
        Assert.Equal(HttpStatusCode.OK, (await admin.Json(HttpMethod.Delete, $"api/directory/members/{original.Id:D}")).Status);
        Assert.Equal(1, await Counted());
        var filler = await Add(admin, new { identifier = NextPhone(), display_name = "补位", initial_password = Initial });
        Assert.Equal(HttpStatusCode.OK, filler.Status);
        // 满额时恢复同样按配额拒绝；腾出名额后恢复。
        Assert.Equal("CARDINALITY_USER_LIMIT", Code((await Add(admin, new { identifier = phone, display_name = "老员工回来", initial_password = Initial })).Body));
        Assert.Equal(HttpStatusCode.OK, (await admin.Json(HttpMethod.Delete, $"api/directory/members/{filler.Body.GetProperty("membership").GetProperty("id").GetGuid():D}")).Status);
        // 恢复时指定了不同的账号：拒绝，什么都不改（账号创建后不可改）。
        var before = await Facts(host, tenant, E164(phone));
        var renamed = await Add(admin, new { identifier = phone, display_name = "老员工回来", initial_password = Initial, account = "lao-2" });
        Assert.Equal((HttpStatusCode.BadRequest, "INPUT_INVALID", ("VALUE_NOT_ALLOWED", (string?)"#/account")), (renamed.Status, Code(renamed.Body), Field(renamed.Body)));
        Assert.Equal(before, await Facts(host, tenant, E164(phone)));
        // 自然人被平台停用：返回 409，不恢复；平台恢复启用后可以恢复。
        await host.Accounts.SetPersonStatusAsync(person.Id, AccountStatuses.Disabled, "test", Ct);
        Assert.Equal(("IDENTIFIER_IN_USE", "#/identifier"), (Code((await Add(admin, new { identifier = phone, display_name = "老员工回来", initial_password = Initial })).Body), "#/identifier"));
        await host.Accounts.SetPersonStatusAsync(person.Id, AccountStatuses.Active, "test", Ct);
        Assert.Equal(before, await Facts(host, tenant, E164(phone)));
        // 恢复：同一成员 id 与账号，显示名取这次的值，状态 active；请求里的初始密码不用，他用自己的密码登录本租户。
        var revisionBefore = long.Parse(await host.Text("SELECT identity_revision::text FROM plugin_qian_identity.memberships WHERE id = @id", ("id", original.Id)), System.Globalization.CultureInfo.InvariantCulture);
        var restored = await Add(admin, new { identifier = "LaoYuanGong", display_name = "老员工回来", initial_password = Initial, account = "lao" });
        Assert.Equal(HttpStatusCode.OK, restored.Status);
        Assert.Equal((true, false), (restored.Body.GetProperty("restored").GetBoolean(), restored.Body.GetProperty("person_created").GetBoolean()));
        var membership = restored.Body.GetProperty("membership");
        Assert.Equal((original.Id, person.Id, "lao", "老员工回来", "active"), (membership.GetProperty("id").GetGuid(), membership.GetProperty("person_id").GetGuid(),
            membership.GetProperty("account").GetString(), membership.GetProperty("display_name").GetString(), membership.GetProperty("status").GetString()));
        Assert.Empty(DirectoryAgentContract.Load(ContractPath).ValidateComponent("AddMemberResult", restored.Body));
        Assert.Equal("active|false", await host.Text("SELECT status || '|' || directory_deleted FROM plugin_qian_identity.memberships WHERE id = @id", ("id", original.Id)));
        Assert.Equal(2, await Counted());
        Assert.Equal("False", await host.Text("SELECT must_change_password FROM plugin_qian_identity.persons WHERE id = @p", ("p", person.Id)));
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.NewBrowser().Login(phone, Initial, tenant: tenant.Id)).Status);
        var back = host.NewBrowser();
        Assert.Equal("authenticated", (await back.Login(phone, Password, tenant: tenant.Id)).Body.GetProperty("status").GetString());
        Assert.Equal(original.Id, (await back.Json(HttpMethod.Get, "api/identity/me")).Body.GetProperty("membership_id").GetGuid());
        Assert.Contains(original.Id, (await admin.Json(HttpMethod.Get, "api/directory/members?limit=200")).Body.GetProperty("items").EnumerateArray()
            .Select(item => item.GetProperty("membership").GetProperty("id").GetGuid()));
        // 事件：恢复发 directory.member.status_changed.v1（删除标记回到 false），操作人是管理员；不再发 member_created。
        var events = (await DirectoryEventTests.DirectoryEvents(host, tenant.Id)).Where(envelope => envelope.Payload.GetProperty("membership_id").GetGuid() == original.Id).ToArray();
        var last = events[^1];
        Assert.Equal(DirectoryEventIds.MemberStatusChanged, last.Type); DirectoryEventTests.AssertSchema(last);
        Assert.Equal(("active", "disabled", false), (last.Payload.GetProperty("status").GetString(), last.Payload.GetProperty("previous_status").GetString(), last.Payload.GetProperty("deleted").GetBoolean()));
        Assert.Equal((EventActorKind.User, adminPerson.Id.ToString("D")), (last.Actor.Kind, last.Actor.Id));
        Assert.Single(events, envelope => envelope.Type == DirectoryEventIds.MemberCreated);
        // 恢复与别的状态变化一样递增成员的身份修订；响应与事件里是同一个修订号。
        Assert.Equal(revisionBefore + 1, membership.GetProperty("identity_revision").GetInt64());
        Assert.Equal(revisionBefore + 1, last.Payload.GetProperty("identity_revision").GetInt64());
        // 恢复后他重新占用名额：租户又满了，再新增先报配额。
        Assert.Equal("CARDINALITY_USER_LIMIT", Code((await Add(admin, new { identifier = phone, display_name = "第三次", initial_password = Initial })).Body));
    }

    [Fact]
    public async Task 非管理员与平台会话调用被拒且无写入_缺防伪与带租户字段被拒_另一租户看不到本租户新成员_并发新建同一号码只有一个自然人()
    {
        await using var host = await AccountsHost.Start(postgres); var a = host.Tenants.Add("member-guard-a"); var b = host.Tenants.Add("member-guard-b");
        var (admin, _) = await Administrator(host, a, "甲管理员"); var (otherAdmin, _) = await Administrator(host, b, "乙管理员");
        var (regular, regularPhone) = await CreatePerson(host, "普通成员"); await Join(host, a, regular.Id, "普通成员");
        var member = host.NewBrowser(); Assert.Equal("authenticated", (await member.Login(regularPhone, Password, tenant: a.Id)).Body.GetProperty("status").GetString());
        var (_, platformPhone) = await CreatePerson(host, "平台运营", platform: true);
        var platform = host.NewBrowser(); Assert.Equal("authenticated", (await platform.Login(platformPhone, Password)).Body.GetProperty("status").GetString());
        Assert.False((await platform.Json(HttpMethod.Get, "api/identity/me")).Body.TryGetProperty("tenant_id", out _)); // 平台身份：没有租户
        var phone = NextPhone(); var body = new { identifier = phone, display_name = "被拒", initial_password = Initial };
        var before = await Facts(host, a, E164(phone)); var beforeB = await Facts(host, b, E164(phone));
        // 非管理员成员与平台会话：403（平台会话没有租户成员身份，不合成成员）；未登录 401。
        Assert.Equal((HttpStatusCode.Forbidden, "AUTHZ_DENIED"), Result(await Add(member, body)));
        Assert.Equal((HttpStatusCode.Forbidden, "AUTHZ_DENIED"), Result(await Add(platform, body)));
        Assert.Equal((HttpStatusCode.Unauthorized, "UNAUTHENTICATED"), Result(await Add(host.NewBrowser(), body)));
        // 管理员也要防伪；请求体里的租户字段一律拒绝（未知字段），不改目标租户；请求体必须是 JSON。
        Assert.Equal((HttpStatusCode.BadRequest, "INPUT_INVALID"), Result(await Add(admin, body, csrf: false)));
        Assert.Equal((HttpStatusCode.BadRequest, "BAD_REQUEST"), Result(await Add(admin, new { identifier = phone, display_name = "带租户", initial_password = Initial, tenant_id = b.Id })));
        var (header, token) = await admin.Csrf();
        using (var form = new HttpRequestMessage(HttpMethod.Post, "api/directory/members") { Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["identifier"] = phone }) })
        {
            form.Headers.Add(header, token);
            using var response = await admin.Http.SendAsync(form, Ct);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }
        Assert.Equal(before, await Facts(host, a, E164(phone))); Assert.Equal(beforeB, await Facts(host, b, E164(phone)));
        // 甲方新增的成员：乙方管理员的列表、按 id 批量与成员归属都看不到；甲方管理员看得到。
        var added = await Add(admin, new { identifier = phone, display_name = "甲方新人", initial_password = Initial });
        Assert.Equal(HttpStatusCode.OK, added.Status);
        var id = added.Body.GetProperty("membership").GetProperty("id").GetGuid();
        static IEnumerable<Guid> Ids(JsonElement list) => list.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("membership").GetProperty("id").GetGuid());
        Assert.DoesNotContain(id, Ids((await otherAdmin.Json(HttpMethod.Get, "api/directory/members?limit=200")).Body));
        Assert.Empty(Ids((await otherAdmin.Json(HttpMethod.Get, $"api/directory/members?ids={id:D}")).Body));
        Assert.Equal(HttpStatusCode.NotFound, (await otherAdmin.Json(HttpMethod.Get, $"api/directory/members/{id:D}/relations")).Status);
        Assert.Contains(id, Ids((await admin.Json(HttpMethod.Get, "api/directory/members?limit=200")).Body));
        // 两个租户同时用同一个新号码新增：只建一个自然人、一个成员，晚到的一方返回 409。
        var shared = NextPhone();
        var both = await Task.WhenAll(Add(admin, new { identifier = shared, display_name = "同号甲", initial_password = Initial }),
            Add(otherAdmin, new { identifier = shared, display_name = "同号乙", initial_password = Initial }));
        Assert.Single(both, result => result.Status == HttpStatusCode.OK);
        Assert.Single(both, result => result.Status == HttpStatusCode.Conflict && Code(result.Body) == "IDENTIFIER_IN_USE");
        Assert.Equal("1", await host.Text("SELECT count(*) FROM plugin_qian_identity.person_identifiers WHERE value = @v", ("v", E164(shared))));
        Assert.Equal("1", await host.Text("SELECT count(*) FROM plugin_qian_identity.memberships m JOIN plugin_qian_identity.person_identifiers i ON i.person_id = m.person_id WHERE i.value = @v",
            ("v", E164(shared))));
    }

    private static (HttpStatusCode, string) Result((HttpStatusCode Status, JsonElement Body, HttpResponseMessage Raw) response) => (response.Status, Code(response.Body));
}
