using Kun.Core.MultiTenancy;

namespace Kun.EntityFrameworkCore.MultiTenancy;

/// <summary>
/// Configuration for <c>AddKunMultiTenancy</c>. Selects the isolation strategy and tunes
/// the few knobs the framework exposes across all three strategies.
/// </summary>
public sealed class KunMultiTenancyOptions
{
    /// <summary>The active isolation strategy. Defaults to <see cref="TenantIsolation.Row"/>.</summary>
    public TenantIsolation Strategy { get; set; } = TenantIsolation.Row;

    /// <summary>租户数据库架构名称的前缀，后接 32 位小写 UUID。</summary>
    public string SchemaPrefix { get; set; } = "tenant_";

    /// <summary>返回指定租户的 PostgreSQL 架构名，可用于上下文工厂和迁移配置。</summary>
    public string GetSchemaName(Guid tenantId)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("A non-empty tenant ID is required.", nameof(tenantId));
        var prefix = SchemaPrefix;
        if (string.IsNullOrEmpty(prefix) || prefix.Length > 31 || !(char.IsAsciiLetter(prefix[0]) || prefix[0] == '_') ||
            !prefix.All(c => char.IsAsciiLetterOrDigit(c) || c == '_'))
            throw new InvalidOperationException("SchemaPrefix must be a PostgreSQL identifier prefix of 1 to 31 ASCII characters.");
        return $"{prefix}{tenantId:N}";
    }

    /// <summary>
    /// When <see langword="true"/>, <c>SchemaPerTenantProvisioner.DeprovisionAsync</c> is
    /// permitted to run <c>DROP SCHEMA IF EXISTS "{schema}" CASCADE</c>. Default is
    /// <see langword="false"/> — deprovisioning is a data-loss operation and must be an
    /// explicit opt-in. When left at the default, <c>DeprovisionAsync</c> throws
    /// <see cref="NotSupportedException"/> with a remediation message.
    /// </summary>
    public bool AllowDestructiveDeprovision { get; set; }
}
