using Kun.Authorization;
using Kun.Core.MultiTenancy;
using Kun.Runtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Kun.Plugin.Sdk.AspNetCore;

/// <summary>HTTP 组合根的显式装配，不在 SDK 内创建另一棵运行时或替身注册表。</summary>
public static class PluginEndpointExtensions
{
    /// <summary>
    /// 调用宿主的权限回调，并在构造端点时核对它给端点接上了执行点：端点带该权限点的 <see cref="RequiresPermissionAttribute"/>，
    /// 授权策略里有执行同一权限点的 <see cref="IPermissionEnforcementRequirement"/>（例如 Kun.Authorization.AspNetCore 的 <c>RequireKunPermission</c>），
    /// 且端点没有匿名放行标记。按名字引用的策略在构造端点时看不到其中的要求，不算接上。任一项不满足即抛出 <see cref="InvalidOperationException"/>。
    /// </summary>
    public static RouteHandlerBuilder RequirePluginPermission(this RouteHandlerBuilder route, string permission,
        Action<RouteHandlerBuilder, string> requirePermission)
    {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(requirePermission);
        requirePermission(route, permission);
        // 放在 Finally：回调之后才追加的约定（包括分组上的匿名放行）也在核对范围内。
        route.Finally(endpoint =>
        {
            if (MissingEnforcement(endpoint.Metadata, permission) is { } reason)
                throw new InvalidOperationException("端点 " + endpoint.DisplayName + " 的权限点 " + permission + " 没有接到 PEP：" + reason);
        });
        return route;
    }

    private static string? MissingEnforcement(IList<object> metadata, string permission)
    {
        if (!metadata.OfType<RequiresPermissionAttribute>().Any(item => item.Permission == permission))
            return "缺少该权限点的 RequiresPermissionAttribute。";
        if (metadata.OfType<IAllowAnonymous>().Any())
            return "端点带匿名放行标记。";
        var requirements = metadata.OfType<AuthorizationPolicy>().SelectMany(policy => policy.Requirements)
            .Concat(metadata.OfType<IAuthorizationRequirementData>().SelectMany(data => data.GetRequirements()));
        return requirements.OfType<IPermissionEnforcementRequirement>().Any(item => item.Permission == permission)
            ? null
            : "授权策略里没有执行该权限点的要求（IPermissionEnforcementRequirement）。";
    }

    /// <summary>登记当前平台宿主的 Registry 上下文工厂。</summary>
    public static IServiceCollection AddKunPluginEndpoints(this IServiceCollection services, Func<IServiceProvider, IPluginContext> registryContext)
    {
        ArgumentNullException.ThrowIfNull(services); ArgumentNullException.ThrowIfNull(registryContext);
        if (services.Any(service => service.ServiceType == typeof(PluginEndpointOptions))) throw new InvalidOperationException("插件端点的注册表来源不能重复登记。");
        return services.AddSingleton(new PluginEndpointOptions(registryContext));
    }
    /// <summary>放在 UseRouting、认证和可信租户边界之后，endpoint 执行之前。</summary>
    public static IApplicationBuilder UseKunPluginEndpoints(this IApplicationBuilder app)
    { ArgumentNullException.ThrowIfNull(app); return app.UseMiddleware<PluginEnabledMiddleware>(); }
    /// <summary>标注 Minimal API 的实际绑定，并附加拒绝缺失/错误排序中间件的 endpoint filter。</summary>
    public static RouteHandlerBuilder RequireKunPlugin(this RouteHandlerBuilder builder, PluginEndpointBinding binding)
    {
        ArgumentNullException.ThrowIfNull(builder); ArgumentNullException.ThrowIfNull(binding);
        builder.WithMetadata(binding);
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var tenant = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
            if (!PluginEnabledMiddleware.HasEvidence(context.HttpContext, binding, tenant)) throw PluginEnabledMiddleware.NotFound();
            return await next(context).ConfigureAwait(false);
        });
    }
}
