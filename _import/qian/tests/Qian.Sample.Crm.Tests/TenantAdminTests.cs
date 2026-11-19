using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kun.Authorization;
using Kun.Core.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qian.Audit;
using Qian.Authz;
using Qian.Authz.Abstractions;
using Qian.Capabilities;
using Kun.Data;
using Qian.Identity.Abstractions;
using Kun.Runtime;
using Kun.Errors;
using Kun.Tenancy;
using Qian.Testing.Containers;

namespace Qian.Sample.Crm.Tests;

// 运营后台维护租户管理员（S1-TEN-010）：账号查找与新建、替换管理员、重置管理员密码。
// 身份组件、租户运营宿主、超管旁路与两条审计链都是真的；成员、凭据与会话只经 qian.identity 的端口改变。
public sealed partial class TenantAdminTests(PostgresFixture postgres) : CustomerCommandTests(postgres), IClassFixture<PostgresFixture>
{
    private const string Accounts = "/api/tenancy/admin-accounts";
    private const string TemporaryPassword = "Temp-Pass-2026";
    private const string DesignatedPassword = "Designated-Pass-2026";

    [Fact]
    public async Task 管理员维护只对平台超管开放_账号经身份组件查找或新建后开户_替换管理员转移owner_满额与开通未完成时拒绝_端点进契约()
    {
        await using var ops = await StartOps();
        var (business, client) = (ops.Business, ops.Client);
        var root = business.Token(null, "", "root", "user");
        var source = business.App.Services.GetRequiredService<CrmCommandRuntime>().Loader.GetContext("tenancy", ScopeKey.Global);
        var phone = NewPhone();
        (HttpMethod Method, string Path, object? Body)[] endpoints =
        [
            (HttpMethod.Post, Accounts + ":lookup", new TenantAdminAccountLookup(phone)),
            (HttpMethod.Post, Accounts, new CreateTenantAdminAccount("管理员甲", TenantAdminIdentifierKind.Phone, phone, TemporaryPassword)),
            (HttpMethod.Get, Admins(A), null),
            (HttpMethod.Put, Admins(A), new ReplaceTenantAdmins([new(Guid.NewGuid())])),
            (HttpMethod.Post, Admins(A) + "/" + Guid.NewGuid() + ":reset-password", new ResetTenantAdminPassword(TemporaryPassword)),
        ];
        // 无凭据 401；租户主体、非超管的平台用户与 Agent 一律 403（运营入口只收人类平台用户）。
        foreach (var token in new[] { null, business.Token(A, "", "root", "user"), business.Token(null, "", "intruder", "user"), business.Token(null, "", "root", "agent") })
            foreach (var (method, path, body) in endpoints)
                await Rejected(await Send(client, method, path, token, body), token is null ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden, token is null ? "UNAUTHENTICATED" : "AUTHZ_DENIED");
        // 目标租户只取自路径：请求体任何一层带 tenantId / tenant_id / id 都在绑定时拒绝。
        foreach (var forbidden in new[] { "tenantId", "tenant_id", "id" })
        {
            await Rejected(await Send(client, HttpMethod.Put, Admins(A), root, new Dictionary<string, object?> { ["items"] = new[] { new { subjectId = Guid.NewGuid() } }, [forbidden] = A }), HttpStatusCode.BadRequest, "BAD_REQUEST");
            await Rejected(await Send(client, HttpMethod.Put, Admins(A), root, new { items = new[] { new Dictionary<string, object?> { ["subjectId"] = Guid.NewGuid(), [forbidden] = A } } }), HttpStatusCode.BadRequest, "BAD_REQUEST");
            await Rejected(await Send(client, HttpMethod.Post, Accounts, root, new Dictionary<string, object?>
                { ["displayName"] = "甲", ["identifierKind"] = "phone", ["identifier"] = phone, ["initialPassword"] = TemporaryPassword, [forbidden] = A }), HttpStatusCode.BadRequest, "BAD_REQUEST");
            await Rejected(await Send(client, HttpMethod.Post, Admins(A) + "/" + Guid.NewGuid() + ":reset-password", root, new Dictionary<string, object?> { ["newPassword"] = TemporaryPassword, [forbidden] = A }),
                HttpStatusCode.BadRequest, "BAD_REQUEST");
        }
        Assert.Equal("#/items", await Rejected(await Send(client, HttpMethod.Put, Admins(A), root, new ReplaceTenantAdmins([])), HttpStatusCode.BadRequest, "INPUT_INVALID"));
        Assert.Equal("#/items", await Rejected(await Send(client, HttpMethod.Put, Admins(A), root, new ReplaceTenantAdmins([new(Guid.NewGuid()), new(Guid.NewGuid())])), HttpStatusCode.BadRequest, "INPUT_INVALID"));
        Assert.Equal("#/items/0/subjectId", await Rejected(await Send(client, HttpMethod.Put, Admins(A), root, new ReplaceTenantAdmins([new(Guid.Empty)])), HttpStatusCode.BadRequest, "INPUT_INVALID"));
        Assert.Equal("#/initialPassword", await Rejected(await Send(client, HttpMethod.Post, Accounts, root, new CreateTenantAdminAccount("甲", TenantAdminIdentifierKind.Phone, phone, "short")),
            HttpStatusCode.BadRequest, "INPUT_INVALID"));
        Assert.Equal("#/identifier", await Rejected(await Send(client, HttpMethod.Post, Accounts + ":lookup", root, new TenantAdminAccountLookup("")), HttpStatusCode.BadRequest, "INPUT_INVALID"));
        Assert.Empty(await Bypasses(source)); // 401 / 403 / 400 都在超管旁路之前拒绝，平台链上没有放行记录。

        // 新建账号：一个登录标识加初始密码；响应只有掩码，没有原值与密码。身份组件的错误位置换成运营请求体的字段。
        string raw;
        using (var response = await Send(client, HttpMethod.Post, Accounts, root, new CreateTenantAdminAccount("管理员甲", TenantAdminIdentifierKind.Phone, phone, TemporaryPassword)))
        { raw = await response.Content.ReadAsStringAsync(Ct); Assert.True(response.StatusCode == HttpStatusCode.Created, raw); Assert.Null(response.Headers.Location); }
        Assert.DoesNotContain(phone, raw, StringComparison.Ordinal); Assert.DoesNotContain(TemporaryPassword, raw, StringComparison.Ordinal);
        var accountA = JsonSerializer.Deserialize<TenantAdminAccount>(raw, JsonSerializerOptions.Web)!;
        Assert.Equal(("管理员甲", AccountStatuses.Active), (accountA.DisplayName, accountA.Status));
        var masked = Assert.Single(accountA.Identifiers);
        Assert.Equal((TenantAdminIdentifierKind.Phone, false), (masked.Kind, masked.Verified));
        Assert.Null(await Rejected(await Send(client, HttpMethod.Post, Accounts, root, new CreateTenantAdminAccount("重复", TenantAdminIdentifierKind.Phone, phone, TemporaryPassword)),
            HttpStatusCode.Conflict, "IDENTIFIER_IN_USE"));
        Assert.Equal("#/identifier", await Rejected(await Send(client, HttpMethod.Post, Accounts, root, new CreateTenantAdminAccount("坏号", TenantAdminIdentifierKind.Phone, "12", TemporaryPassword)),
            HttpStatusCode.BadRequest, "INPUT_INVALID"));
        Assert.Equal("#/initialPassword", await Rejected(await Send(client, HttpMethod.Post, Accounts, root, new CreateTenantAdminAccount("空白", TenantAdminIdentifierKind.Phone, NewPhone(), " " + TemporaryPassword)),
            HttpStatusCode.UnprocessableEntity, "PASSWORD_POLICY_VIOLATION"));
        // 查找：按登录标识（放在请求体）找到可任管理员的账号；不存在与具备平台访问资格的都是 404。
        Assert.Equal((accountA.SubjectId, "管理员甲"), await Lookup(phone));
        await Rejected(await Send(client, HttpMethod.Post, Accounts + ":lookup", root, new TenantAdminAccountLookup(NewPhone())), HttpStatusCode.NotFound, "NOT_FOUND");
        var (superadmin, superPhone) = await NewPerson(business, "平台超管", platform: true);
        await Rejected(await Send(client, HttpMethod.Post, Accounts + ":lookup", root, new TenantAdminAccountLookup(superPhone)), HttpStatusCode.NotFound, "NOT_FOUND");

        // 新建的账号作开户管理员：它的手机号没有经本人验证，开户请求必须带初始密码（R163）。缺了就 400，什么都不建，账号的密码不变。
        var memberA = Guid.CreateVersion7();
        Assert.Equal("#/administrator/initialPassword", await Rejected(await Send(client, HttpMethod.Post, "/api/tenancy/tenants", root,
            new CreateTenant("admins", "管理员租户") { Administrator = new(memberA, accountA.SubjectId.ToString("D")) }), HttpStatusCode.BadRequest, "INPUT_INVALID"));
        Assert.Equal("0", await Text("SELECT count(*)::text FROM plugin_qian_tenancy.tenants WHERE code = 'admins'"));
        var pending = await SignInAs(phone, TemporaryPassword);
        Assert.Equal(SignInStatus.PasswordChangeRequired, pending.Status);
        // 带了初始密码：开户前把账号的密码重置为它、下次登录强制修改；之后成员、指针与 tenant.owner 由开通编排第 05–07 步完成。初始密码不进保存的开户输入。
        var tenant = await Read<TenantDescriptor>(await Send(client, HttpMethod.Post, "/api/tenancy/tenants", root,
            new CreateTenant("admins", "管理员租户") { Administrator = new(memberA, accountA.SubjectId.ToString("D")) { InitialPassword = DesignatedPassword } }), HttpStatusCode.Created);
        // 重置之前登录拿到的改密票据随之作废，不能用它改掉运营设的初始密码。
        using (var through = new AuthenticationProof(Encoding.UTF8.GetBytes("Through-Old-Ticket-1")))
            Assert.Equal("LOGIN_TICKET_INVALID", (await business.Identity.SignIn.ChangeInitialPasswordAsync(new InitialPasswordChange(pending.Ticket!.Token, through), new LoginContext(), Ct)).Error!.Code.Code);
        Assert.Equal(SignInStatus.Failed, (await SignInAs(phone, TemporaryPassword)).Status);
        Assert.Equal(SignInStatus.PasswordChangeRequired, (await SignInAs(phone, DesignatedPassword)).Status);
        Assert.Equal("0", await Text($"SELECT count(*)::text FROM plugin_qian_tenancy.tenants WHERE code = 'admins' AND provision_input::text LIKE '%{DesignatedPassword}%'"));
        var id = tenant.Id.ToString();
        var initial = Assert.Single((await Read<TenantAdminList>(await Send(client, HttpMethod.Get, Admins(id), root), HttpStatusCode.OK)).Items);
        Assert.Equal((memberA, accountA.SubjectId, "管理员甲", AccountStatuses.Active), (initial.MembershipId, initial.SubjectId, initial.DisplayName, initial.Status));
        Assert.Equal(TenantAdminIdentifierKind.Phone, Assert.Single(initial.Identifiers).Kind);
        Assert.Equal(accountA.SubjectId.ToString("D"), await Owners(id));
        // 运营列表（S1-TEN-018）：管理员列取当前成员的显示名，用量经身份侧计数端口统计，开户管理员计 1。
        var listed = Assert.Single((await Read<JsonElement>(await Send(client, HttpMethod.Get, "/api/tenancy/tenants?keyword=admins", root), HttpStatusCode.OK)).GetProperty("items").EnumerateArray());
        Assert.Equal(("管理员甲", 1), (listed.GetProperty("administratorName").GetString(), listed.GetProperty("usersUsed").GetInt32()));

        // 替换为已有账号：新管理员建立成员并得到 tenant.owner；原管理员撤销 owner 与管理员标志，成员身份保留。
        // 乙的手机号经本人验证过，不需要初始密码，凭据也不动。
        var (personB, phoneB) = await NewPerson(business, "管理员乙");
        Assert.Equal((personB.Id, "管理员乙"), await Lookup(phoneB));
        var replaced = Assert.Single((await Read<TenantAdminList>(await Send(client, HttpMethod.Put, Admins(id), root, new ReplaceTenantAdmins([new(personB.Id)])), HttpStatusCode.OK)).Items);
        var memberB = replaced.MembershipId;
        Assert.Equal((personB.Id, "管理员乙"), (replaced.SubjectId, replaced.DisplayName)); Assert.NotEqual(memberA, memberB);
        Assert.Equal(memberB.ToString(), await Text($"SELECT membership_id::text FROM plugin_qian_tenancy.tenant_admins WHERE tenant_id = '{id}'"));
        Assert.Equal(personB.Id.ToString("D"), await Owners(id));
        Assert.Equal(("false|active", "true|active"), (await Member(memberA), await Member(memberB)));
        // 乙的密码没有被重置：原密码照常通过第一因子，只因唯一的租户还待激活而进不去（TENANT_NOT_ACTIVE，不是 INVALID_CREDENTIALS）。
        Assert.Equal("TENANT_NOT_ACTIVE", (await SignInAs(phoneB, IdentityPassword)).Error!.Code.Code);
        // 同一请求重试收敛：成员与指针不变，不重复记审计。
        Assert.Equal(memberB, Assert.Single((await Read<TenantAdminList>(await Send(client, HttpMethod.Put, Admins(id), root, new ReplaceTenantAdmins([new(personB.Id)])), HttpStatusCode.OK)).Items).MembershipId);
        Assert.Equal("2", await Text($"SELECT count(*)::text FROM plugin_qian_identity.memberships WHERE tenant_id = '{id}'"));

        // 不能指定的人：不存在、具备平台访问资格、已停用的账号；与开户不同的租户路径不存在时 404。
        foreach (var subject in new[] { Guid.NewGuid(), superadmin.Id, await Disabled() })
            Assert.Equal("#/items/0/subjectId", await Rejected(await Send(client, HttpMethod.Put, Admins(id), root, new ReplaceTenantAdmins([new(subject)])), HttpStatusCode.BadRequest, "INPUT_INVALID"));
        await Rejected(await Send(client, HttpMethod.Get, Admins(Guid.NewGuid().ToString()), root), HttpStatusCode.NotFound, "NOT_FOUND");
        await Rejected(await Send(client, HttpMethod.Put, Admins(Guid.NewGuid().ToString()), root, new ReplaceTenantAdmins([new(personB.Id)])), HttpStatusCode.NotFound, "NOT_FOUND");
        // 开通第 05–07 步没有全部成功时先续跑开通，替换返回 409。
        await Admin($"UPDATE plugin_qian_tenancy.tenant_provision_steps SET status = 'failed' WHERE tenant_id = '{id}' AND step = '07:owner';");
        await Rejected(await Send(client, HttpMethod.Put, Admins(id), root, new ReplaceTenantAdmins([new(accountA.SubjectId)])), HttpStatusCode.Conflict, "CONFLICT");
        await Admin($"UPDATE plugin_qian_tenancy.tenant_provision_steps SET status = 'succeeded' WHERE tenant_id = '{id}' AND step = '07:owner';");
        Assert.Equal(personB.Id.ToString("D"), await Owners(id));

        // 满额：新成员经身份组件的配额判定被拒，什么都没改；已是成员的原管理员不占新名额，可以换回。
        await Admin($"UPDATE plugin_qian_tenancy.tenants SET quotas = '{{\"users\":2}}' WHERE id = '{id}'::uuid;");
        var (personC, _) = await NewPerson(business, "管理员丙");
        await Rejected(await Send(client, HttpMethod.Put, Admins(id), root, new ReplaceTenantAdmins([new(personC.Id)])), HttpStatusCode.UnprocessableEntity, "CARDINALITY_USER_LIMIT");
        Assert.Equal((memberB.ToString(), personB.Id.ToString("D"), "0"), (await Text($"SELECT membership_id::text FROM plugin_qian_tenancy.tenant_admins WHERE tenant_id = '{id}'"),
            await Owners(id), await Text($"SELECT count(*)::text FROM plugin_qian_identity.memberships WHERE tenant_id = '{id}' AND person_id = '{personC.Id}'")));
        // 换回甲：他的手机号仍没有经本人验证，同样必须给初始密码；缺了就 400，指针与 owner 都不变。
        Assert.Equal("#/items/0/initialPassword", await Rejected(await Send(client, HttpMethod.Put, Admins(id), root,
            new ReplaceTenantAdmins([new(accountA.SubjectId)])), HttpStatusCode.BadRequest, "INPUT_INVALID"));
        Assert.Equal((memberB.ToString(), personB.Id.ToString("D")), (await Text($"SELECT membership_id::text FROM plugin_qian_tenancy.tenant_admins WHERE tenant_id = '{id}'"), await Owners(id)));
        Assert.Equal(memberA, Assert.Single((await Read<TenantAdminList>(await Send(client, HttpMethod.Put, Admins(id), root,
            new ReplaceTenantAdmins([new(accountA.SubjectId) { InitialPassword = TemporaryPassword }])), HttpStatusCode.OK)).Items).MembershipId);
        Assert.Equal(accountA.SubjectId.ToString("D"), await Owners(id));
        Assert.Equal(SignInStatus.Failed, (await SignInAs(phone, DesignatedPassword)).Status);
        Assert.Equal(SignInStatus.PasswordChangeRequired, (await SignInAs(phone, TemporaryPassword)).Status);
        Assert.Equal(("true|active", "false|active"), (await Member(memberA), await Member(memberB)));

        // 租户链：每次指针变化补一条 admin.assign，只记成员指针，没有登录标识。平台链：每次放行都有超管旁路记录。
        using (business.App.Services.GetRequiredService<ITenantContext>().BeginScope(id))
        {
            var assigned = (await ReadTenantChain(source.Audit())).Where(entry => entry.Record.CapabilityId == "qian.tenancy/admin.assign").Select(entry => entry.Record).ToArray();
            Assert.Equal([(memberB, memberA), (memberA, memberB)], assigned.Select(record =>
                (record.Payload.GetProperty("membershipId").GetGuid(), record.Payload.GetProperty("previousMembershipId").GetGuid())));
            Assert.All(assigned, record =>
            {
                Assert.Equal(("user", "root", "api", "success"), (record.ActorType, record.ActorId, record.Channel, record.Result));
                Assert.DoesNotContain(phoneB, record.Payload.GetRawText(), StringComparison.Ordinal);
            });
            Assert.True((await source.Audit().VerifyAsync(Ct)).Valid);
        }
        var bypasses = await Bypasses(source);
        Assert.All(bypasses, record => Assert.Equal("superadmin", record.Payload.GetProperty("bypass").GetString()));
        Assert.Contains(bypasses, record => record.Payload.GetProperty("permission").GetString() == "qian.tenancy:admin:create" && record.Resource == "tenancy/admin-accounts");
        Assert.Contains(bypasses, record => record.Payload.GetProperty("permission").GetString() == "qian.tenancy:admin:assign" && record.Resource == "tenancy/tenants/" + id);
        Assert.True((await source.Audit().VerifyPlatformAsync(source, Ct)).Valid);

        // 契约：四条新路径进组件 OpenAPI 文档（与发布契约的比对与 RT-006 准入见 TenantOperationsTests）。
        var document = JsonNode.Parse((await ops.Management.Services.GetRequiredService<IHttpContractSource>().CompileDocumentAsync("qian.tenancy", Ct)).GetRawText())!;
        var paths = document["paths"]!.AsObject();
        Assert.True(paths[Accounts + ":lookup"]!["post"] is not null && paths[Accounts]!["post"] is not null);
        Assert.True(paths["/api/tenancy/tenants/{id}/admins"]!["get"] is not null && paths["/api/tenancy/tenants/{id}/admins"]!["put"] is not null);
        var replace = TenantContractSchemaTests.Resolve(document, paths["/api/tenancy/tenants/{id}/admins"]!["put"]!["requestBody"]!["content"]!["application/json"]!["schema"]!);
        Assert.Equal((1, 1), (replace["properties"]!["items"]!["minItems"]!.GetValue<int>(), replace["properties"]!["items"]!["maxItems"]!.GetValue<int>()));
        var reset = TenantContractSchemaTests.Resolve(document, paths["/api/tenancy/tenants/{id}/admins/{membershipId}:reset-password"]!["post"]!["requestBody"]!["content"]!["application/json"]!["schema"]!);
        Assert.Equal(8, reset["properties"]!["newPassword"]!["minLength"]!.GetValue<int>());

        async Task<(Guid, string)> Lookup(string identifier)
        {
            var account = await Read<TenantAdminAccount>(await Send(client, HttpMethod.Post, Accounts + ":lookup", root, new TenantAdminAccountLookup(identifier)), HttpStatusCode.OK);
            return (account.SubjectId, account.DisplayName);
        }
        Task<string?> Owners(string tenantId) => Text("SELECT string_agg(a.subject_id, ',' ORDER BY a.subject_id) FROM plugin_qian_authz.role_assignments a " +
            $"JOIN plugin_qian_authz.roles r ON a.role_id = r.id WHERE a.tenant_id = '{tenantId}' AND r.code = 'tenant.owner'");
        Task<string?> Member(Guid membership) => Text($"SELECT is_tenant_administrator::text || '|' || status FROM plugin_qian_identity.memberships WHERE id = '{membership}'");
        async Task<SignInResult> SignInAs(string identifier, string password)
        {
            using var proof = new AuthenticationProof(Encoding.UTF8.GetBytes(password));
            return await business.Identity.SignIn.SignInAsync(new LoginInput("web", null, null, null, proof) { Identifier = identifier }, new LoginContext(), Ct);
        }
        async Task<Guid> Disabled()
        {
            var (person, _) = await NewPerson(business, "已停用");
            await business.Identity.Accounts.SetPersonStatusAsync(person.Id, AccountStatuses.Disabled, "test", Ct);
            return person.Id;
        }
    }

