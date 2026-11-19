using Kun.Core.Security;
using Kun.Mediator.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Kun.Authorization.Rbac.Abstractions;

// 只装配到 SDK 请求的 Kun PermissionBehavior（经 PermissionBehaviorOptions）；不替换宿主 DI、普通 Check 或外部主体。
internal static class InitializationPermissionServices
{
    internal static void Configure(PermissionBehaviorOptions options)
    {
        options.CurrentUser = static sp => sp.GetService<ICurrentUser>() is { } user ? new User(user) : null;
        options.PermissionChecker = static sp => sp.GetService<IPermissionChecker>() is { } checker ? new Checker(checker) : null;
    }
    private sealed class User(ICurrentUser original) : ICurrentUser
    {
        public string? UserId => TenantInitializationAuthorization.Active ? TenantInitializationAuthorization.Actor : original.UserId;
        public bool IsAuthenticated => TenantInitializationAuthorization.Active || original.IsAuthenticated;
        public string PrincipalKind => TenantInitializationAuthorization.Active ? TenantInitializationAuthorization.ActorKind : original.PrincipalKind;
        public IReadOnlyList<string> Roles => original.Roles;
    }
    private sealed class Checker(IPermissionChecker original) : IPermissionChecker
    {
        public ValueTask<PermissionDecision> CheckAsync(PermissionRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return TenantInitializationAuthorization.AllowsPermission(request)
                ? ValueTask.FromResult(new PermissionDecision(true, "tenant_initialization", [])) : original.CheckAsync(request, cancellationToken);
        }
        public async ValueTask<IReadOnlyList<PermissionDecision>> BatchCheckAsync(IReadOnlyList<PermissionRequest> requests, CancellationToken cancellationToken = default)
        {
            var result = new List<PermissionDecision>(requests.Count);
            foreach (var request in requests) result.Add(await CheckAsync(request, cancellationToken).ConfigureAwait(false));
            return result.AsReadOnly();
        }
        public ValueTask<IReadOnlyList<string>> ListAllowedActionsAsync(AuthorizationSubject subject, string resourceType, string? resourceId = null, CancellationToken cancellationToken = default) => original.ListAllowedActionsAsync(subject, resourceType, resourceId, cancellationToken);
        public ValueTask<IReadOnlyList<string>> ListPermissionsAsync(AuthorizationSubject subject, CancellationToken cancellationToken = default) => original.ListPermissionsAsync(subject, cancellationToken);
        public ValueTask<PermissionDecision> ExplainAsync(PermissionRequest request, CancellationToken cancellationToken = default) => original.ExplainAsync(request, cancellationToken);
    }
}
