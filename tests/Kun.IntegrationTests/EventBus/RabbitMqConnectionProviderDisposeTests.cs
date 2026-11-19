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

    // 假连接上收到的调用，按先后次序（建连接时不碰假连接，所以只有释放时的调用）。
    private static IEnumerable<string> Calls(IConnection connection) =>
        connection.ReceivedCalls().Select(call => call.GetMethodInfo().Name);

    private static IConnection Connection(Task close)
    {
        var connection = Substitute.For<IConnection>();
        connection.CloseAsync(default, default!, default, default, default).ReturnsForAnyArgs(close);
        return connection;
    }

    private static async Task<RabbitMqConnectionProvider> OpenAsync(IConnection connection)
    {
        var provider = new RabbitMqConnectionProvider(
            Options.Create(new RabbitMqOptions { ConnectionString = "amqp://localhost:5672/", ConsumerName = "dispose-tests" }),
            NullLogger<RabbitMqConnectionProvider>.Instance,
            (_, _) => Task.FromResult(connection));
        (await provider.GetConnectionAsync(TestContext.Current.CancellationToken)).Should().BeSameAs(connection);
        return provider;
    }
}
