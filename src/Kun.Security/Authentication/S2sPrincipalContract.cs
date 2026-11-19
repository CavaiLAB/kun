using System.Text.RegularExpressions;

namespace Kun.Security.Authentication;

/// <summary>已认证的主体快照在外部身份投影、网关签发与服务端接收之间共用的契约。</summary>
public static partial class S2sPrincipalContract
{
    /// <summary>核对身份字段，返回首个不符合契约的字段或规则名。此方法不校验令牌签名。</summary>
    public static string? ValidateIdentity(Principal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        if (!Text(principal.Subject, 255)) return "sub";
        if (principal.Kind is not (PrincipalKind.User or PrincipalKind.Service) || principal.Actor is not null) return "principal_kind";
        if (principal.TenantId is not null && !Uuid(principal.TenantId)) return "tenant_id";
        if (principal.MembershipId is not null && !Uuid(principal.MembershipId)) return "membership_id";
        if (principal.ProjectId is not null && !Uuid(principal.ProjectId)) return "project_id";
        if (principal.PluginId is not null && !PluginIdPattern().IsMatch(principal.PluginId)) return "plugin_id";
        if (principal.Kind == PrincipalKind.User && (principal.IdentityRevision is null || principal.TenantId is not null && principal.MembershipId is null)) return "user";
        if (principal.Kind == PrincipalKind.Service && (principal.MembershipId is not null || principal.SessionId != principal.TokenId)) return "service";
        if (principal.IdentityRevision < 0) return "identity_revision";
        if (!Uuid(principal.SessionId)) return "session_id";
        if (principal.Scope is not { Length: <= 4096 } || !ScopePattern().IsMatch(principal.Scope)) return "scope";
        return null;
    }

    /// <summary>核对每跳服务令牌的主体、受众、角色与时间窗。签名、密钥、发行方及目标受众绑定由认证方案核验。</summary>
    public static string? Validate(Principal principal, DateTimeOffset now)
    {
        if (ValidateIdentity(principal) is { } identityError) return identityError;
        if (!Text(principal.Issuer, 255)) return "iss";
        if (principal.Audience is not [var audience] || !Text(audience, 255)) return "audience";
        if (!Uuid(principal.TokenId)) return "jti";
        if (principal.Roles is not { Count: <= 32 } roles || roles.Any(role => !Text(role, 64))
            || roles.Distinct(StringComparer.Ordinal).Count() != roles.Count) return "roles";
        if (principal.IssuedAt < 0 || principal.NotBefore < principal.IssuedAt || principal.ExpiresAt <= principal.NotBefore
            || principal.ExpiresAt - principal.IssuedAt > 300) return "lifetime";
        return principal.IssuedAt > now.ToUnixTimeSeconds() + 30 ? "iat" : null;
    }

    private static bool Uuid(string? value)
        => value is not null && Guid.TryParseExact(value, "D", out var id) && id.ToString("D") == value;

    private static bool Text(string? value, int maximum)
        => !string.IsNullOrWhiteSpace(value) && value.Length <= maximum && !value.Any(char.IsControl);

    [GeneratedRegex(@"\A[a-z][a-z0-9-]*(\.[a-z][a-z0-9-]*)+\z", RegexOptions.CultureInvariant)]
    private static partial Regex PluginIdPattern();

    [GeneratedRegex(@"\A([\x21\x23-\x5B\x5D-\x7E]+( [\x21\x23-\x5B\x5D-\x7E]+)*)?\z", RegexOptions.CultureInvariant)]
    private static partial Regex ScopePattern();
}
