using Kun.Authorization.Rbac.Abstractions;
using Kun.Errors;
using Kun.Errors.AspNetCore;
using Kun.Identity.Abstractions;
using Kun.Plugin.Sdk.AspNetCore;
using Kun.Runtime;
using Kun.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kun.Authorization.AspNetCore;

// 两种模式均先读取认证方案建立的主身份；平台还核对 ICurrentUser 的完整快照。
internal static class HttpEnforcement
{
    private static readonly object FailureKey = new();

    internal static bool TryReadRequest(HttpContext http, AuthzEnforcementSource source, string? permission, string entryPoint, string target,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out EnforcementRequest? request)
    {
        request = null;
        if (!Kun.Security.Authentication.PrincipalClaims.TryRead(http.User, out var subject, out var tenant, out var kind)) return false;
        long? revision = null;
        bool delegated;
        if (source.IsLibrary)
        {
            var identity = (System.Security.Claims.ClaimsIdentity)http.User.Identity!;
            delegated = identity.HasClaim(claim => claim.Type == "act");
        }
        else
        {
            var user = http.RequestServices.GetRequiredService<ICurrentUser>();
            var principal = user.Principal;
            if (!user.IsAuthenticated || principal is null || user.UserId != principal.Subject ||
                principal.Subject != subject || principal.TenantId != tenant || Kind(principal.Kind) != kind) return false;
            revision = principal.IdentityRevision;
            delegated = principal.Actor is not null;
        }
        request = new EnforcementRequest(new AuthorizationSubject(subject, tenant, kind), revision, delegated, permission, null, "api", entryPoint, target);
        return true;
    }

    internal static string Target(HttpContext http) => http.Request.Method + " " + ((http.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? http.Request.Path.Value ?? "/");
    internal static string Kind(PrincipalKind kind) => kind switch { PrincipalKind.User => "user", PrincipalKind.Service => "service", _ => "agent" };
    internal static void Fail(HttpContext http, Error error) => http.Items[FailureKey] = error;
    internal static Error? Failure(HttpContext http) => http.Items.TryGetValue(FailureKey, out var value) ? value as Error : null;
}

/// <summary>ASP.NET Core 授权处理器：对 <c>RequireKunPermission</c> 声明的每个权限点执行一次判定。</summary>
internal sealed class KunPermissionHandler(AuthzEnforcementSource source) : AuthorizationHandler<KunPermissionRequirement, HttpContext>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, KunPermissionRequirement requirement, HttpContext http)
    {
        if (source.IsLibrary && http.GetEndpoint()?.Metadata.GetMetadata<PluginEndpointBinding>() is not null)
        {
            var error = LibraryEnforcement.ComponentEndpoint(http.RequestServices.GetRequiredService<ILogger<KunPermissionHandler>>());
            Deny(context, http, error, "component_endpoint_requires_platform");
            return;
        }
        if (!HttpEnforcement.TryReadRequest(http, source, requirement.Permission, AuthzEnforcement.HttpEntryPoint, HttpEnforcement.Target(http), out var request))
        {
            // 普通宿主沿用 ASP.NET Core 的 Forbid：主身份已认证，但声明未形成可用的权限主体。
            var error = source.IsLibrary && http.User.Identity?.IsAuthenticated == true ? PlatformErrors.AuthzDenied : PlatformErrors.Unauthenticated;
            Deny(context, http, new Error(error), error.Code);
            return;
        }
        try
        {
            var decision = await source.Resolve(http).EnforceAsync(request, http.RequestAborted).ConfigureAwait(false);
            if (decision.Allow) context.Succeed(requirement);
            else Deny(context, http, new Error(Denial(decision)), decision.Reason);
        }
        catch (ErrorException error) { Deny(context, http, error.Error, error.Error.Code.Code); }
    }
    // 审计组件已卸载时整个入口不可用，与不存在同为 404（S1-AUD-001）；旧令牌（identity_revision 低于下限）401，客户端据此换令牌（S1-DEC-027）；
    // 其余拒绝 403。gRPC 拦截器按同一映射给出 NotFound / Unauthenticated / PermissionDenied。
    internal static ErrorCode Denial(AuthorizationDecision decision) => decision.Reason switch
    {
        AuthzEnforcement.AuditUnavailable => PlatformErrors.NotFound,
        AuthzEnforcement.StaleIdentityRevision => PlatformErrors.Unauthenticated,
        _ => PlatformErrors.AuthzDenied
    };
    private static void Deny(AuthorizationHandlerContext context, HttpContext http, Error error, string reason)
    {
        HttpEnforcement.Fail(http, error);
        context.Fail(new AuthorizationFailureReason(null!, reason));
    }
}

