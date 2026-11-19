using Kun.EventBus;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Qian.Audit;
using Qian.Authz.Abstractions;
using Kun.Identity.Abstractions;
using Kun.Runtime;
using Kun.Errors;
using Kun.Tenancy;

namespace Kun.Identity.Server;

// 租户管理员直接新增成员（S1-IDP-017，R158 / R163）。新建自然人；标识属于本租户自己从目录删除的成员时，按标识恢复原成员。
// 一个事务里完成：配额 → 解析标识 → 新建自然人与成员（或恢复）→ 目录事件 → 初始角色。
// 加锁顺序与成员建立相同：先成员计数锁，再目录锁，然后才锁行、写行（见 LockDirectoryAsync）。
internal sealed partial class IdentityAccountService
{
    /// <summary>新成员的初始角色（R158）：由本组件经授权组件的授予端口授予，与开通编排第 07 步、开发种子同一端口。</summary>
    internal const string InitialMemberRole = "tenant.user";

    // 身份组件给出的密码策略错误指向改密请求的字段，换成新增成员请求的字段。
    private const string InitialPasswordPointer = "#/initial_password";

    public async ValueTask<DirectoryMemberAdded> AddMemberAsync(DirectoryMemberInput request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var tenantId = RequireTenant();
        var name = request.DisplayName?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 200 || name.Any(char.IsControl)) throw IdentifierRules.Invalid("#/display_name");
        // 只收手机号与用户名（R145）；邮箱形似的输入与非法输入一样按格式不符拒绝，不回显原值。
        if (IdentifierRules.Classify(request.Identifier, _account.DefaultPhoneCountryCode) is not { Kind: IdentifierKind.Phone or IdentifierKind.Username } identifier)
            throw IdentifierRules.Invalid("#/identifier");
        string? account = null;
        if (request.Account is not null)
        {
            account = request.Account.Normalize(System.Text.NormalizationForm.FormKC).Trim().ToLowerInvariant();
            if (!AccountPattern.IsMatch(account)) throw IdentifierRules.Invalid("#/account");
        }
        // 初始密码总是必填，在拿锁之前校验并哈希（Argon2id 较慢）。恢复原成员时这份哈希不用、不保存：他沿用自己的凭据。
        if (request.InitialPassword is null)
            throw new ErrorException(new Error(PlatformErrors.InputInvalid) { Errors = [new FieldError(PlatformErrors.Required, InitialPasswordPointer)] });
        string hash;
        using (var secret = PasswordSecret.From(request.InitialPassword))
        {
            try { CheckPassword(secret); }
            catch (ErrorException error) when (error.Error.Code == IdentityErrors.PasswordPolicyViolation)
            { throw new ErrorException(new Error(error.Error.Code, error.Error.Args) { Pointer = InitialPasswordPointer }, error); }
            hash = _hasher.Hash(secret.Bytes);
        }
        var kind = IdentifierRules.KindName(identifier.Kind);
        using var scope = TenantScope(tenantId); using var origin = OpenOrigin();
        await RevokeRolesBeforeRestoreAsync(tenantId, kind, identifier.Value, cancellationToken).ConfigureAwait(false);
        for (var attempt = 0; ; attempt++)
        {
            await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
            var db = session.Context;
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await IdentitySql.LockAsync(db, "members:" + tenantId.ToString("D"), cancellationToken).ConfigureAwait(false);
            var directoryLock = await LockDirectoryAsync(db, tenantId, cancellationToken).ConfigureAwait(false);
            // 配额端口缺席（未装配租户组件的独立宿主）时不限额；装配后满额抛 CARDINALITY_USER_LIMIT，此时什么都还没写。
            if (_ctx.TryInject<ITenantQuota>(TenantSettings.QuotaKey) is { } quota)
                await quota.EnsureCanAddUserAsync(await CountMembersAsync(db, cancellationToken).ConfigureAwait(false), TenantMemberKind.Member, cancellationToken).ConfigureAwait(false);
            // 标识表是全局表（不受租户 RLS 约束）：解析结果与登录时一致。
            var owner = await db.Identifiers.AsNoTracking().Where(i => i.Kind == kind && i.Value == identifier.Value)
                .Select(i => (Guid?)i.PersonId).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            // 指定的账号已被本租户成员占用：先于标识判定返回 ACCOUNT_IN_USE，标识有没有被占用都是这一个结果。
            // 唯一的例外是标识就属于占着这个账号的人（恢复原成员时指定他自己的账号，或在册成员按已被占用处理）。
            if (account is not null && await db.Memberships.AsNoTracking().Where(m => m.Account == account).Select(m => (Guid?)m.PersonId)
                    .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false) is { } holder && holder != owner)
                throw new ErrorException(new Error(IdentityErrors.AccountInUse) { Pointer = "#/account" });
            var events = new DirectoryEventBatch();
            MembershipRow row;
            if (owner is { } occupied)
            {
                row = await RestorableMemberAsync(db, occupied, cancellationToken).ConfigureAwait(false)
                    ?? throw new ErrorException(new Error(IdentityErrors.IdentifierInUse) { Pointer = "#/identifier" });
                // 恢复原成员：成员 id 与账号不变（账号创建后不可改），显示名取这次请求的值。
                if (account is not null && account != row.Account)
                    throw new ErrorException(new Error(PlatformErrors.InputInvalid) { Errors = [new FieldError(PlatformErrors.ValueNotAllowed, "#/account")] });
                var previousStatus = row.Status;
                row.DirectoryDeleted = false; row.Status = AccountStatuses.Active; row.DisplayName = name; row.IdentityRevision++;
                events.Add(revision => new DirectoryMemberStatusChangedEvent(revision, row.Id, row.PersonId, row.Status, previousStatus, false, row.IdentityRevision));
            }
            else
            {
                var subject = PersonIds();
                if (subject == Guid.Empty) throw new InvalidOperationException("Person 标识不能为空。");
                var now = Now;
                // 管理员录入的手机号没有经过验证码核验，按未验证保存；用户名没有核验流程。登录索引随建档写入本租户。
                db.Persons.Add(new PersonRow
                {
                    Id = subject, DisplayName = name, PasswordHash = hash, PasswordChangedAt = now, MustChangePassword = true, TenantIds = [tenantId.ToString("D")]
                });
                db.Identifiers.Add(new PersonIdentifierRow
                {
                    Id = Guid.CreateVersion7(), PersonId = subject, Kind = kind, Value = identifier.Value, VerifiedAt = identifier.Kind == IdentifierKind.Username ? now : null
                });
                db.PasswordHistory.Add(new PasswordHistoryRow { Id = Guid.CreateVersion7(), PersonId = subject, Hash = hash, CreatedAt = now });
                row = new MembershipRow
                {
                    Id = Guid.CreateVersion7(), PersonId = subject, DisplayName = name, Status = AccountStatuses.Active,
                    Account = account ?? await NewMemberAccountAsync(db, identifier, attempt, cancellationToken).ConfigureAwait(false)
                };
                db.Memberships.Add(row);
                events.Add(revision => new DirectoryMemberCreatedEvent(revision, row.Id, row.PersonId, row.Status, row.IdentityRevision));
            }
            var changes = await RaiseDirectoryEventsAsync(db, directoryLock, events, cancellationToken).ConfigureAwait(false);
            try { await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false); EnsurePublished(changes); }
            catch (DbUpdateException error) when (IsUniqueViolation(error))
            {
                var constraint = (error.InnerException as PostgresException)?.ConstraintName ?? "";
                // 别的请求同时用同一个标识新建了自然人：换会话重来，这次按已被占用处理。
                if (constraint.Contains("person_identifiers", StringComparison.Ordinal) && attempt < 3) continue;
                if (constraint.Contains("account", StringComparison.Ordinal))
                {
                    // 调用方指定的账号被占用是业务拒绝；自动生成的账号撞车换一个重试。
                    if (account is not null || attempt >= 3) throw new ErrorException(new Error(IdentityErrors.AccountInUse) { Pointer = "#/account" });
                    continue;
                }
                // 主键撞车（几乎不可能）换号重试，绝不覆盖已有自然人。
                if (constraint.Contains("persons", StringComparison.Ordinal) && attempt < 3) continue;
                throw Error(PlatformErrors.Conflict);
            }
            // 初始角色在提交之前授予：授予失败则成员、自然人与目录事件随事务一起回滚，调用方可原样重试。授权写入有自己的事务，
            // 先于本事务提交；本事务提交本身失败时只留下一条不属于任何成员的角色绑定，不授予任何访问（没有成员就拿不到本租户令牌）。
            // 授权组件随后递增身份修订时读不到这条尚未提交的成员，按 NOT_FOUND 跳过：新成员还没有任何令牌，无需失效。
            var granted = await GrantInitialRoleAsync(tenantId, row.PersonId, cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            var restored = owner is not null;
            return new DirectoryMemberAdded(ToMembership(row), !restored, granted) { Restored = restored };
        }
    }

    // 标识已被占用时唯一可以继续的情形：占用者是本租户从目录删除的成员，且自然人仍可登录（启用、不是平台身份）。其余返回 null，
    // 由调用方按已被占用拒绝。成员行先加行锁再读（与成员建立相同），恢复改的就是这一行。
    private static async Task<MembershipRow?> RestorableMemberAsync(IdentityDbContext db, Guid personId, CancellationToken ct)
    {
        await IdentitySql.QueryAsync(db, $"SELECT id FROM {IdentitySql.Memberships} WHERE person_id = @person FOR UPDATE", reader => reader.GetGuid(0), ct, ("person", personId)).ConfigureAwait(false);
        var member = await db.Memberships.SingleOrDefaultAsync(m => m.PersonId == personId, ct).ConfigureAwait(false);
        if (member is not { DirectoryDeleted: true } || member.Status == AccountStatuses.Purged) return null;
        var person = await db.Persons.AsNoTracking().SingleOrDefaultAsync(p => p.Id == personId, ct).ConfigureAwait(false);
        return person is { PlatformAccess: false, Status: AccountStatuses.Active } ? member : null;
    }

    // 恢复原成员后只保留 tenant.user（R163）：标识属于本租户从目录删除的成员时，先撤销他删除前的其它内置角色。放在拿锁之前：
    // 撤销会经授权组件递增该成员的身份修订，要更新的正是恢复时锁住的成员行。被删的成员登录不了本租户，先撤销不影响任何访问；
    // 之后的恢复因满额等原因没有成功，他再被恢复时同样只有 tenant.user。授权组件缺席时没有角色可撤销。
    private async Task RevokeRolesBeforeRestoreAsync(Guid tenantId, string kind, string value, CancellationToken ct)
    {
        if (_ctx.Scoped(new ScopeKey("tenant", tenantId.ToString("D"))).TryInject<ITenantRoleGrant>(TenantRoleGrant.Key) is not { } grants) return;
        Guid? deleted;
        await using (var session = await OpenAsync(ct).ConfigureAwait(false))
        {
            var db = session.Context;
            var owner = await db.Identifiers.AsNoTracking().Where(i => i.Kind == kind && i.Value == value).Select(i => (Guid?)i.PersonId).SingleOrDefaultAsync(ct).ConfigureAwait(false);
            deleted = owner is { } person && await db.Memberships.AsNoTracking()
                .AnyAsync(m => m.PersonId == person && m.DirectoryDeleted && m.Status != AccountStatuses.Purged, ct).ConfigureAwait(false) ? owner : null;
        }
        if (deleted is not { } subject) return;
        var actor = EventEnvelopeScope.Current?.Actor.Id ?? "qian.identity";
        foreach (var role in RolesRevokedOnRestore)
            await grants.RevokeAsync(role, "user", subject.ToString("D"), actor, ct).ConfigureAwait(false);
    }

    private static readonly string[] RolesRevokedOnRestore = ["tenant.owner", "tenant.admin", "tenant.developer"];

    // HTTP 入口的审计（S1-IDP-017 验收 6）：宿主装了 Audit 时写目标租户的审计链，开始记录写不进去就不执行；独立身份宿主没装 Audit 时不写。
    // 标准能力入口不走这里：它经调用方上下文取 Audit，缺席即不可用。
    internal IAuditService? TenantAudit(Guid tenantId) => _ctx.Scoped(new ScopeKey("tenant", tenantId.ToString("D"))).TryInject<IAuditService>(AuditDefinition.Key);

    // 新建自然人的缺省账号：用户名（未被本租户占用时），否则随机的不透明账号；不取手机号，账号创建后不可改。
    private static async Task<string> NewMemberAccountAsync(IdentityDbContext db, (IdentifierKind Kind, string Value) identifier, int attempt, CancellationToken ct)
    {
        if (attempt == 0 && identifier.Kind == IdentifierKind.Username && !await db.Memberships.AnyAsync(m => m.Account == identifier.Value, ct).ConfigureAwait(false))
            return identifier.Value;
        return "m" + Convert.ToHexStringLower(System.Security.Cryptography.RandomNumberGenerator.GetBytes(6));
    }

    // R158：同一运行时装了授权组件时授予 tenant.user；授权组件缺席（独立身份宿主）时不授予，返回空集合。授予人取当前事件来源的主体，
    // 即发起新增的租户管理员。恢复原成员时同样授予（已有则不变）；他删除前的其它内置角色已在拿锁之前撤销（见 RevokeRolesBeforeRestoreAsync）。
    private async Task<IReadOnlyList<string>> GrantInitialRoleAsync(Guid tenantId, Guid personId, CancellationToken ct)
    {
        if (_ctx.Scoped(new ScopeKey("tenant", tenantId.ToString("D"))).TryInject<ITenantRoleGrant>(TenantRoleGrant.Key) is not { } grants) return [];
        var actor = EventEnvelopeScope.Current?.Actor.Id ?? "qian.identity";
        await grants.AssignAsync(InitialMemberRole, "user", personId.ToString("D"), actor, ct).ConfigureAwait(false);
        return [InitialMemberRole];
    }
}
