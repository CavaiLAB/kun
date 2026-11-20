using System.Reflection;
using System.Text.Json;
using Kun.Core.Cqrs;
using Kun.Core.MultiTenancy;
using Kun.Core.Security;
using Kun.Mediator;
using Kun.Mediator.Behaviors;
using Kun.Mediator.DependencyInjection;
using Kun.Runtime;
using Kun.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Kun.Authorization.Rbac.Abstractions.Tests;

internal sealed record ProtectedCommand : IRequest<int>, IPermissionProtectedRequest
{
    public static IReadOnlyList<string> RequiredPermissions => ["qian.sample.crm:customer:read", "qian.sample.crm:customer:export"];
}
internal sealed record EmptyCommand : IRequest<int>, IPermissionProtectedRequest
{
    public static IReadOnlyList<string> RequiredPermissions => [];
}
internal sealed record InvalidCommand : IRequest<int>, IPermissionProtectedRequest
{
    public static IReadOnlyList<string> RequiredPermissions => ["invalid"];
}
internal sealed class Handler(List<string> calls) : IRequestHandler<ProtectedCommand, int>
{
    public ValueTask<int> Handle(ProtectedCommand request, CancellationToken cancellationToken) { calls.Add("handler"); return ValueTask.FromResult(42); }
}
internal sealed class EmptyHandler : IRequestHandler<EmptyCommand, int>
{
    public ValueTask<int> Handle(EmptyCommand request, CancellationToken cancellationToken) => ValueTask.FromResult(0);
}
internal sealed class InvalidHandler : IRequestHandler<InvalidCommand, int>
{
    public ValueTask<int> Handle(InvalidCommand request, CancellationToken cancellationToken) => ValueTask.FromResult(0);
}

