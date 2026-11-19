using System.Text.Json.Serialization;

namespace Kun.Core.MultiTenancy;

/// <summary>多租户数据隔离策略。数值用于宿主配置，小写 JSON 值用于租户契约。</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TenantIsolation>))]
public enum TenantIsolation
{
    /// <summary>共享数据库与 schema，以租户列隔离数据。</summary>
    [JsonStringEnumMemberName("row")]
    Row = 0,

    /// <summary>每个租户使用单独的 schema。</summary>
    [JsonStringEnumMemberName("schema")]
    Schema = 1,

    /// <summary>每个租户使用单独的数据库。</summary>
    [JsonStringEnumMemberName("database")]
    Database = 2,
}
