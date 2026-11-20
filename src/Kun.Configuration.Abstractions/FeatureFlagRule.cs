using System.Collections.Frozen;

namespace Kun.Configuration;

/// <summary>从快照读取的一份开关规则。</summary>
public sealed class FeatureFlagRule
{
    internal FeatureFlagRule(bool fallback, FrozenSet<Guid> tenants, int percentage)
    {
        Default = fallback;
        Tenants = tenants;
        Percentage = percentage;
    }

    /// <summary>未命中定向条件时的值。</summary>
    public bool Default { get; }
    /// <summary>指定的租户集合。</summary>
    public IReadOnlySet<Guid> Tenants { get; }
    /// <summary>百分比。</summary>
    public int Percentage { get; }
}
