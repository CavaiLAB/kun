using Kun.EntityFrameworkCore;
using Kun.EventBus;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Kun.Identity.Abstractions;
using Kun.Errors;

namespace Kun.Identity.Server;

// 账号管理：Person、标识、凭据、Membership、外部身份。
internal sealed partial class IdentityAccountService
{
    private const int MaxIdentifiers = 8;

    // 集成事件的来源：调用方（如租户运营接口）已打开的作用域优先，否则以本组件的服务主体发布。
    private static IDisposable OpenOrigin(Guid? personId = null) => EventEnvelopeScope.Begin(new EventOrigin("qian.identity",
        EventEnvelopeScope.Current?.Actor ?? (personId is { } id ? EventActor.User(id.ToString("D")) : EventActor.Service("qian.identity"))));

    private static async Task<PersonRow> LockPersonAsync(IdentityDbContext db, Guid personId, CancellationToken ct)
    {
        // 行锁串行化同一 Person 的写入，之后读到的跟踪实体不会被并发的计数更新改写（避免 xmin 冲突与丢失更新）。
        var locked = await IdentitySql.QueryAsync(db, $"SELECT id FROM {IdentitySql.Persons} WHERE id = @id FOR UPDATE", r => r.GetGuid(0), ct, ("id", personId)).ConfigureAwait(false);
        if (locked.Count != 1) throw Error(PlatformErrors.NotFound);
        return await db.Persons.SingleAsync(p => p.Id == personId, ct).ConfigureAwait(false);
    }

    private void CheckPassword(PasswordSecret secret)
    {
        var policy = _account.Password;
        string? rule = secret.Length < policy.MinLength ? "minLength=" + policy.MinLength
            : secret.Length > policy.MaxLength ? "maxLength=" + policy.MaxLength
            : secret.HasControl ? "noControlCharacters" : secret.HasEdgeWhitespace ? "noEdgeWhitespace" : null;
        if (rule is not null)
            throw new ErrorException(new Error(IdentityErrors.PasswordPolicyViolation, new Dictionary<string, string> { ["rule"] = rule }) { Pointer = "#/new_password" });
    }

    // 新密码不得与当前密码或最近 HistorySize 个密码相同；比较的是哈希，历史里没有明文。
    private async Task EnsureNotReusedAsync(IdentityDbContext db, PersonRow person, PasswordSecret next, CancellationToken ct)
    {
        var hashes = await db.PasswordHistory.AsNoTracking().Where(h => h.PersonId == person.Id).OrderByDescending(h => h.CreatedAt)
            .Take(_account.Password.HistorySize).Select(h => h.Hash).ToListAsync(ct).ConfigureAwait(false);
        if (person.PasswordHash is not null && !hashes.Contains(person.PasswordHash)) hashes.Add(person.PasswordHash);
        foreach (var hash in hashes)
            if (Argon2idPasswordHasher.Verify(hash, next.Bytes))
                throw new ErrorException(new Error(IdentityErrors.PasswordReused, new Dictionary<string, string> { ["count"] = _account.Password.HistorySize.ToString(System.Globalization.CultureInfo.InvariantCulture) }) { Pointer = "#/new_password" });
    }

    // 设置密码的唯一入口：写哈希与历史、清失败计数；mustChange 控制是否强制下次改密。
    private async Task SetPasswordAsync(IdentityDbContext db, PersonRow person, PasswordSecret next, bool mustChange, DateTimeOffset now, CancellationToken ct)
    {
        var hash = _hasher.Hash(next.Bytes);
        person.PasswordHash = hash; person.PasswordChangedAt = now; person.MustChangePassword = mustChange;
        person.FailedAttempts = 0; person.LastFailedAt = null; person.LockoutUntil = null;
        db.PasswordHistory.Add(new PasswordHistoryRow { Id = Guid.CreateVersion7(), PersonId = person.Id, Hash = hash, CreatedAt = now });
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        // 历史只留最近 HistorySize 条（含当前）。
        await IdentitySql.ExecuteAsync(db, $"""
            DELETE FROM {IdentitySql.PasswordHistory} WHERE person_id = @person AND id NOT IN
              (SELECT id FROM {IdentitySql.PasswordHistory} WHERE person_id = @person ORDER BY created_at DESC, id DESC LIMIT @keep)
            """, ct, ("person", person.Id), ("keep", _account.Password.HistorySize)).ConfigureAwait(false);
    }

