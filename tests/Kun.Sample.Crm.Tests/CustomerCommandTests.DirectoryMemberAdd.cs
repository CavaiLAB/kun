using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Json.Schema;
using Kun.Audit;
using Kun.Authorization;
using Kun.Authorization.Rbac;
using Kun.Authorization.Rbac.Abstractions;
using Kun.Capabilities;
using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore;
using Kun.Errors;
using Kun.EventBus;
using Kun.Identity.Abstractions;
using Kun.Identity.Server;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using Kun.Security;
using Kun.Surfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Kun.Sample.Crm.Tests;

// 租户管理员新增成员（S1-IDP-017）在参考宿主上的验收：真实 Authz（目录、默认角色、RBAC 判定与授予端口）、真实 Audit 与标准能力目录。
// 身份宿主用例（Kun.Identity.Server.Tests 的 DirectoryMemberAddTests）覆盖 HTTP 入口的规则、配额与恢复；这里验初始角色、能力契约、共用守卫与两个入口的审计。
public sealed partial class IdentityAuthzTests
{
    [Fact]
    public async Task 新增成员能力经真实授权与审计执行_新成员获初始角色tenant_user_能力标write与confirm且不对Agent暴露_非管理员与跨租户无写入()
    {
        const string id = "qian.identity.server/directory.member.add";
        const string initial = "Initial-Pass-2026";
        const string MemberCreate = "qian.identity.server:member:create";
        await using var host = await Start(capabilities: true, identity: true);
        // 代码派生的元数据与发布表面一致：权限目录、工具契约与默认角色（member:create 给 tenant.owner / tenant.admin）。
        var metadata = ((IComponentCodeMetadataFactory)Activator.CreateInstance(typeof(IdentityServerComponent).Assembly.GetCustomAttribute<KunCodeMetadataAttribute>()!.FactoryType)!).Create();
        var contract = Assert.Single(metadata.Capabilities, item => item.Id == id);
        var artifact = Path.Combine(RepositoryRoot(), "plugins/qian.identity.server");
        using var actualPermissions = JsonDocument.Parse(File.ReadAllText(Path.Combine(artifact, "metadata/permissions.json")));
        Assert.True(JsonElement.DeepEquals(metadata.Permissions, actualPermissions.RootElement));
        var tool = Directory.EnumerateFiles(Path.Combine(artifact, "tools"), "*.json").Select(file => JsonElement.Parse(File.ReadAllText(file)))
            .Single(item => item.GetProperty("name").GetString() == contract.Name);
        Assert.Equal(("write", "confirm", true, "pii"), (tool.GetProperty("sideEffect").GetString(), tool.GetProperty("policyHint").GetString(),
            tool.GetProperty("idempotent").GetBoolean(), tool.GetProperty("dataClassification").GetString()));
        Assert.False(tool.GetProperty("agent").GetProperty("expose").GetBoolean()); Assert.True(tool.GetProperty("agent").GetProperty("requiresConfirmation").GetBoolean());
        Assert.Equal(MemberCreate, tool.GetProperty("requiredPermission").GetString());
        var input = tool.GetProperty("inputSchema");
        Assert.Contains(input.GetProperty("required").EnumerateArray(), field => field.GetString() == "idempotencyKey");
        Assert.True(input.GetProperty("properties").GetProperty("initialPassword").GetProperty("sensitive").GetBoolean());
        var data = host.App.Services.GetRequiredService<IDataService>(); var tenant = host.App.Services.GetRequiredService<ITenantContext>();
        using var actualRoles = JsonDocument.Parse(File.ReadAllText(Path.Combine(artifact, "metadata/roles.json")));
        var catalog = new PermissionCatalogCompiler(new SurfaceValidator()).FromMetadata(metadata.ComponentId, "1.0.0", metadata.Permissions, actualRoles.RootElement);
        Assert.Equal(["tenant.admin", "tenant.owner"], catalog.Roles.Select(role => role.Role));
        Assert.All(catalog.Roles, role => Assert.Equal(["qian.identity.server:member:create", "qian.identity.server:member:disable"], role.Grants.Order(StringComparer.Ordinal)));
        Assert.Equal("Medium", Assert.Single(catalog.Permissions, item => item.Code == MemberCreate).Risk.ToString());
        using var registration = host.App.Services.GetRequiredService<IPermissionRegistry>().Register(catalog.Version, catalog.Permissions);
        await new PostgresPermissionCatalogStore(data).PutAsync(catalog, Ct);
        var (admin, adminPhone) = await NewPerson(host, "目录管理员"); await Join(host, A, admin.Id, "管理员", administrator: true);
        var (member, memberPhone) = await NewPerson(host, "普通成员"); await Join(host, A, member.Id, "普通成员");
        var (bare, barePhone) = await NewPerson(host, "未授角色的管理员"); await Join(host, A, bare.Id, "未授角色的管理员", administrator: true);
        var (outsider, _) = await NewPerson(host, "乙方老员工"); await Join(host, B, outsider.Id, "乙方老员工");
        using var tenantScope = tenant.BeginScope(Guid.ParseExact(A, "D"));
        var roles = new PostgresTenantRoleStore(data, tenant);
        await roles.InstallDefaultsAsync(metadata.ComponentId, "1.0.0", null, "member-add-acceptance", Ct);
        await roles.AssignAsync("tenant.admin", "user", admin.Id.ToString("D"), "member-add-acceptance", Ct);
        // 对照：普通成员虽经真实 RBAC 持有 member:create，仍不能越过 Identity 的租户管理员守卫。
        await roles.AssignAsync("tenant.admin", "user", member.Id.ToString("D"), "member-add-acceptance", Ct);
        var administrator = await DirectoryUser(host, adminPhone, A); var nonAdministrator = await DirectoryUser(host, memberPhone, A);
        var unauthorized = await DirectoryUser(host, barePhone, A);
        var loader = host.Identity.Loader!; var caller = loader.GetContext("capabilities", ScopeKey.Global).Scoped(new ScopeKey("tenant", A));
        var service = caller.Capabilities(); var binding = caller.Inject<IExecutableCapability>(CapabilityBindingRegistration.Key, id);
        Assert.True(JsonElement.DeepEquals(contract.Metadata, binding.Contract.Metadata));
        var schema = JsonSchema.FromText(File.ReadAllText(Path.Combine(RepositoryRoot(), "contracts/v0/capability-entry.schema.json")), new BuildOptions { SchemaRegistry = new SchemaRegistry() });
        var permissions = host.App.Services.GetRequiredService<IPermissionChecker>();
        async Task<bool> Allowed(Guid person, string permission) =>
            (await permissions.CheckAsync(new PermissionRequest(new AuthorizationSubject(person.ToString("D"), A, "user"), permission), Ct)).Allow;
        async Task<string> Roles(Guid person, string tenantId) => await Text("SELECT coalesce(string_agg(r.code, ',' ORDER BY r.code), '') FROM plugin_qian_authz.role_assignments a " +
            $"JOIN plugin_qian_authz.roles r ON a.role_id = r.id WHERE a.tenant_id = '{tenantId}' AND a.subject_type = 'user' AND a.subject_id = '{person:D}'") ?? "";
        async Task<string> Persons(string phone) => await Text($"SELECT count(*) FROM plugin_qian_identity.person_identifiers WHERE value = '+86{phone}'") ?? "";
        await using var scope = host.App.Services.CreateAsyncScope();
        var accessor = host.App.Services.GetRequiredService<IHttpContextAccessor>(); var before = accessor.HttpContext;
        var current = new DirectoryCurrentUser(tenant, administrator.Principal!);
        accessor.HttpContext = new DefaultHttpContext { RequestServices = new DirectoryInvocationServices(scope.ServiceProvider, current) };
        try
        {
            var entry = Assert.Single((await service.ListAsync(caller, current, cancellationToken: Ct)).Items, item => item.GetProperty("id").GetString() == id);
            var check = schema.Evaluate(entry, new EvaluationOptions { OutputFormat = OutputFormat.List }); Assert.True(check.IsValid, JsonSerializer.Serialize(check));
            var phone = NewPhone(); var key = Guid.CreateVersion7();
            var request = JsonSerializer.SerializeToElement(new { idempotencyKey = key, identifier = phone, displayName = "新同事", initialPassword = initial });
            // 非管理员（有权限点）、管理员但没有权限点（未授任何角色）与跨租户作用域：都被拒，不建自然人与成员，不写审计。
            current.Snapshot = nonAdministrator.Principal!;
            Assert.Equal("AUTHZ_DENIED", (await Assert.ThrowsAsync<ErrorException>(() => binding.InvokeAsync(caller, request, Ct).AsTask())).Error.Code.Code);
            current.Snapshot = unauthorized.Principal!;
            Assert.Equal("AUTHZ_DENIED", (await Assert.ThrowsAsync<ErrorException>(() => binding.InvokeAsync(caller, request, Ct).AsTask())).Error.Code.Code);
            current.Snapshot = administrator.Principal!;
            Assert.Equal("NOT_FOUND", (await Assert.ThrowsAsync<ErrorException>(() => binding.InvokeAsync(caller.Scoped(new ScopeKey("tenant", B)), request, Ct).AsTask())).Error.Code.Code);
            Assert.Equal("0", await Persons(phone));
            Assert.DoesNotContain(await ReadTenantChain(caller.Audit()), row => row.Record.CapabilityId == id);
            // 守卫核对服务端会话与身份修订：快照里的身份修订落后于服务端（如授权变更之后）一律拒绝；带 act 的委托主体同样拒绝（S1 不执行 Agent 委托）。
            await host.Identity.Accounts.BumpIdentityRevisionAsync(admin.Id, "member-add-acceptance", Ct);
            Assert.Equal("AUTHZ_DENIED", (await Assert.ThrowsAsync<ErrorException>(() => binding.InvokeAsync(caller, request, Ct).AsTask())).Error.Code.Code);
            administrator = await DirectoryUser(host, adminPhone, A); current.Snapshot = administrator.Principal!;
            var delegated = System.Text.Json.Nodes.JsonNode.Parse(JsonSerializer.Serialize(administrator.Principal, IdentityJson.PrincipalTypeInfo))!;
            delegated[IdentityClaimNames.Actor] = new System.Text.Json.Nodes.JsonObject { [IdentityClaimNames.Subject] = "agent-member-add", [IdentityClaimNames.PrincipalKind] = "agent" };
            current.Snapshot = delegated.Deserialize(IdentityJson.PrincipalTypeInfo)!;
            Assert.Equal("AUTHZ_DENIED", (await Assert.ThrowsAsync<ErrorException>(() => binding.InvokeAsync(caller, request, Ct).AsTask())).Error.Code.Code);
            current.Snapshot = administrator.Principal!;
            // 幂等键不能为空：直接调用绑定也拒绝，不执行、不写审计。
            var emptyKey = JsonSerializer.SerializeToElement(new { idempotencyKey = Guid.Empty, identifier = phone, displayName = "新同事", initialPassword = initial });
            Assert.Equal("INPUT_INVALID", (await Assert.ThrowsAsync<ErrorException>(() => binding.InvokeAsync(caller, emptyKey, Ct).AsTask())).Error.Code.Code);
            Assert.Equal("0", await Persons(phone));
            Assert.DoesNotContain(await ReadTenantChain(caller.Audit()), row => row.Record.CapabilityId == id);
            // 管理员经标准入口新增：新建自然人，初始角色 tenant.user 已经授予——有参考应用的 customer:read，没有 customer:write。
            var first = await service.InvokeAsync(caller, current, id, request, key, Ct);
            Assert.False(first.Replayed);
            Assert.True(first.Output.GetProperty("personCreated").GetBoolean()); Assert.False(first.Output.GetProperty("restored").GetBoolean());
            Assert.Equal(["tenant.user"], first.Output.GetProperty("grantedRoles").EnumerateArray().Select(role => role.GetString()));
            var newPerson = first.Output.GetProperty("personId").GetGuid(); var newMember = first.Output.GetProperty("membershipId").GetGuid();
            Assert.Equal("tenant.user", await Roles(newPerson, A));
            Assert.True(await Allowed(newPerson, "qian.sample.crm:customer:read")); Assert.False(await Allowed(newPerson, CustomerWrite));
            Assert.False(await Allowed(newPerson, MemberCreate));
            // 授予人与事件来源都是发起新增的管理员。
            Assert.Equal(admin.Id.ToString("D"), await Text("SELECT a.assigned_by FROM plugin_qian_authz.role_assignments a JOIN plugin_qian_authz.roles r ON a.role_id = r.id " +
                $"WHERE a.tenant_id = '{A}' AND a.subject_type = 'user' AND a.subject_id = '{newPerson:D}' AND r.code = 'tenant.user'"));
            var created = KunEventEnvelope.Parse((await Text("SELECT payload FROM plugin_qian_identity.outbox_messages WHERE event_type_id = 'directory.member.created.v1' " +
                $"AND payload LIKE '%{newMember:D}%'"))!);
            Assert.Equal((EventActorKind.User, admin.Id.ToString("D")), (created.Actor.Kind, created.Actor.Id));
            // 能力标 pii：回执只记「已执行」不存响应体（组件规格 §9.5），同键同输入得到 409 CAPABILITY_REPLAY_UNAVAILABLE、不再执行；改输入 409 CONFLICT。
            var replay = await Assert.ThrowsAsync<ErrorException>(() => service.InvokeAsync(caller, current, id, request, key, Ct).AsTask());
            Assert.Equal(("CAPABILITY_REPLAY_UNAVAILABLE", "data_classification"), (replay.Error.Code.Code, replay.Error.Args["reason"]));
            Assert.Equal("1", await Persons(phone));
            var changed = JsonSerializer.SerializeToElement(new { idempotencyKey = key, identifier = NewPhone(), displayName = "另一个人", initialPassword = initial });
            Assert.Equal("CONFLICT", (await Assert.ThrowsAsync<ErrorException>(() => service.InvokeAsync(caller, current, id, changed, key, Ct).AsTask())).Error.Code.Code);
            // 号码已被占用：返回 IDENTIFIER_IN_USE，甲方不授予任何角色；错误位置换成能力输入的字段。
            var outsiderPhone = await Text($"SELECT value FROM plugin_qian_identity.person_identifiers WHERE person_id = '{outsider.Id:D}'");
            var taken = await Assert.ThrowsAsync<ErrorException>(() => service.InvokeAsync(caller, current, id,
                JsonSerializer.SerializeToElement(new { idempotencyKey = Guid.CreateVersion7(), identifier = outsiderPhone, displayName = "老员工（甲方）", initialPassword = initial }), null, Ct).AsTask());
            Assert.Equal(("IDENTIFIER_IN_USE", "#/identifier"), (taken.Error.Code.Code, taken.Error.Pointer));
            Assert.Equal(("", ""), (await Roles(outsider.Id, A), await Roles(outsider.Id, B)));
            Assert.Equal("0", await Text($"SELECT count(*) FROM plugin_qian_identity.memberships WHERE tenant_id = '{A}' AND person_id = '{outsider.Id:D}'"));
            // 重复新增同样是已被占用。缺初始密码：标准入口先按输入 Schema 拒绝并指出字段，直接调用绑定在反序列化时拒绝（必填构造参数）；都不执行、不写审计。
            var duplicate = await Assert.ThrowsAsync<ErrorException>(() => service.InvokeAsync(caller, current, id,
                JsonSerializer.SerializeToElement(new { idempotencyKey = Guid.CreateVersion7(), identifier = phone, displayName = "再加一次", initialPassword = initial }), null, Ct).AsTask());
            Assert.Equal(("IDENTIFIER_IN_USE", "#/identifier"), (duplicate.Error.Code.Code, duplicate.Error.Pointer));
            var withoutPassword = JsonSerializer.SerializeToElement(new { idempotencyKey = Guid.CreateVersion7(), identifier = NewPhone(), displayName = "缺密码" });
            var missing = await Assert.ThrowsAsync<ErrorException>(() => service.InvokeAsync(caller, current, id, withoutPassword, null, Ct).AsTask());
            Assert.Equal(("INPUT_INVALID", "#/initialPassword"), (missing.Error.Code.Code, Assert.Single(missing.Error.Errors).Pointer));
            Assert.Equal("INPUT_INVALID", (await Assert.ThrowsAsync<ErrorException>(() => binding.InvokeAsync(caller, withoutPassword, Ct).AsTask())).Error.Code.Code);
            // 审计进甲方的链：操作人是管理员，成员在载荷里；成功一次有开始与成功记录，被占用两次各有开始与失败记录、失败记录带实际的错误码；任何记录都不含初始密码。
            var records = (await ReadTenantChain(caller.Audit())).Where(row => row.Record.CapabilityId == id).Select(row => row.Record).ToArray();
            Assert.All(records, record => Assert.Equal((A, "user", admin.Id.ToString("D"), "api"), (record.TenantId!, record.ActorType, record.ActorId, record.Channel)));
            var success = Assert.Single(records, record => record.Result == "success");
            Assert.Equal((newMember, newPerson, true, false), (success.Payload.GetProperty("membershipId").GetGuid(), success.Payload.GetProperty("personId").GetGuid(),
                success.Payload.GetProperty("personCreated").GetBoolean(), success.Payload.GetProperty("restored").GetBoolean()));
            Assert.Equal(["tenant.user"], success.Payload.GetProperty("grantedRoles").EnumerateArray().Select(role => role.GetString()));
            Assert.Equal(3, records.Count(record => record.Result == "started"));
            Assert.Equal(["IDENTIFIER_IN_USE", "IDENTIFIER_IN_USE"],
                records.Where(record => record.Result == "failure").Select(record => record.Payload.GetProperty("code").GetString()));
            Assert.DoesNotContain(initial, JsonSerializer.Serialize(records.Select(record => new { record.Payload, record.InputHash })), StringComparison.Ordinal);
        }
        finally { accessor.HttpContext = before; }
    }

