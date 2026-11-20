using Kun.Authorization;
using Kun.Authorization.Rbac.Abstractions;
using Kun.Runtime;

namespace Qian.Dev.TenantOps;

// 本机手测的受限运营（S1-TEN-027）：持平台超管，但不持 qian.tenancy:admin:reset-credential，用来看带初始密码开户与更换管理员被拒时页面的提示（R165）。
// 平台目前只有超管一个平台角色，持角色即持全部已登记的权限点，所以在真实旁路之前收窄：只对这个身份、只扣这一个权限点，按旁路拒绝处理（不写审计）；
// 其余判定原样交给真实旁路。只有本机手测宿主的运营入口认这个身份，产品宿主与外壳联调环境都没有它。
internal sealed class DevLimitedOperatorBypass(ISuperadminBypass inner) : ISuperadminBypass
{
    internal const string Withheld = "qian.tenancy:admin:reset-credential";

    public ValueTask<PermissionDecision> AuthorizeAsync(SuperadminAccess access, CancellationToken cancellationToken = default) =>
        access.Subject.Id == DevKeys.LimitedOperator && access.Permission == Withheld
            ? ValueTask.FromResult(new PermissionDecision(false, "dev_limited_operator", []))
            : inner.AuthorizeAsync(access, cancellationToken);
}

// 运营入口每个请求解析的 qian.tenancy 上下文：只把超管旁路换成 DevLimitedOperatorBypass，其余原样转发。
internal sealed class DevLimitedOperatorContext(IPluginContext inner) : IPluginContext
{
    public PluginEntry Entry => inner.Entry;
    public ScopeKey Scope => inner.Scope;
    public IServiceProvider Services => inner.Services;
    public ILogger Logger => inner.Logger;
    public TService Inject<TService>(ServiceKey key) where TService : class => Wrap(key, inner.Inject<TService>(key))!;
    public TService? TryInject<TService>(ServiceKey key) where TService : class => Wrap(key, inner.TryInject<TService>(key));
    private static TService? Wrap<TService>(ServiceKey key, TService? service) where TService : class =>
        key == AuthzDefinition.SuperadminBypassKey && service is ISuperadminBypass bypass ? (TService)(object)new DevLimitedOperatorBypass(bypass) : service;
    public IRegistrationHandle<TService> Provide<TService>(ServiceKey key, TService instance) where TService : class => inner.Provide(key, instance);
    public IRegistrationHandle<TService> Provide<TService>(ServiceKey key, string name, TService instance) where TService : class => inner.Provide(key, name, instance);
    public TService Inject<TService>(ServiceKey key, string name) where TService : class => inner.Inject<TService>(key, name);
    public ValueTask<TService> WhenReadyAsync<TService>(ServiceKey key, CancellationToken ct) where TService : class => inner.WhenReadyAsync<TService>(key, ct);
    public void Effect(IDisposable disposer) => inner.Effect(disposer);
    public void Effect(Action dispose) => inner.Effect(dispose);
    public void Effect(Func<ValueTask> disposeAsync) => inner.Effect(disposeAsync);
    public IDisposable On<TPayload, TResult>(KunEvent<TPayload, TResult> evt, DispatchMode mode, Delegate handler, int priority) => inner.On(evt, mode, handler, priority);
    public IDisposable On<TPayload, TResult>(KunEvent<TPayload, IReadOnlyList<TResult>> evt, DispatchMode mode, Delegate handler, int priority) => inner.On(evt, mode, handler, priority);
    public ValueTask<TResult> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, TResult> evt, TPayload payload, CancellationToken ct) => inner.DispatchAsync(evt, payload, ct);
    public ValueTask<IReadOnlyList<TResult>> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, IReadOnlyList<TResult>> evt, TPayload payload, CancellationToken ct) => inner.DispatchAsync(evt, payload, ct);
    public ValueTask<TResult> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, TResult> evt, TPayload payload, Func<ValueTask<TResult>> terminal, CancellationToken ct) => inner.DispatchAsync(evt, payload, terminal, ct);
    public IPluginContext Scoped(ScopeKey scope) => inner.Scoped(scope);
    public IDisposable Restrict(ScopeKey scope, Func<Registration, bool> keep) => inner.Restrict(scope, keep);
    public IReadOnlyList<Registration> List(ServiceKey key) => inner.List(key);
}
