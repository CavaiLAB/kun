using Kun.Authorization.Rbac.Abstractions;
using Kun.Errors;
using Kun.Plugin.Sdk.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kun.Authorization.AspNetCore;

internal sealed partial class LibraryEnforcement(IServiceProvider services, ILogger<LibraryEnforcement> logger) : IAuthzEnforcement
{
    public async ValueTask<AuthorizationDecision> EnforceAsync(EnforcementRequest request, CancellationToken cancellationToken = default)
    {
        PermissionDecision decision;
        if (request.Delegated || request.Subject.Kind == "agent") decision = new(false, "delegation_not_supported", []);
        else if (request.Permission is null || !PermissionCode.IsValid(request.Permission)) decision = new(false, "permission_not_declared", []);
        else if (services.GetService<IPermissionChecker>() is not { } checker) { MissingChecker(logger); decision = new(false, "permission_checker_unavailable", []); }
        else decision = await checker.CheckAsync(new PermissionRequest(request.Subject, request.Permission, request.Resource), cancellationToken).ConfigureAwait(false);
        return new AuthorizationDecision(Guid.CreateVersion7(), request, new AuthorizationExplanation(decision, [], [], []), false, false, DateTimeOffset.UtcNow);
    }

    public ValueTask<AuthorizationDecision?> CheckIdentityRevisionAsync(EnforcementRequest request, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult<AuthorizationDecision?>(null);

    internal static Error ComponentEndpoint(ILogger logger)
    {
        ComponentEndpointRejected(logger);
        return new Error(PlatformErrors.AuthzDenied);
    }

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "权限判定器未登记，本次授权拒绝。")]
    private static partial void MissingChecker(ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "组件端点要求平台授权模式。")]
    private static partial void ComponentEndpointRejected(ILogger logger);
}

internal sealed partial class LibraryAuthorizationStartup(ILogger<LibraryAuthorizationStartup> logger) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        if (app.ApplicationServices.GetService<PluginEndpointOptions>() is not null)
            throw new InvalidOperationException("组件宿主须使用平台模式登记授权。");
        LibraryMode(logger);
        next(app);
    };

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "授权已使用库模式，只检查 IPermissionChecker，不写决策审计、不比对身份修订。")]
    private static partial void LibraryMode(ILogger logger);
}
