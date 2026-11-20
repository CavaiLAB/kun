using Microsoft.Extensions.Logging;

namespace Kun.Configuration.Internal;

internal sealed partial class ReloadFailureSource(IEnumerable<IObserver<OptionsReloadFailure>> observers) : IObservable<OptionsReloadFailure>
{
    private readonly Lock _gate = new();
    private readonly List<IObserver<OptionsReloadFailure>> _observers = [.. observers];

    public IDisposable Subscribe(IObserver<OptionsReloadFailure> observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        lock (_gate) _observers.Add(observer);
        return new Subscription(this, observer);
    }

    internal void Publish(OptionsReloadFailure failure, ILogger logger)
    {
        IObserver<OptionsReloadFailure>[] observers;
        lock (_gate) observers = [.. _observers];
        foreach (var observer in observers)
        {
            try { observer.OnNext(failure); }
            catch (Exception) { LogObserverFailed(logger, failure.Section); }
        }
    }

    private sealed class Subscription(ReloadFailureSource source, IObserver<OptionsReloadFailure> observer) : IDisposable
    {
        private int _disposed;
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            lock (source._gate) source._observers.Remove(observer);
        }
    }

    [LoggerMessage(EventId = 2103, Level = LogLevel.Warning, Message = "配置节 {Section} 的重载失败观察者未能处理通知。")]
    private static partial void LogObserverFailed(ILogger logger, string section);
}
