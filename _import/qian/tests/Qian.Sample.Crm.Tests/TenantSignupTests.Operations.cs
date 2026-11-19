using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Qian.Audit;
using Kun.Identity.Abstractions;
using Kun.Runtime;
using Qian.Surfaces.Abstractions;
using Kun.Tenancy;
using Kun.Tenancy.Signup;

namespace Qian.Sample.Crm.Tests;

// 开通申请的平台运营一侧（S1-TEN-019）：运营宿主上的列表、详情、通过、驳回、重试与状态机元数据；
// 授权只走平台超管显式旁路（每次放行在平台审计链记一条），开通经 qian.tenancy 的开通编排与生命周期端口完成。
public sealed partial class TenantSignupTests
{
    private const string Applications = "/api/signup/applications";
    private const string StateMachine = "/api/signup/meta/state-machine";
    private const string Permission = "qian.tenancy.signup:application:";
    private static readonly string[] AdministratorSteps = ["05:membership", "06:administrator", "07:owner"];

    private static string Detail(Guid id) => $"{Applications}/{id}";
    private static string Act(Guid id, string action) => $"{Applications}/{id}:{action}";

    // R163：号码在既有账号上没有经本人验证、账号也没有加入任何企业时，核验过验证码的申请人按新账号填写姓名与密码，提交时认领这个账号，
    // 批准后新租户的管理员就是这个自然人。已加入企业的账号不认领，按账号不可申请拒绝。
    [Fact]
    public async Task 号码在既有账号上未经本人验证时按新账号填写_提交即认领该账号_换成申请人的密码与姓名并标为已验证_批准后申请人以自己的密码登录新租户_已加入企业的账号不认领()
    {
        await using var ops = await StartOps();
        var (business, client) = (ops.Business, ops.Client);
        var accounts = business.Identity.Accounts;
        string Root() => business.Token(null, "", "root", "user");
        const string previous = "Previous-Pass-1"; const string chosen = "Claimant-Pass-2026";
        async Task<Person> Unverified(string number, string name)
        {
            using var proof = new AuthenticationProof(Encoding.UTF8.GetBytes(previous));
            return await accounts.CreatePersonAsync(new CreatePerson(name, [new IdentifierInput(IdentifierKind.Phone, number, false)]) { InitialPassword = proof, RequirePasswordChange = false }, Ct);
        }
        static string? Failure(SignInResult result) => result.Error?.Code.Code;
        async Task<string> ClaimDirectly(Guid person, string number, string name)
        {
            using var proof = new AuthenticationProof(Encoding.UTF8.GetBytes("Direct-Claim-2026"));
            return (await Assert.ThrowsAsync<Kun.Errors.ErrorException>(() =>
                accounts.ClaimPersonAsync(new PersonClaim(person, number, name, proof), Ct).AsTask())).Error.Code.Code;
        }

        // 已加入企业的账号：核验时按账号不可申请拒绝（403），端口同样拒绝（CONFLICT）；密码、姓名与号码状态都不变。
        var memberPhone = NewPhone();
        var member = await Unverified(memberPhone, "企业成员");
        await Join(business, A, member.Id, "A 的成员");
        using (var blocked = new Browser(business.Client.BaseAddress!))
        {
            var challenge = await SendCode(blocked, memberPhone);
            await Rejected(await blocked.Send(HttpMethod.Post, Session, new { verificationId = challenge, code = ops.Outbox.Code(challenge) }), HttpStatusCode.Forbidden, "SIGNUP_ACCOUNT_UNAVAILABLE");
            Assert.Null(blocked.Token);
        }
        Assert.Equal("CONFLICT", await ClaimDirectly(member.Id, memberPhone, "认领人"));
        Assert.Equal((SignInStatus.Authenticated, false, "企业成员"), ((await SignIn(business, memberPhone, previous)).Status,
            await accounts.IsIdentifierVerifiedAsync(memberPhone, Ct), (await accounts.FindPersonAsync(memberPhone, Ct))!.DisplayName));
        // 核验之后、提交之前加入了企业：提交同样按账号不可申请拒绝，什么都不改。
        var latePhone = NewPhone();
        var late = await Unverified(latePhone, "后来加入");
        using (var lateBrowser = new Browser(business.Client.BaseAddress!))
        {
            Assert.Equal((Mask(latePhone), false), Applicant(await Verify(ops, lateBrowser, latePhone)));
            await Join(business, A, late.Id, "后来加入");
            await Rejected(await Submit(lateBrowser, Body("lateco", company: "后来科技", name: "认领人", password: chosen)), HttpStatusCode.Forbidden, "SIGNUP_ACCOUNT_UNAVAILABLE");
        }
        Assert.Equal((SignInStatus.Authenticated, false), ((await SignIn(business, latePhone, previous)).Status, await accounts.IsIdentifierVerifiedAsync(latePhone, Ct)));
        Assert.Equal("0", await Text($"SELECT count(*) FROM {Schema}.applications WHERE code = 'lateco'"));

        // 没有加入企业的账号：它登录不了任何租户，密码对不对看错误码（对了 NO_AVAILABLE_TENANT，不对 INVALID_CREDENTIALS）。
        var phone = NewPhone();
        var original = await Unverified(phone, "原来的姓名");
        Assert.Equal("NO_AVAILABLE_TENANT", Failure(await SignIn(business, phone, previous)));
        Assert.False(await accounts.IsIdentifierVerifiedAsync(phone, Ct));
        // 核验通过：申请人看到新账号表单，没有那个账号的姓名。
        using var browser = new Browser(business.Client.BaseAddress!);
        var applicant = await Verify(ops, browser, phone);
        Assert.Equal((Mask(phone), false), Applicant(applicant));
        Assert.False(applicant.TryGetProperty("displayName", out var shown) && shown.ValueKind == JsonValueKind.String);
        // 按既有账号提交（不带姓名与密码）是缺必填；弱密码按密码策略拒绝、位置在申请的字段上；两次都什么都不改。
        Assert.Equal("#/displayName", (await Rejected(await Submit(browser, ExistingBody("claimco")), HttpStatusCode.BadRequest, "INPUT_INVALID")).Field());
        Assert.Equal("#/password", (await Rejected(await Submit(browser, Body("claimco", name: "认领人", password: "short")), HttpStatusCode.UnprocessableEntity, "PASSWORD_POLICY_VIOLATION")).Field());
        Assert.Equal("NO_AVAILABLE_TENANT", Failure(await SignIn(business, phone, previous)));
        Assert.Equal("0", await Text($"SELECT count(*) FROM {Schema}.applications WHERE code = 'claimco'"));
        // 提交：认领原账号，不新建自然人；号码标为已验证，换成申请人的姓名与密码，原密码不能再登录。
        var id = (await Json(await Submit(browser, Body("claimco", company: "认领科技", name: "认领人", password: chosen)), HttpStatusCode.Created)).GetProperty("id").GetGuid();
        Assert.Equal(original.Id.ToString("D"), await Text($"SELECT person_id::text FROM {Schema}.applications WHERE id = '{id}'"));
        Assert.Equal("1", await Text($"SELECT count(*) FROM plugin_qian_identity.person_identifiers WHERE value = '+86{phone}'"));
        Assert.True(await accounts.IsIdentifierVerifiedAsync(phone, Ct));
        Assert.Equal("认领人", (await accounts.FindPersonAsync(phone, Ct))!.DisplayName);
        Assert.Equal(("INVALID_CREDENTIALS", "NO_AVAILABLE_TENANT"), (Failure(await SignIn(business, phone, previous)), Failure(await SignIn(business, phone, chosen))));
        // 号码经本人验证之后不能再被认领：端口直接拒绝（CONFLICT），姓名与密码都不变。
        Assert.Equal("CONFLICT", await ClaimDirectly(original.Id, phone, "又一个人"));
        Assert.Equal(("认领人", "NO_AVAILABLE_TENANT"), ((await accounts.FindPersonAsync(phone, Ct))!.DisplayName, Failure(await SignIn(business, phone, chosen))));
        // 之后这个号码就是经本人验证的既有账号：同一会话再读申请人，是「以既有账号申请」。
        Assert.True((await Json(await browser.Send(HttpMethod.Get, Session), HttpStatusCode.OK)).GetProperty("existingAccount").GetBoolean());
        // 运营看到的申请人：自然人原本就在（已有账号），联系人是认领人。
        var profile = (await Json(await Operate(client, HttpMethod.Get, Detail(id), Root()), HttpStatusCode.OK)).GetProperty("applicant");
        Assert.Equal((original.Id, "认领人", true), (profile.GetProperty("subjectId").GetGuid(), profile.GetProperty("contactName").GetString(), profile.GetProperty("existingAccount").GetBoolean()));
        // 批准：新租户的管理员就是这个自然人，申请人用自己设的密码登录；原密码不能再登录。
        var approved = await Json(await Operate(client, HttpMethod.Post, Act(id, "approve"), Root(), new { plan = "trial" }), HttpStatusCode.OK);
        var tenant = approved.GetProperty("provisioning").GetProperty("tenantId").GetGuid();
        Assert.Equal(original.Id.ToString("D"), await Owners(tenant));
        async Task<SignInResult> Into(string password)
        {
            using var proof = new AuthenticationProof(Encoding.UTF8.GetBytes(password));
            return await business.Identity.SignIn.SignInAsync(new LoginInput("web", tenant, null, null, proof) { Identifier = phone }, new LoginContext(), Ct);
        }
        Assert.Equal((SignInStatus.Authenticated, tenant), ((await Into(chosen)).Status, (await Into(chosen)).Session!.TenantId));
        Assert.Equal(SignInStatus.Failed, (await Into(previous)).Status);

        // 运营入口新建的账号走同一条路：申请人按新账号填写，提交即认领。
        // 运营新建的账号首次登录必须改密，登录时拿到改密票据；认领之后这张票据作废，认领后也不再要求改密。
        const string opsInitial = "Ops-Initial-2026"; const string opsChosen = "Ops-Claimant-2026";
        var opsPhone = NewPhone();
        var opsSubject = (await Json(await Operate(client, HttpMethod.Post, "/api/tenancy/admin-accounts", Root(),
            new { displayName = "运营代建", identifierKind = "phone", identifier = opsPhone, initialPassword = opsInitial }), HttpStatusCode.Created)).GetProperty("subjectId").GetGuid();
        Assert.False(await accounts.IsIdentifierVerifiedAsync(opsPhone, Ct));
        var opsPending = await SignIn(business, opsPhone, opsInitial);
        Assert.Equal(SignInStatus.PasswordChangeRequired, opsPending.Status);
        using var second = new Browser(business.Client.BaseAddress!);
        Assert.Equal((Mask(opsPhone), false), Applicant(await Verify(ops, second, opsPhone)));
        var claimed = (await Json(await Submit(second, Body("opsclaim", company: "代建认领", name: "代建认领人", password: opsChosen)), HttpStatusCode.Created)).GetProperty("id").GetGuid();
        Assert.Equal(opsSubject.ToString("D"), await Text($"SELECT person_id::text FROM {Schema}.applications WHERE id = '{claimed}'"));
        Assert.True(await accounts.IsIdentifierVerifiedAsync(opsPhone, Ct));
        Assert.Equal("代建认领人", (await accounts.FindPersonAsync(opsPhone, Ct))!.DisplayName);
        using (var through = new AuthenticationProof(Encoding.UTF8.GetBytes("Through-Old-Ticket-1")))
            Assert.Equal("LOGIN_TICKET_INVALID", Failure(await business.Identity.SignIn.ChangeInitialPasswordAsync(new InitialPasswordChange(opsPending.Ticket!.Token, through), new LoginContext(), Ct)));
        Assert.Equal(("INVALID_CREDENTIALS", "NO_AVAILABLE_TENANT"), (Failure(await SignIn(business, opsPhone, opsInitial)), Failure(await SignIn(business, opsPhone, opsChosen))));
    }

