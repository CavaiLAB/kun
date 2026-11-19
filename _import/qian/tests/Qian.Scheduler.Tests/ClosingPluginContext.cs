using System.Collections.Concurrent;
using System.Text.Json;
using Kun.Core.MultiTenancy;
using Kun.Data;
using Kun.Plugin.Sdk;
using Kun.Runtime;
using Microsoft.Extensions.Logging;

namespace Qian.Scheduler.Tests;

// 停机窗口的测试替身：包一层真实插件上下文，按开关模拟 Kun.Runtime 卸载时「先关闭上下文、后跑效果」的那段时间，不靠时序。
// 关闭后和 PluginContext 一样，受关闭检查保护的成员抛 ObjectDisposedException；Entry、Scope、Services 照常。
// Scoped 出来的子上下文共用同一个开关；Logger 换成收集器，用例据此断言日志级别。
internal sealed class ClosingPluginContext(IPluginContext inner, ClosingPluginContext.Switch state) : IPluginContext
{
    internal ClosingPluginContext(IPluginContext inner, ITenantContext tenant) : this(inner, new Switch(tenant)) { }
    internal Switch State => state;
    public PluginEntry Entry => inner.Entry;
    public ScopeKey Scope => inner.Scope;
    public IServiceProvider Services => inner.Services;
    public ILogger Logger => state.Log;

    public TService Inject<TService>(ServiceKey key) where TService : class
    {
        if (key == DataDefinition.Key) state.DataRequested();
        return TryInject<TService>(key) ?? throw new ServiceNotReadyException();
    }
    public TService? TryInject<TService>(ServiceKey key) where TService : class
    {
        Open();
        return key == DataDefinition.Key && state.DataMissing ? null : inner.TryInject<TService>(key);
    }
    public TService Inject<TService>(ServiceKey key, string name) where TService : class
    {
        Open();
        var service = inner.Inject<TService>(key, name);
        return service is IExecutableCapability capability && state.Handler is { } handler ? (TService)(object)new HookedCapability(capability, handler) : service;
    }
    public IPluginContext Scoped(ScopeKey scope)
    {
        if (state.CloseOnFirstScoped) state.Close();
        Open();
        return new ClosingPluginContext(inner.Scoped(scope), state);
    }
    public ValueTask<TService> WhenReadyAsync<TService>(ServiceKey key, CancellationToken ct) where TService : class { Open(); return inner.WhenReadyAsync<TService>(key, ct); }
    public IRegistrationHandle<TService> Provide<TService>(ServiceKey key, TService instance) where TService : class { Open(); return inner.Provide(key, instance); }
    public IRegistrationHandle<TService> Provide<TService>(ServiceKey key, string name, TService instance) where TService : class { Open(); return inner.Provide(key, name, instance); }
    public void Effect(IDisposable disposer) { Open(); inner.Effect(disposer); }
    public void Effect(Action dispose) { Open(); inner.Effect(dispose); }
    public void Effect(Func<ValueTask> disposeAsync) { Open(); inner.Effect(disposeAsync); }
    public IDisposable On<TPayload, TResult>(KunEvent<TPayload, TResult> evt, DispatchMode mode, Delegate handler, int priority) { Open(); return inner.On(evt, mode, handler, priority); }
    public IDisposable On<TPayload, TResult>(KunEvent<TPayload, IReadOnlyList<TResult>> evt, DispatchMode mode, Delegate handler, int priority) { Open(); return inner.On(evt, mode, handler, priority); }
    public ValueTask<TResult> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, TResult> evt, TPayload payload, CancellationToken ct) { Open(); return inner.DispatchAsync(evt, payload, ct); }
    public ValueTask<IReadOnlyList<TResult>> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, IReadOnlyList<TResult>> evt, TPayload payload, CancellationToken ct) { Open(); return inner.DispatchAsync(evt, payload, ct); }
    public ValueTask<TResult> DispatchAsync<TPayload, TResult>(KunEvent<TPayload, TResult> evt, TPayload payload, Func<ValueTask<TResult>> terminal, CancellationToken ct) { Open(); return inner.DispatchAsync(evt, payload, terminal, ct); }
    public IDisposable Restrict(ScopeKey scope, Func<Registration, bool> keep) { Open(); return inner.Restrict(scope, keep); }
    public IReadOnlyList<Registration> List(ServiceKey key) { Open(); return inner.List(key); }
    private void Open() => ObjectDisposedException.ThrowIf(state.Closed, this);

    internal sealed class Switch(ITenantContext tenant)
    {
        private volatile bool _closed, _dataMissing;
        private int _dataRequests;
        internal bool Closed => _closed;
        internal void Close() => _closed = true;
        // 经上下文取数据服务（Inject）的次数：构造 worker 一次，此后每次领取、每次 BeginAsync 各一次；关闭后抛出的也算。
        internal int DataRequests => Volatile.Read(ref _dataRequests);
        // BeginAsync 经上下文取数据服务时关闭：领取循环不带租户，执行里带租户，带租户的那一次就是 BeginAsync（领到了、还没开始准备）。
        internal bool CloseOnBegin { get; set; }
        // 第一次 Scoped 时关闭：BeginAsync 已提交、开始解析的那一刻。
        internal bool CloseOnFirstScoped { get; set; }
        // 上下文开着而数据服务缺失（例如数据组件的注册已撤销、条目回到 Pending）：TryInject 返回 null，Inject 抛 ServiceNotReadyException。
        internal bool DataMissing { get => _dataMissing; set => _dataMissing = value; }
        // 换掉处理器能力的调用：拿到真实能力与调用参数，可以在调用前后关闭上下文。
        internal Func<IExecutableCapability, IPluginContext, JsonElement, CancellationToken, ValueTask<JsonElement>>? Handler { get; set; }
        internal CapturingLogger Log { get; } = new();
        internal void DataRequested()
        {
            Interlocked.Increment(ref _dataRequests);
            if (CloseOnBegin && tenant.TenantId is not null) Close();
        }
    }

    // 能力绑定调用时经调用方重新解析自己、按引用比对：经包过的调用方拿到的是钩子，报 NOT_FOUND；上下文已关闭时它把 ObjectDisposedException 也换成 NOT_FOUND。
    // 钩子要让处理器照常跑，用 Unwrap 取没包过的真实上下文去调。
    internal static IPluginContext Unwrap(IPluginContext context) => context is ClosingPluginContext closing ? closing.Inner : context;
    private IPluginContext Inner => inner;
    private sealed class HookedCapability(IExecutableCapability real, Func<IExecutableCapability, IPluginContext, JsonElement, CancellationToken, ValueTask<JsonElement>> handler) : IExecutableCapability
    {
        public CapabilityContract Contract => real.Contract;
        public ValueTask<JsonElement> InvokeAsync(IPluginContext caller, JsonElement input, CancellationToken cancellationToken = default) => handler(real, caller, input, cancellationToken);
    }
}

internal sealed class CapturingLogger : ILogger
{
    private readonly ConcurrentQueue<(LogLevel Level, string Message)> _entries = new();
    internal bool Has(string text) => _entries.Any(entry => entry.Message.Contains(text, StringComparison.Ordinal));
    // Warning 及以上的条目，断言失败时 xUnit 会列出内容。
    internal IReadOnlyList<string> Warnings => [.. _entries.Where(entry => entry.Level >= LogLevel.Warning).Select(entry => entry.Level + " " + entry.Message)];
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => _entries.Enqueue((logLevel, formatter(state, exception) + (exception is null ? "" : " | " + exception.GetType().Name)));
}