    [Fact]
    public async Task 重置管理员密码经身份组件_旧密码与全部会话立即失效_下次登录强制改密_只对当前管理员_审计不含密码_标humanOnly与确认()
    {
        await using var ops = await StartOps();
        var (business, client) = (ops.Business, ops.Client);
        var root = business.Token(null, "", "root", "user");
        var source = business.App.Services.GetRequiredService<CrmCommandRuntime>().Loader.GetContext("tenancy", ScopeKey.Global);
        var identity = business.Identity.Loader!.GetContext("identity", ScopeKey.Global).Inject<IIdentityService>(IdentityDefinition.Key);
        var (former, formerPhone) = await NewPerson(business, "原管理员");
        var formerMember = Guid.CreateVersion7();
        var tenant = await Read<TenantDescriptor>(await Send(client, HttpMethod.Post, "/api/tenancy/tenants", root,
            new CreateTenant("resets", "重置租户") { Administrator = new(formerMember, former.Id.ToString("D")) }), HttpStatusCode.Created);
        var id = tenant.Id.ToString();
        await Read<TenantDescriptor>(await Send(client, HttpMethod.Post, $"/api/tenancy/tenants/{id}:activate", root, new ActivateTenant()), HttpStatusCode.OK);
        var (admin, phone) = await NewPerson(business, "现任管理员");
        var member = Assert.Single((await Read<TenantAdminList>(await Send(client, HttpMethod.Put, Admins(id), root, new ReplaceTenantAdmins([new(admin.Id)])), HttpStatusCode.OK)).Items).MembershipId;
        var before = await SignInAs(phone, IdentityPassword);
        Assert.Equal((SignInStatus.Authenticated, tenant.Id, member), (before.Status, before.Session!.TenantId, before.Session.MembershipId));
        Assert.NotEmpty(await identity.ListSessionsAsync(admin.Id, Ct));

        // 只对当前管理员：原管理员、不存在的成员、别的租户路径都是 404；策略不允许的临时密码按运营请求体的字段报错。
        foreach (var target in new[] { formerMember, Guid.NewGuid() })
            await Rejected(await Send(client, HttpMethod.Post, Reset(id, target), root, new ResetTenantAdminPassword(TemporaryPassword)), HttpStatusCode.NotFound, "NOT_FOUND");
        await Rejected(await Send(client, HttpMethod.Post, Reset(A, member), root, new ResetTenantAdminPassword(TemporaryPassword)), HttpStatusCode.NotFound, "NOT_FOUND");
        Assert.Equal("#/newPassword", await Rejected(await Send(client, HttpMethod.Post, Reset(id, member), root, new ResetTenantAdminPassword("short")), HttpStatusCode.BadRequest, "INPUT_INVALID"));
        Assert.Equal("#/newPassword", await Rejected(await Send(client, HttpMethod.Post, Reset(id, member), root, new ResetTenantAdminPassword(TemporaryPassword + " ")),
            HttpStatusCode.UnprocessableEntity, "PASSWORD_POLICY_VIOLATION"));
        Assert.Equal((SignInStatus.Authenticated, 1), ((await SignInAs(formerPhone, IdentityPassword)).Status, (await identity.ListSessionsAsync(former.Id, Ct)).Count));

        using (var reset = await Send(client, HttpMethod.Post, Reset(id, member), root, new ResetTenantAdminPassword(TemporaryPassword)))
            Assert.True(reset.StatusCode == HttpStatusCode.NoContent, await reset.Content.ReadAsStringAsync(Ct));
        // 旧密码立即失效、该自然人的会话全部吊销；临时密码登录必须先改密（与开户初始密码同一语义）。
        Assert.Empty(await identity.ListSessionsAsync(admin.Id, Ct));
        Assert.Equal("INVALID_CREDENTIALS", (await SignInAs(phone, IdentityPassword)).Error!.Code.Code);
        var temporary = await SignInAs(phone, TemporaryPassword);
        Assert.Equal(SignInStatus.PasswordChangeRequired, temporary.Status); Assert.Null(temporary.Session); Assert.NotNull(temporary.Ticket);
        // 其他人不受影响：原管理员的会话还在，仍是成员但已不是管理员。
        Assert.Single(await identity.ListSessionsAsync(former.Id, Ct));
        Assert.Equal("false|active", await Text($"SELECT is_tenant_administrator::text || '|' || status FROM plugin_qian_identity.memberships WHERE id = '{formerMember}'"));

        // 租户链记 admin.reset-password（只有成员指针），平台链记 reset-credential 的超管旁路；两条链都没有密码。
        using (business.App.Services.GetRequiredService<ITenantContext>().BeginScope(id))
        {
            var record = Assert.Single(await ReadTenantChain(source.Audit()), entry => entry.Record.CapabilityId == "qian.tenancy/admin.reset-password").Record;
            Assert.Equal(("user", "root", "api", "success", member), (record.ActorType, record.ActorId, record.Channel, record.Result, record.Payload.GetProperty("membershipId").GetGuid()));
            Assert.DoesNotContain(TemporaryPassword, record.Payload.GetRawText(), StringComparison.Ordinal);
        }
        // 旁路记的是放行决定：放行后才得出的 404（3 次）与 422（1 次）同样留有记录，校验失败的 400 没有。
        var resets = (await Bypasses(source)).Where(record => record.Payload.GetProperty("permission").GetString() == "qian.tenancy:admin:reset-credential").ToArray();
        Assert.Equal(5, resets.Length);
        Assert.Equal(2, resets.Count(record => record.Resource == $"tenancy/tenants/{id}/admins/{member}"));
        Assert.All(await ReadPlatformChain(source), entry => Assert.DoesNotContain(TemporaryPassword, entry.Record.Payload.GetRawText(), StringComparison.Ordinal));

        // 机器可读的操作描述：替换与重置都不对模型暴露（humanOnly）且要求确认，页面据此弹出二次确认。
        var machine = await Read<JsonElement>(await Send(client, HttpMethod.Get, "/api/tenancy/meta/state-machine", root), HttpStatusCode.OK);
        foreach (var name in new[] { "assign-admin", "reset-admin-password" })
        {
            var action = machine.GetProperty("actions").EnumerateArray().Single(item => item.GetProperty("name").GetString() == name);
            Assert.Equal((false, true), (action.GetProperty("agent").GetProperty("expose").GetBoolean(), action.GetProperty("agent").GetProperty("requiresConfirmation").GetBoolean()));
        }

        async Task<SignInResult> SignInAs(string identifier, string password)
        {
            using var proof = new AuthenticationProof(Encoding.UTF8.GetBytes(password));
            return await business.Identity.SignIn.SignInAsync(new LoginInput("web", null, null, null, proof) { Identifier = identifier }, new LoginContext(), Ct);
        }
    }

