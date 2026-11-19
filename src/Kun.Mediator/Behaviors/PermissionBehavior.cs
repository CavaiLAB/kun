using Kun.Authorization;
using Kun.Core.Cqrs;
using Kun.Core.MultiTenancy;
using Kun.Core.Security;
using Kun.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Kun.Mediator.Behaviors;

/// <summary>为静态声明权限的命令装配判定行为，缺失引擎时拒绝执行。</summary>
public static class PermissionBehaviorExtensions
{
    /// <summary>
    /// 拒绝响应由调用方的 Result 工厂提供，不以异常表达正常权限失败；未认证给 <see cref="PlatformErrors.Unauthenticated"/>，
    /// 其余拒绝（引擎或权限声明缺失、主体类型无效、判定不允许）一律给 <see cref="PlatformErrors.AuthzDenied"/>。
    /// 权限判定器与当前主体从请求作用域的服务里取。
    /// </summary>
    public static KunMediatorBuilder AddPermissionBehavior<TRequest, TResponse>(this KunMediatorBuilder builder,
        Func<Error, TResponse> denied) where TRequest : IRequest<TResponse>, IPermissionProtectedRequest
        => builder.AddPermissionBehavior<TRequest, TResponse>(denied, static _ => { });

    /// <summary>
    /// 同 <see cref="AddPermissionBehavior{TRequest, TResponse}(KunMediatorBuilder, Func{Error, TResponse})"/>，
    /// 另由 <paramref name="configure"/> 改写这一个请求取权限判定器与当前主体的方式（<see cref="PermissionBehaviorOptions"/>），
    /// 比如在特定流程里包一层判定器。判定算法与拒绝映射不变。
    /// </summary>
    public static KunMediatorBuilder AddPermissionBehavior<TRequest, TResponse>(this KunMediatorBuilder builder,
        Func<Error, TResponse> denied, Action<PermissionBehaviorOptions> configure) where TRequest : IRequest<TResponse>, IPermissionProtectedRequest
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(denied);
        ArgumentNullException.ThrowIfNull(configure);
        var options = new PermissionBehaviorOptions();
        configure(options);
        var checker = options.PermissionChecker ?? throw new InvalidOperationException("PermissionBehaviorOptions.PermissionChecker 不能为 null。");
        var currentUser = options.CurrentUser ?? throw new InvalidOperationException("PermissionBehaviorOptions.CurrentUser 不能为 null。");
        builder.AddPipelineBehavior<TRequest, TResponse, PermissionBehavior<TRequest, TResponse>>();
        builder.MarkPermissionGuarded(typeof(TRequest));
        // AddPipelineBehavior also registers constructor activation. Replace it with the
        // factory: optional ports and the per-command denied delegate are not DI services.
        builder.Services.RemoveAll<PermissionBehavior<TRequest, TResponse>>();
        builder.Services.AddTransient(sp => new PermissionBehavior<TRequest, TResponse>(
            checker(sp), currentUser(sp), sp.GetService<ITenantContext>(), denied, sp.GetService<ILogger<PermissionBehavior<TRequest, TResponse>>>()));
        return builder;
    }
}

/// <summary>权限行为取权限判定器与当前主体的方式；缺省都从请求作用域的服务里取，取不到即拒绝。</summary>
public sealed class PermissionBehaviorOptions
{
    /// <summary>取权限判定器；返回 null 时一律拒绝（<see cref="PlatformErrors.AuthzDenied"/>）。</summary>
    public Func<IServiceProvider, IPermissionChecker?> PermissionChecker { get; set; } = static sp => sp.GetService<IPermissionChecker>();

    /// <summary>取当前主体；返回 null 或未认证时拒绝（<see cref="PlatformErrors.Unauthenticated"/>）。</summary>
    public Func<IServiceProvider, ICurrentUser?> CurrentUser { get; set; } = static sp => sp.GetService<ICurrentUser>();
}

internal sealed partial class PermissionBehavior<TRequest, TResponse>(IPermissionChecker? checker,
    ICurrentUser? user, ITenantContext? tenant, Func<Error, TResponse> denied, ILogger? logger = null) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IPermissionProtectedRequest
{
    public async ValueTask<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (user?.IsAuthenticated != true || string.IsNullOrWhiteSpace(user.UserId))
            return denied(new Error(PlatformErrors.Unauthenticated));
        if (checker is null || TRequest.RequiredPermissions.Count == 0)
        {
            // 装配不全与正常的无权给同一个拒绝；记一条警告，排查时分得出来。
            if (logger is not null)
                LogIncompleteSetup(logger, typeof(TRequest).FullName ?? typeof(TRequest).Name, checker is null ? "no permission checker" : "no required permissions");
            return denied(new Error(PlatformErrors.AuthzDenied));
        }
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

    [LoggerMessage(Level = LogLevel.Warning, Message = "Kun.Mediator denied {RequestName}: permission behavior is incomplete ({Missing})")]
    private static partial void LogIncompleteSetup(ILogger logger, string requestName, string missing);
}
