using Microsoft.Extensions.Logging;

namespace Kun.IntegrationTests.EventBus;

/// <summary>把日志记在内存里，用例按事件号查，或等某个事件号出现。</summary>
internal sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly List<LogEntry> _entries = [];
    private readonly List<(int EventId, TaskCompletionSource<LogEntry> Signal)> _waiters = [];

    public IReadOnlyList<LogEntry> Entries
    {
        get
        {
            lock (_entries)
            {
                return [.. _entries];
            }
        }
    }

    /// <summary>等第一条带这个事件号的日志；已经有了就立即返回。</summary>
    public Task<LogEntry> WaitForAsync(int eventId)
    {
        lock (_entries)
        {
            var existing = _entries.Find(entry => entry.EventId == eventId);
            if (existing is not null)
            {
                return Task.FromResult(existing);
            }

            var signal = new TaskCompletionSource<LogEntry>(TaskCreationOptions.RunContinuationsAsynchronously);
            _waiters.Add((eventId, signal));
            return signal.Task;
        }
    }

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(this, categoryName);

    public void Dispose()
    {
    }

    private void Add(LogEntry entry)
    {
        lock (_entries)
        {
            _entries.Add(entry);
            for (var i = _waiters.Count - 1; i >= 0; i--)
            {
                if (_waiters[i].EventId == entry.EventId)
                {
                    _waiters[i].Signal.TrySetResult(entry);
                    _waiters.RemoveAt(i);
                }
            }
        }
    }

    internal sealed record LogEntry(string Category, LogLevel Level, int EventId, string Message, Exception? Exception);

    private sealed class CapturingLogger(CapturingLoggerProvider provider, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            provider.Add(new LogEntry(category, logLevel, eventId.Id, formatter(state, exception), exception));
    }
}