    // R163：运营指定管理员（开户与更换）按账号情况处理。
    [Fact]
    public async Task 指定管理员按账号情况处理_已验证账号不重置也不收初始密码_只靠外部身份登录的账号不能指定_重置解除外部身份绑定并留审计_开户先判租户号与账号资格()
    {
        await using var ops = await StartOps();
        var (business, client) = (ops.Business, ops.Client);
        var root = business.Token(null, "", "root", "user");
        var source = business.App.Services.GetRequiredService<CrmCommandRuntime>().Loader.GetContext("tenancy", ScopeKey.Global);
        var accounts = business.Identity.Accounts;
        static string? Failure(SignInResult result) => result.Error?.Code.Code;
        async Task<SignInResult> SignInAs(string identifier, string password)
        {
            using var proof = new AuthenticationProof(Encoding.UTF8.GetBytes(password));
            return await business.Identity.SignIn.SignInAsync(new LoginInput("web", null, null, null, proof) { Identifier = identifier }, new LoginContext(), Ct);
        }
        Task<HttpResponseMessage> Open(string code, Guid subject, string? initialPassword) => Send(client, HttpMethod.Post, "/api/tenancy/tenants", root,
            new CreateTenant(code, "指定核对 " + code) { Administrator = new(Guid.CreateVersion7(), subject.ToString("D")) { InitialPassword = initialPassword } });
        Task<HttpResponseMessage> Replace(string tenant, Guid subject, string? initialPassword) => Send(client, HttpMethod.Put, Admins(tenant), root,
            new ReplaceTenantAdmins([new(subject) { InitialPassword = initialPassword }]));
        Task<string?> Tenants(string code) => Text($"SELECT count(*)::text FROM plugin_qian_tenancy.tenants WHERE code = '{code}'");
        Task<string?> Links(Guid person) => Text($"SELECT count(*)::text FROM plugin_qian_identity.external_identities WHERE person_id = '{person}' AND NOT is_deleted");
        async Task<(string? Pointer, string? Code)> FieldOf(HttpResponseMessage response)
        {
            using (response)
            {
                var body = await response.Content.ReadAsStringAsync(Ct);
                Assert.True(response.StatusCode == HttpStatusCode.BadRequest, body);
                var error = ErrorDetailJson.Deserialize(body)!;
                Assert.Equal("INPUT_INVALID", error.Code);
                return error.Errors is [var first, ..] ? (first.Pointer, first.Code) : (error.Pointer, null);
            }
        }
        async Task<TenantAdminAccount> NewAccount(string name, string number) =>
            await Read<TenantAdminAccount>(await Send(client, HttpMethod.Post, Accounts, root, new CreateTenantAdminAccount(name, TenantAdminIdentifierKind.Phone, number, TemporaryPassword)), HttpStatusCode.Created);

        // 手机号经本人验证的账号：直接任管理员、不重置；开户与更换时给了初始密码反而 400（VALUE_NOT_ALLOWED），什么都不改。
        var (vouched, vouchedPhone) = await NewPerson(business, "已验证");
        Assert.Equal(("#/administrator/initialPassword", "VALUE_NOT_ALLOWED"), await FieldOf(await Open("vouched", vouched.Id, DesignatedPassword)));
        Assert.Equal("0", await Tenants("vouched"));
        var id = (await Read<TenantDescriptor>(await Open("vouched", vouched.Id, null), HttpStatusCode.Created)).Id.ToString();
        Assert.Equal(("#/items/0/initialPassword", "VALUE_NOT_ALLOWED"), await FieldOf(await Replace(id, vouched.Id, DesignatedPassword)));
        Assert.Equal(("TENANT_NOT_ACTIVE", "INVALID_CREDENTIALS"), (Failure(await SignInAs(vouchedPhone, IdentityPassword)), Failure(await SignInAs(vouchedPhone, DesignatedPassword))));

        // 只靠外部身份登录、没有经本人验证的联系方式的账号（连接器首登建出的那种：没有密码，邮箱未验证）：查找不返回，开户与更换都不能指定。
        var email = "ext" + Guid.NewGuid().ToString("N")[..10] + "@example.test";
        var external = await accounts.CreatePersonAsync(new CreatePerson("外部身份", [new IdentifierInput(IdentifierKind.Email, email, false)]), Ct);
        await Rejected(await Send(client, HttpMethod.Post, Accounts + ":lookup", root, new TenantAdminAccountLookup(email)), HttpStatusCode.NotFound, "NOT_FOUND");
        Assert.Equal("#/administrator/subjectId", await Rejected(await Open("external", external.Id, DesignatedPassword), HttpStatusCode.BadRequest, "INPUT_INVALID"));
        Assert.Equal("0", await Tenants("external"));
        Assert.Equal("#/items/0/subjectId", await Rejected(await Replace(id, external.Id, DesignatedPassword), HttpStatusCode.BadRequest, "INPUT_INVALID"));
        Assert.Equal("true", await Text($"SELECT (password_hash IS NULL)::text FROM plugin_qian_identity.persons WHERE id = '{external.Id}'"));

        // 有密码、手机号未经本人验证的账号，在租户 A 有成员身份并绑了外部身份：更换时必须给初始密码；给了就重置、
        // 解除它在各租户的外部身份绑定，并在本租户的审计链记一条带自然人 id 的重置（不含密码）。
        var phoneC = NewPhone();
        var created = await NewAccount("待指定", phoneC);
        var elsewhere = await Join(business, A, created.SubjectId, "A 的成员");
        using (business.App.Services.GetRequiredService<ITenantContext>().BeginScope(A))
            await accounts.LinkExternalIdentityAsync(new ExternalIdentityLink(elsewhere.Id, "oidc", "upstream-" + Guid.NewGuid().ToString("N")), Ct);
        Assert.Equal("1", await Links(created.SubjectId));
        Assert.Equal(("#/items/0/initialPassword", "REQUIRED"), await FieldOf(await Replace(id, created.SubjectId, null)));
        Assert.Equal("1", await Links(created.SubjectId));
        await Read<TenantAdminList>(await Replace(id, created.SubjectId, DesignatedPassword), HttpStatusCode.OK);
        Assert.Equal("0", await Links(created.SubjectId));
        Assert.Equal((SignInStatus.PasswordChangeRequired, "INVALID_CREDENTIALS"), ((await SignInAs(phoneC, DesignatedPassword)).Status, Failure(await SignInAs(phoneC, TemporaryPassword))));
        using (business.App.Services.GetRequiredService<ITenantContext>().BeginScope(id))
        {
            var reset = Assert.Single(await ReadTenantChain(source.Audit()), entry => entry.Record.CapabilityId == "qian.tenancy/admin.reset-password").Record;
            Assert.Equal(("user", "root", "success", created.SubjectId), (reset.ActorType, reset.ActorId, reset.Result, reset.Payload.GetProperty("subjectId").GetGuid()));
            Assert.DoesNotContain(DesignatedPassword, reset.Payload.GetRawText(), StringComparison.Ordinal);
        }

        // 开户时同样：重置记在平台审计链（租户还没建成），带自然人 id。
        var phoneD = NewPhone();
        var designated = await NewAccount("开户指定", phoneD);
        await Read<TenantDescriptor>(await Open("designated", designated.SubjectId, DesignatedPassword), HttpStatusCode.Created);
        var platform = Assert.Single((await ReadPlatformChain(source)).Select(entry => entry.Record), record => record.CapabilityId == "qian.tenancy/admin.reset-password" &&
            record.Payload.GetProperty("subjectId").GetGuid() == designated.SubjectId);
        Assert.Equal(((string?)null, "root"), (platform.TenantId, platform.ActorId));

        // 开通一定会失败时先拒绝、不动账号：租户号已被另一份开户占用 409；平台身份 400。两者的密码都没有被改。
        var phoneE = NewPhone();
        var taken = await NewAccount("撞号", phoneE);
        await Rejected(await Open("designated", taken.SubjectId, DesignatedPassword), HttpStatusCode.Conflict, "CONFLICT");
        Assert.Equal(SignInStatus.PasswordChangeRequired, (await SignInAs(phoneE, TemporaryPassword)).Status);
        var (superadmin, superPhone) = await NewPerson(business, "平台超管", platform: true);
        Assert.Equal("#/administrator/subjectId", await Rejected(await Open("superadmin", superadmin.Id, DesignatedPassword), HttpStatusCode.BadRequest, "INPUT_INVALID"));
        Assert.Equal("0", await Tenants("superadmin"));
        Assert.NotEqual("INVALID_CREDENTIALS", Failure(await SignInAs(superPhone, IdentityPassword)));
    }

