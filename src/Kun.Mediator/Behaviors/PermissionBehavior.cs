using Kun.Authorization;
using Kun.Core.Cqrs;
using Kun.Core.MultiTenancy;
using Kun.Core.Security;
using Kun.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kun.Mediator.Behaviors;

/// <summary>为静态声明权限的命令装配判定行为，缺失引擎时拒绝执行。</summary>
public static class PermissionBehaviorExtensions
{
    /// <summary>
    /// 拒绝响应由调用方的 Result 工厂提供，不以异常表达正常权限失败；未认证给 <see cref="PlatformErrors.Unauthenticated"/>，
    /// 其余拒绝（引擎或权限声明缺失、主体类型无效、判定不允许）一律给 <see cref="PlatformErrors.AuthzDenied"/>。
    /// </summary>
    public static KunMediatorBuilder AddPermissionBehavior<TRequest, TResponse>(this KunMediatorBuilder builder,
        Func<Error, TResponse> denied) where TRequest : IRequest<TResponse>, IPermissionProtectedRequest
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(denied);
        builder.AddPipelineBehavior<TRequest, TResponse, PermissionBehavior<TRequest, TResponse>>();
        builder.MarkPermissionGuarded(typeof(TRequest));
        // AddPipelineBehavior also registers constructor activation. Replace it with the
        // factory: optional ports and the per-command denied delegate are not DI services.
        builder.Services.RemoveAll<PermissionBehavior<TRequest, TResponse>>();
        builder.Services.AddTransient(sp => new PermissionBehavior<TRequest, TResponse>(
            sp.GetService<IPermissionChecker>(), sp.GetService<ICurrentUser>(), sp.GetService<ITenantContext>(), denied));
        return builder;
    }
}

internal sealed class PermissionBehavior<TRequest, TResponse>(IPermissionChecker? checker,
    ICurrentUser? user, ITenantContext? tenant, Func<Error, TResponse> denied) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IPermissionProtectedRequest
{
    public async ValueTask<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (user?.IsAuthenticated != true || string.IsNullOrWhiteSpace(user.UserId))
            return denied(new Error(PlatformErrors.Unauthenticated));
        if (checker is null || TRequest.RequiredPermissions.Count == 0)
            return denied(new Error(PlatformErrors.AuthzDenied));
        var kind = user.PrincipalKind;
        if (kind is not ("user" or "service" or "agent"))
            return denied(new Error(PlatformErrors.AuthzDenied));
        var subject = new AuthorizationSubject(user.UserId, tenant?.TenantId, kind);
        foreach (var permission in TRequest.RequiredPermissions)
        {
            if (string.IsNullOrWhiteSpace(permission) ||
                !(await checker.CheckAsync(new PermissionRequest(subject, permission), cancellationToken).ConfigureAwait(false)).Allow)
                return denied(new Error(PlatformErrors.AuthzDenied));
        }
        return await next().ConfigureAwait(false);
    }
}
