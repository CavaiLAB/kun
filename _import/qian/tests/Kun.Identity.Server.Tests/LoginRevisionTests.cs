using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Npgsql;
using Kun.Identity.Abstractions;
using Qian.Testing.Containers;
using static Kun.Identity.Server.Tests.AccountSupport;

namespace Kun.Identity.Server.Tests;

// 建立会话与兑现登录票据时复核身份修订：认证（或票据签发所依据的那次认证）之后 Person 的身份修订变了，直接建立会话、兑换选择票据、
// 用改密票据改密都按票据失效处理（LOGIN_TICKET_INVALID，审计原因 identity_changed），要求重新认证。修订变化用确定的方式放进两步之间：
// 包一层真实的密码因子，在密码验证通过之后、流水线继续之前调用账号端口；或在票据签发之后、兑换之前调用。连接器登录的同类用例在 ConnectorFlowTests。
public sealed class LoginRevisionTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private static readonly LoginContext Net = new("203.0.113.7", "UT-Agent/1.0");
    private const string Checked = "checked";
    private static readonly string[] PickOutcomes = ["<Authenticated>", "LOGIN_TICKET_INVALID"];
    private static readonly string[] ChangeOutcomes = ["<SelectionRequired>", "LOGIN_TICKET_INVALID"];

    private static string Code(SignInResult result) => result.Error?.Code.Code ?? $"<{result.Status}>";

    private static string Id(Guid id) => id.ToString("D");

    // 经包装因子登录：密码照常验证，通过之后先执行布置的操作，再交回流水线。
    private static LoginInput Through(string identifier, string password) => new("web", null, null, Checked, Proof(password)) { Identifier = identifier };

    private static Task<SignInResult> Login(AccountsHost.Running host, string identifier, string password = Password) => host.SignIn.SignInAsync(Input(identifier, password), Net, Ct).AsTask();

    private static Task<SignInResult> Pick(AccountsHost.Running host, LoginTicket ticket, Guid? membership) =>
        host.SignIn.SelectAsync(new TenantSelection(ticket.Token, membership), Net, Ct).AsTask();

    private static async Task<SignInResult> Change(AccountsHost.Running host, LoginTicket ticket, string password)
    {
        using var proof = Proof(password);
        return await host.SignIn.ChangeInitialPasswordAsync(new InitialPasswordChange(ticket.Token, proof), Net, Ct);
    }

    private static Task<string> LiveSessions(AccountsHost.Running host, Guid person) =>
        host.Text("SELECT count(*) FROM plugin_qian_identity.sessions WHERE person_id = @p::uuid AND revoked_at IS NULL", ("p", Id(person)));

    private static async Task<long> Revision(AccountsHost.Running host, Guid person) =>
        long.Parse(await host.Text("SELECT identity_revision FROM plugin_qian_identity.persons WHERE id = @p::uuid", ("p", Id(person))), CultureInfo.InvariantCulture);

    private static Task<string> Audits(AccountsHost.Running host, Guid person, string reason) =>
        host.Text("SELECT count(*) FROM plugin_qian_identity.login_audits WHERE person_id = @p::uuid AND reason = @r", ("p", Id(person)), ("r", reason));

    // 包装真实的密码因子：密码验证通过之后先执行 between（这时这次登录已过第一因子，还没有建立会话或签发票据），再把结果交回流水线。
    private sealed class RevisingFactor(IAuthenticatorFactor password, Func<Guid, Task> between) : IAuthenticatorFactor
    {
        public FactorDescriptor Descriptor { get; } = new(Checked, "密码（测试包装）", "pwd", false);

        public Task<AuthenticationChallenge?> BeginAsync(AuthenticationFactorContext context, CancellationToken cancellationToken = default) => password.BeginAsync(context, cancellationToken);

        public async Task<AuthenticationResult> VerifyAsync(AuthenticationProof proof, AuthenticationFactorContext context, CancellationToken cancellationToken = default)
        {
            var result = await password.VerifyAsync(proof, context, cancellationToken);
            if (result.Succeeded) await between(context.PersonId);
            return result;
        }
    }

    private static IDisposable Between(AccountsHost.Running host, Func<Guid, Task> between) =>
        host.Factors.Register(new RevisingFactor(host.Factors.Find(IdentityAccountService.PasswordFactorId)!, between));

    private static string Describe(Exception error)
    {
        for (var current = error; current is not null; current = current.InnerException)
            if (current is PostgresException postgresError) return "PostgresException " + postgresError.SqlState;
        return error.GetType().Name;
    }

    [Theory]
    [InlineData("reset", "LOGIN_TICKET_INVALID")]
    [InlineData("change", "LOGIN_TICKET_INVALID")]
    [InlineData("identifier", "LOGIN_TICKET_INVALID")]
    [InlineData("unlink", "LOGIN_TICKET_INVALID")]
    [InlineData("disable", "ACCOUNT_DISABLED")]
    public async Task 第一因子验证通过之后身份修订变化_不建立会话_重新认证后照常登录(string change, string expected)
    {
        await using var host = await AccountsHost.Start(postgres);
        var tenant = host.Tenants.Add("acme");
        var (person, phone) = await CreatePerson(host);
        var member = await Join(host, tenant, person.Id);
        using (host.Tenant.BeginScope(tenant.Id)) await host.Accounts.LinkExternalIdentityAsync(new ExternalIdentityLink(member.Id, "oidc", "sub-1"), Ct);
        var (identifier, password) = (phone, Password);
        using var registration = Between(host, async id =>
        {
            switch (change)
            {
                case "reset":
                    using (var reset = Proof("Reset-By-Ops-1")) await host.Accounts.ResetPasswordAsync(id, new PasswordReset(reset, "ops_reset") { RequireChange = false }, Ct);
                    password = "Reset-By-Ops-1";
                    break;
                case "change":
                    using (var current = Proof(Password)) using (var next = Proof("Changed-Pass-2")) await host.Accounts.ChangePasswordAsync(id, new PasswordChange(current, next), Ct);
                    password = "Changed-Pass-2";
                    break;
                case "identifier":
                    identifier = NextPhone();
                    await host.Accounts.ReplaceIdentifierAsync(id, IdentifierKind.Phone, phone, new IdentifierInput(IdentifierKind.Phone, identifier, true), Ct);
                    break;
                case "unlink":
                    using (host.Tenant.BeginScope(tenant.Id)) await host.Accounts.UnlinkExternalIdentityAsync(member.Id, "oidc", "sub-1", Ct);
                    break;
                case "disable":
                    await host.Accounts.SetPersonStatusAsync(id, AccountStatuses.Disabled, "ops", Ct);
                    break;
            }
        });
        var before = await Revision(host, person.Id);
        var refused = await host.SignIn.SignInAsync(Through(phone, Password), Net, Ct);
        Assert.Equal(expected, Code(refused));
        Assert.Equal(before + 1, await Revision(host, person.Id));
        Assert.Equal("0", await LiveSessions(host, person.Id));
        // 账号状态先于修订核对：停用的账号仍以 ACCOUNT_DISABLED 拒绝。
        Assert.Equal(change == "disable" ? "0" : "1", await Audits(host, person.Id, "identity_changed"));
        if (change == "disable") await host.Accounts.SetPersonStatusAsync(person.Id, AccountStatuses.Active, "ops", Ct);
        // 重新认证（用此刻有效的标识与密码）照常建立会话。
        var again = await Login(host, identifier, password);
        Assert.Equal((SignInStatus.Authenticated, member.Id), (again.Status, again.Session!.MembershipId));
    }

    [Fact]
    public async Task 第一因子验证通过之后被重置_随后签发的选择票据与改密票据都不能兑现()
    {
        await using var world = await TwoTenants(postgres);
        var (host, a, _, person, phone, inA, _) = world;
        using var registration = Between(host, async id =>
        {
            using var reset = Proof("Reset-By-Ops-3");
            await host.Accounts.ResetPasswordAsync(id, new PasswordReset(reset, "ops_reset") { RequireChange = false }, Ct);
        });
        // 两个身份：选择票据在重置之后签发（重置时还没有这张票据），兑换时按修订对不上拒绝。
        var selection = await host.SignIn.SignInAsync(Through(phone, Password), Net, Ct);
        Assert.Equal(SignInStatus.SelectionRequired, selection.Status);
        Assert.Equal("LOGIN_TICKET_INVALID", Code(await Pick(host, selection.Ticket!, inA.Id)));
        Assert.Equal("0", await LiveSessions(host, person.Id));
        Assert.Equal("1", await Audits(host, person.Id, "identity_changed"));
        Assert.Equal(SignInStatus.SelectionRequired, (await Login(host, phone, "Reset-By-Ops-3")).Status);

        // 初始密码账号：改密票据同样在重置之后签发，改密被拒，密码仍是重置给的那个。
        var (fresh, freshPhone) = await CreatePerson(host, "新人", requireChange: true);
        await Join(host, a, fresh.Id);
        var pending = await host.SignIn.SignInAsync(Through(freshPhone, Password), Net, Ct);
        Assert.Equal(SignInStatus.PasswordChangeRequired, pending.Status);
        Assert.Equal("LOGIN_TICKET_INVALID", Code(await Change(host, pending.Ticket!, "Brand-New-Pass-4")));
        Assert.Equal("1", await Audits(host, fresh.Id, "identity_changed"));
        Assert.Equal("INVALID_CREDENTIALS", Code(await Login(host, freshPhone, "Brand-New-Pass-4")));
        Assert.Equal(SignInStatus.Authenticated, (await Login(host, freshPhone, "Reset-By-Ops-3")).Status);
    }

    [Fact]
    public async Task 票据签发之后身份修订变化_选择票据_切换票据与改密票据都不能兑现_切换失败时原会话不受影响()
    {
        await using var world = await TwoTenants(postgres);
        var (host, _, _, person, phone, inA, inB) = world;
        // 换绑手机号：递增 Person 修订，但不吊销会话、不作废票据。
        async Task Bump()
        {
            var replacement = NextPhone();
            await host.Accounts.ReplaceIdentifierAsync(person.Id, IdentifierKind.Phone, phone, new IdentifierInput(IdentifierKind.Phone, replacement, true), Ct);
            phone = replacement;
        }
        var selection = await Login(host, phone);
        await Bump();
        Assert.Equal("LOGIN_TICKET_INVALID", Code(await Pick(host, selection.Ticket!, inA.Id)));
        Assert.Equal("0", await LiveSessions(host, person.Id));

        var inAlpha = (await Pick(host, (await Login(host, phone)).Ticket!, inA.Id)).Session!;
        var begin = await host.SignIn.BeginSwitchAsync(inAlpha, Net, Ct);
        Assert.Equal(SignInStatus.SelectionRequired, begin.Status);
        await Bump();
        Assert.Equal("LOGIN_TICKET_INVALID", Code(await Pick(host, begin.Ticket!, inB.Id)));
        Assert.NotNull(await host.Service.ValidateAsync(inAlpha.SessionId, person.Id, inA.Id, Ct));
        Assert.Equal("1", await LiveSessions(host, person.Id));

        await host.Exec("UPDATE plugin_qian_identity.persons SET must_change_password = true WHERE id = @p::uuid", ("p", Id(person.Id)));
        var pending = await Login(host, phone);
        Assert.Equal(SignInStatus.PasswordChangeRequired, pending.Status);
        await Bump();
        Assert.Equal("LOGIN_TICKET_INVALID", Code(await Change(host, pending.Ticket!, "Brand-New-Pass-5")));
        Assert.Equal("true", await host.Text("SELECT must_change_password::text FROM plugin_qian_identity.persons WHERE id = @p::uuid", ("p", Id(person.Id))));
        Assert.Equal("3", await Audits(host, person.Id, "identity_changed"));

        // 重新登录照常：改密在同一流程里递增修订，之后按新修订继续，多个身份仍进入选择，选定后建立会话。
        var again = await Login(host, phone);
        var changed = await Change(host, again.Ticket!, "Brand-New-Pass-6");
        Assert.Equal(SignInStatus.SelectionRequired, changed.Status);
        var established = await Pick(host, changed.Ticket!, inB.Id);
        Assert.Equal((SignInStatus.Authenticated, inB.Id), (established.Status, established.Session!.MembershipId));
        Assert.Equal(await Revision(host, person.Id), established.Session.IdentityRevision);
    }

    // 票据记下签发时的身份修订。没有记下修订的票据（加列之前签发的）一律按票据无效拒绝，与账号此刻的修订是多少无关：
    // 修订仍为 0 的账号同样不兑换，选择票据与改密票据都是如此；修订变过之后也一样。
    [Fact]
    public async Task 票据记下签发时的身份修订_没有记下修订的票据一律不兑换()
    {
        await using var world = await TwoTenants(postgres);
        var (host, _, _, person, phone, inA, _) = world;
        static byte[] Hash(LoginTicket ticket) => SHA256.HashData(Encoding.ASCII.GetBytes(ticket.Token));
        Task<string> Recorded(LoginTicket ticket) =>
            host.Text("SELECT coalesce(person_revision::text, 'null') FROM plugin_qian_identity.login_tickets WHERE token_hash = @h", ("h", Hash(ticket)));
        Task Forget(LoginTicket ticket) => host.Exec("UPDATE plugin_qian_identity.login_tickets SET person_revision = NULL WHERE token_hash = @h", ("h", Hash(ticket)));
        Task<string> TicketInvalid() => host.Text("SELECT count(*) FROM plugin_qian_identity.login_audits WHERE outcome = 'failure' AND reason = 'ticket_invalid'");

        // 修订仍为 0：选择票据与改密票据都记下 0，抹掉之后都不兑换。
        Assert.Equal(0, await Revision(host, person.Id));
        var selection = (await Login(host, phone)).Ticket!;
        Assert.Equal("0", await Recorded(selection));
        await Forget(selection);
        Assert.Equal("LOGIN_TICKET_INVALID", Code(await Pick(host, selection, inA.Id)));
        await host.Exec("UPDATE plugin_qian_identity.persons SET must_change_password = true WHERE id = @p::uuid", ("p", Id(person.Id)));
        var pending = (await Login(host, phone)).Ticket!;
        Assert.Equal((LoginTicketPurpose.PasswordChange, "0"), (pending.Purpose, await Recorded(pending)));
        await Forget(pending);
        Assert.Equal("LOGIN_TICKET_INVALID", Code(await Change(host, pending, "Brand-New-Pass-7")));
        Assert.Equal(0, await Revision(host, person.Id));
        Assert.Equal("0", await LiveSessions(host, person.Id));
        Assert.Equal("2", await TicketInvalid());
        await host.Exec("UPDATE plugin_qian_identity.persons SET must_change_password = false WHERE id = @p::uuid", ("p", Id(person.Id)));

        // 修订变过之后同样如此。
        var replacement = NextPhone();
        await host.Accounts.ReplaceIdentifierAsync(person.Id, IdentifierKind.Phone, phone, new IdentifierInput(IdentifierKind.Phone, replacement, true), Ct);
        phone = replacement;
        selection = (await Login(host, phone)).Ticket!;
        Assert.Equal("1", await Recorded(selection));
        await Forget(selection);
        Assert.Equal("LOGIN_TICKET_INVALID", Code(await Pick(host, selection, inA.Id)));
        Assert.Equal("0", await LiveSessions(host, person.Id));
        Assert.Equal("3", await TicketInvalid());
        Assert.Equal("0", await Audits(host, person.Id, "identity_changed"));
        Assert.Equal(SignInStatus.Authenticated, (await Pick(host, (await Login(host, phone)).Ticket!, inA.Id)).Status);
    }

    // 兑换选择票据、首次改密与运营重置并发：三者都先锁 Person 再动票据，不会死锁；无论谁先提交，重置之后这个人都没有存活的会话。
    [Fact]
    public async Task 兑换选择票据_首次改密与重置并发_不死锁且重置之后没有存活的会话()
    {
        await using var world = await TwoTenants(postgres);
        var (host, _, _, person, phone, inA, _) = world;
        var failures = new ConcurrentQueue<string>();
        async Task Guarded(string name, Func<Task> action)
        {
            try { await action(); }
            catch (Exception error) when (error is not OperationCanceledException) { failures.Enqueue(name + " " + Describe(error)); }
        }
        Task Reset(string password) => Guarded("重置", async () =>
        {
            using var proof = Proof(password);
            await host.Accounts.ResetPasswordAsync(person.Id, new PasswordReset(proof, "ops_reset") { RequireChange = false }, Ct);
        });
        var current = Password;
        for (var round = 0; round < 20; round++)
        {
            var ticket = (await Login(host, phone, current)).Ticket!;
            SignInResult? picked = null;
            var next = "Round-Reset-" + round.ToString("D2", CultureInfo.InvariantCulture);
            await Task.WhenAll(Task.Run(() => Guarded("兑换", async () => picked = await Pick(host, ticket, inA.Id)), Ct), Task.Run(() => Reset(next), Ct));
            current = next;
            // 兑换先提交：建出会话，随后被重置吊销；重置先提交：票据已作废。
            if (picked is not null) Assert.Contains(Code(picked), PickOutcomes);
            Assert.Equal("0", await LiveSessions(host, person.Id));
        }
        await host.Exec("UPDATE plugin_qian_identity.persons SET must_change_password = true WHERE id = @p::uuid", ("p", Id(person.Id)));
        for (var round = 0; round < 20; round++)
        {
            var ticket = (await Login(host, phone, current)).Ticket!;
            Assert.Equal(LoginTicketPurpose.PasswordChange, ticket.Purpose);
            SignInResult? changed = null;
            var next = "Round-Again-" + round.ToString("D2", CultureInfo.InvariantCulture);
            await Task.WhenAll(Task.Run(() => Guarded("改密", async () => changed = await Change(host, ticket, "Changed-In-Round-" + round.ToString("D2", CultureInfo.InvariantCulture))), Ct),
                Task.Run(() => Guarded("重置", async () =>
                {
                    using var proof = Proof(next);
                    await host.Accounts.ResetPasswordAsync(person.Id, new PasswordReset(proof, "ops_reset"), Ct);
                }), Ct));
            current = next;
            // 改密先提交：两个身份，继续进入租户选择（还没有会话），之后的选择票据随重置作废或因修订对不上而兑现不了；重置先提交：改密票据已作废。
            if (changed is not null) Assert.Contains(Code(changed), ChangeOutcomes);
            Assert.Equal("0", await LiveSessions(host, person.Id));
        }
        Assert.True(failures.IsEmpty, string.Join("；", failures.GroupBy(text => text, StringComparer.Ordinal).Select(group => group.Key + " ×" + group.Count().ToString(CultureInfo.InvariantCulture))));
    }
}
