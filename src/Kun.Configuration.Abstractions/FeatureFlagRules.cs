using System.Collections.Frozen;
using System.Text.Json;

namespace Kun.Configuration;

/// <summary>解析并校验配置快照中的特性开关规则。</summary>
public static class FeatureFlagRules
{
    /// <summary>约定的开关命名空间。</summary>
    public const string Namespace = "qian.common.flags";
    /// <summary>规则键前缀。</summary>
    public const string Prefix = "flag.";
    private const int MaximumRules = 1024;
    /// <summary>每份规则最多指定的租户数。</summary>
    public const int MaximumTenants = 256;

    /// <summary>检查开关名称的格式。</summary>
    public static bool ValidName(string? name) => name is { Length: > 0 and <= 100 } &&
        name[0] is >= 'a' and <= 'z' && name.All(character =>
            character is >= 'a' and <= 'z' or >= '0' and <= '9' or '.' or '-' or '_');

    /// <summary>读取不可变规则集，规则无效时抛出 InvalidDataException。</summary>
    public static FrozenDictionary<string, FeatureFlagRule> Read(IEnumerable<KeyValuePair<string, string?>> values)
    {
        var result = new Dictionary<string, FeatureFlagRule>(StringComparer.Ordinal);
        foreach (var (key, value) in values)
        {
            if (!key.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)) continue;
            var name = key[Prefix.Length..];
            if (!key.StartsWith(Prefix, StringComparison.Ordinal) || !ValidName(name) || value is null ||
                result.Count >= MaximumRules || !result.TryAdd(name, Parse(value))) throw Invalid();
        }
        return result.ToFrozenDictionary(StringComparer.Ordinal);
    }

    /// <summary>检查快照中的规则是否都能解析。</summary>
    public static bool Validate(IReadOnlyDictionary<string, string?> values)
    {
        try { _ = Read(values); return true; }
        catch (InvalidDataException) { return false; }
    }

    private static FeatureFlagRule Parse(string json)
    {
        if (json.Length is 0 or > 16_384) throw Invalid();
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 4 });
            if (document.RootElement.ValueKind != JsonValueKind.Object) throw Invalid();
            bool? fallback = null;
            var percentage = 0;
            var tenants = new HashSet<Guid>();
            var fields = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!fields.Add(property.Name)) throw Invalid();
                switch (property.Name)
                {
                    case "default" when property.Value.ValueKind is JsonValueKind.True or JsonValueKind.False:
                        fallback = property.Value.GetBoolean(); break;
                    case "percentage" when property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out var amount) && amount is >= 0 and <= 100:
                        percentage = amount; break;
                    case "tenants" when property.Value.ValueKind == JsonValueKind.Array:
                        foreach (var value in property.Value.EnumerateArray())
                        {
                            if (value.ValueKind != JsonValueKind.String || !Guid.TryParseExact(value.GetString(), "D", out var tenant) ||
                                tenant == Guid.Empty || tenants.Count >= MaximumTenants || !tenants.Add(tenant)) throw Invalid();
                        }
                        break;
                    default: throw Invalid();
                }
            }
            if (fallback is null) throw Invalid();
            return new(fallback.Value, tenants.ToFrozenSet(), percentage);
        }
        catch (JsonException) { throw Invalid(); }
    }

    // JSON 异常可能包含原值、字节片段；只保留固定诊断，绝不把原异常挂到 InnerException。
    private static InvalidDataException Invalid() => new("特性开关规则不满足固定契约。");
}
