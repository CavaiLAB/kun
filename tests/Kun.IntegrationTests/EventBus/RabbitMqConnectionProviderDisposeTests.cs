using FluentAssertions;
using Kun.EventBus.RabbitMQ;
using Kun.EventBus.RabbitMQ.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Xunit;

namespace Kun.IntegrationTests.EventBus;

// 释放路径不靠时序：用假连接直接造出关闭时的各种结果，不起 RabbitMQ。
public sealed class RabbitMqConnectionProviderDisposeTests
{
    [Fact]
    public async Task DisposeAsync_ConnectionAlreadyClosedByBroker_ReleasesWithoutThrowing()
    {
        // 与 CI 上撞到的一样：对端以 504 CHANNEL_ERROR 关掉了连接，关闭时客户端抛 AlreadyClosedException。
        var closedByBroker = new AlreadyClosedException(
            new ShutdownEventArgs(ShutdownInitiator.Peer, 504, "CHANNEL_ERROR - expected 'channel.open'", 60, 80));
        var connection = Connection(Task.FromException(closedByBroker));
        var provider = await OpenAsync(connection);

        var dispose = () => provider.DisposeAsync().AsTask();

        await dispose.Should().NotThrowAsync();
        Calls(connection).Should().Equal("CloseAsync", "Dispose");
    }

    [Fact]
    public async Task DisposeAsync_OtherCloseFailure_IsRethrownAfterReleasing()
    {
        var failure = new TimeoutException("close timed out");
        var connection = Connection(Task.FromException(failure));
        var provider = await OpenAsync(connection);

        var dispose = () => provider.DisposeAsync().AsTask();

        (await dispose.Should().ThrowAsync<TimeoutException>()).Which.Should().BeSameAs(failure);
        Calls(connection).Should().Equal("CloseAsync", "Dispose");
    }

    [Fact]
    public async Task DisposeAsync_OpenConnection_ClosesThenDisposesOnce()
    {
        var connection = Connection(Task.CompletedTask);
        var provider = await OpenAsync(connection);

        await provider.DisposeAsync();
        await provider.DisposeAsync();

        Calls(connection).Should().Equal("CloseAsync", "Dispose");
    }

    [Fact]
    public async Task DisposeAsync_CloseInterruptedOtherThanAlreadyClosed_IsRethrownAfterReleasing()
    {
        // 只容忍 AlreadyClosedException 这一种；父类的其他情形说明关闭本身被打断了，照常抛出。
        var interrupted = new OperationInterruptedException(
            new ShutdownEventArgs(ShutdownInitiator.Library, 541, "INTERNAL_ERROR"));
        var connection = Connection(Task.FromException(interrupted));
        var provider = await OpenAsync(connection);

        var dispose = () => provider.DisposeAsync().AsTask();

        (await dispose.Should().ThrowAsync<OperationInterruptedException>()).Which.Should().BeSameAs(interrupted);
        Calls(connection).Should().Equal("CloseAsync", "Dispose");
    }

    [Fact]
    public async Task GetConnectionAsync_AfterDispose_ThrowsObjectDisposedWithoutConnectingAgain()
    {
        var connection = Connection(Task.CompletedTask);
        var connects = 0;
        var provider = await OpenAsync(connection, () => Interlocked.Increment(ref connects));

        await provider.DisposeAsync();
        var get = () => provider.GetConnectionAsync(TestContext.Current.CancellationToken).AsTask().WaitAsync(Timeout, TestContext.Current.CancellationToken);

        await get.Should().ThrowAsync<ObjectDisposedException>();
        connects.Should().Be(1);
    }

    [Fact]
    public async Task GetConnectionAsync_AfterACloseFailure_ThrowsObjectDisposed_TheLockWasReleased()
    {
        // 释放时关闭失败、异常照常抛出之后，内部锁也已放开：之后的调用拿得到锁，按已释放抛异常，而不是一直等锁。
        var connection = Connection(Task.FromException(new TimeoutException("close timed out")));
        var connects = 0;
        var provider = await OpenAsync(connection, () => Interlocked.Increment(ref connects));
        await provider.Awaiting(p => p.DisposeAsync().AsTask()).Should().ThrowAsync<TimeoutException>();

        var get = () => provider.GetConnectionAsync(TestContext.Current.CancellationToken).AsTask().WaitAsync(Timeout, TestContext.Current.CancellationToken);

        await get.Should().ThrowAsync<ObjectDisposedException>();
        connects.Should().Be(1);
    }

    [Fact]
    public async Task DisposeAsync_WhileAConnectionIsBeingOpened_ClosesIt_AndLaterCallersGetObjectDisposed()
    {
        var ct = TestContext.Current.CancellationToken;
        var opening = new TaskCompletionSource<IConnection>(TaskCreationOptions.RunContinuationsAsynchronously);
        var connects = 0;
        var provider = new RabbitMqConnectionProvider(
            Options.Create(new RabbitMqOptions { ConnectionString = "amqp://localhost:5672/", ConsumerName = "dispose-tests" }),
            NullLogger<RabbitMqConnectionProvider>.Instance,
            (_, _) =>
            {
                Interlocked.Increment(ref connects);
                return opening.Task;
            });

        // 第一个调用方拿着锁在建连接；这时开始释放，之后又来一个调用方。
        var first = provider.GetConnectionAsync(ct).AsTask();
        var dispose = provider.DisposeAsync().AsTask();
        var late = provider.GetConnectionAsync(ct).AsTask();
        var connection = Connection(Task.CompletedTask);
        opening.SetResult(connection);

        (await first.WaitAsync(Timeout, ct)).Should().BeSameAs(connection);
        await dispose.WaitAsync(Timeout, ct);
        await late.Awaiting(t => t.WaitAsync(Timeout, ct)).Should().ThrowAsync<ObjectDisposedException>();
        connects.Should().Be(1, "开始释放之后不再建新连接");
        Calls(connection).Should().Equal(["CloseAsync", "Dispose"], "释放期间建好的连接也要关掉，不能没人管");
    }

    // 只在用例失败时才会等满：正常情况下这些调用立即完成。
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    // 假连接上收到的调用，按先后次序（建连接时不碰假连接，所以只有释放时的调用）。
    private static IEnumerable<string> Calls(IConnection connection) =>
        connection.ReceivedCalls().Select(call => call.GetMethodInfo().Name);

    private static IConnection Connection(Task close)
    {
        var connection = Substitute.For<IConnection>();
        connection.CloseAsync(default, default!, default, default, default).ReturnsForAnyArgs(close);
        return connection;
    }

    private static async Task<RabbitMqConnectionProvider> OpenAsync(IConnection connection, Action? onConnect = null)
    {
        var provider = new RabbitMqConnectionProvider(
            Options.Create(new RabbitMqOptions { ConnectionString = "amqp://localhost:5672/", ConsumerName = "dispose-tests" }),
            NullLogger<RabbitMqConnectionProvider>.Instance,
            (_, _) =>
            {
                onConnect?.Invoke();
                return Task.FromResult(connection);
            });
        (await provider.GetConnectionAsync(TestContext.Current.CancellationToken)).Should().BeSameAs(connection);
        return provider;
    }
}
