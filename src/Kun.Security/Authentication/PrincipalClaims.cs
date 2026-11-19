using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kun.Security.Authentication;

/// <summary>从已认证的主身份读取授权主体，不拼接不同身份的 claim。</summary>
public static class PrincipalClaims
{
    /// <summary>
    /// 从认证方案已验证的 JWT 载荷与对应的主身份读取 S2S 快照。仅返回符合冻结契约且与该身份一致的快照。
    /// 调用方负责验签、核验目标发行方和受众、校验有效期。返回值为错误类别，成功时为 null。
    /// </summary>
    public static string? ReadS2s(ClaimsPrincipal? identity, JsonElement payload, DateTimeOffset now, out Principal? snapshot)
    {
        snapshot = null;
        if (payload.ValueKind != JsonValueKind.Object) return "shape";
        var fields = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in payload.EnumerateObject())
        {
            if (!fields.Add(property.Name)) return "shape";
            if (!IdentityClaimNames.S2sRequired.Contains(property.Name) && !IdentityClaimNames.S2sOptional.Contains(property.Name)) return "shape";
            if (property.Value.ValueKind == JsonValueKind.Null && property.Name != IdentityClaimNames.TenantId) return "null";
        }
        if (!fields.IsSupersetOf(IdentityClaimNames.S2sRequired)) return "shape";
        Principal? value;
        try { value = payload.Deserialize(PrincipalClaimJsonContext.Default.Principal); }
        catch (JsonException) { return "shape"; }
        if (value is null) return "shape";
        if (S2sPrincipalContract.Validate(value, now) is { } reason) return reason;
        if (!TryRead(identity, out var subject, out var tenant, out var kind) || subject != value.Subject
            || tenant != value.TenantId || kind != (value.Kind == PrincipalKind.User ? "user" : "service")) return "claims";
        snapshot = value;
        return null;
    }

    /// <summary>读取 sub/NameIdentifier、tenant_id 与 principal_kind；缺少类型的旧浏览器身份按 user，重复或冲突字段拒绝。</summary>
    public static bool TryRead(ClaimsPrincipal? principal, [NotNullWhen(true)] out string? subjectId,
        out string? tenantId, [NotNullWhen(true)] out string? principalKind)
    {
        subjectId = null; tenantId = null; principalKind = null;
        if (principal?.Identity is not ClaimsIdentity { IsAuthenticated: true } identity) return false;
        var identifiers = identity.Claims.Where(claim => claim.Type is "sub" or ClaimTypes.NameIdentifier)
            .Select(claim => claim.Value).Distinct(StringComparer.Ordinal).ToArray();
        if (identifiers.Length != 1 || string.IsNullOrWhiteSpace(identifiers[0])) return false;
        var kinds = identity.FindAll("principal_kind").ToArray();
        var kind = kinds.Length == 0 ? "user" : kinds.Length == 1 ? kinds[0].Value : null;
        if (kind is not ("user" or "service" or "agent")) return false;
        var tenants = identity.FindAll("tenant_id").ToArray();
        if (tenants.Length > 1) return false;
        subjectId = identifiers[0]; principalKind = kind;
        tenantId = tenants.Length == 0 || tenants[0].ValueType == "JSON_NULL" ? null : tenants[0].Value;
        return true;
    }
}

[JsonSerializable(typeof(Principal))]
internal sealed partial class PrincipalClaimJsonContext : JsonSerializerContext;
