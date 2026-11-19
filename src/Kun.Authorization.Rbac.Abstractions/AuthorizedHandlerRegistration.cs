using System.Diagnostics.CodeAnalysis;
using Kun.Core.Cqrs;
using Kun.Errors;
using Kun.Mediator;
using Kun.Mediator.Behaviors;

namespace Kun.Authorization.Rbac.Abstractions;

/// <summary>把 Kun 逐命令权限行为与处理器登记到同一 builder；不另实现权限引擎。</summary>
public static class AuthorizedHandlerRegistration
{
    /// <summary>注册受保护命令；权限声明必须非空且符合 R9。拒绝结果由调用方映射。</summary>
    public static KunMediatorBuilder AddAuthorizedHandler<TRequest, TResponse,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(
        this KunMediatorBuilder builder, Func<Error, TResponse> denied)
        where TRequest : IRequest<TResponse>, IPermissionProtectedRequest
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        Validate<TRequest>(builder, denied);
        builder.AddPermissionBehavior<TRequest, TResponse>(denied);
        return builder.AddRequestHandler<TRequest, TResponse, THandler>();
    }

    // 仅 SDK 使用的受限开户分支：仍是 Kun 的 PermissionBehavior，只经它的扩展点把判定器与当前主体换成开户凭据感知的包装，不复制 RBAC 算法。
    internal static KunMediatorBuilder AddAuthorizedHandler<TRequest, TResponse,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>(
        this KunMediatorBuilder builder, Func<Error, TResponse> denied, bool tenantInitialization)
        where TRequest : IRequest<TResponse>, IPermissionProtectedRequest
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        if (!tenantInitialization) return builder.AddAuthorizedHandler<TRequest, TResponse, THandler>(denied);
        Validate<TRequest>(builder, denied);
        builder.AddRequestHandler<TRequest, TResponse, THandler>();
        return builder.AddPermissionBehavior<TRequest, TResponse>(denied, InitializationPermissionServices.Configure);
    }

    private static void Validate<TRequest>(KunMediatorBuilder builder, Delegate denied)
        where TRequest : IPermissionProtectedRequest
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(denied);
        var permissions = TRequest.RequiredPermissions;
        if (permissions is null || permissions.Count == 0 || permissions.Any(p => !PermissionCode.IsValid(p)))
            throw new ArgumentException("受保护命令必须声明非空且符合 R9 的 RequiredPermissions。", nameof(TRequest));
    }
}
