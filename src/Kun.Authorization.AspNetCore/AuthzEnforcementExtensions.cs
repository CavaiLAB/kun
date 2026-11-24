using Grpc.AspNetCore.Server;
using Kun.Authorization.Rbac.Abstractions;
using Kun.Errors;
using Kun.Runtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Kun.Authorization.AspNetCore;

/// <summary>
/// HTTP / gRPC 强制执行点（PEP）的宿主装配：端点以 <see cref="RequireKunPermission{TBuilder}"/> 声明权限点（同时写进 OpenAPI 的 x-qian-permission），
/// 平台模式经 qian.authz 的执行端口（<see cref="IAuthzEnforcement"/>）判定、比对身份修订并记录决策审计。库模式使用宿主登记的 <see cref="IPermissionChecker"/>。
/// 平台组件端点未声明权限点时，生产模式返回 403 并记录，开发模式放行并告警。
/// <see cref="UseKunAuthorization"/> 对携带修订的租户身份核对下限，陈旧的租户令牌返回 401。
/// </summary>
public static class AuthzEnforcementExtensions
{
    /// <summary>对普通 ASP.NET Core 宿主使用 IPermissionChecker 检查权限。组件宿主须提供组件上下文。</summary>
    public static IServiceCollection AddKunAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Register(services, new AuthzEnforcementSource(null, new AuthzEnforcementOptions()));
        if (!services.Any(item => !item.IsKeyedService && item.ServiceType == typeof(ErrorCatalog)))
            services.AddSingleton(ErrorCatalog.CreateDefault());
        services.AddScoped<LibraryEnforcement>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<Microsoft.AspNetCore.Hosting.IStartupFilter, LibraryAuthorizationStartup>());
        return services;
    }

    /// <summary>
    /// 登记平台模式：<paramref name="source"/> 返回当前请求可注入 <c>AuthzEnforcement</c> 的插件上下文（通常是 authz 组件按请求租户收窄的上下文）。
    /// 宿主须先装配身份（<c>ICurrentUser</c>）、可信租户边界与 ErrorDetail 中间件；gRPC 宿主把 <see cref="AuthzGrpcServerInterceptor"/> 加进拦截器。
    /// </summary>
    public static IServiceCollection AddKunAuthorization(this IServiceCollection services, Func<HttpContext, IPluginContext> source, Action<AuthzEnforcementOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services); ArgumentNullException.ThrowIfNull(source);
        var options = new AuthzEnforcementOptions();
        configure?.Invoke(options);
        return Register(services, new AuthzEnforcementSource(source, options));
    }

    private static IServiceCollection Register(IServiceCollection services, AuthzEnforcementSource source)
    {
        if (services.Any(service => service.ServiceType == typeof(AuthzEnforcementSource)))
            throw new InvalidOperationException("授权只能登记一次。组件宿主须提供平台上下文。");
        services.AddSingleton(source);
        services.AddAuthorization();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAuthorizationHandler, KunPermissionHandler>());
        // 拒绝写成 ErrorDetail（403 AUTHZ_DENIED / 401 UNAUTHENTICATED / 404 NOT_FOUND），其余结果交给默认处理器。
        services.TryAddSingleton<AuthorizationMiddlewareResultHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, KunAuthorizationResultHandler>();
        services.TryAddSingleton<AuthzGrpcServerInterceptor>();
        return services;
    }

    /// <summary>给端点声明所需权限点（R9 全名）并接入 PEP；多次调用表示全部权限都须放行。契约同时得到 401 / 403 的 ErrorDetail 响应声明。</summary>
    public static TBuilder RequireKunPermission<TBuilder>(this TBuilder builder, string permission) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (!PermissionCode.IsValid(permission)) throw new ArgumentException("权限点必须是 <plugin-id>:<resource>:<action> 全名。", nameof(permission));
        builder.Add(endpoint =>
        {
            endpoint.Metadata.Add(new RequiresPermissionAttribute(permission));
            if (!endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().Any(item => item.StatusCode == StatusCodes.Status403Forbidden))
            {
                endpoint.Metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status401Unauthorized, typeof(ErrorDetail), [ErrorDetailJson.MediaType]));
                endpoint.Metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status403Forbidden, typeof(ErrorDetail), [ErrorDetailJson.MediaType]));
            }
        });
        builder.RequireAuthorization(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().AddRequirements(new KunPermissionRequirement(permission)).Build());
        return builder;
    }

    /// <summary>
    /// 放在 UseAuthorization 之后，管两件事，匿名端点（<c>AllowAnonymous</c>）都不在范围内：
    /// 一是没有声明权限点的已认证请求先比对令牌的 identity_revision（带租户与修订的主体；处理器内判定、只要求已认证的入口都算），旧令牌 401 并记录拒绝决策，
    /// 与声明了权限点的端点用同一张下限表（S1-YUE-012）；二是带组件端点绑定（<c>PluginEndpointBinding</c>）却没有声明权限点、也没有标记处理器内判定的端点，
    /// 在生产模式缺省拒绝并记录决策审计，开发模式记一条警告后放行，便于本地调试。
    /// </summary>
    public static IApplicationBuilder UseKunAuthorization(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<UndeclaredPermissionMiddleware>();
    }

    /// <summary>把 gRPC 拦截器加进服务选项；放在认证与租户拦截器之后。</summary>
    public static GrpcServiceOptions AddKunAuthorizationInterceptor(this GrpcServiceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Interceptors.Add<AuthzGrpcServerInterceptor>();
        return options;
    }
}

/// <summary>PEP 的宿主选项。</summary>
public sealed class AuthzEnforcementOptions
{
    /// <summary>是否按生产模式对未声明权限点的组件端点缺省拒绝；null 时取宿主环境（Production 即拒绝）。</summary>
    public bool? Production { get; set; }
}

/// <summary>宿主登记的 PEP 上下文来源与选项；由 <see cref="AuthzEnforcementExtensions"/> 创建，适配器经 DI 取用。</summary>
public sealed class AuthzEnforcementSource
{
    internal AuthzEnforcementSource(Func<HttpContext, IPluginContext>? source, AuthzEnforcementOptions options) { Source = source; Options = options; }
    internal Func<HttpContext, IPluginContext>? Source { get; }
    internal AuthzEnforcementOptions Options { get; }
    internal bool IsLibrary => Source is null;

    internal IAuthzEnforcement Resolve(HttpContext http)
    {
        if (Source is null) return http.RequestServices.GetRequiredService<LibraryEnforcement>();
        try { return Source(http).Enforcement(); }
        catch (Exception error) when (error is ServiceNotReadyException or ObjectDisposedException or KeyNotFoundException)
        { throw new ErrorException(new Error(PlatformErrors.NotFound)); }
    }

    internal bool IsProduction(IServiceProvider services) => Options.Production ?? services.GetService<IHostEnvironment>()?.IsProduction() ?? true;
}

internal sealed record KunPermissionRequirement(string Permission) : IAuthorizationRequirement, IPermissionEnforcementRequirement;