/// <summary>授权失败写成 ErrorDetail；没有 PEP 记录的失败交给默认处理器（Challenge / Forbid）。</summary>
internal sealed class KunAuthorizationResultHandler(AuthorizationMiddlewareResultHandler fallback) : IAuthorizationMiddlewareResultHandler
{
    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (!authorizeResult.Succeeded && HttpEnforcement.Failure(context) is { } error && !context.Response.HasStarted)
        {
            context.Response.Headers.CacheControl = "private, no-store";
            return KunResults.Error(error).ExecuteAsync(context);
        }
        return fallback.HandleAsync(next, context, policy, authorizeResult);
    }
}

/// <summary>
/// 没有声明权限点的已认证端点：先比对令牌 identity_revision，旧令牌 401（处理器内判定的平台表面与只要求已认证的端点，S1-YUE-012）；
/// 其中带组件端点绑定的，生产模式缺省拒绝并记录，开发模式告警放行；显式标记 <see cref="PermissionEnforcedInHandlerAttribute"/> 的端点不在缺省拒绝范围内。
/// 声明了权限点的端点在 <see cref="KunPermissionHandler"/> 的判定里已比对修订，匿名端点不比对。
/// </summary>
internal sealed partial class UndeclaredPermissionMiddleware(RequestDelegate next, AuthzEnforcementSource source, ILogger<UndeclaredPermissionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null) { await next(context).ConfigureAwait(false); return; }
        if (source.IsLibrary && endpoint.Metadata.GetMetadata<PluginEndpointBinding>() is not null)
        {
            context.Response.Headers.CacheControl = "private, no-store";
            await KunResults.Error(LibraryEnforcement.ComponentEndpoint(logger)).ExecuteAsync(context).ConfigureAwait(false);
            return;
        }
        if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null || endpoint.Metadata.GetOrderedMetadata<Kun.Authorization.RequiresPermissionAttribute>().Count != 0)
        { await next(context).ConfigureAwait(false); return; }
        var target = HttpEnforcement.Target(context);
        var authenticated = HttpEnforcement.TryReadRequest(context, source, null, AuthzEnforcement.HttpEntryPoint, target, out var request);
        if (request is { Subject.TenantId: not null, IdentityRevision: not null } &&
            await source.Resolve(context).CheckIdentityRevisionAsync(request, context.RequestAborted).ConfigureAwait(false) is { } stale)
        {
            context.Response.Headers.CacheControl = "private, no-store";
            throw new ErrorException(new Error(KunPermissionHandler.Denial(stale)));
        }
        if (endpoint.Metadata.GetMetadata<PluginEndpointBinding>() is null || endpoint.Metadata.GetMetadata<PermissionEnforcedInHandlerAttribute>() is not null)
        { await next(context).ConfigureAwait(false); return; }
        if (!source.IsProduction(context.RequestServices)) { LogUndeclared(logger, target); await next(context).ConfigureAwait(false); return; }
        if (!authenticated) throw new ErrorException(new Error(PlatformErrors.Unauthenticated));
        var decision = await source.Resolve(context).EnforceAsync(request!, context.RequestAborted).ConfigureAwait(false);
        if (!decision.Allow) throw new ErrorException(new Error(KunPermissionHandler.Denial(decision)));
        await next(context).ConfigureAwait(false);
    }
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "组件端点 {Target} 没有声明权限点；开发模式放行，生产模式将缺省拒绝。")]
    private static partial void LogUndeclared(ILogger logger, string target);
}
