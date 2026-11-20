using System.Text.Json.Serialization;
using Kun.Runtime;

namespace Kun.Gateway;

/// <summary>网关 seam；Provider 安装到 gateway profile，业务组件只消费本契约。</summary>
public static class GatewayDefinition
{
    /// <summary>网关服务键。</summary>
    public static readonly ServiceKey Key = new("Gateway");
    /// <summary>取得当前已挂载网关。</summary>
    public static IGatewayService Gateway(this IPluginContext context) => context.Inject<IGatewayService>(Key);
}

/// <summary>网关只读状态；路由配置在构建宿主时固定，未挂载时禁止代理请求。</summary>
public interface IGatewayService
{
    /// <summary>对应组件当前是否挂载。</summary>
    bool IsReady { get; }
    /// <summary>取得静态下游的唯一受众；未登记的 cluster 拒绝。</summary>
    string AudienceFor(string clusterId);
}

/// <summary>受保护路由；不开放匿名策略覆盖。</summary>
public sealed record GatewayRoute(string Id, string Path, string Cluster)
{
    /// <summary>必填：kun-default（可信客户端 IP）、tenant 或 user；缺失时拒绝构建网关配置。</summary>
    public required string RateLimiterPolicy { get; init; }
}

/// <summary>代理路由的 Redis 固定窗口限额；同一逻辑网关的副本必须使用相同配置。</summary>
public sealed record GatewayRateLimits
{
    /// <summary>Redis 键前缀；环境或独立网关可显式隔离，键还包含规范 issuer、路由与主体的摘要。</summary>
    public string KeyPrefix { get; init; } = "qian:gateway:rate";
    /// <summary>窗口秒数，1–86400，默认 60。</summary>
    public int WindowSeconds { get; init; } = 60;
    /// <summary>每个 IP 的窗口请求数，1–1000000，默认 100；缺少规范身份时也使用此限额。</summary>
    public int IpPermitLimit { get; init; } = 100;
    /// <summary>每个租户的窗口请求数，1–1000000，默认 100。</summary>
    public int TenantPermitLimit { get; init; } = 100;
    /// <summary>每个租户内用户或平台主体的窗口请求数，1–1000000，默认 100。</summary>
    public int UserPermitLimit { get; init; } = 100;
}

/// <summary>静态下游；Id 同时是 S2S 的唯一受众。</summary>
public sealed record GatewayCluster(string Id, string Address);

/// <summary>网关声明式配置；只允许一个身份发行方，密钥值从不进入配置，PEM 路径指向宿主的 Secret 投射。</summary>
[method: JsonConstructor]
public sealed record GatewayConfiguration(string IdentityIssuer, string IdentityAudience, string Issuer,
    string SigningKeyPath, string PublicKeyPath, IReadOnlyList<string> TrustedProxyNetworks,
    IReadOnlyList<GatewayRoute> Routes, IReadOnlyList<GatewayCluster> Clusters)
{
    /// <summary>Creates configuration for a host supplying its own YARP configuration provider.</summary>
    public GatewayConfiguration() : this("", "", "kun-gateway", "", "", [], [], []) { }

    /// <summary>
    /// The JWKS endpoint URI of the external IdP, used to fetch public keys for validating incoming JWTs.
    /// </summary>
    [JsonIgnore] public string ExternalJwksUri { get; init; } = string.Empty;

    /// <summary>
    /// The audience claim placed in the minted internal S2S JWT. Defaults to <c>kun-internal</c>.
    /// </summary>
    [JsonIgnore] public string InternalAudience { get; init; } = "kun-internal";

    /// <summary>
    /// Lifetime of the minted internal S2S JWT. Defaults to 60 seconds per zero-trust TTL policy.
    /// </summary>
    [JsonIgnore] public TimeSpan InternalTokenLifetime { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Compatibility allowlist option. Only canonical scope/session/revision/project/plugin field names are accepted;
    /// the canonical field set is fixed and does not require an allowlist. Arbitrary application claims are rejected.
    /// </summary>
    [JsonIgnore] public string[] ForwardedClaimTypes { get; init; } = [];

    /// <summary>
    /// Retained for configuration compatibility; no longer changes the closed JWT payload.
    /// Trace context is propagated through standard HTTP headers.
    /// </summary>
    [JsonIgnore] public bool EmitTraceIdClaim { get; init; }

    /// <summary>
    /// When <see langword="true"/>, the gateway exposes the internal JWKS endpoint
    /// <c>/internal/.well-known/jwks.json</c> in non-Development environments. Defaults to
    /// <see langword="false"/> so the internal endpoint returns 404 in production unless the
    /// operator deliberately opts in. The public endpoint <c>/.well-known/jwks.json</c>
    /// remains always-on; this option only gates the internal-network variant.
    /// </summary>
    /// <remarks>
    /// Keep <see langword="false"/> in internet-facing Kubernetes deployments and rely on
    /// NetworkPolicy / service-mesh egress rules to isolate the internal route. Flip to
    /// <see langword="true"/> only when JWKS fetching from internal pods must bypass the
    /// public path (e.g. rate-limiting sidecars that reshape external traffic).
    /// </remarks>
    [JsonIgnore] public bool ExposeInternalJwks { get; init; }

    /// <summary>允许转发客户端地址与协议的代理 IP；缺省不信任任何转发头。</summary>
    [JsonIgnore] public string[] TrustedProxyAddresses { get; init; } = [];

    /// <summary>最多接受的可信代理跳数，默认一跳。</summary>
    [JsonIgnore] public int ForwardLimit { get; init; } = 1;

    /// <summary>轮换后旧公钥的重叠发布时长，默认五分钟；消费者仍须执行令牌寿命验证。</summary>
    [JsonIgnore] public TimeSpan KeyOverlapPeriod { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>检查私钥文件变更的最短间隔，默认一秒；零表示每次签发或获取 JWKS 都检查。</summary>
    [JsonIgnore] public TimeSpan KeyReloadInterval { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>每条代理路由独立计数，副本间共享 Redis；省略时每种策略均为 100 次/分钟。</summary>
    public GatewayRateLimits RateLimits { get; init; } = new();
}
