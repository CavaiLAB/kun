using FluentAssertions;
using Kun.EventBus.RabbitMQ.Consuming;
using NSubstitute;
using RabbitMQ.Client;
using Xunit;

namespace Kun.IntegrationTests.EventBus;

// 确认与关通道之间的闸，直接对着假通道测：每一步都在调用返回时就能判定，不靠时序。
public sealed class ConsumeSessionTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task TrySettleAsync_BeforeClose_AcknowledgesAndRejectsOnTheChannel()
    {
        var channel = Substitute.For<IChannel>();
        var session = new ConsumeSession(channel);

        (await session.TrySettleAsync(1, ack: true, requeue: false)).Should().BeTrue();
        (await session.TrySettleAsync(2, ack: false, requeue: true)).Should().BeTrue();

        await channel.Received(1).BasicAckAsync(1, false, Arg.Any<CancellationToken>());
        await channel.Received(1).BasicNackAsync(2, false, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TrySettleAsync_OnceCloseHasStarted_SendsNothing()
    {
        var ct = TestContext.Current.CancellationToken;
        var channel = Substitute.For<IChannel>();
        // 关通道还没完成：真实客户端关通道要等正在执行的处理器返回，那些处理器这时来确认。
        var closing = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        CloseReturns(channel, closing.Task);
        var session = new ConsumeSession(channel);

        var close = session.CloseAsync();
        (await session.TrySettleAsync(1, ack: true, requeue: false)).Should().BeFalse();
        (await session.TrySettleAsync(2, ack: false, requeue: true)).Should().BeFalse();
        closing.SetResult();
        await close.WaitAsync(Timeout, ct);
        (await session.TrySettleAsync(3, ack: true, requeue: false)).Should().BeFalse();

        Calls(channel).Should().Equal("CloseAsync");
    }

    [Fact]
    public async Task CloseAsync_WaitsForAnAckAlreadyBeingWritten_BeforeClosingTheChannel()
    {
        var ct = TestContext.Current.CancellationToken;
        var channel = Substitute.For<IChannel>();
        var ackWritten = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        AckReturns(channel, ackWritten.Task);
        var session = new ConsumeSession(channel);

        var settle = session.TrySettleAsync(1, ack: true, requeue: false).AsTask();
        var close = session.CloseAsync();

        // 确认帧还在写，关通道不能先发出去，否则确认可能落在关通道之后。
        Calls(channel).Should().Equal("BasicAckAsync");
        ackWritten.SetResult();
        (await settle.WaitAsync(Timeout, ct)).Should().BeTrue();
        await close.WaitAsync(Timeout, ct);
        Calls(channel).Should().Equal("BasicAckAsync", "CloseAsync");
    }

    [Fact]
    public async Task CloseAsync_AfterAnAckWriteFailed_IsNotBlocked()
    {
        var ct = TestContext.Current.CancellationToken;
        var channel = Substitute.For<IChannel>();
        AckReturns(channel, Task.FromException(new IOException("写确认帧失败")));
        var session = new ConsumeSession(channel);

        await session.Awaiting(s => s.TrySettleAsync(1, ack: true, requeue: false).AsTask()).Should().ThrowAsync<IOException>();
        await session.CloseAsync().WaitAsync(Timeout, ct);

        Calls(channel).Should().Equal("BasicAckAsync", "CloseAsync");
    }

    private static IEnumerable<string> Calls(IChannel channel) =>
        channel.ReceivedCalls().Select(call => call.GetMethodInfo().Name);

    private static void AckReturns(IChannel channel, Task written)
    {
#pragma warning disable CA2012 // 设置 mock 的 ValueTask 返回值，不消费业务 ValueTask。
        channel.BasicAckAsync(default, default, default).ReturnsForAnyArgs(new ValueTask(written));
#pragma warning restore CA2012
    }

    private static void CloseReturns(IChannel channel, Task closed) =>
        channel.CloseAsync(default, default!, default, default).ReturnsForAnyArgs(closed);
}