public sealed class AuthorizedHandlerTests
{
    [Fact]
    public async Task 开户分支经权限行为的扩展点接上_凭据外拒绝_凭据内以开户执行人放行()
    {
        // 服务里的当前主体未认证、判定器一律拒绝：能放行的只有开户分支经 PermissionBehaviorOptions 接上的两个包装。
        // 包装没接上（换回缺省取法）时，凭据内照样以未认证被拒。
        var calls = new List<string>();
        var codes = new List<string>();
        var tenant = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddSingleton(calls);
        services.AddSingleton<ICurrentUser>(new User(false));
        services.AddSingleton<ITenantContext>(new FixedTenant(tenant));
        services.AddSingleton<IPermissionChecker>(new DenyAll(calls));
        services.AddKunMediator(builder => typeof(AuthorizedHandlerRegistration).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(method => method.Name == nameof(AuthorizedHandlerRegistration.AddAuthorizedHandler) && method.GetParameters().Length == 3)
            .MakeGenericMethod(typeof(ProtectedCommand), typeof(int), typeof(Handler))
            .Invoke(null, [builder, (Func<Kun.Errors.Error, int>)(error => { codes.Add(error.Code.Code); return -1; }), true]));
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        var sender = provider.GetRequiredService<ISender>();
        var ct = TestContext.Current.CancellationToken;
        using var host = new ServiceCollection().BuildServiceProvider();
        await using var loader = new PluginLoader(host);
        await loader.MountAsync([new PluginDefinition(new PluginEntry("tenancy", "qian.tenancy", "1.0.0", ScopeKey.Global, 0,
            JsonSerializer.SerializeToElement(new { })), new Provider(null), [])], ct);
        var source = loader.GetContext("tenancy", ScopeKey.Global).Scoped(new ScopeKey("tenant", tenant));

        Assert.Equal(-1, await sender.Send(new ProtectedCommand(), ct));
        using (TenantInitializationAccess.Begin(source, "operator", new HashSet<string> { "qian.sample.crm:seed" }))
        {
            // 有开户凭据、还没进这条能力的调用：照旧拒绝。
            Assert.Equal(-1, await sender.Send(new ProtectedCommand(), ct));
            using (TenantInitializationAccess.Enter(source, "qian.sample.crm:seed", typeof(ProtectedCommand), ProtectedCommand.RequiredPermissions))
            {
                Assert.Equal(42, await sender.Send(new ProtectedCommand(), ct));
                // 开户分支给权限行为的取法：主体是开户的执行人，判定原因是 tenant_initialization，没有问服务里的判定器。
                var options = new PermissionBehaviorOptions();
                typeof(AuthzDefinition).Assembly.GetType("Kun.Authorization.Rbac.Abstractions.InitializationPermissionServices", true)!
                    .GetMethod("Configure", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [options]);
                var user = options.CurrentUser(provider)!;
                Assert.Equal(("operator", "user", true), (user.UserId, user.PrincipalKind, user.IsAuthenticated));
                foreach (var permission in ProtectedCommand.RequiredPermissions)
                {
                    var decision = await options.PermissionChecker(provider)!.CheckAsync(
                        new PermissionRequest(new AuthorizationSubject("operator", tenant, "user"), permission), ct);
                    Assert.Equal((true, "tenant_initialization"), (decision.Allow, decision.Reason));
                }
            }
            Assert.Equal(-1, await sender.Send(new ProtectedCommand(), ct));
        }
        Assert.Equal(-1, await sender.Send(new ProtectedCommand(), ct));
        Assert.Equal(["UNAUTHENTICATED", "UNAUTHENTICATED", "UNAUTHENTICATED", "UNAUTHENTICATED"], codes);
        Assert.Equal(["handler"], calls);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public async Task Real_Kun_dispatch_denies_before_handler_and_checks_every_permission(bool? allow, bool authenticated)
    {
        var calls = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(calls);
        services.AddSingleton<ICurrentUser>(new User(authenticated));
        services.AddSingleton<ITenantContext>(new Tenant());
        if (allow is not null) services.AddSingleton<IPermissionChecker>(new Checker(allow.Value, calls));
        services.AddKunMediator(builder => builder.AddAuthorizedHandler<ProtectedCommand, int, Handler>(_ => -1));
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        var result = await provider.GetRequiredService<ISender>().Send(new ProtectedCommand(), TestContext.Current.CancellationToken);
        Assert.Equal(allow == true && authenticated ? 42 : -1, result);
        if (allow == true && authenticated) Assert.Equal([.. ProtectedCommand.RequiredPermissions, "handler"], calls);
        else Assert.DoesNotContain("handler", calls);
    }

    [Fact]
    public void Reflection_registration_that_bypasses_QIAN0005_is_refused_before_any_handler_runs()
    {
        // QIAN0005 只在编译期拦截；反射注册由 Kun 2026.10.13 起的 Mediator 构建期兜底拒绝（S1-KUN-014，S1-DEC-019 R66）。
        var calls = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(calls);
        services.AddSingleton<ICurrentUser>(new User(true));
        services.AddSingleton<ITenantContext>(new Tenant());
        services.AddKunMediator(builder => typeof(KunMediatorBuilder).GetMethods()
            .Single(method => method.Name == nameof(KunMediatorBuilder.AddRequestHandler) && method.GetParameters().Length == 0)
            .MakeGenericMethod(typeof(ProtectedCommand), typeof(int), typeof(Handler))
            .Invoke(builder, null));
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        var error = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ISender>());

        Assert.Contains(typeof(ProtectedCommand).FullName!, error.Message, StringComparison.Ordinal);
        Assert.Empty(calls);
    }

    [Fact]
    public void Invalid_or_empty_permissions_fail_during_registration()
    {
        var builder = new KunMediatorBuilder(new ServiceCollection());
        Assert.Throws<ArgumentException>(() => builder.AddAuthorizedHandler<EmptyCommand, int, EmptyHandler>(_ => -1));
        Assert.Throws<ArgumentException>(() => builder.AddAuthorizedHandler<InvalidCommand, int, InvalidHandler>(_ => -1));
    }