    private static async Task SaveAsync(IDataSession<IdentityDbContext> session, PersonRow person, IdentityIntegrationEvent? integrationEvent, CancellationToken ct)
    {
        if (integrationEvent is null) { await session.SaveChangesAsync(ct).ConfigureAwait(false); return; }
        var change = new IdentityChange(integrationEvent);
        person.Raise(change);
        await IdentityChange.Save(session, [change], ct).ConfigureAwait(false);
    }

    // ---- Person ----

    public async ValueTask<Person> CreatePersonAsync(CreatePerson request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var name = request.DisplayName?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 200 || name.Any(char.IsControl)) throw IdentifierRules.Invalid("#/display_name");
        if (request.Identifiers is null || request.Identifiers.Count is 0 or > MaxIdentifiers) throw IdentifierRules.Invalid("#/identifiers");
        var identifiers = new List<(IdentifierKind Kind, string Value, bool Verified)>();
        for (var i = 0; i < request.Identifiers.Count; i++)
        {
            var input = request.Identifiers[i];
            var value = IdentifierRules.Normalize(input.Kind, input.Value, _account.DefaultPhoneCountryCode) ?? throw IdentifierRules.Invalid($"#/identifiers/{i}/value");
            if (identifiers.Any(item => item.Kind == input.Kind && item.Value == value)) throw IdentifierRules.Invalid($"#/identifiers/{i}/value");
            identifiers.Add((input.Kind, value, input.Verified));
        }
        string? hash = null;
        if (request.InitialPassword is not null)
        {
            using var secret = PasswordSecret.From(request.InitialPassword);
            CheckPassword(secret); hash = _hasher.Hash(secret.Bytes);
        }
        var now = Now;
        for (var attempt = 0; ; attempt++)
        {
            var personId = PersonIds();
            if (personId == Guid.Empty) throw new InvalidOperationException("Person 标识不能为空。");
            using var neutral = TenantScope(null);
            await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
            var db = session.Context;
            var row = new PersonRow
            {
                Id = personId, DisplayName = name, PlatformAccess = request.PlatformAccess, PasswordHash = hash,
                PasswordChangedAt = hash is null ? null : now, MustChangePassword = hash is not null && request.RequirePasswordChange
            };
            db.Persons.Add(row);
            var rows = identifiers.Select(item => new PersonIdentifierRow
            {
                Id = Guid.CreateVersion7(), PersonId = personId, Kind = IdentifierRules.KindName(item.Kind), Value = item.Value, VerifiedAt = item.Verified ? now : null
            }).ToList();
            db.Identifiers.AddRange(rows);
            if (hash is not null) db.PasswordHistory.Add(new PasswordHistoryRow { Id = Guid.CreateVersion7(), PersonId = personId, Hash = hash, CreatedAt = now });
            try
            {
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                return ToPerson(row, rows);
            }
            catch (DbUpdateException error) when (IsUniqueViolation(error))
            {
                var constraint = (error.InnerException as PostgresException)?.ConstraintName ?? "";
                // 标识冲突是业务拒绝；主键冲突（几乎不可能）换一个新 sub 重试，绝不覆盖已有 Person。
                if (constraint.Contains("person_identifiers", StringComparison.Ordinal)) throw Error(IdentityErrors.IdentifierInUse);
                if (attempt >= 3 || !constraint.Contains("persons", StringComparison.Ordinal)) throw;
            }
        }
    }

    public async ValueTask<Person?> FindPersonAsync(string identifier, CancellationToken cancellationToken = default)
    {
        if (IdentifierRules.Classify(identifier, _account.DefaultPhoneCountryCode) is not { } classified) return null;
        using var neutral = TenantScope(null);
        await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var kind = IdentifierRules.KindName(classified.Kind);
        var id = await session.Context.Identifiers.AsNoTracking().Where(i => i.Kind == kind && i.Value == classified.Value).Select(i => (Guid?)i.PersonId).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return id is null ? null : await LoadPersonAsync(session.Context, id.Value, cancellationToken).ConfigureAwait(false);
    }

    // 只有手机号与邮箱有本人核验：VerifiedAt 由验证码核验或可信身份源写入。用户名建档即记为已验证（没有核验流程），这里不算。
    public async ValueTask<bool> IsIdentifierVerifiedAsync(string identifier, CancellationToken cancellationToken = default)
    {
        if (IdentifierRules.Classify(identifier, _account.DefaultPhoneCountryCode) is not { Kind: IdentifierKind.Phone or IdentifierKind.Email } classified) return false;
        using var neutral = TenantScope(null);
        await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var kind = IdentifierRules.KindName(classified.Kind);
        return await session.Context.Identifiers.AsNoTracking().AnyAsync(i => i.Kind == kind && i.Value == classified.Value && i.VerifiedAt != null, cancellationToken).ConfigureAwait(false);
    }

    // 登录索引即 Person 上的租户集合（建立成员时同事务追加，只增不减）；只读，不进入任何租户作用域。
    public async ValueTask<IReadOnlyList<Guid>?> ListPersonTenantsAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        using var neutral = TenantScope(null);
        await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var tenants = await session.Context.Persons.AsNoTracking().Where(p => p.Id == personId).Select(p => p.TenantIds).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return Array.AsReadOnly((tenants ?? []).Select(raw => Guid.TryParseExact(raw, "D", out var tenant) ? tenant : Guid.Empty).Where(tenant => tenant != Guid.Empty).Distinct().ToArray());
    }

    // ---- 标识绑定：软删除、占用冲突、绑定守卫 ----

    public async ValueTask<Person> BindIdentifierAsync(Guid personId, IdentifierInput identifier, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        var value = IdentifierRules.Normalize(identifier.Kind, identifier.Value, _account.DefaultPhoneCountryCode) ?? throw IdentifierRules.Invalid("#/identifier/value");
        using var neutral = TenantScope(null);
        await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var db = session.Context;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var person = await LockPersonAsync(db, personId, cancellationToken).ConfigureAwait(false);
        if (person.Status == AccountStatuses.Purged) throw Error(PlatformErrors.NotFound);
        var kind = IdentifierRules.KindName(identifier.Kind);
        var owner = await db.Identifiers.SingleOrDefaultAsync(i => i.Kind == kind && i.Value == value, cancellationToken).ConfigureAwait(false);
        if (owner is not null && owner.PersonId != personId) throw Error(IdentityErrors.IdentifierInUse);
        if (owner is null) db.Identifiers.Add(new PersonIdentifierRow { Id = Guid.CreateVersion7(), PersonId = personId, Kind = kind, Value = value, VerifiedAt = identifier.Verified ? Now : null });
        else if (identifier.Verified && owner.VerifiedAt is null) owner.VerifiedAt = Now;
        try { await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false); }
        catch (DbUpdateException error) when (IsUniqueViolation(error)) { throw Error(IdentityErrors.IdentifierInUse); }
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return (await LoadPersonAsync(db, personId, cancellationToken).ConfigureAwait(false))!;
    }

    public async ValueTask<Person> UnbindIdentifierAsync(Guid personId, IdentifierKind kind, string value, CancellationToken cancellationToken = default)
    {
        var normalized = IdentifierRules.Normalize(kind, value, _account.DefaultPhoneCountryCode) ?? throw IdentifierRules.Invalid("#/value");
        using var neutral = TenantScope(null);
        await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var db = session.Context;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await LockPersonAsync(db, personId, cancellationToken).ConfigureAwait(false);
        var rows = await db.Identifiers.Where(i => i.PersonId == personId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var name = IdentifierRules.KindName(kind);
        var target = rows.SingleOrDefault(i => i.Kind == name && i.Value == normalized) ?? throw Error(PlatformErrors.NotFound);
        EnsureNotLastVerified(target, rows.Where(i => i.Id != target.Id).ToList());
        db.Identifiers.Remove(target);
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return (await LoadPersonAsync(db, personId, cancellationToken).ConfigureAwait(false))!;
    }

    public async ValueTask<Person> ReplaceIdentifierAsync(Guid personId, IdentifierKind kind, string oldValue, IdentifierInput replacement, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(replacement);
        if (replacement.Kind != kind) throw IdentifierRules.Invalid("#/replacement/kind");
        var oldNormalized = IdentifierRules.Normalize(kind, oldValue, _account.DefaultPhoneCountryCode) ?? throw IdentifierRules.Invalid("#/old_value");
        var newNormalized = IdentifierRules.Normalize(kind, replacement.Value, _account.DefaultPhoneCountryCode) ?? throw IdentifierRules.Invalid("#/replacement/value");
        using var neutral = TenantScope(null);
        await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var db = session.Context;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var person = await LockPersonAsync(db, personId, cancellationToken).ConfigureAwait(false);
        var rows = await db.Identifiers.Where(i => i.PersonId == personId).ToListAsync(cancellationToken).ConfigureAwait(false);
        var name = IdentifierRules.KindName(kind);
        var old = rows.SingleOrDefault(i => i.Kind == name && i.Value == oldNormalized) ?? throw Error(PlatformErrors.NotFound);
        if (oldNormalized != newNormalized)
        {
            var added = new PersonIdentifierRow { Id = Guid.CreateVersion7(), PersonId = personId, Kind = name, Value = newNormalized, VerifiedAt = replacement.Verified ? Now : null };
            EnsureNotLastVerified(old, rows.Where(i => i.Id != old.Id).Append(added).ToList());
            // 先释放旧标识再占用新标识；同一事务内提交，对外原子。换绑只改标识，不改 Person.Id（sub）。
            db.Identifiers.Remove(old); await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            db.Identifiers.Add(added);
            try { await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false); }
            catch (DbUpdateException error) when (IsUniqueViolation(error)) { throw Error(IdentityErrors.IdentifierInUse); }
        }
        person.IdentityRevision++; await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return (await LoadPersonAsync(db, personId, cancellationToken).ConfigureAwait(false))!;
    }

    // 绑定守卫：解绑不能让账号失去最后一个已验证标识（无法找回）或全部标识（无法登录）。
    private static void EnsureNotLastVerified(PersonIdentifierRow target, List<PersonIdentifierRow> remaining)
    {
        if (remaining.Count == 0 || (target.VerifiedAt is not null && remaining.All(i => i.VerifiedAt is null)))
            throw new ErrorException(new Error(IdentityErrors.LastVerifiedIdentifier) { Pointer = "#/value" });
    }

    // ---- 凭据 ----

    public async ValueTask ChangePasswordAsync(Guid personId, PasswordChange change, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(change);
        using var current = PasswordSecret.From(change.Current); using var next = PasswordSecret.From(change.New);
        CheckPassword(next);
        // 当前密码的校验与登录共用同一套限流：猜测当前密码也计入错误次数。
        var throttle = await VerifyWithThrottleAsync(personId, async token => await VerifyStoredPasswordAsync(personId, current, token).ConfigureAwait(false)
            ? new AuthenticationResult(true, null, [PasswordAmr], null) : new AuthenticationResult(false, new Error(IdentityErrors.CurrentPasswordInvalid), [], null),
            cancellationToken).ConfigureAwait(false);
        if (throttle.Locked) throw Error(IdentityErrors.AccountLocked);
        if (throttle.Result is not { Succeeded: true }) throw new ErrorException(new Error(IdentityErrors.CurrentPasswordInvalid) { Pointer = "#/current_password" });
        var now = Now;
        using var neutral = TenantScope(null); using var origin = OpenOrigin(personId);
        await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var db = session.Context;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var row = await LockPersonAsync(db, personId, cancellationToken).ConfigureAwait(false);
        await EnsureNotReusedAsync(db, row, next, cancellationToken).ConfigureAwait(false);
        await SetPasswordAsync(db, row, next, false, now, cancellationToken).ConfigureAwait(false);
        row.IdentityRevision++;
        await RevokeSessionRowsAsync(db, personId, null, null, "password_changed", now, cancellationToken).ConfigureAwait(false);
        await SaveAsync(session, row, new IdentityPasswordChangedEvent(personId, row.IdentityRevision), cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask ResetPasswordAsync(Guid personId, PasswordReset reset, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reset);
        if (string.IsNullOrWhiteSpace(reset.Reason) || reset.Reason.Length > 200) throw IdentifierRules.Invalid("#/reason");
        using var next = PasswordSecret.From(reset.New);
        CheckPassword(next);
        var now = Now;
        using var neutral = TenantScope(null); using var origin = OpenOrigin();
        await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var db = session.Context;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var row = await LockPersonAsync(db, personId, cancellationToken).ConfigureAwait(false);
        if (row.Status == AccountStatuses.Purged) throw Error(PlatformErrors.NotFound);
        await SetPasswordAsync(db, row, next, reset.RequireChange, now, cancellationToken).ConfigureAwait(false);
        row.IdentityRevision++;
        await RevokeSessionRowsAsync(db, personId, null, null, "password_reset", now, cancellationToken).ConfigureAwait(false);
        await SaveAsync(session, row, new IdentityPasswordChangedEvent(personId, row.IdentityRevision), cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        if (reset.UnlinkExternalIdentities) await UnlinkExternalIdentitiesAsync(personId, row.TenantIds, cancellationToken).ConfigureAwait(false);
    }

    // 解除该 Person 在各租户的全部外部身份绑定（R163）。外部身份是租户表（强制 RLS），逐个租户在各自的作用域与事务里软删除。
    // 放在重置提交之后：重置已作废未用完的登录票据与进行中的绑定流程，确认绑定也会因身份修订已变而拒绝，此后不会再有新绑定落库。
    // 解除了绑定的那个事务先锁 Person，同时递增身份修订并吊销他的全部会话，效果与重置一致：凭这些绑定的会话与登录流程都要重新认证。
    // 中途失败时异常照常抛出，调用方（运营指定管理员）随之中止，用同一个初始密码重试即可收敛。
    internal async Task UnlinkExternalIdentitiesAsync(Guid personId, IReadOnlyList<string> tenantIds, CancellationToken ct)
    {
        var revoked = 0;
        foreach (var raw in tenantIds)
        {
            if (!Guid.TryParseExact(raw, "D", out var tenantId) || tenantId == Guid.Empty) continue;
            using var scope = TenantScope(tenantId); using var origin = OpenOrigin();
            await using var session = await OpenAsync(ct).ConfigureAwait(false);
            var db = session.Context;
            await using var transaction = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
            var person = await LockPersonAsync(db, personId, ct).ConfigureAwait(false);
            var links = await db.ExternalIdentities.Where(e => e.PersonId == personId).ToListAsync(ct).ConfigureAwait(false);
            if (links.Count == 0) continue;
            db.ExternalIdentities.RemoveRange(links);
            person.IdentityRevision++;
            revoked += await RevokeSessionRowsAsync(db, personId, null, null, "password_reset", Now, ct).ConfigureAwait(false);
            await session.SaveChangesAsync(ct).ConfigureAwait(false);
            await transaction.CommitAsync(ct).ConfigureAwait(false);
        }
        if (revoked > 0) _signal.Notify();
    }

    // 本人认领账号：申请人已用验证码证明持有这个标识，而它在该自然人上还没有经本人验证。一个事务里换成申请人的密码与显示名、
    // 标识标为已验证；与改密一样使既有会话失效，并递增身份修订。已加入企业的账号不认领（R163）：它的凭据由本人或企业管理员维护。
    public async ValueTask<Person> ClaimPersonAsync(PersonClaim claim, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(claim);
        var name = claim.DisplayName?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 200 || name.Any(char.IsControl)) throw IdentifierRules.Invalid("#/display_name");
        if (IdentifierRules.Classify(claim.Identifier, _account.DefaultPhoneCountryCode) is not { Kind: IdentifierKind.Phone or IdentifierKind.Email } identifier)
            throw IdentifierRules.Invalid("#/identifier");
        using var next = PasswordSecret.From(claim.Password);
        CheckPassword(next);
        var now = Now;
        using var neutral = TenantScope(null); using var origin = OpenOrigin(claim.PersonId);
        await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var db = session.Context;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var person = await LockPersonAsync(db, claim.PersonId, cancellationToken).ConfigureAwait(false);
        if (person.Status != AccountStatuses.Active || person.PlatformAccess || person.TenantIds.Length > 0) throw Error(PlatformErrors.Conflict);
        var kind = IdentifierRules.KindName(identifier.Kind);
        var owned = await db.Identifiers.SingleOrDefaultAsync(i => i.Kind == kind && i.Value == identifier.Value, cancellationToken).ConfigureAwait(false);
        if (owned is null || owned.PersonId != person.Id || owned.VerifiedAt is not null) throw Error(PlatformErrors.Conflict);
        owned.VerifiedAt = now; person.DisplayName = name;
        await SetPasswordAsync(db, person, next, false, now, cancellationToken).ConfigureAwait(false);
        person.IdentityRevision++;
        await RevokeSessionRowsAsync(db, person.Id, null, null, "account_claimed", now, cancellationToken).ConfigureAwait(false);
        await SaveAsync(session, person, new IdentityPasswordChangedEvent(person.Id, person.IdentityRevision), cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return (await LoadPersonAsync(db, person.Id, cancellationToken).ConfigureAwait(false))!;
    }

    // ---- 状态 ----

    public async ValueTask<Person> SetPersonStatusAsync(Guid personId, string status, string reason, CancellationToken cancellationToken = default)
    {
        if (status is not (AccountStatuses.Active or AccountStatuses.Disabled or AccountStatuses.Locked))
            throw new ErrorException(new Error(PlatformErrors.InputInvalid) { Errors = [new FieldError(PlatformErrors.ValueNotAllowed, "#/status")] });
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 32) throw IdentifierRules.Invalid("#/reason");
        var now = Now;
        using var neutral = TenantScope(null); using var origin = OpenOrigin();
        await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var db = session.Context;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var row = await LockPersonAsync(db, personId, cancellationToken).ConfigureAwait(false);
        if (row.Status == AccountStatuses.Purged) throw Error(PlatformErrors.NotFound);
        var changed = row.Status != status;
        if (changed)
        {
            row.Status = status; row.IdentityRevision++;
            if (status == AccountStatuses.Active) { row.FailedAttempts = 0; row.LastFailedAt = null; row.LockoutUntil = null; }
            else await RevokeSessionRowsAsync(db, personId, null, null, status, now, cancellationToken).ConfigureAwait(false);
        }
        await SaveAsync(session, row, changed && status == AccountStatuses.Locked ? new IdentityLockedEvent(personId, row.IdentityRevision, reason) : null, cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return (await LoadPersonAsync(db, personId, cancellationToken).ConfigureAwait(false))!;
    }

    public ValueTask<Membership> SetMembershipStatusAsync(Guid membershipId, string status, string reason, CancellationToken cancellationToken = default) =>
        SetMembershipStatusCoreAsync(membershipId, status, reason, false, false, cancellationToken);

    private async ValueTask<Membership> SetMembershipStatusCoreAsync(Guid membershipId, string status, string reason, bool directoryRules, bool deleted, CancellationToken cancellationToken)
    {
        if (status is not (AccountStatuses.Active or AccountStatuses.Disabled or AccountStatuses.Locked))
            throw new ErrorException(new Error(PlatformErrors.InputInvalid) { Errors = [new FieldError(PlatformErrors.ValueNotAllowed, "#/status")] });
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 32) throw IdentifierRules.Invalid("#/reason");
        var tenantId = RequireTenant(); var now = Now;
        Membership result;
        using (var scope = TenantScope(tenantId))
        using (var origin = OpenOrigin())
        {
            await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
            var db = session.Context;
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            var directoryLock = await LockDirectoryAsync(db, tenantId, cancellationToken).ConfigureAwait(false);
            var locked = await IdentitySql.QueryAsync(db, $"SELECT id FROM {IdentitySql.Memberships} WHERE id = @id FOR UPDATE", reader => reader.GetGuid(0), cancellationToken, ("id", membershipId)).ConfigureAwait(false);
            if (locked.Count == 0) throw Error(PlatformErrors.NotFound);
            var row = await db.Memberships.SingleAsync(m => m.Id == membershipId, cancellationToken).ConfigureAwait(false);
            if (row.Status == AccountStatuses.Purged || row.DirectoryDeleted && !deleted) throw Error(PlatformErrors.NotFound);
            if (directoryRules && status != AccountStatuses.Active && row.IsTenantAdministrator) throw Error(IdentityErrors.DirectoryAdministratorProtected);
            if (directoryRules && !DirectoryRules.CanTransition(row.Status, status)) throw Error(IdentityErrors.DirectoryTransitionInvalid);
            var (previousStatus, wasDeleted) = (row.Status, row.DirectoryDeleted);
            if (deleted) row.DirectoryDeleted = true;
            if (row.Status != status)
            {
                row.Status = status; row.IdentityRevision++;
                if (status != AccountStatuses.Active) await RevokeSessionRowsAsync(db, row.PersonId, null, row.Id, "member_" + status, now, cancellationToken).ConfigureAwait(false);
            }
            // 目录入口与受信账号入口共用这里：状态或删除标记真的变了才发 directory.member.status_changed.v1，与写入同事务。
            var events = new DirectoryEventBatch();
            if (previousStatus != row.Status || wasDeleted != row.DirectoryDeleted)
                events.Add(revision => new DirectoryMemberStatusChangedEvent(revision, row.Id, row.PersonId, row.Status, previousStatus, row.DirectoryDeleted, row.IdentityRevision));
            var changes = await RaiseDirectoryEventsAsync(db, directoryLock, events, cancellationToken).ConfigureAwait(false);
            await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            EnsurePublished(changes);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            result = ToMembership(row);
        }
        if (status != AccountStatuses.Active)
            await _ctx.DispatchAsync(IdentityRuntimeEvents.MemberDisabled, new MembershipChanged(result.Id, result.TenantId, result.IdentityRevision), cancellationToken).ConfigureAwait(false);
        return result;
    }

    // ---- 外部身份 ----

    public async ValueTask LinkExternalIdentityAsync(ExternalIdentityLink link, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(link);
        if (string.IsNullOrWhiteSpace(link.ConnectorId) || link.ConnectorId.Length > 100 || string.IsNullOrWhiteSpace(link.Subject) || link.Subject.Length > 256 || link.UnionId?.Length > 256)
            throw IdentifierRules.Invalid("#/subject");
        var tenantId = RequireTenant();
        using var scope = TenantScope(tenantId);
        await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var db = session.Context;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var member = await db.Memberships.AsNoTracking().SingleOrDefaultAsync(m => m.Id == link.MembershipId, cancellationToken).ConfigureAwait(false) ?? throw Error(PlatformErrors.NotFound);
        if (member.Status != AccountStatuses.Active) throw Error(IdentityErrors.AccountDisabled);
        var existing = await db.ExternalIdentities.SingleOrDefaultAsync(e => e.ConnectorId == link.ConnectorId && e.Subject == link.Subject, cancellationToken).ConfigureAwait(false);
        if (existing is not null) { if (existing.MembershipId == link.MembershipId) return; throw Error(IdentityErrors.IdentifierInUse); }
        db.ExternalIdentities.Add(new ExternalIdentityRow
        {
            Id = Guid.CreateVersion7(), MembershipId = member.Id, PersonId = member.PersonId, ConnectorId = link.ConnectorId, Subject = link.Subject, UnionId = link.UnionId
        });
        try { await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false); }
        catch (DbUpdateException error) when (IsUniqueViolation(error)) { throw Error(IdentityErrors.IdentifierInUse); }
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    // 解除一个外部身份绑定，同事务递增该 Person 的身份修订：解除之前查到这个绑定、还没建立会话的连接器登录要重新认证。已建立的会话不吊销。
    public async ValueTask UnlinkExternalIdentityAsync(Guid membershipId, string connectorId, string subject, CancellationToken cancellationToken = default)
    {
        var tenantId = RequireTenant();
        using var scope = TenantScope(tenantId);
        await using var session = await OpenAsync(cancellationToken).ConfigureAwait(false);
        var db = session.Context;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var owner = await db.ExternalIdentities.AsNoTracking().Where(e => e.MembershipId == membershipId && e.ConnectorId == connectorId && e.Subject == subject)
            .Select(e => (Guid?)e.PersonId).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? throw Error(PlatformErrors.NotFound);
        // 先锁 Person 再动绑定，与重置、确认绑定同一顺序；锁住之后重新读绑定，期间已被别处解除的按不存在处理。
        var person = await LockPersonAsync(db, owner, cancellationToken).ConfigureAwait(false);
        var target = await db.ExternalIdentities.SingleOrDefaultAsync(e => e.MembershipId == membershipId && e.ConnectorId == connectorId && e.Subject == subject, cancellationToken).ConfigureAwait(false)
            ?? throw Error(PlatformErrors.NotFound);
        // 守卫：解绑后仍要留有登录方式——密码、本租户其他外部身份，或可用于找回的已验证联系方式。
        var otherLinks = await db.ExternalIdentities.AsNoTracking().CountAsync(e => e.PersonId == target.PersonId && e.Id != target.Id, cancellationToken).ConfigureAwait(false);
        var verifiedContacts = await db.Identifiers.AsNoTracking().CountAsync(i => i.PersonId == target.PersonId && i.VerifiedAt != null && (i.Kind == "phone" || i.Kind == "email"), cancellationToken).ConfigureAwait(false);
        if (person.PasswordHash is null && otherLinks == 0 && verifiedContacts == 0) throw Error(IdentityErrors.LastLoginMethod);
        db.ExternalIdentities.Remove(target);
        person.IdentityRevision++;
        await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
