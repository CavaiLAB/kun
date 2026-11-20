using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kun.Configuration.Platform;

internal static class ConfigTelemetry
{
    internal const string MeterName = "Kun.Configuration.Platform";
    private static readonly Meter Meter = new(MeterName);
    // 点号命名；Prometheus 导出后即 qian_config_reload_failed_total。
    internal static readonly Counter<long> ReloadFailed = Meter.CreateCounter<long>("qian.config.reload.failed", description: "强类型配置重载被校验拒绝、保留上一份有效值的次数");
}

internal sealed record ReloadNotice(ConfigReloadFailed Payload);

// 监视器与挂载中的配置组件之间的进程内转发：组件挂载期间把重载失败作为 config/reload-failed 发出；没有组件挂载时只有指标与日志。
internal sealed partial class ConfigReloadNotifier(ILoggerFactory? loggerFactory = null) : IObserver<OptionsReloadFailure>
{
    private readonly ILogger _logger = loggerFactory?.CreateLogger("Kun.Configuration.Platform.Options") ?? NullLogger.Instance;

    public void OnNext(OptionsReloadFailure failure)
    {
        var tags = new TagList { { "options.type", failure.OptionsType }, { "options.name", failure.OptionsName } };
        if (failure.Plugin is not null) tags.Add("plugin", failure.Plugin);
        ConfigTelemetry.ReloadFailed.Add(1, tags);
        Publish(new ConfigReloadFailed(failure.OptionsType, failure.OptionsName, failure.Section, failure.Failures, ConfigErrors.ReloadRejected.Code)
        { Plugin = failure.Plugin }, _logger);
    }

    public void OnCompleted() { }
    public void OnError(Exception error) { }

    private readonly Lock _gate = new();
    private readonly List<Func<ReloadNotice, ValueTask>> _handlers = [];

    internal IDisposable Subscribe(Func<ReloadNotice, ValueTask> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        lock (_gate) _handlers.Add(handler);
        return new Unsubscriber(this, handler);
    }

    // 在变更令牌回调线程上触发，不等待分发：监听器失败只记日志，不影响保留旧值的结论。
    internal void Publish(ConfigReloadFailed failure, ILogger logger)
    {
        Func<ReloadNotice, ValueTask>[] handlers;
        lock (_gate) handlers = [.. _handlers];
        var notice = new ReloadNotice(failure);
        foreach (var handler in handlers) _ = Dispatch(handler, notice, logger);
    }

    private static async Task Dispatch(Func<ReloadNotice, ValueTask> handler, ReloadNotice notice, ILogger logger)
    {
        try { await handler(notice).ConfigureAwait(false); }
        catch (Exception error) { LogDispatchFailed(logger, notice.Payload.Section, error); }
    }

    private sealed class Unsubscriber(ConfigReloadNotifier owner, Func<ReloadNotice, ValueTask> handler) : IDisposable
    {
        private int _disposed;
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            lock (owner._gate) owner._handlers.Remove(handler);
        }
    }

    [LoggerMessage(EventId = 2102, Level = LogLevel.Warning, Message = "配置节 {Section} 的重载失败告警未能发出。")]
    private static partial void LogDispatchFailed(ILogger logger, string section, Exception error);
}