    [Fact]
    public async Task 运营审核_只收平台超管且每次放行一条平台旁路记录_列表计数筛选与自动检查_通过即建租户与管理员成员申请人可登录_租户号冲突先修正_驳回原因可见备注不可见并释放租户号_发件端口缺席不阻断()
    {
        await using var ops = await StartOps();
        var (business, client) = (ops.Business, ops.Client);
        var address = business.Client.BaseAddress!;
        var accounts = business.Identity.Accounts;
        // 运营令牌一分钟有效，每次请求现签。
        string Root() => business.Token(null, "", "root", "user");

        // 四份申请：甲（新号码）、乙（已有账号，已是租户 A 的成员）、丙（与甲同名，新号码）、丁（新号码，将被驳回）。
        using var jia = new Browser(address); var jiaPhone = NewPhone();
        await Verify(ops, jia, jiaPhone);
        var jiaId = (await Json(await Submit(jia, Body("jiafang", company: "甲方科技", name: "申请人甲")), HttpStatusCode.Created)).GetProperty("id").GetGuid();
        var (yi, yiPhone) = await NewPerson(business, "老用户乙"); await Join(business, A, yi.Id, "老用户乙");
        using var yiBrowser = new Browser(address);
        await Verify(ops, yiBrowser, yiPhone);
        var yiApplication = await Json(await Submit(yiBrowser, ExistingBody("yifang", company: "乙方科技")), HttpStatusCode.Created);
        var yiId = yiApplication.GetProperty("id").GetGuid();
        using var bing = new Browser(address);
        await Verify(ops, bing, NewPhone());
        var bingId = (await Json(await Submit(bing, Body("bingfang", company: "甲方科技", name: "申请人丙")), HttpStatusCode.Created)).GetProperty("id").GetGuid();
        using var ding = new Browser(address);
        await Verify(ops, ding, NewPhone());
        var dingId = (await Json(await Submit(ding, Body("dingfang", company: "丁方科技", name: "申请人丁", size: "101-500")), HttpStatusCode.Created)).GetProperty("id").GetGuid();

        // 无凭据 401；租户主体、非超管的平台用户与 Agent 一律 403（运营入口只收人类平台超管）。
        var unknown = Guid.NewGuid();
        (HttpMethod Method, string Path, object? Body)[] endpoints =
        [
            (HttpMethod.Get, Applications, null), (HttpMethod.Get, Detail(unknown), null), (HttpMethod.Post, Act(unknown, "approve"), new { plan = "trial" }),
            (HttpMethod.Post, Act(unknown, "reject"), new { reason = "资料不全" }), (HttpMethod.Post, Act(unknown, "retry"), null), (HttpMethod.Get, StateMachine, null),
        ];
        foreach (var token in new[] { null, business.Token(A, "", "root", "user"), business.Token(null, "", "intruder", "user"), business.Token(null, "", "root", "agent") })
            foreach (var (method, path, body) in endpoints)
                await Rejected(await Operate(client, method, path, token, body), token is null ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden, token is null ? "UNAUTHENTICATED" : "AUTHZ_DENIED");
        // 申请人会话 Cookie 不是运营凭据。
        using (var request = new HttpRequestMessage(HttpMethod.Get, Applications))
        {
            request.Headers.Add("Cookie", TenancySignupEndpoints.CookieName + "=" + jia.Token);
            await Rejected(await client.SendAsync(request, Ct), HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
        }
        // 目标只取自路径：请求体带 tenantId / tenant_id / id 在绑定时拒绝；其它输入错误同样在旁路之前拒绝。
        foreach (var forbidden in new[] { "tenantId", "tenant_id", "id" })
        {
            await Rejected(await Operate(client, HttpMethod.Post, Act(jiaId, "approve"), Root(), new Dictionary<string, object?> { ["plan"] = "trial", [forbidden] = A }), HttpStatusCode.BadRequest, "BAD_REQUEST");
            await Rejected(await Operate(client, HttpMethod.Post, Act(dingId, "reject"), Root(), new Dictionary<string, object?> { ["reason"] = "资料不全", [forbidden] = A }), HttpStatusCode.BadRequest, "BAD_REQUEST");
            await Rejected(await Operate(client, HttpMethod.Post, Act(jiaId, "retry"), Root(), new Dictionary<string, object?> { [forbidden] = A }), HttpStatusCode.BadRequest, "BAD_REQUEST");
        }
        await Rejected(await Operate(client, HttpMethod.Post, Act(jiaId, "approve"), Root(), new { }), HttpStatusCode.BadRequest, "BAD_REQUEST");
        await Rejected(await Operate(client, HttpMethod.Post, Act(jiaId, "approve"), Root(), new { plan = "free" }), HttpStatusCode.BadRequest, "BAD_REQUEST");
        Assert.Equal("#/users", (await Rejected(await Operate(client, HttpMethod.Post, Act(yiId, "approve"), Root(), new { plan = "formal", users = 0 }), HttpStatusCode.BadRequest, "INPUT_INVALID")).Field());
        Assert.Equal("#/code", (await Rejected(await Operate(client, HttpMethod.Post, Act(bingId, "approve"), Root(), new { plan = "trial", code = "Bad-Code" }), HttpStatusCode.BadRequest, "INPUT_INVALID")).Field());
        Assert.Equal("#/reason", (await Rejected(await Operate(client, HttpMethod.Post, Act(dingId, "reject"), Root(), new { reason = "" }), HttpStatusCode.BadRequest, "INPUT_INVALID")).Field());
        foreach (var query in new[] { "?status=open", "?take=0", "?take=101", "?skip=-1", "?size=2-3", "?keyword=" + new string('x', 201) })
            await Rejected(await Operate(client, HttpMethod.Get, Applications + query, Root()), HttpStatusCode.BadRequest, "INPUT_INVALID");
        Assert.Empty(await Bypasses(business)); // 401 / 403 / 400 都在超管旁路之前拒绝，平台链上没有放行记录。

        // 列表：新的在前；计数只受关键字与规模筛选影响；手机号为掩码；每行只带最需要关注的一条检查。
        var all = await List("");
        Assert.Equal((4, 4, 4, 0), (all.GetProperty("total").GetInt32(), Counted(all, "all"), Counted(all, "submitted"), Counted(all, "approved")));
        var rows = all.GetProperty("items").EnumerateArray().ToArray();
        Assert.Equal([dingId, bingId, yiId, jiaId], rows.Select(row => row.GetProperty("id").GetGuid()));
        Assert.All(rows, row => Assert.Equal("submitted", row.GetProperty("status").GetString()));
        Assert.Equal(("乙方科技", "yifang", "21-100", "老用户乙", Mask(yiPhone)), (rows[2].GetProperty("companyName").GetString(), rows[2].GetProperty("code").GetString(),
            rows[2].GetProperty("size").GetString(), rows[2].GetProperty("contactName").GetString(), rows[2].GetProperty("phone").GetString()));
        // 甲、丙同名（警告，带数量）；乙已加入租户 A（提示，带数量）；丁全部通过。
        Assert.Equal<(string?, string?, int?)>(("duplicateName", "warning", 1), Check(rows[3].GetProperty("check")));
        Assert.Equal<(string?, string?, int?)>(("existingMemberships", "info", 1), Check(rows[2].GetProperty("check")));
        Assert.Equal<(string?, string?, int?)>(("duplicateName", "warning", 1), Check(rows[1].GetProperty("check")));
        Assert.Equal<(string?, string?, int?)>(("code", "pass", null), Check(rows[0].GetProperty("check")));
        Assert.Equal((0, 4), Page(await List("?status=approved")));
        Assert.Equal((2, 2), Page(await List("?keyword=" + Uri.EscapeDataString("甲方"))));
        Assert.Equal((1, 1), Page(await List("?keyword=" + yiApplication.GetProperty("number").GetString())));
        Assert.Equal((0, 0), Page(await List("?keyword=" + Uri.EscapeDataString("%"))));
        Assert.Equal((1, 1), Page(await List("?size=21-100")));
        Assert.Equal([dingId, bingId], (await List("?take=2")).GetProperty("items").EnumerateArray().Select(row => row.GetProperty("id").GetGuid()));
        Assert.Equal([jiaId], (await List("?skip=3&take=2")).GetProperty("items").EnumerateArray().Select(row => row.GetProperty("id").GetGuid()));

        // 详情：申请人（自然人 ID、掩码）、全部自动检查与审核缺省值；未审核时没有审核结果与开通进度。
        var yiDetail = await Json(await Operate(client, HttpMethod.Get, Detail(yiId), Root()), HttpStatusCode.OK);
        var profile = yiDetail.GetProperty("applicant");
        Assert.Equal((yi.Id, "老用户乙", Mask(yiPhone), true), (profile.GetProperty("subjectId").GetGuid(), profile.GetProperty("contactName").GetString(),
            profile.GetProperty("phone").GetString(), profile.GetProperty("existingAccount").GetBoolean()));
        Assert.Equal([("code", "pass", null), ("duplicateName", "pass", null), ("existingMemberships", "info", 1)], Checks(yiDetail));
        Assert.Equal((30, 20), (yiDetail.GetProperty("defaults").GetProperty("trialDays").GetInt32(), yiDetail.GetProperty("defaults").GetProperty("users").GetInt32()));
        Assert.False(Has(yiDetail, "review") || Has(yiDetail, "provisioning"));
        await Rejected(await Operate(client, HttpMethod.Get, Detail(Guid.NewGuid()), Root()), HttpStatusCode.NotFound, "NOT_FOUND");

        // 通过（试用、全用缺省值）：开通编排建租户（试用、到期日 = 今天 + 30 天、用户配额 20）与申请人的管理员成员，tenant.owner 落在申请人；
        // 申请人用自己提交时设的密码即可登录进新租户；申请人看到已通过，结果通知发出。
        var approved = await Json(await Operate(client, HttpMethod.Post, Act(jiaId, "approve"), Root(), new { plan = "trial" }), HttpStatusCode.OK);
        Assert.Equal("approved", approved.GetProperty("status").GetString());
        Assert.Empty(approved.GetProperty("checks").EnumerateArray());
        var review = approved.GetProperty("review");
        Assert.Equal(("root", "trial", 20), (review.GetProperty("reviewer").GetString(), review.GetProperty("plan").GetString(), review.GetProperty("users").GetInt32()));
        Assert.InRange(review.GetProperty("validUntil").GetDateTimeOffset(), DateTimeOffset.UtcNow.AddDays(30).AddHours(-1), DateTimeOffset.UtcNow.AddDays(30).AddHours(1));
        var provisioning = approved.GetProperty("provisioning");
        var tenant = provisioning.GetProperty("tenantId").GetGuid(); var membership = provisioning.GetProperty("membershipId").GetGuid();
        Assert.Equal(1, provisioning.GetProperty("attempts").GetInt32());
        Assert.False(Has(provisioning, "failureReason"));
        foreach (var step in AdministratorSteps) Assert.Equal("succeeded", Steps(provisioning)[step]);
        Assert.Equal("jiafang|甲方科技|trial|true|20", await Text("SELECT code || '|' || name || '|' || status || '|' || (abs(extract(epoch FROM valid_until - now()) - 30 * 86400) < 3600)::text || " +
            $"'|' || (quotas->>'users') FROM plugin_qian_tenancy.tenants WHERE id = '{tenant}'"));
        var jiaPerson = await accounts.FindPersonAsync(jiaPhone, Ct) ?? throw new InvalidOperationException("没有申请人甲。");
        Assert.Equal($"{membership}|{jiaPerson.Id}|true|active", await Text($"SELECT string_agg(id::text || '|' || person_id::text || '|' || is_tenant_administrator::text || '|' || status, ',') FROM plugin_qian_identity.memberships WHERE tenant_id = '{tenant}'"));
        Assert.Equal(jiaPerson.Id.ToString("D"), await Owners(tenant));
        var signedIn = await SignIn(business, jiaPhone, Password);
        Assert.Equal((SignInStatus.Authenticated, tenant, membership), (signedIn.Status, signedIn.Session!.TenantId, signedIn.Session.MembershipId));
        var jiaView = Assert.Single((await Json(await jia.Send(HttpMethod.Get, Mine), HttpStatusCode.OK)).GetProperty("items").EnumerateArray());
        Assert.Equal(("approved", "trial"), (jiaView.GetProperty("status").GetString(), jiaView.GetProperty("plan").GetString()));
        Assert.True(Has(jiaView, "validUntil") && Has(jiaView, "completedAt") && !Has(jiaView, "rejectReason"));
        var notified = Assert.Single(ops.Outbox.Results, message => message.Id == jiaId);
        Assert.Equal((SignupResultKind.Approved, "jiafang", "+86" + jiaPhone), (notified.Kind, notified.TenantCode, notified.Phone));
        // 已通过的不能再通过、驳回或重试。
        await Rejected(await Operate(client, HttpMethod.Post, Act(jiaId, "approve"), Root(), new { plan = "trial" }), HttpStatusCode.Conflict, "SIGNUP_STATE_CONFLICT");
        await Rejected(await Operate(client, HttpMethod.Post, Act(jiaId, "reject"), Root(), new { reason = "重复" }), HttpStatusCode.Conflict, "SIGNUP_STATE_CONFLICT");
        await Rejected(await Operate(client, HttpMethod.Post, Act(jiaId, "retry"), Root()), HttpStatusCode.Conflict, "SIGNUP_STATE_CONFLICT");

        // 租户号在审核前被占用：检查报错误；照原样通过 409（位置 #/code）；修正租户号后通过（试用、指定到期日与配额）。
        await Admin($"INSERT INTO plugin_qian_tenancy.tenants(id, code, name, isolation, status, created_at) VALUES ('{Guid.CreateVersion7()}'::uuid, 'bingfang', '别家', 'row', 'active', now());");
        var bingDetail = await Json(await Operate(client, HttpMethod.Get, Detail(bingId), Root()), HttpStatusCode.OK);
        Assert.Equal([("code", "error", null), ("duplicateName", "warning", 1), ("existingMemberships", "pass", 0)], Checks(bingDetail));
        Assert.Equal("taken", bingDetail.GetProperty("checks")[0].GetProperty("reason").GetString());
        Assert.Equal("#/code", (await Rejected(await Operate(client, HttpMethod.Post, Act(bingId, "approve"), Root(), new { plan = "trial" }), HttpStatusCode.Conflict, "SIGNUP_CODE_TAKEN")).Field());
        Assert.Equal("submitted", await Text($"SELECT status FROM {Schema}.applications WHERE id = '{bingId}'"));
        var until = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.AddDays(10).ToUnixTimeSeconds());
        var bingApproved = await Json(await Operate(client, HttpMethod.Post, Act(bingId, "approve"), Root(), new { plan = "trial", code = "bingfang2", validUntil = until, users = 5 }), HttpStatusCode.OK);
        Assert.Equal(("approved", "bingfang2"), (bingApproved.GetProperty("status").GetString(), bingApproved.GetProperty("company").GetProperty("code").GetString()));
        Assert.Equal((until, 5), (bingApproved.GetProperty("review").GetProperty("validUntil").GetDateTimeOffset(), bingApproved.GetProperty("review").GetProperty("users").GetInt32()));
        Assert.Equal("trial|true|5", await Text($"SELECT status || '|' || (valid_until = to_timestamp({until.ToUnixTimeSeconds()}))::text || '|' || (quotas->>'users') FROM plugin_qian_tenancy.tenants WHERE code = 'bingfang2'"));

        // 驳回：原因必填且对申请人可见，审核备注只给运营；驳回释放租户号，同一申请人可以另交新申请。
        var rejected = await Json(await Operate(client, HttpMethod.Post, Act(dingId, "reject"), Root(), new { reason = "资料不全", note = "内部备注：营业执照模糊" }), HttpStatusCode.OK);
        Assert.Equal(("rejected", "资料不全", "内部备注：营业执照模糊"), (rejected.GetProperty("status").GetString(), rejected.GetProperty("review").GetProperty("rejectReason").GetString(),
            rejected.GetProperty("review").GetProperty("note").GetString()));
        Assert.True(Has(rejected, "completedAt") && !Has(rejected, "provisioning"));
        await Rejected(await Operate(client, HttpMethod.Post, Act(dingId, "reject"), Root(), new { reason = "资料不全" }), HttpStatusCode.Conflict, "SIGNUP_STATE_CONFLICT");
        await Rejected(await Operate(client, HttpMethod.Post, Act(dingId, "approve"), Root(), new { plan = "trial" }), HttpStatusCode.Conflict, "SIGNUP_STATE_CONFLICT");
        var dingView = Assert.Single((await Json(await ding.Send(HttpMethod.Get, Mine), HttpStatusCode.OK)).GetProperty("items").EnumerateArray());
        Assert.Equal(("rejected", "资料不全"), (dingView.GetProperty("status").GetString(), dingView.GetProperty("rejectReason").GetString()));
        Assert.DoesNotContain(Strings(dingView), value => value.Contains("内部备注", StringComparison.Ordinal));
        var refusal = Assert.Single(ops.Outbox.Results, message => message.Id == dingId);
        Assert.Equal((SignupResultKind.Rejected, "资料不全", (string?)null), (refusal.Kind, refusal.Reason, refusal.TenantCode));
        Assert.Equal((true, null), await Availability(ding, "dingfang"));
        Assert.Equal("submitted", (await Json(await Submit(ding, ExistingBody("dingfang", company: "丁方科技")), HttpStatusCode.Created)).GetProperty("status").GetString());

        // 到期日须晚于当前时间（放行之后的业务校验，旁路已记录）。发件端口卸下后审核照常完成，只是没有通知。
        Assert.Equal("#/validUntil", (await Rejected(await Operate(client, HttpMethod.Post, Act(yiId, "approve"), Root(), new { plan = "formal", validUntil = DateTimeOffset.UtcNow.AddDays(-1) }),
            HttpStatusCode.BadRequest, "INPUT_INVALID")).Field());
        await Loader(business).UnmountAsync("signup.messenger", ScopeKey.Global, Ct);
        var sent = ops.Outbox.Results.Count;
        var yiApproved = await Json(await Operate(client, HttpMethod.Post, Act(yiId, "approve"), Root(), new { plan = "formal", note = "老客户" }), HttpStatusCode.OK);
        Assert.Equal(("approved", "formal", "老客户"), (yiApproved.GetProperty("status").GetString(), yiApproved.GetProperty("review").GetProperty("plan").GetString(),
            yiApproved.GetProperty("review").GetProperty("note").GetString()));
        Assert.False(Has(yiApproved.GetProperty("review"), "validUntil"));
        var yiTenant = yiApproved.GetProperty("provisioning").GetProperty("tenantId").GetGuid();
        Assert.Equal("yifang|active|true|20", await Text($"SELECT code || '|' || status || '|' || (valid_until IS NULL)::text || '|' || (quotas->>'users') FROM plugin_qian_tenancy.tenants WHERE id = '{yiTenant}'"));
        Assert.Equal(sent, ops.Outbox.Results.Count);
        // 既有账号作为新租户的管理员：原有的成员身份保留，新增一个（身份组件的成员归属查询）。
        var expectedTenants = new[] { Guid.Parse(A), yiTenant };
        Assert.Equal(expectedTenants.Order(), ((await accounts.ListPersonTenantsAsync(yi.Id, Ct)) ?? []).Order());
        Assert.Equal(yi.Id.ToString("D"), await Owners(yiTenant));

        // 机器可读的操作描述：通过、驳回只给人且要求确认，重试不对 Agent 暴露。
        var machine = await Json(await Operate(client, HttpMethod.Get, StateMachine, Root()), HttpStatusCode.OK);
        foreach (var name in new[] { "approve", "reject" })
            Assert.Equal((false, true), Agent(machine, name));
        Assert.False(Agent(machine, "retry").Expose);
        // 运营接口只在运营宿主上：业务宿主（申请人入口所在）没有这些路由。
        using (var request = new HttpRequestMessage(HttpMethod.Get, Applications))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Root());
            using var response = await business.Client.SendAsync(request, Ct);
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        }

        // 平台审计链：每次放行恰好一条超管旁路记录（主体、权限点、资源 = 目标申请），放行后才得出的 409 / 400 同样留有记录；重试按 approve 授权。
        var bypasses = await Bypasses(business);
        Assert.All(bypasses, record => Assert.Equal(("superadmin", "user", "root", "api"), (record.Payload.GetProperty("bypass").GetString(), record.ActorType, record.ActorId, record.Channel)));
        int Allowed(string action, Guid? id) => bypasses.Count(record => record.Payload.GetProperty("permission").GetString() == Permission + action &&
            record.Resource == (id is null ? "signup/applications" : "signup/applications/" + id.Value.ToString("D")));
        Assert.Equal((3, 1), (Allowed("approve", jiaId), Allowed("reject", jiaId)));
        Assert.Equal((2, 0), (Allowed("approve", bingId), Allowed("reject", bingId)));
        Assert.Equal((1, 2), (Allowed("approve", dingId), Allowed("reject", dingId)));
        Assert.Equal((2, 1), (Allowed("approve", yiId), Allowed("read", yiId)));
        Assert.Equal(0, Allowed("approve", unknown) + Allowed("reject", unknown) + Allowed("read", unknown));
        Assert.Equal(9, Allowed("read", null)); // 8 次列表 + 1 次状态机元数据
        var source = Loader(business).GetContext("tenancy", ScopeKey.Global);
        Assert.True((await source.Audit().VerifyPlatformAsync(source, Ct)).Valid);

        async Task<JsonElement> List(string query) => await Json(await Operate(client, HttpMethod.Get, Applications + query, Root()), HttpStatusCode.OK);
    }

    [Fact]
    public async Task 开通中途失败转failed且申请人仍见开通中_重试沿用同一租户与管理员成员收敛到approved_终态再重试409_不重复建租户与成员()
    {
        await using var ops = await StartOps();
        var (business, client) = (ops.Business, ops.Client);
        string Root() => business.Token(null, "", "root", "user");
        using var applicant = new Browser(business.Client.BaseAddress!);
        var phone = NewPhone();
        await Verify(ops, applicant, phone);
        var id = (await Json(await Submit(applicant, Body("retryco", company: "重试科技")), HttpStatusCode.Created)).GetProperty("id").GetGuid();
        var person = await business.Identity.Accounts.FindPersonAsync(phone, Ct) ?? throw new InvalidOperationException("没有建自然人。");
        // 待审核的申请不能重试（放行后由状态机拒绝）。
        await Rejected(await Operate(client, HttpMethod.Post, Act(id, "retry"), Root()), HttpStatusCode.Conflict, "SIGNUP_STATE_CONFLICT");

        // 注入故障：开通编排第 06 步（写管理员指针）失败；第 05 步（管理员成员）已成功。
        await Admin("CREATE FUNCTION plugin_qian_tenancy.signup_fault() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'injected fault'; END $$; " +
            "CREATE TRIGGER signup_fault BEFORE INSERT ON plugin_qian_tenancy.tenant_admins FOR EACH ROW EXECUTE FUNCTION plugin_qian_tenancy.signup_fault();");
        var failed = await Json(await Operate(client, HttpMethod.Post, Act(id, "approve"), Root(), new { plan = "trial" }), HttpStatusCode.OK);
        Assert.Equal("failed", failed.GetProperty("status").GetString());
        var provisioning = failed.GetProperty("provisioning");
        Assert.Equal(("step_failed", 1), (provisioning.GetProperty("failureReason").GetString(), provisioning.GetProperty("attempts").GetInt32()));
        var tenant = provisioning.GetProperty("tenantId").GetGuid(); var membership = provisioning.GetProperty("membershipId").GetGuid();
        var steps = Steps(provisioning);
        Assert.Equal(("succeeded", "failed"), (steps["05:membership"], steps["06:administrator"]));
        Assert.False(steps.ContainsKey("07:owner"));
        Assert.Equal("pending", await Text($"SELECT status FROM plugin_qian_tenancy.tenants WHERE id = '{tenant}'"));
        // 运营列表里是开通失败（仍算进行中，带检查）；申请人看到的仍是开通中，没有内部原因；还不能登录新租户，也没有结果通知。
        var row = Assert.Single((await Json(await Operate(client, HttpMethod.Get, Applications + "?status=failed", Root()), HttpStatusCode.OK)).GetProperty("items").EnumerateArray());
        Assert.Equal(id, row.GetProperty("id").GetGuid());
        Assert.True(Has(row, "check"));
        var waiting = Assert.Single((await Json(await applicant.Send(HttpMethod.Get, Mine), HttpStatusCode.OK)).GetProperty("items").EnumerateArray());
        Assert.Equal("provisioning", waiting.GetProperty("status").GetString());
        Assert.DoesNotContain(Strings(waiting), value => value.Contains("step_failed", StringComparison.Ordinal));
        Assert.NotEqual(SignInStatus.Authenticated, (await SignIn(business, phone, Password)).Status);
        Assert.Empty(ops.Outbox.Results);

        // 排除故障后重试：沿用同一租户与管理员成员，只续跑没成功的步骤，再激活；请求体非空在旁路之前拒绝。
        await Admin("DROP TRIGGER signup_fault ON plugin_qian_tenancy.tenant_admins; DROP FUNCTION plugin_qian_tenancy.signup_fault();");
        await Rejected(await Operate(client, HttpMethod.Post, Act(id, "retry"), Root(), new Dictionary<string, object?> { ["tenantId"] = A }), HttpStatusCode.BadRequest, "BAD_REQUEST");
        var retried = await Json(await Operate(client, HttpMethod.Post, Act(id, "retry"), Root()), HttpStatusCode.OK);
        Assert.Equal("approved", retried.GetProperty("status").GetString());
        var after = retried.GetProperty("provisioning");
        Assert.Equal((tenant, membership, 2), (after.GetProperty("tenantId").GetGuid(), after.GetProperty("membershipId").GetGuid(), after.GetProperty("attempts").GetInt32()));
        Assert.False(Has(after, "failureReason"));
        foreach (var step in AdministratorSteps) Assert.Equal("succeeded", Steps(after)[step]);
        Assert.Equal("1|trial", await Text("SELECT count(*)::text || '|' || min(status) FROM plugin_qian_tenancy.tenants WHERE code = 'retryco'"));
        Assert.Equal($"{membership}|{person.Id}", await Text($"SELECT string_agg(id::text || '|' || person_id::text, ',') FROM plugin_qian_identity.memberships WHERE tenant_id = '{tenant}'"));
        Assert.Equal(membership.ToString(), await Text($"SELECT membership_id::text FROM plugin_qian_tenancy.tenant_admins WHERE tenant_id = '{tenant}'"));
        Assert.Equal(person.Id.ToString("D"), await Owners(tenant));
        var signedIn = await SignIn(business, phone, Password);
        Assert.Equal((SignInStatus.Authenticated, tenant, membership), (signedIn.Status, signedIn.Session!.TenantId, signedIn.Session.MembershipId));
        Assert.Equal("approved", Assert.Single((await Json(await applicant.Send(HttpMethod.Get, Mine), HttpStatusCode.OK)).GetProperty("items").EnumerateArray()).GetProperty("status").GetString());
        Assert.Single(ops.Outbox.Results, message => message.Id == id && message.Kind == SignupResultKind.Approved);

        // 已通过再重试 409，什么都不变。平台链：重试（待审核）、通过、重试、再重试各一条，请求体被拒的那次没有。
        await Rejected(await Operate(client, HttpMethod.Post, Act(id, "retry"), Root()), HttpStatusCode.Conflict, "SIGNUP_STATE_CONFLICT");
        Assert.Equal("1", await Text($"SELECT count(*) FROM plugin_qian_identity.memberships WHERE tenant_id = '{tenant}'"));
        Assert.Equal(4, (await Bypasses(business)).Count(record => record.Payload.GetProperty("permission").GetString() == Permission + "approve" &&
            record.Resource == "signup/applications/" + id.ToString("D")));
    }

    [Fact]
    public async Task 开通编排正常返回但第05步被跳过时转failed且原因码取自步骤表_租户不激活也没有管理员成员_成员开户端口恢复后重试收敛到approved()
    {
        await using var ops = await StartOps();
        var (business, client) = (ops.Business, ops.Client);
        string Root() => business.Token(null, "", "root", "user");
        using var applicant = new Browser(business.Client.BaseAddress!);
        var phone = NewPhone();
        await Verify(ops, applicant, phone);
        var id = (await Json(await Submit(applicant, Body("skipco", company: "跳步科技")), HttpStatusCode.Created)).GetProperty("id").GetGuid();
        var person = await business.Identity.Accounts.FindPersonAsync(phone, Ct) ?? throw new InvalidOperationException("没有建自然人。");

        // 新租户里看不到身份组件的成员开户端口：第 05 步按端口缺席记 skipped，第 06、07 步照常成功，编排正常返回，没有异常可接。
        await Mount(business, MembershipHiddenDefinition());
        var failed = await Json(await Operate(client, HttpMethod.Post, Act(id, "approve"), Root(), new { plan = "trial" }), HttpStatusCode.OK);
        Assert.Equal("failed", failed.GetProperty("status").GetString());
        var provisioning = failed.GetProperty("provisioning");
        // 原因码取自步骤表：被跳过的第 05 步记的 identity_unavailable。
        Assert.Equal(("identity_unavailable", 1), (provisioning.GetProperty("failureReason").GetString(), provisioning.GetProperty("attempts").GetInt32()));
        var steps = provisioning.GetProperty("steps").EnumerateArray().ToDictionary(step => step.GetProperty("step").GetString()!,
            step => (step.GetProperty("status").GetString(), Has(step, "reason") ? step.GetProperty("reason").GetString() : null));
        Assert.Equal<(string?, string?)>([("skipped", "identity_unavailable"), ("succeeded", null), ("succeeded", null)], AdministratorSteps.Select(step => steps[step]));
        var tenant = provisioning.GetProperty("tenantId").GetGuid(); var membership = provisioning.GetProperty("membershipId").GetGuid();
        // 没有激活，管理员成员也没有建；申请人看到的仍是开通中、没有内部原因，还不能登录，也没有结果通知。
        Assert.Equal("pending", await Text($"SELECT status FROM plugin_qian_tenancy.tenants WHERE id = '{tenant}'"));
        Assert.Equal("0", await Text($"SELECT count(*) FROM plugin_qian_identity.memberships WHERE tenant_id = '{tenant}'"));
        var waiting = Assert.Single((await Json(await applicant.Send(HttpMethod.Get, Mine), HttpStatusCode.OK)).GetProperty("items").EnumerateArray());
        Assert.Equal("provisioning", waiting.GetProperty("status").GetString());
        Assert.DoesNotContain(Strings(waiting), value => value.Contains("identity_unavailable", StringComparison.Ordinal));
        Assert.NotEqual(SignInStatus.Authenticated, (await SignIn(business, phone, Password)).Status);
        Assert.Empty(ops.Outbox.Results);

        // 卸下后新租户里重新看得到端口：重试补上第 05 步（成员 ID 沿用申请上那一个）再激活，收敛到 approved，申请人能登录新租户。
        await Loader(business).UnmountAsync(MembershipHiddenEntry, ScopeKey.Global, Ct);
        var retried = await Json(await Operate(client, HttpMethod.Post, Act(id, "retry"), Root()), HttpStatusCode.OK);
        Assert.Equal("approved", retried.GetProperty("status").GetString());
        var after = retried.GetProperty("provisioning");
        Assert.Equal((tenant, membership, 2), (after.GetProperty("tenantId").GetGuid(), after.GetProperty("membershipId").GetGuid(), after.GetProperty("attempts").GetInt32()));
        Assert.False(Has(after, "failureReason"));
        foreach (var step in AdministratorSteps) Assert.Equal("succeeded", Steps(after)[step]);
        Assert.Equal("trial", await Text($"SELECT status FROM plugin_qian_tenancy.tenants WHERE id = '{tenant}'"));
        Assert.Equal($"{membership}|{person.Id}", await Text($"SELECT string_agg(id::text || '|' || person_id::text, ',') FROM plugin_qian_identity.memberships WHERE tenant_id = '{tenant}'"));
        var signedIn = await SignIn(business, phone, Password);
        Assert.Equal((SignInStatus.Authenticated, tenant, membership), (signedIn.Status, signedIn.Session!.TenantId, signedIn.Session.MembershipId));
        Assert.Single(ops.Outbox.Results, message => message.Id == id && message.Kind == SignupResultKind.Approved);
    }

    // ---- 运营侧辅助 ----

    private static async Task<HttpResponseMessage> Operate(HttpClient client, HttpMethod method, string path, string? token, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) request.Content = JsonContent.Create(body, body.GetType());
        return await client.SendAsync(request, Ct);
    }

    // 平台链上本组件的超管旁路记录（引导时生命周期条目与租户主档的放行不算）。
    private static async Task<AuditRecord[]> Bypasses(RunningApp business) =>
        [.. (await ReadPlatformChain(Loader(business).GetContext("tenancy", ScopeKey.Global))).Select(entry => entry.Record)
            .Where(record => record.Payload.ValueKind == JsonValueKind.Object && record.Payload.TryGetProperty("permission", out var permission) &&
                permission.GetString()!.StartsWith(Permission, StringComparison.Ordinal))];

    private Task<string?> Owners(Guid tenant) => Text("SELECT string_agg(a.subject_id, ',' ORDER BY a.subject_id) FROM plugin_qian_authz.role_assignments a " +
        $"JOIN plugin_qian_authz.roles r ON a.role_id = r.id WHERE a.tenant_id = '{tenant}' AND r.code = 'tenant.owner'");

    private const string MembershipHiddenEntry = "signup.membership-hidden";

    private static PluginDefinition MembershipHiddenDefinition() => new(new PluginEntry(MembershipHiddenEntry, "qian.tests.membership-hidden", "1.0.0", ScopeKey.Global, 0,
        JsonSerializer.SerializeToElement(new { })), new MembershipHiddenPlugin(), []);

    // 新开通的租户里看不到身份组件的成员开户端口：开通第 04 步的「租户已开通」通知到达时，对该租户 scope 加 restrict 隐去它；卸下本插件即撤销。
    private sealed class MembershipHiddenPlugin : IKunPlugin
    {
        public IReadOnlyList<ServiceKey> Inject => [];
        public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
        {
            ctx.On(SurfaceHookEvents.TenantProvisioned, DispatchMode.Serial, (Func<HookNotification, ValueTask<JsonElement>>)(notification =>
            {
                ctx.Restrict(notification.Source.Scope, registration => registration.Key != TenantProvisioning.MembershipKey);
                return ValueTask.FromResult(JsonSerializer.SerializeToElement<object?>(null));
            }));
            return ValueTask.CompletedTask;
        }
    }

    private static int Counted(JsonElement list, string status) => list.GetProperty("counts").GetProperty(status).GetInt32();

    private static (int Total, int All) Page(JsonElement list) => (list.GetProperty("total").GetInt32(), Counted(list, "all"));

    private static (string? Kind, string? Level, int? Count) Check(JsonElement check) =>
        (check.GetProperty("kind").GetString(), check.GetProperty("level").GetString(), Has(check, "count") ? check.GetProperty("count").GetInt32() : null);

    private static (string? Kind, string? Level, int? Count)[] Checks(JsonElement detail) => [.. detail.GetProperty("checks").EnumerateArray().Select(Check)];

    private static Dictionary<string, string?> Steps(JsonElement provisioning) =>
        provisioning.GetProperty("steps").EnumerateArray().ToDictionary(step => step.GetProperty("step").GetString()!, step => step.GetProperty("status").GetString());

    private static (bool Expose, bool RequiresConfirmation) Agent(JsonElement machine, string name)
    {
        var agent = machine.GetProperty("actions").EnumerateArray().Single(action => action.GetProperty("name").GetString() == name).GetProperty("agent");
        return (agent.GetProperty("expose").GetBoolean(), agent.GetProperty("requiresConfirmation").GetBoolean());
    }

    private static bool Has(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null;

    // 响应里出现的全部字符串值（不依赖序列化器是否转义非 ASCII 字符）。
    private static IEnumerable<string> Strings(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => [element.GetString()!],
        JsonValueKind.Object => element.EnumerateObject().SelectMany(property => Strings(property.Value)),
        JsonValueKind.Array => element.EnumerateArray().SelectMany(Strings),
        _ => []
    };
}
