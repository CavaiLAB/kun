using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Kun.Core.MultiTenancy;
using Kun.Runtime;

namespace Kun.Tenancy;

/// <summary>租户目录 seam；仅提供内部服务端投影，不发布匿名 HTTP 租户列表。</summary>
public static class TenancyDefinition
{
    /// <summary>目录服务键。</summary>
    public static readonly ServiceKey Key = new("Tenancy");
    /// <summary>当前 scope 可见的目录。</summary>
    public static ITenantDirectory Tenancy(this IPluginContext context) => context.Inject<ITenantDirectory>(Key);
}

/// <summary>身份及开户组件消费的主档目录；不包含管理员、联系方式、数据库凭据。</summary>
public interface ITenantDirectory
{
    /// <summary>按不可变的小写字母数字租户号读取；未知返回 null。</summary>
    ValueTask<TenantDescriptor?> GetAsync(string code, CancellationToken cancellationToken = default);
    /// <summary>按已验证主体的租户 ID 读取主档；此内部服务端口不构成匿名 HTTP 查询。</summary>
    ValueTask<TenantDescriptor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>租户主档的非敏感投影。</summary>
public sealed record TenantDescriptor(Guid Id, string Code, string Name, TenantIsolation Isolation, TenantStatus Status)
{
    /// <summary>当前主档的乐观并发版本；编辑请求须回传。</summary>
    public uint Revision { get; init; }
}

/// <summary>生命周期状态；本任务仅创建 pending，迁移由租户生命周期实现。</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TenantStatus>))]
public enum TenantStatus
{
    /// <summary>待激活。</summary>
    [JsonStringEnumMemberName("pending")] Pending,
    /// <summary>试用。</summary>
    [JsonStringEnumMemberName("trial")] Trial,
    /// <summary>正常。</summary>
    [JsonStringEnumMemberName("active")] Active,
    /// <summary>宽限。</summary>
    [JsonStringEnumMemberName("grace")] Grace,
    /// <summary>冻结。</summary>
    [JsonStringEnumMemberName("suspended")] Suspended
}

/// <summary>新建租户请求；目标租户 ID 由服务端生成，不能经请求输入。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateTenant(
    [property: MinLength(1), MaxLength(64), RegularExpression(@"^[a-z0-9]+$(?![\s\S])")] string Code,
    [property: MinLength(1), MaxLength(200), RegularExpression(@"^(?=[\s\S]*[^\u0009-\u000D\u0020\u0085\u00A0\u1680\u2000-\u200A\u2028\u2029\u202F\u205F\u3000])[^\u0000-\u001F\u007F-\u009F]*$(?![\s\S])")] string Name,
    [property: AllowedValues(TenantIsolation.Row)] TenantIsolation Isolation = TenantIsolation.Row)
{
    /// <summary>运营开户的管理员指针；HTTP 新建必填，不代表 Identity 已创建 Membership。</summary>
    [JsonRequired, DisallowNull]
    public TenantAdministrator? Administrator { get; init; }
    /// <summary>初始限额；空表示无限额。</summary>
    public TenantQuotas? Quotas { get; init; }
}

/// <summary>编辑主档；租户号和隔离策略不在可编辑字段中。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record UpdateTenant(
    [property: MinLength(1), MaxLength(200), RegularExpression(@"^(?=[\s\S]*[^\u0009-\u000D\u0020\u0085\u00A0\u1680\u2000-\u200A\u2028\u2029\u202F\u205F\u3000])[^\u0000-\u001F\u007F-\u009F]*$(?![\s\S])")] string Name,
    uint ExpectedRevision)
{
    /// <summary>非空时替换限额；省略保持原值。</summary>
    public TenantQuotas? Quotas { get; init; }
}
