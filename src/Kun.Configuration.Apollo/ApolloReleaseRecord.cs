namespace Kun.Configuration.Apollo;

/// <summary>一份快照的发布结果，仅含资源标识、变更数量、原因码与脱敏摘要。</summary>
public sealed class ApolloReleaseRecord
{
    internal ApolloReleaseRecord(string @namespace, string resource, bool rejected, string inputHash, int changedKeys,
        IReadOnlyList<string> reasons, DateTimeOffset occurredAt)
    {
        Namespace = @namespace;
        Resource = resource;
        Rejected = rejected;
        InputHash = inputHash;
        ChangedKeys = changedKeys;
        Reasons = Array.AsReadOnly(reasons.ToArray());
        OccurredAt = occurredAt;
    }
    /// <summary>配置命名空间。</summary>
    public string Namespace { get; }
    /// <summary>本次发布的资源标识。</summary>
    public string Resource { get; }
    /// <summary>该快照是否被拒绝。</summary>
    public bool Rejected { get; }
    /// <summary>脱敏快照的 SHA-256 摘要。</summary>
    public string InputHash { get; }
    /// <summary>与上次生效快照比较的键变更数。</summary>
    public int ChangedKeys { get; }
    /// <summary>拒绝原因码。</summary>
    public IReadOnlyList<string> Reasons { get; }
    /// <summary>实例记录此次发布的时间。</summary>
    public DateTimeOffset OccurredAt { get; }
}