    private const string CreatePermission = "qian.tenancy:tenant:create", AssignPermission = "qian.tenancy:admin:assign", CredentialPermission = "qian.tenancy:admin:reset-credential";
    private const string NextPassword = "Next-Designated-2026";

    // R165：运营端为已有账号设定凭据的入口统一要求 reset-credential。开户与更换管理员带了初始密码，本操作的权限点放行之后还要这一项，
    // 缺了就 403、什么都不改。平台目前只有超管一个平台角色，运营持有哪些权限点由 OperatorGrants 在真实旁路之前收窄。
    [Fact]
    public async Task 带初始密码的开户与更换管理员另要重置凭据权限_只有开户或只有指定权限时403且什么都不改_两项都有时照常_不带初始密码不受影响()
    {
        var grants = new OperatorGrants(); var logs = new OperationsLog();
        await using var ops = await StartOps(grants, logs);
        var (business, client) = (ops.Business, ops.Client);
        var root = business.Token(null, "", "root", "user");
        var source = business.App.Services.GetRequiredService<CrmCommandRuntime>().Loader.GetContext("tenancy", ScopeKey.Global);
        var tenants = business.App.Services.GetRequiredService<ITenantContext>();
        // 两位运营都持平台超管，经 OperatorGrants 只留下各自的权限点：opener 只能开户，assigner 只能指定管理员。
        var roles = new PostgresPlatformRoleStore(business.App.Services.GetRequiredService<IDataService>(), tenants, ops.Platform.AuthzRole);
        foreach (var name in new[] { "opener", "assigner" }) await roles.AssignSuperadminAsync(name, "operator", Ct);
        grants.Limit("opener", CreatePermission); grants.Limit("assigner", AssignPermission);
        var (opener, assigner) = (business.Token(null, "", "opener", "user"), business.Token(null, "", "assigner", "user"));
        Task<HttpResponseMessage> Open(string token, string code, Guid subject, string? initialPassword) => Send(client, HttpMethod.Post, "/api/tenancy/tenants", token,
            new CreateTenant(code, "凭据权限 " + code) { Administrator = new(Guid.CreateVersion7(), subject.ToString("D")) { InitialPassword = initialPassword } });
        Task<HttpResponseMessage> Replace(string token, string tenant, Guid subject, string? initialPassword) => Send(client, HttpMethod.Put, Admins(tenant), token,
            new ReplaceTenantAdmins([new(subject) { InitialPassword = initialPassword }]));
        Task<string?> Tenants(string code) => Text($"SELECT count(*)::text FROM plugin_qian_tenancy.tenants WHERE code = '{code}'");
        Task<string?> Pointer(string tenant) => Text($"SELECT membership_id::text FROM plugin_qian_tenancy.tenant_admins WHERE tenant_id = '{tenant}'");
        Task<string?> Members(string tenant, Guid person) => Text($"SELECT count(*)::text FROM plugin_qian_identity.memberships WHERE tenant_id = '{tenant}' AND person_id = '{person}'");
        Task<string?> Links(Guid person) => Text($"SELECT count(*)::text FROM plugin_qian_identity.external_identities WHERE person_id = '{person}' AND NOT is_deleted");
        async Task<SignInStatus> SignInAs(string identifier, string password)
        {
            using var proof = new AuthenticationProof(Encoding.UTF8.GetBytes(password));
            return (await business.Identity.SignIn.SignInAsync(new LoginInput("web", null, null, null, proof) { Identifier = identifier }, new LoginContext(), Ct)).Status;
        }
        // 运营新建的账号：手机号未经本人验证，指定它任管理员要带初始密码（R163）。
        async Task<(Guid Subject, string Phone)> Unverified(string name)
        {
            var phone = NewPhone();
            var account = await Read<TenantAdminAccount>(await Send(client, HttpMethod.Post, Accounts, root, new CreateTenantAdminAccount(name, TenantAdminIdentifierKind.Phone, phone, TemporaryPassword)), HttpStatusCode.Created);
            return (account.SubjectId, phone);
        }
        // 平台链上这位运营的旁路放行，按链上顺序列出权限点与资源。
        async Task<(string Permission, string? Resource)[]> Allowed(string actor) => [.. (await ReadPlatformChain(source)).Select(entry => entry.Record)
            .Where(record => record.CapabilityId == "qian.authz/superadmin.bypass" && record.ActorId == actor)
            .Select(record => (record.Payload.GetProperty("permission").GetString()!, record.Resource))];
        int Denials(string actor, string permission) => logs.Entries.Count(entry => Equals(entry["Subject"], actor) && Equals(entry["Permission"], permission));

        // 一、开户。目标账号的手机号未经本人验证，并在租户 A 绑了外部身份。只有开户权限的运营带初始密码：403，pointer 指向初始密码；
        // 什么都没改：没有建租户，账号的密码与外部身份绑定都在，平台链上没有重置记录。
        var (subject, phone) = await Unverified("待指定");
        var elsewhere = await Join(business, A, subject, "A 的成员");
        async Task Link()
        {
            using (tenants.BeginScope(A)) await business.Identity.Accounts.LinkExternalIdentityAsync(new ExternalIdentityLink(elsewhere.Id, "oidc", "upstream-" + Guid.NewGuid().ToString("N")), Ct);
        }
        await Link();
        Assert.Equal("#/administrator/initialPassword", await Rejected(await Open(opener, "credopen", subject, DesignatedPassword), HttpStatusCode.Forbidden, "AUTHZ_DENIED"));
        Assert.Equal(("0", "1"), (await Tenants("credopen"), await Links(subject)));
        Assert.Equal((SignInStatus.PasswordChangeRequired, SignInStatus.Failed), (await SignInAs(phone, TemporaryPassword), await SignInAs(phone, DesignatedPassword)));
        Assert.DoesNotContain(await ReadPlatformChain(source), entry => entry.Record.CapabilityId == "qian.tenancy/admin.reset-password");
        // 不带初始密码的开户不受影响：手机号经本人验证的账号直接任管理员。
        var (vouched, _) = await NewPerson(business, "已验证");
        await Read<TenantDescriptor>(await Open(opener, "credplain", vouched.Id, null), HttpStatusCode.Created);
        // 两项权限都有时照常：同一租户号开户成功（前面被拒的请求没有留下开户输入），账号的密码设为初始密码，外部身份绑定解除，平台链记下重置。
        grants.Limit("opener", CreatePermission, CredentialPermission);
        var credited = (await Read<TenantDescriptor>(await Open(opener, "credopen", subject, DesignatedPassword), HttpStatusCode.Created)).Id.ToString();
        Assert.Equal("0", await Links(subject));
        Assert.Equal((SignInStatus.PasswordChangeRequired, SignInStatus.Failed), (await SignInAs(phone, DesignatedPassword), await SignInAs(phone, TemporaryPassword)));
        Assert.Equal(subject, Assert.Single(await ReadPlatformChain(source), entry => entry.Record.CapabilityId == "qian.tenancy/admin.reset-password").Record.Payload.GetProperty("subjectId").GetGuid());

        // 二、更换管理员。只有指定权限的运营带初始密码指定三种账号（现任管理员、另一个未经本人验证的账号、已验证的账号），都是 403，pointer 指向初始密码；
        // 已验证的账号带初始密码本应 400，缺这项权限时先得到 403。
        var current = await Pointer(credited);
        await Link();
        var (other, otherPhone) = await Unverified("另一位");
        foreach (var target in new[] { subject, other, vouched.Id })
            Assert.Equal("#/items/0/initialPassword", await Rejected(await Replace(assigner, credited, target, NextPassword), HttpStatusCode.Forbidden, "AUTHZ_DENIED"));
        // 什么都没改：指针不变，另一位没有成为成员，两人的密码与现任管理员的外部身份绑定都在，租户链上没有重置记录。
        Assert.Equal((current, "0", "1"), (await Pointer(credited), await Members(credited, other), await Links(subject)));
        Assert.Equal((SignInStatus.PasswordChangeRequired, SignInStatus.Failed), (await SignInAs(phone, DesignatedPassword), await SignInAs(phone, NextPassword)));
        Assert.Equal(SignInStatus.PasswordChangeRequired, await SignInAs(otherPhone, TemporaryPassword));
        using (tenants.BeginScope(credited))
            Assert.DoesNotContain(await ReadTenantChain(source.Audit()), entry => entry.Record.CapabilityId == "qian.tenancy/admin.reset-password");
        // 不带初始密码的更换不受影响。
        var plain = Assert.Single((await Read<TenantAdminList>(await Replace(assigner, credited, vouched.Id, null), HttpStatusCode.OK)).Items);
        Assert.Equal(plain.MembershipId.ToString(), await Pointer(credited));
        // 重置管理员密码本就要这一项：assigner 没有，403。本操作的权限点先判：opener 没有指定权限，带初始密码也只在这一步被拒。两处都不带 pointer。
        Assert.Null(await Rejected(await Send(client, HttpMethod.Post, Reset(credited, plain.MembershipId), assigner, new ResetTenantAdminPassword(TemporaryPassword)),
            HttpStatusCode.Forbidden, "AUTHZ_DENIED"));
        Assert.Null(await Rejected(await Replace(opener, credited, subject, NextPassword), HttpStatusCode.Forbidden, "AUTHZ_DENIED"));
        // 两项权限都有时照常：换回原管理员，密码设为新的初始密码、外部身份绑定解除，租户链记下带自然人 id 的重置。
        grants.Limit("assigner", AssignPermission, CredentialPermission);
        var restored = Assert.Single((await Read<TenantAdminList>(await Replace(assigner, credited, subject, NextPassword), HttpStatusCode.OK)).Items);
        Assert.Equal((subject, restored.MembershipId.ToString(), "0"), (restored.SubjectId, await Pointer(credited), await Links(subject)));
        Assert.Equal((SignInStatus.PasswordChangeRequired, SignInStatus.Failed), (await SignInAs(phone, NextPassword), await SignInAs(phone, DesignatedPassword)));
        using (tenants.BeginScope(credited))
            Assert.Equal(subject, Assert.Single(await ReadTenantChain(source.Audit()), entry => entry.Record.CapabilityId == "qian.tenancy/admin.reset-password")
                .Record.Payload.GetProperty("subjectId").GetGuid());

        // 平台链：每次放行一条旁路记录，reset-credential 只在两项都有时出现，资源分别是租户集合与目标租户的 admins 集合。
        // 拒绝不写审计，原因只进结构化日志：opener 在开户时缺 reset-credential 一次、更换时缺指定权限一次；assigner 缺 reset-credential 四次（三次更换、一次重置）。
        var tenantPath = "tenancy/tenants/" + credited;
        var (openerAllowed, assignerAllowed) = (await Allowed("opener"), await Allowed("assigner"));
        Assert.Equal([(CreatePermission, "tenancy/tenants"), (CreatePermission, "tenancy/tenants"), (CreatePermission, "tenancy/tenants"), (CredentialPermission, "tenancy/tenants")], openerAllowed);
        Assert.Equal([.. Enumerable.Repeat((AssignPermission, (string?)tenantPath), 5), (CredentialPermission, tenantPath + "/admins")], assignerAllowed);
        Assert.Equal((1, 1, 4), (Denials("opener", CredentialPermission), Denials("opener", AssignPermission), Denials("assigner", CredentialPermission)));
        Assert.True((await source.Audit().VerifyPlatformAsync(source, Ct)).Valid);
    }

