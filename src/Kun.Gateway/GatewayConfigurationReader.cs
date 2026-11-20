using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace Kun.Gateway;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
[JsonSerializable(typeof(GatewayConfiguration))]
internal sealed partial class GatewayJson : JsonSerializerContext;

internal static class GatewayConfigurationReader
{
    internal static GatewayConfiguration ReadStandalone(IConfiguration section)
    {
        T Read<T>(string key, T fallback) where T : IParsable<T> =>
            section[key] is { } value ? T.Parse(value, System.Globalization.CultureInfo.InvariantCulture) : fallback;
        string[] ReadStrings(string key) => section.GetSection(key).GetChildren().Select(child => child.Value ?? string.Empty).ToArray();
        var defaults = new GatewayConfiguration();
        return defaults with
        {
            IdentityIssuer = section[nameof(defaults.IdentityIssuer)] ?? defaults.IdentityIssuer,
            IdentityAudience = section[nameof(defaults.IdentityAudience)] ?? defaults.IdentityAudience,
            ExternalJwksUri = section[nameof(defaults.ExternalJwksUri)] ?? defaults.ExternalJwksUri,
            Issuer = section[nameof(defaults.Issuer)] ?? defaults.Issuer,
            InternalAudience = section[nameof(defaults.InternalAudience)] ?? defaults.InternalAudience,
            SigningKeyPath = section[nameof(defaults.SigningKeyPath)] ?? defaults.SigningKeyPath,
            PublicKeyPath = section[nameof(defaults.PublicKeyPath)] ?? defaults.PublicKeyPath,
            InternalTokenLifetime = Read(nameof(defaults.InternalTokenLifetime), defaults.InternalTokenLifetime),
            ExposeInternalJwks = Read(nameof(defaults.ExposeInternalJwks), defaults.ExposeInternalJwks),
            EmitTraceIdClaim = Read(nameof(defaults.EmitTraceIdClaim), defaults.EmitTraceIdClaim),
            ForwardLimit = Read(nameof(defaults.ForwardLimit), defaults.ForwardLimit),
            KeyOverlapPeriod = Read(nameof(defaults.KeyOverlapPeriod), defaults.KeyOverlapPeriod),
            KeyReloadInterval = Read(nameof(defaults.KeyReloadInterval), defaults.KeyReloadInterval),
            TrustedProxyAddresses = ReadStrings(nameof(defaults.TrustedProxyAddresses)),
            TrustedProxyNetworks = ReadStrings(nameof(defaults.TrustedProxyNetworks)),
            ForwardedClaimTypes = ReadStrings(nameof(defaults.ForwardedClaimTypes))
        };
    }

    internal static GatewayConfiguration Read(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object) throw Invalid();
        if (value.EnumerateObject().Any(p => p.Name is not ("identityIssuer" or "identityAudience" or "issuer" or
            "signingKeyPath" or "publicKeyPath" or "trustedProxyNetworks" or "routes" or "clusters" or "rateLimits"))) throw Invalid();
        RejectDuplicateNames(value);
        var parsed = value.Deserialize(GatewayJson.Default.GatewayConfiguration) ?? throw Invalid();
        // 源生成的 record 构造器会给未出现的 init 属性传默认值；按字段是否出现保留声明式缺省值，显式非法值仍拒绝。
        var defaults = new GatewayRateLimits();
        if (!value.TryGetProperty("rateLimits", out var rates)) return Copy(parsed with { RateLimits = defaults });
        if (rates.ValueKind != JsonValueKind.Object) throw Invalid();
        return Copy(parsed with
        {
            RateLimits = new GatewayRateLimits
            {
                KeyPrefix = rates.TryGetProperty("keyPrefix", out var prefix) ? prefix.GetString() ?? "" : defaults.KeyPrefix,
                WindowSeconds = Integer("windowSeconds", defaults.WindowSeconds),
                IpPermitLimit = Integer("ipPermitLimit", defaults.IpPermitLimit),
                TenantPermitLimit = Integer("tenantPermitLimit", defaults.TenantPermitLimit),
                UserPermitLimit = Integer("userPermitLimit", defaults.UserPermitLimit)
            }
        });

        int Integer(string name, int fallback) => rates.TryGetProperty(name, out var number) ? number.GetInt32() : fallback;
    }

    internal static GatewayConfiguration Copy(GatewayConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!Address(value.IdentityIssuer) || !Text(value.IdentityAudience) || !Address(value.Issuer) ||
            !System.IO.Path.IsPathFullyQualified(value.SigningKeyPath) || !System.IO.Path.IsPathFullyQualified(value.PublicKeyPath) ||
            value.TrustedProxyNetworks is null || value.Routes is null || value.Clusters is null || value.RatesInvalid() || value.Routes.Count == 0 || value.Clusters.Count == 0 ||
            value.Clusters.Any(cluster => cluster is null || !Text(cluster.Id) || !Address(cluster.Address)) ||
            value.Clusters.Select(cluster => cluster.Id).Distinct(StringComparer.Ordinal).Count() != value.Clusters.Count ||
            value.Routes.Any(route => route is null || !Text(route.Id) || route.Path is not { Length: > 0 } || route.Path[0] != '/' ||
                route.Path.Contains('?', StringComparison.Ordinal) || route.Path.Any(char.IsControl) || !value.Clusters.Any(cluster => cluster.Id == route.Cluster) ||
                route.RateLimiterPolicy is not ("kun-default" or "tenant" or "user")) ||
            value.Routes.Select(route => route.Id).Distinct(StringComparer.Ordinal).Count() != value.Routes.Count ||
            value.TrustedProxyNetworks.Any(network => !IPNetwork.TryParse(network, out var parsed) || parsed.PrefixLength == 0))
            throw Invalid();
        return value with
        {
            TrustedProxyNetworks = Array.AsReadOnly(value.TrustedProxyNetworks.ToArray()),
            Routes = Array.AsReadOnly(value.Routes.ToArray()),
            Clusters = Array.AsReadOnly(value.Clusters.ToArray()),
            RateLimits = value.RateLimits with { }
        };
    }

    internal static string Serialize(GatewayConfiguration value) => JsonSerializer.Serialize(value, GatewayJson.Default.GatewayConfiguration);
    private static bool RatesInvalid(this GatewayConfiguration value) => value.RateLimits is not { } limits ||
        limits.KeyPrefix is not { Length: > 0 and <= 128 } || limits.KeyPrefix.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not (':' or '-' or '_')) ||
        limits.WindowSeconds is < 1 or > 86400 || limits.IpPermitLimit is < 1 or > 1000000 ||
        limits.TenantPermitLimit is < 1 or > 1000000 || limits.UserPermitLimit is < 1 or > 1000000;
    private static bool Address(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        uri.Scheme is "https" or "http" && uri.UserInfo.Length == 0 && uri.Query.Length == 0 && uri.Fragment.Length == 0;
    private static bool Text(string value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 255 && value.All(character => !char.IsControl(character));
    private static ArgumentException Invalid() => new("网关配置不满足单一发行方、静态路由、限流策略、密钥路径或可信代理约束。");
    private static void RejectDuplicateNames(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in value.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw Invalid();
                RejectDuplicateNames(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (var item in value.EnumerateArray()) RejectDuplicateNames(item);
    }
}
