using System.Security.Claims;
using System.Text.Json;
using Kun.Errors;
using Kun.Errors.AspNetCore;
using Kun.Gateway.Endpoints;
using Kun.Gateway.Identity;
using Kun.Gateway.RateLimiting;
using Kun.Gateway.RateLimiting.Resolvers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Configuration;

namespace Kun.Gateway;

/// <summary>网关宿主装配；Identity.Client 的 Provider 由宿主独立组合，组件库不直接依赖其他 Provider。</summary>
public static class GatewayHosting
{
    internal const string ExternalScheme = "qian.gateway.external";
    private const string Scheme = "qian.gateway";

    /// <summary>从声明式 JSON 读取严格配置；拒绝重复字段、未知字段与无效静态路由。</summary>
    public static GatewayConfiguration ReadConfiguration(JsonElement configuration) => GatewayConfigurationReader.Read(configuration);

    /// <summary>固定注册认证、路由、可信代理与热轮换。宿主需先装 Identity.Client 的规范 Cookie 方案，并在启用前挂载 GatewayComponent。</summary>
    internal static GatewayConfiguration AddManaged(IServiceCollection services, GatewayConfiguration configuration, string sessionAuthenticationScheme)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionAuthenticationScheme);
        if (services.Any(service => service.ServiceType == typeof(GatewayRuntime))) throw new InvalidOperationException("同一宿主只允许一个网关。");
        var snapshot = GatewayConfigurationReader.Copy(configuration);
        services.AddSingleton(new GatewayRuntime(snapshot));
        services.AddHealthChecks().AddCheck<GatewayMountHealthCheck>("qian.gateway", tags: ["ready"]);
        services.AddKunErrors();
        services.AddSingleton<IS2sPrincipalResolver, GatewayPrincipalResolver>();
        services.AddSingleton<IRateLimitPartitionResolver, IpRateLimitPartitionResolver>();
        services.AddSingleton<IProxyConfigProvider>(new InMemoryConfigProvider(
            snapshot.Routes.Select(route => new RouteConfig
            {
                RouteId = route.Id,
                ClusterId = route.Cluster,
                Match = new RouteMatch { Path = route.Path },
                AuthorizationPolicy = "Default",
                RateLimiterPolicy = route.RateLimiterPolicy
            }).ToArray(),
            snapshot.Clusters.Select(cluster => new ClusterConfig
            {
                ClusterId = cluster.Id,
                Destinations = new Dictionary<string, DestinationConfig> { ["primary"] = new() { Address = cluster.Address } }
            }).ToArray()));
        GatewayRateLimiting.Add(services, snapshot);
        services.AddAuthentication(options => { options.DefaultScheme = Scheme; options.DefaultChallengeScheme = Scheme; })
            .AddPolicyScheme(Scheme, Scheme, options => options.ForwardDefaultSelector = http =>
                http.Request.Headers.ContainsKey("Authorization") ? ExternalScheme : sessionAuthenticationScheme)
            .AddJwtBearer(ExternalScheme, options =>
            {
                options.MetadataAddress = snapshot.IdentityIssuer.TrimEnd('/') + "/.well-known/openid-configuration";
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = snapshot.IdentityIssuer,
                    ValidateAudience = true,
                    ValidAudience = snapshot.IdentityAudience,
                    ValidateIssuerSigningKey = true,
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256, SecurityAlgorithms.EcdsaSha256],
                    ValidTypes = ["at+jwt"],
                    AuthenticationType = ExternalScheme,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                options.Events.OnTokenValidated = context =>
                {
                    if (context.SecurityToken is not JsonWebToken token) { context.Fail("身份令牌无效。"); return Task.CompletedTask; }
                    using var payload = JsonDocument.Parse(Base64UrlEncoder.DecodeBytes(token.EncodedPayload));
                    var root = payload.RootElement;
                    if (root.ValueKind != JsonValueKind.Object || root.EnumerateObject().Select(property => property.Name).Distinct(StringComparer.Ordinal).Count() != root.EnumerateObject().Count())
                        context.Fail("身份令牌字段重复。");
                    else if (!root.TryGetProperty("sub", out var subject) || subject.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(subject.GetString()))
                        context.Fail("身份令牌缺少主体。");
                    return Task.CompletedTask;
                };
            });
        services.AddOptions<JwtBearerOptions>(ExternalScheme).Configure<IHostEnvironment>((options, environment) =>
            options.RequireHttpsMetadata = !environment.IsDevelopment());
        return snapshot with
        {
            ExternalJwksUri = snapshot.IdentityIssuer.TrimEnd('/') + "/.well-known/openid-configuration",
            InternalAudience = snapshot.Clusters[0].Id,
            InternalTokenLifetime = TimeSpan.FromSeconds(60),
            KeyOverlapPeriod = TimeSpan.FromMinutes(10)
        };
    }

    /// <summary>在 UseForwardedHeaders、UseRouting、UseKunIdentityClient 之后调用；业务请求还须通过真实组件挂载门。</summary>
    public static IApplicationBuilder UseKunGateway(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        var runtime = app.ApplicationServices.GetRequiredService<GatewayRuntime>();
        app.UseKunErrors();
        app.Use(async (http, next) =>
        {
            if (http.GetEndpoint()?.Metadata.GetMetadata<GatewayEndpoint>() is not null && !runtime.IsReady)
            { http.Response.StatusCode = StatusCodes.Status404NotFound; return; }
            // AuthenticationMiddleware 验签失败不会自动清掉已存在的 Cookie 主体；显式 Authorization 必须独立成功，不能降级回 Cookie。
            if (http.Request.Headers.ContainsKey("Authorization")) http.User = new ClaimsPrincipal(new ClaimsIdentity());
            await next(http).ConfigureAwait(false);
        });
        app.UseAuthentication(); app.UseRateLimiter(); app.UseAuthorization();
        return app;
    }

    /// <summary>映射受保护静态代理、公开 JWKS 及基础健康端点；完整依赖健康聚合由 GW-003 接续。</summary>
    public static IEndpointRouteBuilder MapKunGateway(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        endpoints.MapKunJwks();
        endpoints.MapHealthChecks("/healthz", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
        endpoints.MapHealthChecks("/readyz", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = (http, report) => report.Status == HealthStatus.Healthy
                ? http.Response.WriteAsync("Healthy", http.RequestAborted)
                : KunResults.Error(new Error(PlatformErrors.Unavailable)).ExecuteAsync(http)
        }).AllowAnonymous();
        var proxy = endpoints.MapReverseProxy();
        proxy.WithMetadata(new GatewayEndpoint());
        proxy.RequireRateLimiting(endpoints.ServiceProvider.GetRequiredService<GatewayRateLimitPolicy>());
        return endpoints;
    }

    private sealed class GatewayEndpoint { }

}

internal sealed class GatewayMountHealthCheck(GatewayRuntime runtime) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) =>
        Task.FromResult(runtime.IsReady ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("网关组件未挂载。"));
}
