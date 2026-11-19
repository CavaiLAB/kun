using Kun.EventBus.RabbitMQ.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Kun.EventBus.RabbitMQ;

/// <summary>
/// Singleton that owns and lazily opens the <see cref="IConnection"/> to RabbitMQ.
/// Automatic recovery is enabled via <see cref="ConnectionFactory.AutomaticRecoveryEnabled"/>.
/// </summary>
public sealed partial class RabbitMqConnectionProvider : IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConnectionProvider> _logger;
    private readonly Func<ConnectionFactory, CancellationToken, Task<IConnection>> _connect;
    private IConnection? _connection;
    // 释放一开始就置 1；此后 GetConnectionAsync 一律抛 ObjectDisposedException，不再建连接。
    private int _disposed;
    // 释放时只放开、不 Dispose：SemaphoreSlim.Dispose 不唤醒排在锁上的调用方，它们会一直挂着；
    // 放开后它们醒来，看到已释放就抛 ObjectDisposedException。它没用 AvailableWaitHandle，不持有非托管资源。
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>Initializes the provider with the supplied options.</summary>
    public RabbitMqConnectionProvider(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqConnectionProvider> logger)
        : this(options, logger, static (factory, cancellationToken) => factory.CreateConnectionAsync(cancellationToken))
    {
    }

    // 建连接这一步可替换：用例用它换上假连接（比如关闭时报「对端已关」的），其余逻辑不变。
    internal RabbitMqConnectionProvider(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqConnectionProvider> logger,
        Func<ConnectionFactory, CancellationToken, Task<IConnection>> connect)
    {
        _options = options.Value;
        _logger = logger;
        _connect = connect;
    }

    /// <summary>Returns the open connection, lazily creating it on first call.</summary>
    /// <exception cref="ObjectDisposedException">The provider is being or has been disposed.</exception>
    public async ValueTask<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        // 先取到局部变量再判断：释放会把字段置空，直接读两次字段可能返回 null。
        if (Volatile.Read(ref _disposed) == 0 && _connection is { IsOpen: true } open)
        {
            return open;
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            if (_connection is not null)
            {
                // 发布器和订阅器共享连接。网络恢复期间替换它会销毁消费者拓扑，
                // 而订阅循环仍在等待旧连接。保留自动恢复实例，调用方稍后重试。
                throw new IOException("RabbitMQ 连接正在恢复，暂不能创建发布通道。");
            }

            // CA1873 flags the argument computation, but masking is a handful
            // of IndexOf calls — and credentials must NEVER reach the sink.
#pragma warning disable CA1873
            Log.OpeningConnection(_logger, SanitizeUri(_options.ConnectionString));
#pragma warning restore CA1873

            var factory = new ConnectionFactory
            {
                Uri = new Uri(_options.ConnectionString),
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                ClientProvidedName = _options.ConsumerName,
                // Use a short fixed timeout so an unreachable broker fails fast with
                // BrokerUnreachableException rather than hanging until the caller's
                // CancellationToken fires (which would produce OperationCanceledException
                // and silently skip AttemptCount increment in the outbox publisher loop).
                RequestedConnectionTimeout = TimeSpan.FromSeconds(2),
            };

            // Pass CancellationToken.None: we want a fast failure (BrokerUnreachableException)
            // on bad hosts rather than OperationCanceledException from the stopping token,
            // which the outbox publisher's catch guard would silently discard.
            _connection = await _connect(factory, CancellationToken.None).ConfigureAwait(false);

            Log.ConnectionEstablished(_logger);
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Closes and disposes the connection. From the moment disposal starts,
    /// <see cref="GetConnectionAsync"/> throws <see cref="ObjectDisposedException"/>; a connection
    /// still being opened is waited for and closed too. A connection the broker (or the network) has
    /// already closed counts as closed; any other failure while closing — including other
    /// <see cref="OperationInterruptedException"/>s — is rethrown after the connection has been
    /// disposed and the internal lock released.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        // 等正在建的连接建完再关，免得它建好之后没人关；建连接本身有超时（RequestedConnectionTimeout 与握手超时）。
        await _lock.WaitAsync(CancellationToken.None).ConfigureAwait(false);
        try
        {
            var connection = _connection;
            _connection = null;
            if (connection is not null)
            {
                try
                {
                    await connection.CloseAsync().ConfigureAwait(false);
                }
                catch (AlreadyClosedException closed)
                {
                    // 客户端只在关闭前查一次 IsOpen：对端的关闭落在「查」与「关」之间时，CloseAsync 抛 AlreadyClosedException。
                    // 要达到的状态本来就是已关闭，照常释放。只容忍这一种：父类 OperationInterruptedException 的其他情形
                    // 说明关闭本身被打断了，连接有没有关好不由类型担保，照常抛出。
                    Log.ConnectionAlreadyClosed(_logger, closed.ShutdownReason?.ReplyCode ?? 0, closed.ShutdownReason?.ReplyText ?? "");
                }
                finally
                {
                    connection.Dispose();
                }
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Masks the password in an AMQP URI for logging —
    /// <c>amqp://user:secret@host</c> → <c>amqp://user:***@host</c>.
    /// Credentials must never reach the log sink.
    /// </summary>
    internal static string SanitizeUri(string uri)
    {
        var schemeEnd = uri.IndexOf("://", StringComparison.Ordinal);
        var at = uri.IndexOf('@', StringComparison.Ordinal);
        if (schemeEnd < 0 || at < 0)
        {
            return uri;
        }

        var colon = uri.IndexOf(':', schemeEnd + 3);
        if (colon < 0 || colon > at)
        {
            return uri;
        }

        return string.Concat(uri.AsSpan(0, colon + 1), "***", uri.AsSpan(at));
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 10, Level = LogLevel.Information,
            Message = "Opening RabbitMQ connection to {Uri}")]
        public static partial void OpeningConnection(ILogger logger, string uri);

        [LoggerMessage(EventId = 11, Level = LogLevel.Information,
            Message = "RabbitMQ connection established.")]
        public static partial void ConnectionEstablished(ILogger logger);

        [LoggerMessage(EventId = 12, Level = LogLevel.Debug,
            Message = "RabbitMQ connection was already closed before shutdown ({ReplyCode} {ReplyText}).")]
        public static partial void ConnectionAlreadyClosed(ILogger logger, ushort replyCode, string replyText);
    }
}