    [Fact]
    public async Task Authz_service_key_uses_real_scope_injection_and_unmount()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        await using var loader = new PluginLoader(services);
        var checker = new Checker(true, []);
        await loader.MountAsync([new PluginDefinition(
            new PluginEntry("authz", "qian.authz", "0.1.0", ScopeKey.Global, 0, JsonSerializer.SerializeToElement(new { })),
            new Provider(checker), [AuthzDefinition.Key])], TestContext.Current.CancellationToken);
        var context = loader.GetContext("authz", ScopeKey.Global);
        Assert.Same(checker, context.Authz());
        await loader.UnmountAsync("authz", ScopeKey.Global, TestContext.Current.CancellationToken);
        Assert.Throws<ObjectDisposedException>(() => context.Authz());
        await loader.MountAsync([new PluginDefinition(
            new PluginEntry("empty", "qian.empty", "0.1.0", ScopeKey.Global, 0, JsonSerializer.SerializeToElement(new { })),
            new Provider(null), [])], TestContext.Current.CancellationToken);
        Assert.Throws<ServiceNotReadyException>(() => loader.GetContext("empty", ScopeKey.Global).Authz());
    }

    private sealed class Provider(IAuthzService? service) : IKunPlugin
    {
        public IReadOnlyList<ServiceKey> Inject => [];
        public ValueTask ApplyAsync(IPluginContext ctx, JsonElement config, CancellationToken ct)
        { if (service is not null) ctx.Provide(AuthzDefinition.Key, service); return ValueTask.CompletedTask; }
    }
    private sealed class User(bool authenticated) : ICurrentUser
    {
        public string? UserId => authenticated ? "user-1" : null;
        public IReadOnlyList<string> Roles => [];
        public bool IsAuthenticated => authenticated;
    }
    private sealed class Tenant : ITenantContext
    {
        internal static readonly Guid Id = new("01900000-0000-7000-8000-000000000001");
        public Guid? TenantId => Id;
        public bool IsMultiTenant => true;
    }
    private sealed class FixedTenant(string tenant) : ITenantContext
    {
        public Guid? TenantId { get; } = Guid.ParseExact(tenant, "D");
        public bool IsMultiTenant => true;
    }
    private sealed class DenyAll(List<string> calls) : IPermissionChecker
    {
        public ValueTask<PermissionDecision> CheckAsync(PermissionRequest request, CancellationToken cancellationToken = default)
        { calls.Add(request.Permission); return ValueTask.FromResult(new PermissionDecision(false, "fixture", [])); }
        public ValueTask<IReadOnlyList<PermissionDecision>> BatchCheckAsync(IReadOnlyList<PermissionRequest> requests, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<IReadOnlyList<string>> ListAllowedActionsAsync(AuthorizationSubject subject, string resourceType, string? resourceId = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<IReadOnlyList<string>> ListPermissionsAsync(AuthorizationSubject subject, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<PermissionDecision> ExplainAsync(PermissionRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
    private sealed class Checker(bool allow, List<string> calls) : IAuthzService
    {
        public IDataPermissionFilter DataPermissions => throw new NotSupportedException();
        public IFieldPolicy Fields => throw new NotSupportedException();
        public ValueTask<IReadOnlyList<EffectiveGrant>> ListEffectivePermissionsAsync(AuthorizationSubject subject, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<PermissionDecision> CheckAsync(PermissionRequest request, CancellationToken cancellationToken = default)
        {
            Assert.Equal(Tenant.Id.ToString("D"), request.Subject.TenantId);
            Assert.Equal("user-1", request.Subject.Id);
            calls.Add(request.Permission);
            return ValueTask.FromResult(new PermissionDecision(allow, "fixture", []));
        }
        public ValueTask<IReadOnlyList<PermissionDecision>> BatchCheckAsync(IReadOnlyList<PermissionRequest> requests, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<IReadOnlyList<string>> ListAllowedActionsAsync(AuthorizationSubject subject, string resourceType, string? resourceId = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<IReadOnlyList<string>> ListPermissionsAsync(AuthorizationSubject subject, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<PermissionDecision> ExplainAsync(PermissionRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public ValueTask<AuthorizationExplanation> ExplainDetailedAsync(PermissionRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