    // HTTP 入口（POST /api/directory/members）在挂了 Audit 的参考宿主上：与标准入口共用 DirectoryMemberAddition 的审计，渠道记 ui；
    // 初始角色在身份事务提交之前授予，授予失败时自然人、成员、目录事件都不落库，原样重试成功。
    [Fact]
    public async Task 新增成员HTTP入口写目标租户审计_渠道ui_操作人是管理员_输入哈希不含初始密码_初始角色授予失败时整体回滚()
    {
        const string id = "qian.identity.server/directory.member.add";
        await using var host = await Start(capabilities: true, identity: true);
        var tenant = host.App.Services.GetRequiredService<ITenantContext>(); var loader = host.Identity.Loader!;
        var (admin, adminPhone) = await NewPerson(host, "甲管理员"); await Join(host, A, admin.Id, "甲管理员", administrator: true);
        using var browser = new MemberBrowser(host.Identity.Issuer);
        Assert.Equal(200, (await browser.Post("api/identity/login", new { client_id = "web", identifier = adminPhone, password = IdentityPassword, tenant_id = A })).Status);
        async Task<AuditRecord[]> Records()
        {
            using var scope = tenant.BeginScope(Guid.ParseExact(A, "D"));
            var caller = loader.GetContext("capabilities", ScopeKey.Global).Scoped(new ScopeKey("tenant", A));
            return [.. (await ReadTenantChain(caller.Audit())).Where(row => row.Record.CapabilityId == id).Select(row => row.Record)];
        }
        // 新建：审计有开始与成功两条，操作人是管理员、渠道 ui；成功载荷带成员、自然人、是否新建与授予的角色。授予人是这位管理员。
        var phone = NewPhone();
        var added = await browser.Post("api/directory/members", new { identifier = phone, display_name = "HTTP 新成员", initial_password = "Initial-Pass-2026" });
        Assert.Equal(200, added.Status);
        using var body = JsonDocument.Parse(added.Text);
        var member = body.RootElement.GetProperty("membership").GetProperty("id").GetGuid(); var person = body.RootElement.GetProperty("membership").GetProperty("person_id").GetGuid();
        Assert.Equal(["tenant.user"], body.RootElement.GetProperty("granted_roles").EnumerateArray().Select(role => role.GetString()));
        Assert.Equal(admin.Id.ToString("D"), await Text("SELECT a.assigned_by FROM plugin_qian_authz.role_assignments a JOIN plugin_qian_authz.roles r ON a.role_id = r.id " +
            $"WHERE a.tenant_id = '{A}' AND a.subject_type = 'user' AND a.subject_id = '{person:D}' AND r.code = 'tenant.user'"));
        var records = await Records();
        Assert.Equal(["started", "success"], records.Select(record => record.Result));
        Assert.All(records, record => Assert.Equal((A, "user", admin.Id.ToString("D"), "ui"), (record.TenantId!, record.ActorType, record.ActorId, record.Channel)));
        Assert.Equal((member, person, true, false), (records[1].Payload.GetProperty("membershipId").GetGuid(), records[1].Payload.GetProperty("personId").GetGuid(),
            records[1].Payload.GetProperty("personCreated").GetBoolean(), records[1].Payload.GetProperty("restored").GetBoolean()));
        Assert.Equal(["tenant.user"], records[1].Payload.GetProperty("grantedRoles").EnumerateArray().Select(role => role.GetString()));
        // 输入哈希只取脱敏投影：两次请求只差初始密码时哈希相同，只差显示名时不同；失败记录带实际的错误码。
        var hashed = NewPhone();
        Assert.Equal(200, (await browser.Post("api/directory/members", new { identifier = hashed, display_name = "哈希", initial_password = "Initial-Pass-2026" })).Status);
        Assert.Equal(409, (await browser.Post("api/directory/members", new { identifier = hashed, display_name = "哈希", initial_password = "Another-Pass-2027" })).Status);
        Assert.Equal(409, (await browser.Post("api/directory/members", new { identifier = hashed, display_name = "哈希二", initial_password = "Another-Pass-2027" })).Status);
        var started = (await Records()).Where(record => record.Result == "started").Select(record => record.InputHash).TakeLast(3).ToArray();
        Assert.False(string.IsNullOrEmpty(started[0]));
        Assert.Equal(started[0], started[1]); Assert.NotEqual(started[1], started[2]);
        Assert.Equal(["IDENTIFIER_IN_USE", "IDENTIFIER_IN_USE"], (await Records()).Where(record => record.Result == "failure").Select(record => record.Payload.GetProperty("code").GetString()));
        Assert.DoesNotContain("Pass-202", JsonSerializer.Serialize((await Records()).Select(record => new { record.Payload, record.InputHash })), StringComparison.Ordinal);
        // 甲方没有内置角色 tenant.user（挂授权组件之前开通的租户）：授予失败，什么都不落库，审计记失败；角色补上之后原样重试成功。
        await Admin($"UPDATE plugin_qian_authz.roles SET code = 'tenant.user.gone' WHERE tenant_id = '{A}' AND code = 'tenant.user'");
        var orphan = NewPhone();
        async Task<string> Facts() => string.Join('|', await Text($"SELECT count(*) FROM plugin_qian_identity.person_identifiers WHERE value = '+86{orphan}'"),
            await Text($"SELECT count(*) FROM plugin_qian_identity.memberships WHERE tenant_id = '{A}'"),
            await Text($"SELECT coalesce(max(revision), 0) FROM plugin_qian_identity.directory_revisions WHERE tenant_id = '{A}'"),
            await Text("SELECT count(*) FROM plugin_qian_identity.outbox_messages"));
        var before = await Facts();
        var failed = await browser.Post("api/directory/members", new { identifier = orphan, display_name = "授予会失败", initial_password = "Initial-Pass-2026" });
        Assert.Equal(404, failed.Status);
        Assert.Equal(before, await Facts());
        Assert.Equal("NOT_FOUND", (await Records())[^1].Payload.GetProperty("code").GetString());
        await Admin($"UPDATE plugin_qian_authz.roles SET code = 'tenant.user' WHERE tenant_id = '{A}' AND code = 'tenant.user.gone'");
        Assert.Equal(200, (await browser.Post("api/directory/members", new { identifier = orphan, display_name = "原样重试", initial_password = "Initial-Pass-2026" })).Status);

        // 恢复原成员后只保留 tenant.user（R163）：在职时另授 tenant.admin，从目录删除后同号重新新增，恢复的成员只有 tenant.user。
        async Task<string> Roles(Guid subject) => await Text("SELECT coalesce(string_agg(r.code, ',' ORDER BY r.code), '') FROM plugin_qian_authz.role_assignments a " +
            $"JOIN plugin_qian_authz.roles r ON a.role_id = r.id WHERE a.tenant_id = '{A}' AND a.subject_type = 'user' AND a.subject_id = '{subject:D}'") ?? "";
        using (tenant.BeginScope(Guid.ParseExact(A, "D")))
            await loader.GetContext("identity", ScopeKey.Global).Scoped(new ScopeKey("tenant", A)).Inject<ITenantRoleGrant>(TenantRoleGrant.Key)
                .AssignAsync("tenant.admin", "user", person.ToString("D"), admin.Id.ToString("D"), Ct);
        Assert.Equal("tenant.admin,tenant.user", await Roles(person));
        // 在职成员的标识再新增一次是 409，他的角色不动。
        Assert.Equal(409, (await browser.Post("api/directory/members", new { identifier = phone, display_name = "在职重复", initial_password = "Initial-Pass-2026" })).Status);
        Assert.Equal("tenant.admin,tenant.user", await Roles(person));
        Assert.Equal(200, (await browser.Send(HttpMethod.Delete, $"api/directory/members/{member:D}")).Status);
        var restored = await browser.Post("api/directory/members", new { identifier = phone, display_name = "HTTP 老成员", initial_password = "Initial-Pass-2026" });
        Assert.Equal(200, restored.Status);
        using var back = JsonDocument.Parse(restored.Text);
        Assert.Equal((member, true), (back.RootElement.GetProperty("membership").GetProperty("id").GetGuid(), back.RootElement.GetProperty("restored").GetBoolean()));
        Assert.Equal(["tenant.user"], back.RootElement.GetProperty("granted_roles").EnumerateArray().Select(role => role.GetString()));
        Assert.Equal("tenant.user", await Roles(person));
    }

    // 带 Cookie 的浏览器：每个写请求先取一枚防伪令牌，和前端一致。
    private sealed class MemberBrowser(string issuer) : IDisposable
    {
        private readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = true, CookieContainer = new() }) { BaseAddress = new Uri(issuer) };
        internal Task<(int Status, string Text)> Post(string path, object body) => Send(HttpMethod.Post, path, body);
        internal async Task<(int Status, string Text)> Send(HttpMethod method, string path, object? body = null)
        {
            using var csrfResponse = await _http.GetAsync("api/identity/csrf", Ct); csrfResponse.EnsureSuccessStatusCode();
            var csrf = await csrfResponse.Content.ReadFromJsonAsync<JsonElement>(Ct);
            using var request = new HttpRequestMessage(method, path) { Content = body is null ? null : JsonContent.Create(body) };
            request.Headers.Add(csrf.GetProperty("header").GetString()!, csrf.GetProperty("token").GetString()!);
            using var response = await _http.SendAsync(request, Ct);
            return ((int)response.StatusCode, await response.Content.ReadAsStringAsync(Ct));
        }
        public void Dispose() => _http.Dispose();
    }
}
