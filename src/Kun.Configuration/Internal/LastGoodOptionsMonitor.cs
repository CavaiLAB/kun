using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Kun.Configuration.Internal;

internal static class ConfigurationTelemetry
{
    private static readonly Meter Meter = new("Kun.Configuration");
    internal static readonly Counter<long> Rejected = Meter.CreateCounter<long>("kun.configuration.reload.rejected");
}

internal sealed partial class LastGoodOptionsMonitor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(
    IOptionsFactory<T> factory, IEnumerable<IOptionsChangeTokenSource<T>> sources, string section, string? plugin,
    ReloadFailureSource failuresSource, ILogger logger) : IOptionsMonitor<T>, IDisposable where T : class
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, T> _values = new(StringComparer.Ordinal);
    private readonly List<IDisposable> _subscriptions = [];
    private Action<T, string?>? _listeners;
    private bool _disposed;

    // DI 工厂显式调用一次，避免主构造器初始化期间捕获尚未就绪的实例。
    internal LastGoodOptionsMonitor<T> Start()
    {
        foreach (var source in sources)
        {
            var name = source.Name ?? Options.DefaultName;
            _subscriptions.Add(ChangeToken.OnChange(source.GetChangeToken, () => Reload(name)));
        }
        return this;
    }

    public T CurrentValue => Get(Options.DefaultName);

    // 首次读取即校验：没有上一份有效值时，校验失败照常抛出 OptionsValidationException。
    public T Get(string? name)
    {
        name ??= Options.DefaultName;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_values.TryGetValue(name, out var value)) _values[name] = value = factory.Create(name);
            return value;
        }
    }

    private void Reload(string name)
    {
        T candidate;
        Action<T, string?>? notify;
        int failures;
        lock (_gate)
        {
            if (_disposed) return;
            try
            {
                candidate = factory.Create(name);
                _values[name] = candidate;
                notify = _listeners;
                failures = 0;
            }
            // 绑定器把无法转换的值报成 InvalidOperationException；配置根已释放（ObjectDisposedException）不算候选被拒。
            catch (Exception error) when (error is OptionsValidationException or FormatException or OverflowException ||
                error is InvalidOperationException and not ObjectDisposedException)
            {
                candidate = null!; notify = null;
                failures = error is OptionsValidationException validation ? Math.Max(1, validation.Failures.Count()) : 1;
            }
        }
        if (failures == 0) { notify?.Invoke(candidate, name); return; }
        var failure = new OptionsReloadFailure(typeof(T).FullName ?? typeof(T).Name, name, section, plugin, failures);
        var tags = new TagList { { "options.type", failure.OptionsType }, { "options.name", name } };
        if (plugin is not null) tags.Add("plugin", plugin);
        ConfigurationTelemetry.Rejected.Add(1, tags);
        LogRejected(logger, failure.OptionsType, name, section, failures);
        failuresSource.Publish(failure, logger);
    }

    public IDisposable OnChange(Action<T, string?> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        lock (_gate) _listeners += listener;
        return new Subscription(() => { lock (_gate) _listeners -= listener; });
    }

    public void Dispose()
    {
        lock (_gate) { if (_disposed) return; _disposed = true; _listeners = null; }
        foreach (var subscription in _subscriptions) subscription.Dispose();
    }

    private sealed class Subscription(Action remove) : IDisposable
    {
        private Action? _remove = remove;
        public void Dispose() => Interlocked.Exchange(ref _remove, null)?.Invoke();
    }

    // 只记类型、节与失败条数；校验消息可能带值，不进日志。
    [LoggerMessage(EventId = 2101, Level = LogLevel.Warning, Message = "配置 {OptionsType}/{OptionsName}（节 {Section}）重载被拒绝：{Failures} 条校验未通过，保留上一份有效值。")]
    private static partial void LogRejected(ILogger logger, string optionsType, string optionsName, string section, int failures);
}