    private static string Admins(string tenant) => $"/api/tenancy/tenants/{tenant}/admins";
    private static string Reset(string tenant, Guid membership) => $"{Admins(tenant)}/{membership}:reset-password";

    private async Task<Ops> StartOps(OperatorGrants? grants = null, OperationsLog? logs = null)
    {
        var platform = new LifecyclePlatform { Management = true };
        RunningApp? business = null;
        try
        {
            business = await Start(lifecycle: platform, identity: true);
            var loader = business.App.Services.GetRequiredService<CrmCommandRuntime>().Loader;
            var management = await ManagementHost(business, platform, logs, grants is null ? null : () => new GrantedContext(loader.GetContext("tenancy", ScopeKey.Global), grants));
            return new Ops(platform, business, management, new HttpClient { BaseAddress = new Uri(management.Urls.Single()) });
        }
        catch
        {
            if (business is not null) await business.DisposeAsync();
            platform.Dispose(); throw;
        }
    }

    private sealed record Ops(LifecyclePlatform Platform, RunningApp Business, WebApplication Management, HttpClient Client) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            Client.Dispose(); await Management.StopAsync(Ct); await Management.DisposeAsync(); await Business.DisposeAsync(); Platform.Dispose();
        }
    }

    // 运营持有的平台权限点。平台目前只有超管一个平台角色，持角色即持全部已登记的权限点；用例经它收窄某位运营的权限点。
    // 收窄之外的判定原样交给真实旁路（持角色核对与平台链审计照常），收窄掉的权限点按旁路拒绝处理：不写审计，原因为 withheld。
    private sealed class OperatorGrants
    {
        private readonly ConcurrentDictionary<string, string[]> _limits = new(StringComparer.Ordinal);
        internal void Limit(string subject, params string[] permissions) => _limits[subject] = permissions;
        internal bool Withheld(SuperadminAccess access) => _limits.TryGetValue(access.Subject.Id, out var permitted) && !permitted.Contains(access.Permission, StringComparer.Ordinal);
    }

    private sealed class GrantedBypass(ISuperadminBypass inner, OperatorGrants grants) : ISuperadminBypass
    {
        public ValueTask<PermissionDecision> AuthorizeAsync(SuperadminAccess access, CancellationToken cancellationToken = default) =>
            grants.Withheld(access) ? ValueTask.FromResult(new PermissionDecision(false, "withheld", [])) : inner.AuthorizeAsync(access, cancellationToken);
    }

    // 运营入口每个请求解析的 qian.tenancy 上下文：只把超管旁路换成 GrantedBypass，其余原样转发。
    private sealed class GrantedContext(IPluginContext inner, OperatorGrants grants) : IPluginContext
    {
        public PluginEntry Entry => inner.Entry;
        public ScopeKey Scope => inner.Scope;
        public IServiceProvider Services => inner.Services;
        public ILogger Logger => inner.Logger;
        public TService Inject<TService>(ServiceKey key) where TService : class => Wrap(key, inner.Inject<TService>(key))!;
        public TService? TryInject<TService>(ServiceKey key) where TService : class => Wrap(key, inner.TryInject<TService>(key));
        private TService? Wrap<TService>(ServiceKey key, TService? service) where TService : class =>
            key == AuthzDefinition.SuperadminBypassKey && service is ISuperadminBypass bypass ? (TService)(object)new GrantedBypass(bypass, grants) : service;
        public IRegistrationHandle<TService> Provide<TService>(ServiceKey key, TService instance) where TService : class => inner.Provide(key, instance);
        public IRegistrationHandle<TService> Provide<TService>(ServiceKey key, string name, TService instance) where TService : class => inner.Provide(key, name, instance);
        public TService Inject<TService>(ServiceKey key, string name) where TService : class => inner.Inject<TService>(key, name);
        public ValueTask<TService> WhenReadyAsync<TService>(ServiceKey key, CancellationToken ct) where TService : class => inner.WhenReadyAsync<TService>(key, ct);
        public void Effect(IDisposable disposer) => inner.Effect(disposer);
        public void Effect(Action dispose) => inner.Effect(dispose);
        public void Effect(Func<ValueTask> disposeAsync) => inner.Effect(disposeAsync);
        public IDisposable On<TPayload, TResult>(KunEvent<TPayload, TResult> evt, DispatchMode mode, Delegate handler, int priority) => inner.On(evt, mode, handler, priority);
        public IDisposable On<TPayload, TResult>(KunEvent<TPayload, IReadOnlyList<TResult>> evt, DispatchMode mode, Delegate handler, int priority) => inner.On(evt, mode, handler, priority);
        public ValueTask<TResult> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, TResult> evt, TPayload payload, CancellationToken ct) => inner.DispatchAsync(evt, payload, ct);
        public ValueTask<IReadOnlyList<TResult>> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, IReadOnlyList<TResult>> evt, TPayload payload, CancellationToken ct) => inner.DispatchAsync(evt, payload, ct);
        public ValueTask<TResult> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, TResult> evt, TPayload payload, Func<ValueTask<TResult>> terminal, CancellationToken ct) => inner.DispatchAsync(evt, payload, terminal, ct);
        public IPluginContext Scoped(ScopeKey scope) => inner.Scoped(scope);
        public IDisposable Restrict(ScopeKey scope, Func<Registration, bool> keep) => inner.Restrict(scope, keep);
        public IReadOnlyList<Registration> List(ServiceKey key) => inner.List(key);
    }

    // 平台链上管理员维护的超管旁路记录；引导时安装、激活 system 组件的生命周期条目（S1-RT-037）与租户主档的放行不算。
    private static async Task<AuditRecord[]> Bypasses(IPluginContext source) =>
        [.. (await ReadPlatformChain(source)).Select(entry => entry.Record)
            .Where(record => record.Payload.ValueKind == JsonValueKind.Object && record.Payload.TryGetProperty("permission", out var permission) &&
                permission.GetString()!.StartsWith("qian.tenancy:admin:", StringComparison.Ordinal))];

    private static async Task<HttpResponseMessage> Send(HttpClient client, HttpMethod method, string url, string? token, object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) request.Content = JsonContent.Create(body, body.GetType());
        return await client.SendAsync(request, Ct);
    }

    private static async Task<T> Read<T>(HttpResponseMessage response, HttpStatusCode status)
    {
        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(Ct);
            Assert.True(response.StatusCode == status, body);
            return JsonSerializer.Deserialize<T>(body, JsonSerializerOptions.Web)!;
        }
    }

    // 断言错误码，返回错误位置（顶层 pointer 或第一个字段错误的 pointer）。
    private static async Task<string?> Rejected(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(Ct);
            Assert.True(response.StatusCode == status, body);
            var error = ErrorDetailJson.Deserialize(body)!;
            Assert.Equal(code, error.Code);
            return error.Pointer ?? (error.Errors is [var first, ..] ? first.Pointer : null);
        }
    }
}
