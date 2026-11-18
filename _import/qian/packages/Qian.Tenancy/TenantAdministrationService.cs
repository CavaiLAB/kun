using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Kun.Core.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qian.Audit;
using Qian.Authz.Abstractions;
using Qian.Data;
using Qian.Identity.Abstractions;
using Qian.Runtime;
using Kun.Errors;

namespace Qian.Tenancy;

// 管理员维护（S1-TEN-010）：账号、成员与凭据都经 qian.identity 的端口读写，tenant_admins 只存成员指针。
// 替换按「新管理员成员 → 新 tenant.owner → 撤销原管理员 → 改指针 → 补审计」的顺序：每步幂等、指针最后才改，
// 中途失败用同一请求重试即可收敛；与开通续跑共用同一把租户锁，开通第 05–07 步全部成功之前不接受替换。
internal sealed class TenantAdministrationService(IPluginContext context, PostgresTenantDirectory directory) : ITenantAdministration
{
    private const string Owner = "tenant.owner";
    private static readonly string[] AdministratorSteps = ["05:membership", "06:administrator", "07:owner"];
    // 身份组件按它自己的请求形状给出错误位置，换成运营请求体的字段，页面才能把错误定位到对应输入框。
    private static readonly Dictionary<string, string> AccountPointers = new(StringComparer.Ordinal)
    { ["#/display_name"] = "#/displayName", ["#/identifiers"] = "#/identifier", ["#/identifiers/0/value"] = "#/identifier", ["#/new_password"] = "#/initialPassword" };
    private static readonly Dictionary<string, string> PasswordPointers = new(StringComparer.Ordinal) { ["#/new_password"] = "#/newPassword" };
    private readonly ITenantContext _tenant = context.Services.GetRequiredService<ITenantContext>();

    // 身份组件缺席时 Inject 抛 ServiceNotReadyException，运营入口按组件缺席返回 404。
    private IIdentityAccounts Accounts => context.Inject<IIdentityAccounts>(IdentityDefinition.AccountsKey);
    private IIdentityService Identity => context.Inject<IIdentityService>(IdentityDefinition.Key);

    public async ValueTask<TenantAdminAccount?> FindAccountAsync(TenantAdminAccountLookup request, CancellationToken cancellationToken = default)
    {
        Check(); ArgumentNullException.ThrowIfNull(request);
        var identifier = request.Identifier?.Trim();
        if (string.IsNullOrEmpty(identifier) || identifier.Length > 254 || identifier.Any(char.IsControl)) throw Invalid("#/identifier");
        var person = await Accounts.FindPersonAsync(identifier, cancellationToken).ConfigureAwait(false);
        // 只靠外部身份登录、又没有经本人验证的联系方式的账号不能任管理员（R163），查找同样不返回。
        return person is null || !Eligible(person) || !Vouched(person) && !person.HasPassword ? null : Account(person);
    }

    public async ValueTask<TenantAdminAccount> CreateAccountAsync(CreateTenantAdminAccount request, CancellationToken cancellationToken = default)
    {
        Check(); ArgumentNullException.ThrowIfNull(request);
        var kind = request.IdentifierKind switch
        {
            TenantAdminIdentifierKind.Username => IdentifierKind.Username, TenantAdminIdentifierKind.Phone => IdentifierKind.Phone,
            TenantAdminIdentifierKind.Email => IdentifierKind.Email, _ => throw Invalid("#/identifierKind")
        };
        if (string.IsNullOrEmpty(request.InitialPassword)) throw Invalid("#/initialPassword");
        using var proof = new AuthenticationProof(Encoding.UTF8.GetBytes(request.InitialPassword));
        try
        {
            // 运营录入的手机号与邮箱没有经过验证码核验，按未验证落库；用户名没有核验流程。
            var person = await Accounts.CreatePersonAsync(new CreatePerson(request.DisplayName, [new IdentifierInput(kind, request.Identifier, kind == IdentifierKind.Username)])
            { InitialPassword = proof, RequirePasswordChange = true }, cancellationToken).ConfigureAwait(false);
            return Account(person);
        }
        catch (ErrorException error) { throw Remap(error, AccountPointers); }
    }

