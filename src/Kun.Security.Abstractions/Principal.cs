using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kun.Security;

/// <summary>统一主体模型；Agent 在 S1 只预留，不因此开放签发。</summary>
[JsonConverter(typeof(PrincipalKindConverter))]
public enum PrincipalKind
{
    /// <summary>自然人。</summary>
    User,
    /// <summary>机器服务。</summary>
    Service,
    /// <summary>Agent 客户端，S4 启用。</summary>
    Agent
}

/// <summary>冻结 S2S payload 的 DTO，不承担 JWT 验签、发行或权限判定。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class Principal
{
    /// <summary>签发方。</summary>
    [JsonPropertyName(IdentityClaimNames.Issuer)] public required string Issuer { get; init; }
    /// <summary>服务受众。</summary>
    [JsonPropertyName(IdentityClaimNames.Audience)] public required IReadOnlyList<string> Audience { get; init; }
    /// <summary>person/client 标识。</summary>
    [JsonPropertyName(IdentityClaimNames.Subject)] public required string Subject { get; init; }
    /// <summary>主体类型。</summary>
    [JsonPropertyName(IdentityClaimNames.PrincipalKind)] public required PrincipalKind Kind { get; init; }
    /// <summary>租户 UUID 字符串；平台主体必须显式输出 null。</summary>
    [JsonPropertyName(IdentityClaimNames.TenantId), JsonIgnore(Condition = JsonIgnoreCondition.Never)] public required string? TenantId { get; init; }
    /// <summary>用户租户成员；机器主体不得携带。</summary>
    [JsonPropertyName(IdentityClaimNames.MembershipId)] public string? MembershipId { get; init; }
    /// <summary>用户身份修订。</summary>
    [JsonPropertyName(IdentityClaimNames.IdentityRevision)] public long? IdentityRevision { get; init; }
    /// <summary>可选项目。</summary>
    [JsonPropertyName(IdentityClaimNames.ProjectId)] public string? ProjectId { get; init; }
    /// <summary>可选插件主体。</summary>
    [JsonPropertyName(IdentityClaimNames.PluginId)] public string? PluginId { get; init; }
    /// <summary>粗粒度角色。</summary>
    [JsonPropertyName(IdentityClaimNames.Roles)] public required IReadOnlyList<string> Roles { get; init; }
    /// <summary>空格分隔 scope。</summary>
    [JsonPropertyName(IdentityClaimNames.Scope)] public required string Scope { get; init; }
    /// <summary>会话 UUID；机器主体使用 jti。</summary>
    [JsonPropertyName(IdentityClaimNames.SessionId)] public required string SessionId { get; init; }
    /// <summary>预留委托链。</summary>
    [JsonPropertyName(IdentityClaimNames.Actor)] public DelegationActor? Actor { get; init; }
    /// <summary>NumericDate。</summary>
    [JsonPropertyName(IdentityClaimNames.IssuedAt)] public required long IssuedAt { get; init; }
    /// <summary>NumericDate。</summary>
    [JsonPropertyName(IdentityClaimNames.NotBefore)] public required long NotBefore { get; init; }
    /// <summary>NumericDate，签发方须保证有效期不超过 300 秒。</summary>
    [JsonPropertyName(IdentityClaimNames.ExpiresAt)] public required long ExpiresAt { get; init; }
    /// <summary>令牌 UUID。</summary>
    [JsonPropertyName(IdentityClaimNames.TokenId)] public required string TokenId { get; init; }
    /// <summary>避免默认字符串化把 session_id、jti 或委托链写入日志。</summary>
    public override string ToString() => "Principal(redacted)";
}
/// <summary>RFC 8693 委托链，保持与 v0 Actor 定义同形。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class DelegationActor
{
    /// <summary>行事主体。</summary>
    [JsonPropertyName(IdentityClaimNames.Subject)] public required string Subject { get; init; }
    /// <summary>主体类型。</summary>
    [JsonPropertyName(IdentityClaimNames.PrincipalKind)] public required PrincipalKind Kind { get; init; }
    /// <summary>更早的行事方。</summary>
    [JsonPropertyName(IdentityClaimNames.Actor)] public DelegationActor? Actor { get; init; }
    /// <inheritdoc />
    public override string ToString() => "DelegationActor(redacted)";
}

/// <summary>主体类型的 JSON 转换器：只收 user、service、agent 三个小写字符串，数字与其它值一律报错。</summary>
/// <remarks>公开是因为别的程序集里的 JSON 源生成上下文要引用它。不能换成 <see cref="JsonStringEnumConverter{TEnum}"/>：以特性指定时它也收数字。</remarks>
public sealed class PrincipalKindConverter : JsonConverter<PrincipalKind>
{
    /// <inheritdoc />
    public override PrincipalKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType == JsonTokenType.String
        ? reader.GetString() switch { "user" => PrincipalKind.User, "service" => PrincipalKind.Service, "agent" => PrincipalKind.Agent, _ => throw new JsonException("未知主体类型。") }
        : throw new JsonException("主体类型必须为字符串。");
    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, PrincipalKind value, JsonSerializerOptions options) => writer.WriteStringValue(value switch { PrincipalKind.User => "user", PrincipalKind.Service => "service", PrincipalKind.Agent => "agent", _ => throw new JsonException("未知主体类型。") });
}
