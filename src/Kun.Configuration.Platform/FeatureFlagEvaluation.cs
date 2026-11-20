using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Kun.Configuration.Platform;

internal static class FeatureFlagEvaluation
{
    internal static bool Evaluate(this FeatureFlagRule rule, string name, Guid tenantId)
    {
        if (rule.Tenants.Contains(tenantId)) return true;
        if (rule.Percentage > 0)
        {
            // 明确字节协议，不用进程随机化的 string.GetHashCode。增加百分比只扩大同一集合。
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(name + "\n" + tenantId.ToString("D")));
            if (BinaryPrimitives.ReadUInt64BigEndian(hash) % 10_000 < (ulong)rule.Percentage * 100) return true;
        }
        return rule.Default;
    }
}