    public async ValueTask<TenantAdminList> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Check(); await Existing(id, cancellationToken).ConfigureAwait(false);
        using var scope = _tenant.BeginScope(id.ToString());
        var admin = await PointerAsync(cancellationToken).ConfigureAwait(false) is { } membershipId ? await DescribeAsync(membershipId, cancellationToken).ConfigureAwait(false) : null;
        return new TenantAdminList(admin is null ? [] : [admin]);
    }

    public async ValueTask<TenantAdminList> ReplaceAsync(Guid id, ReplaceTenantAdmins request, string actor, CancellationToken cancellationToken = default)
    {
        Check(); ArgumentNullException.ThrowIfNull(request); ValidateActor(actor);
        if (request.Items is not [{ SubjectId: var subjectId } designation] || subjectId == Guid.Empty) throw Invalid("#/items");
        await Existing(id, cancellationToken).ConfigureAwait(false);
        return await TenancyLocks.RunAsync(context, "tenant:" + id, async () =>
        {
            using var scope = _tenant.BeginScope(id.ToString());
            await EnsureProvisionedAsync(cancellationToken).ConfigureAwait(false);
            var identity = Identity; var accounts = Accounts;
            // 平台身份不能当租户管理员（Q12：平台超管对租户不可见、不计配额）；停用或锁定的账号登录不了，指定了也没有意义。
            var person = await identity.GetPersonAsync(subjectId, cancellationToken).ConfigureAwait(false);
            if (person is null || !Eligible(person) || person.Status != AccountStatuses.Active) throw Invalid("#/items/0/subjectId");
            // 没有经本人验证的手机号或邮箱的账号，先按初始密码重置、在本租户的审计链记下被重置的自然人，再建成员、授 owner（R163）。
            if (await DesignateAsync(person, designation.InitialPassword, "#/items/0/initialPassword", "#/items/0/subjectId", cancellationToken).ConfigureAwait(false))
                await AuditAsync(id, actor, ResetPasswordCapability, person.Id.ToString("D"), JsonSerializer.SerializeToElement(new { subjectId = person.Id, designation = true })).ConfigureAwait(false);
            var grants = context.Scoped(new ScopeKey("tenant", id.ToString())).Inject<ITenantRoleGrant>(TenantRoleGrant.Key);
            var subject = person.Id.ToString("D");
            // 已是成员时幂等返回并补上管理员标志；新成员经配额判定，满额 CARDINALITY_USER_LIMIT，此时成员、授权与指针都还没改（需要时密码已重置）。
            var membership = await accounts.CreateMembershipAsync(new CreateMembership(person.Id, person.DisplayName) { IsTenantAdministrator = true }, cancellationToken).ConfigureAwait(false);
            await grants.AssignAsync(Owner, "user", subject, actor, cancellationToken).ConfigureAwait(false);
            var previous = await PointerAsync(cancellationToken).ConfigureAwait(false);
            // 原管理员只撤销 tenant.owner 与管理员标志，成员身份与其它角色保留，会话不吊销（授权变更递增 identity_revision，PRD 13.3.3）。
            // 指针仍指向原管理员时重试会再做一遍，两步都幂等；新 owner 已经授予，撤销不会撞上防自锁。
            if (previous is { } old && old != membership.Id && await identity.GetMembershipAsync(old, cancellationToken).ConfigureAwait(false) is { } prior)
            {
                await grants.RevokeAsync(Owner, "user", prior.PersonId.ToString("D"), actor, cancellationToken).ConfigureAwait(false);
                try { await accounts.SetTenantAdministratorAsync(old, false, cancellationToken).ConfigureAwait(false); }
                catch (ErrorException error) when (error.Error.Code == PlatformErrors.NotFound) { /* 原成员已清除或已从目录删除，没有标志可撤销 */ }
            }
            if (previous != membership.Id)
            {
                await SetPointerAsync(membership.Id, actor, cancellationToken).ConfigureAwait(false);
                await AuditAsync(id, actor, "qian.tenancy/admin.assign", subject, JsonSerializer.SerializeToElement(new { membershipId = membership.Id, previousMembershipId = previous })).ConfigureAwait(false);
            }
            return new TenantAdminList([await DescribeAsync(membership.Id, cancellationToken).ConfigureAwait(false) ?? throw Error(PlatformErrors.Conflict)]);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask ResetPasswordAsync(Guid id, Guid membershipId, ResetTenantAdminPassword request, string actor, CancellationToken cancellationToken = default)
    {
        Check(); ArgumentNullException.ThrowIfNull(request); ValidateActor(actor);
        if (string.IsNullOrEmpty(request.NewPassword)) throw Invalid("#/newPassword");
        await Existing(id, cancellationToken).ConfigureAwait(false);
        using var scope = _tenant.BeginScope(id.ToString());
        // 只对当前管理员：否则借任一租户的路径就能重置任意自然人的密码。
        if (membershipId == Guid.Empty || await PointerAsync(cancellationToken).ConfigureAwait(false) != membershipId) throw Error(PlatformErrors.NotFound);
        var identity = Identity;
        var membership = await identity.GetMembershipAsync(membershipId, cancellationToken).ConfigureAwait(false);
        var person = membership is null ? null : await identity.GetPersonAsync(membership.PersonId, cancellationToken).ConfigureAwait(false);
        if (person is null || !Eligible(person)) throw Error(PlatformErrors.NotFound);
        // 自然人全局唯一：旧密码立即失效、下次登录强制改密、该自然人的全部会话吊销，他在其它租户的登录也随之改变。
        using (var proof = new AuthenticationProof(Encoding.UTF8.GetBytes(request.NewPassword)))
        {
            try { await Accounts.ResetPasswordAsync(person.Id, new PasswordReset(proof, "tenant_admin_reset"), cancellationToken).ConfigureAwait(false); }
            catch (ErrorException error) { throw Remap(error, PasswordPointers); }
        }
        await AuditAsync(id, actor, "qian.tenancy/admin.reset-password", membershipId.ToString("D"), JsonSerializer.SerializeToElement(new { membershipId })).ConfigureAwait(false);
    }

    public async ValueTask PrepareAdministratorAsync(CreateTenant request, string actor, CancellationToken cancellationToken = default)
    {
        Check(); ArgumentNullException.ThrowIfNull(request); ValidateActor(actor);
        var administrator = request.Administrator ?? throw Error(PlatformErrors.InputInvalid);
        const string passwordPointer = "#/administrator/initialPassword", subjectPointer = "#/administrator/subjectId";
        // 先做不改动任何东西的判定：开通一定会失败（输入不合格、租户号已被另一份开户输入占用）时，不去动指定账号的密码。
        var designated = administrator with { InitialPassword = null };
        await TenantProvisioningOrchestrator.PrecheckAsync(context, request with { Administrator = designated }, designated, cancellationToken).ConfigureAwait(false);
        // 主体不是自然人 id、身份组件缺席或自然人不存在：这里不判定（开通编排第 05 步会失败或按组件缺席跳过），也就没有账号可以重置。
        var person = Guid.TryParseExact(administrator.SubjectId, "D", out var subjectId) && subjectId != Guid.Empty &&
            context.TryInject<IIdentityService>(IdentityDefinition.Key) is { } identity ? await identity.GetPersonAsync(subjectId, cancellationToken).ConfigureAwait(false) : null;
        if (person is null)
        {
            if (administrator.InitialPassword is not null) throw Invalid(passwordPointer);
            return;
        }
        // 平台身份与停用、锁定的账号不能任租户管理员（与更换管理员相同），在动它的密码之前拒绝。
        if (!Eligible(person) || person.Status != AccountStatuses.Active) throw Invalid(subjectPointer);
        if (await DesignateAsync(person, administrator.InitialPassword, passwordPointer, subjectPointer, cancellationToken).ConfigureAwait(false))
            await PlatformAuditAsync(actor, person.Id).ConfigureAwait(false);
    }

    // 运营指定管理员（开户与更换）共用（R163），返回是否重置了密码，调用方据此写审计：
    // - 手机号、邮箱全部经本人验证的账号：直接任管理员，不重置；给了初始密码反而拒绝。
    // - 只靠外部身份登录（没有密码）的账号：不能任管理员。
    // - 其余账号：必须给初始密码，重置为它、下次登录强制修改（与运营重置密码相同），并解除它在各租户的外部身份绑定。
    private async Task<bool> DesignateAsync(Person person, string? initialPassword, string passwordPointer, string subjectPointer, CancellationToken ct)
    {
        if (Vouched(person))
        {
            if (initialPassword is not null) throw Field(PlatformErrors.ValueNotAllowed, passwordPointer);
            return false;
        }
        if (!person.HasPassword) throw Invalid(subjectPointer);
        if (initialPassword is null) throw Field(PlatformErrors.Required, passwordPointer);
        using var proof = new AuthenticationProof(Encoding.UTF8.GetBytes(initialPassword));
        try { await Accounts.ResetPasswordAsync(person.Id, new PasswordReset(proof, "tenant_admin_designation") { UnlinkExternalIdentities = true }, ct).ConfigureAwait(false); }
        catch (ErrorException error) { throw Remap(error, new Dictionary<string, string>(StringComparer.Ordinal) { ["#/new_password"] = passwordPointer }); }
        return true;
    }

    // 开户时的重置记在平台审计链（租户还没建成），带被重置的自然人 id；更换管理员时的重置记在该租户的审计链，能力名相同。
    private Task PlatformAuditAsync(string actor, Guid personId)
    {
        var subject = personId.ToString("D");
        var record = new AuditRecord(Guid.CreateVersion7(), null, "user", actor, null, ResetPasswordCapability, "v1",
            Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(subject))), "api", "admins", "success",
            ActivityTraceId.CreateRandom().ToHexString(), DateTimeOffset.UtcNow, JsonSerializer.SerializeToElement(new { subjectId = personId, designation = true }));
        return TenantAuditCompletion.AppendAsync(context, record, (entry, token) => context.Audit().AppendPlatformAsync(context, entry, token));
    }

    private const string ResetPasswordCapability = "qian.tenancy/admin.reset-password";

    private static ErrorException Field(ErrorCode code, string pointer) =>
        new(new Error(PlatformErrors.InputInvalid) { Errors = [new FieldError(code, pointer)] });

    // 可以直接任管理员的账号：至少有一个手机号或邮箱，且手机号、邮箱都经本人验证过。用户名没有核验流程，不算。
    private static bool Vouched(Person person)
    {
        var contacts = person.Identifiers.Where(item => item.Kind is IdentifierKind.Phone or IdentifierKind.Email).ToArray();
        return contacts.Length > 0 && contacts.All(item => item.Verified);
    }

    private void Check()
    {
        if (_tenant.TenantId is not null) throw Error(PlatformErrors.AuthzDenied);
    }
    private async Task Existing(Guid id, CancellationToken ct)
    {
        if (await directory.GetByIdAsync(id, ct).ConfigureAwait(false) is null) throw Error(PlatformErrors.NotFound);
    }
    private async Task EnsureProvisionedAsync(CancellationToken ct)
    {
        await using var session = await context.Data().OpenAsync<TenancyDbContext>(ct).ConfigureAwait(false);
        var done = await session.Context.ProvisionSteps.AsNoTracking().CountAsync(step => AdministratorSteps.Contains(step.Step) && step.Status == "succeeded", ct).ConfigureAwait(false);
        if (done != AdministratorSteps.Length) throw Error(PlatformErrors.Conflict);
    }
    // 运营列表的管理员列：只读成员显示名；没有指针、身份组件缺席或成员已清除都返回 null，不让整页失败。
    internal async Task<string?> AdministratorNameAsync(Guid id, CancellationToken ct)
    {
        using var scope = _tenant.BeginScope(id.ToString());
        if (await PointerAsync(ct).ConfigureAwait(false) is not { } membershipId) return null;
        if (context.TryInject<IIdentityService>(IdentityDefinition.Key) is not { } identity) return null;
        var membership = await identity.GetMembershipAsync(membershipId, ct).ConfigureAwait(false);
        return membership is null || membership.Status == AccountStatuses.Purged ? null : membership.DisplayName;
    }
    private async Task<Guid?> PointerAsync(CancellationToken ct)
    {
        await using var session = await context.Data().OpenAsync<TenancyDbContext>(ct).ConfigureAwait(false);
        return await session.Context.Administrators.AsNoTracking().Select(row => (Guid?)row.MembershipId).SingleOrDefaultAsync(ct).ConfigureAwait(false);
    }
    private async Task SetPointerAsync(Guid membershipId, string actor, CancellationToken ct)
    {
        await using var session = await context.Data().OpenAsync<TenancyDbContext>(ct).ConfigureAwait(false);
        var row = await session.Context.Administrators.SingleOrDefaultAsync(ct).ConfigureAwait(false);
        if (row is null) session.Context.Administrators.Add(new TenantAdminRow { MembershipId = membershipId, CreatedBy = actor });
        else { row.MembershipId = membershipId; row.UpdatedBy = actor; }
        await session.SaveChangesAsync(ct).ConfigureAwait(false);
    }
    private async Task<TenantAdmin?> DescribeAsync(Guid membershipId, CancellationToken ct)
    {
        var identity = Identity;
        var membership = await identity.GetMembershipAsync(membershipId, ct).ConfigureAwait(false);
        if (membership is null || membership.Status == AccountStatuses.Purged) return null;
        var person = await identity.GetPersonAsync(membership.PersonId, ct).ConfigureAwait(false);
        return person is null ? null : new TenantAdmin(membership.Id, person.Id, membership.DisplayName, membership.Account, membership.Status, Identifiers(person));
    }
    // 补充审计进目标租户的审计链，只记成员指针，不含密码与原始标识；前置的超管旁路审计在平台链上。
    private Task AuditAsync(Guid id, string actor, string capability, string input, JsonElement payload) =>
        TenantAuditCompletion.AppendAsync(context, new AuditRecord(Guid.CreateVersion7(), id.ToString(), "user", actor, null, capability, "v1",
            Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(input))), "api", "admins", "success",
            ActivityTraceId.CreateRandom().ToHexString(), DateTimeOffset.UtcNow, payload),
            (record, token) => context.Scoped(new ScopeKey("tenant", id.ToString())).Audit().AppendAsync(record, token));

    private static bool Eligible(Person person) => !person.PlatformAccess && person.Status != AccountStatuses.Purged;
    private static TenantAdminAccount Account(Person person) => new(person.Id, person.DisplayName, person.Status, Identifiers(person));
    private static System.Collections.ObjectModel.ReadOnlyCollection<TenantAdminIdentifier> Identifiers(Person person) => Array.AsReadOnly(person.Identifiers.Select(item => new TenantAdminIdentifier(item.Kind switch
    {
        IdentifierKind.Phone => TenantAdminIdentifierKind.Phone, IdentifierKind.Email => TenantAdminIdentifierKind.Email, _ => TenantAdminIdentifierKind.Username
    }, item.Masked, item.Verified)).ToArray());
    private static ErrorException Remap(ErrorException error, Dictionary<string, string> pointers)
    {
        // 映射表之外的位置换成整个请求体，不把身份组件内部的字段名带进运营契约。
        string Map(string pointer) => pointers.TryGetValue(pointer, out var mapped) ? mapped : "#";
        var source = error.Error;
        if (source.Pointer is null && source.Errors.Count == 0) return error;
        return new ErrorException(new Error(source.Code, source.Args)
        {
            Pointer = source.Pointer is null ? null : Map(source.Pointer), Allowed = source.Allowed,
            Errors = [.. source.Errors.Select(item => new FieldError(item.Code, Map(item.Pointer), item.Args) { Allowed = item.Allowed })]
        }, error);
    }
    private static void ValidateActor(string actor)
    { if (string.IsNullOrWhiteSpace(actor) || actor.Length > 200 || actor.Any(char.IsControl)) throw Error(PlatformErrors.InputInvalid); }
    private static ErrorException Invalid(string pointer) => new(new Error(PlatformErrors.InputInvalid) { Pointer = pointer });
    private static ErrorException Error(ErrorCode code) => new(new Error(code));
}
