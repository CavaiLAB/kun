using System.Text.Json.Serialization;
using FluentAssertions;
using Kun.Core.MultiTenancy;
using Kun.EventBus;
using Kun.EventBus.DependencyInjection;
using Kun.EventBus.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Xunit;

namespace Kun.IntegrationTests.EventBus;

internal sealed record StopOrderEvent(Guid Id);

[JsonSerializable(typeof(StopOrderEvent))]
internal sealed partial class StopOrderJsonContext : JsonSerializerContext { }

/// <summary>处理器放开之后怎么结束。</summary>
public enum StopOrderOutcome
{
    /// <summary>正常返回，订阅端随后确认。</summary>
    Succeed,

    /// <summary>抛出异常，订阅端走重试或死信，再确认原消息（假连接照样建出重试通道）。</summary>
    Fail,

    /// <summary>
    /// 抛出异常，而建重试通道时停止已开始：真实客户端建通道要拿已取消的令牌等通道的 RPC 信号量，抛取消。
    /// 订阅端走重试失败时「拒绝并重新入队」那一处；这个取消随后由客户端的事件包装吞掉。
    /// </summary>
    FailAndRetryChannelCancelled,

    /// <summary>响应取消（停止已开始），订阅端拒绝并重新入队。</summary>
    HonourCancellation,
}

/// <summary>用例控制处理器：进来时报到，停在闸上，放开后按设定的结果结束。</summary>
internal sealed class StopOrderProbe
{
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public StopOrderOutcome Outcome { get; init; }
}

internal sealed class StopOrderHandler(StopOrderProbe probe) : IIntegrationEventHandler<StopOrderEvent>
{
    public async ValueTask HandleAsync(StopOrderEvent @event, MessageContext context, CancellationToken cancellationToken)
    {
        probe.Entered.TrySetResult();
        await probe.Release.Task;
        switch (probe.Outcome)
        {
            case StopOrderOutcome.Fail:
            case StopOrderOutcome.FailAndRetryChannelCancelled:
                throw new InvalidOperationException("处理失败");
            case StopOrderOutcome.HonourCancellation:
                cancellationToken.ThrowIfCancellationRequested();
                break;
        }
    }
}

/// <summary>
/// 订阅端停止或会话出错时的确认顺序，用假连接、假通道，不起 RabbitMQ，不靠时序：用例自己投递消息、
/// 控制处理器何时结束、控制关通道何时完成。假通道的关通道要等用例放行，与真实客户端一致：
/// 客户端关通道要等正在执行的处理器返回，所以那时订阅端手里的通道还没释放。
/// </summary>
public sealed class RabbitMqSubscriberStopTests
{
    private const string EventTypeId = "integration.test.stop-order.v1";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    [Theory]
    [InlineData(StopOrderOutcome.Succeed)]
    [InlineData(StopOrderOutcome.Fail)]
    [InlineData(StopOrderOutcome.FailAndRetryChannelCancelled)]
    [InlineData(StopOrderOutcome.HonourCancellation)]
    public async Task Stop_WhileADeliveryIsBeingHandled_NothingIsSettledAfterTheChannelClose(StopOrderOutcome outcome)
    {
        var ct = TestContext.Current.CancellationToken;
        var broker = new FakeBroker { RetryChannelCancelled = outcome == StopOrderOutcome.FailAndRetryChannelCancelled };
        var probe = new StopOrderProbe { Outcome = outcome };
        using var host = BuildHost(broker, probe);
        await host.StartAsync(ct);
        var consumer = await broker.Consumer.Task.WaitAsync(Timeout, ct);

        var delivery = DeliverAsync(consumer, deliveryTag: 1);
        await probe.Entered.Task.WaitAsync(Timeout, ct);

        // 处理器还停在闸上时停止宿主，等订阅端发出关通道。
        var stopping = host.StopAsync(CancellationToken.None);
        await broker.CloseRequested.Task.WaitAsync(Timeout, ct);

        // 关通道已经发出，处理器这时才结束；它结束之后，关通道才完成。
        probe.Release.SetResult();
        await delivery.WaitAsync(Timeout, ct);
        broker.CloseCompletion.SetResult();
        await stopping.WaitAsync(Timeout, ct);

        broker.SettlementsAfterClose().Should().BeEmpty(
            "通道的关闭发出之后再确认或拒绝，RabbitMQ 会在已清掉的通道号上收到帧，以 504 关掉整条连接");
        await broker.Channel.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task DeliveryHandledBeforeStop_IsAcknowledged()
    {
        var ct = TestContext.Current.CancellationToken;
        var broker = new FakeBroker();
        var probe = new StopOrderProbe { Outcome = StopOrderOutcome.Succeed };
        probe.Release.SetResult();
        using var host = BuildHost(broker, probe);
        await host.StartAsync(ct);
        var consumer = await broker.Consumer.Task.WaitAsync(Timeout, ct);

        await DeliverAsync(consumer, deliveryTag: 7).WaitAsync(Timeout, ct);

        await broker.Channel.Received(1).BasicAckAsync(7, false, Arg.Any<CancellationToken>());
        broker.CloseCompletion.SetResult();
        await host.StopAsync(ct);
        broker.SettlementsAfterClose().Should().BeEmpty();
    }

    [Fact]
    public async Task Stop_ChannelAlreadyClosedByTheBroker_ChannelIsDisposedAndTheStopIsClean()
    {
        var ct = TestContext.Current.CancellationToken;
        var broker = new FakeBroker();
        broker.CloseCompletion.SetException(new AlreadyClosedException(
            new ShutdownEventArgs(ShutdownInitiator.Peer, 504, "CHANNEL_ERROR - expected 'channel.open'", 60, 80)));
        var logs = new CapturingLoggerProvider();
        using var host = BuildHost(broker, new StopOrderProbe(), logs);
        await host.StartAsync(ct);
        await broker.Consumer.Task.WaitAsync(Timeout, ct);

        await host.StopAsync(ct);

        await broker.Channel.Received(1).DisposeAsync();
        host.Services.GetServices<IHostedService>().OfType<KunEventSubscriber>().Single().ExecuteTask!.IsCompletedSuccessfully.Should().BeTrue();
        logs.Entries.Should().NotContain(entry => entry.EventId == ConsumeSessionFailed,
            "连接已被对端关掉时关通道，不算消费会话失败");
        logs.Entries.Should().Contain(entry => entry.EventId == ChannelAlreadyClosed)
            .And.NotContain(entry => entry.EventId == ChannelCloseFailed, "对端已经关掉的通道记 Debug，不算关通道失败");
    }

    [Fact]
    public async Task SessionFails_AndClosingTheChannelFailsToo_TheOriginalFailureIsReported()
    {
        var ct = TestContext.Current.CancellationToken;
        var broker = new FakeBroker();
        var original = new InvalidOperationException("设置预取失败");
        broker.FailPrefetch(original);
        broker.CloseCompletion.SetException(new IOException("关通道失败"));
        var logs = new CapturingLoggerProvider();
        using var host = BuildHost(broker, new StopOrderProbe(), logs);
        await host.StartAsync(ct);

        var failure = await logs.WaitForAsync(ConsumeSessionFailed).WaitAsync(Timeout, ct);
        await host.StopAsync(ct);

        failure.Exception.Should().BeSameAs(original, "关通道的失败不能盖掉会话本来的失败");
        logs.Entries.Should().Contain(entry => entry.EventId == ChannelCloseFailed && entry.Exception is IOException);
        await broker.Channel.Received(1).DisposeAsync();
    }

    /// <summary>
    /// 投递在哪条通道上到达，就只在哪条通道上确认。会话出错、订阅端重连之后，上一次会话里还没结束的处理器
    /// 不能把确认发到下一次会话的通道上（投递标签按通道各自从 1 数起），也不能发到自己那条已经关了的通道上。
    /// </summary>
    [Fact]
    public async Task SessionFailsWhileADeliveryIsBeingHandled_NothingIsSettledOnTheNextSessionsChannel()
    {
        var ct = TestContext.Current.CancellationToken;
        var broker = new FakeBroker();
        var subscription = broker.HoldSubscription();
        var closedByBroker = new ShutdownEventArgs(ShutdownInitiator.Peer, 406, "PRECONDITION_FAILED", 50, 10);
        broker.CloseCompletion.SetException(new AlreadyClosedException(closedByBroker));
        var probe = new StopOrderProbe { Outcome = StopOrderOutcome.Succeed };
        using var host = BuildHost(broker, probe);
        await host.StartAsync(ct);
        var consumer = await broker.Consumer.Task.WaitAsync(Timeout, ct);

        var delivery = DeliverAsync(consumer, deliveryTag: 1);
        await probe.Entered.Task.WaitAsync(Timeout, ct);

        // 处理器还停在闸上，第一次会话出错（这里让订阅这一步失败；真实情形比如同一会话里下一个订阅的声明被 broker 拒绝、
        // 通道被关）。订阅端退避一秒之后建第二条通道，重新订阅。
        subscription.SetException(new OperationInterruptedException(closedByBroker));
        await broker.SecondConsumer.Task.WaitAsync(Timeout, ct);

        // 第一次会话里的处理器这时才结束。
        probe.Release.SetResult();
        await delivery.WaitAsync(Timeout, ct);

        FakeBroker.Settlements(broker.SecondChannel).Should().BeEmpty(
            "投递标签只在它到达的那条通道上有效；发到下一次会话的通道上，要么确认了别的消息，要么撞上未知标签被 RabbitMQ 关掉通道");
        broker.SettlementsAfterClose().Should().BeEmpty("第一次会话的通道已经关了");
        await host.StopAsync(ct);
    }

    // RabbitMqIntegrationEventSubscriber 的日志事件号。
    private const int ConsumeSessionFailed = 22;
    private const int ChannelAlreadyClosed = 39;
    private const int ChannelCloseFailed = 31;

    private static Task DeliverAsync(IAsyncBasicConsumer consumer, ulong deliveryTag)
    {
        var id = Guid.CreateVersion7();
        var row = EventMessages.Create(id, EventTypeId, new StopOrderEvent(id), StopOrderJsonContext.Default.StopOrderEvent);
        var properties = new BasicProperties { Type = EventTypeId, MessageId = id.ToString() };
        return consumer.HandleBasicDeliverAsync("consumer-1", deliveryTag, redelivered: false, KunEventTopology.Exchange, "integration.test.stop-order",
            properties, System.Text.Encoding.UTF8.GetBytes(row.Payload), CancellationToken.None);
    }

    private static IHost BuildHost(FakeBroker broker, StopOrderProbe probe, CapturingLoggerProvider? logs = null) =>
        Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                if (logs is not null)
                {
                    // 输出目录里的 appsettings.json 把 Logging:LogLevel:Default 定为 Information，按提供器单独放开到 Debug。
                    logging.AddProvider(logs);
                    logging.AddFilter<CapturingLoggerProvider>(null, LogLevel.Debug);
                }
            })
            .ConfigureServices(services =>
            {
                services.AddSingleton<ITenantContext>(NullTenantContext.Instance);
                services.AddSingleton<TimeProvider, ExpiredDrainClock>();
                services.AddSingleton(probe);
                services.AddSingleton<OpenConsumerConnection>((_, _) => Task.FromResult(broker.Connection));
                services.AddKunEventBus(b =>
                {
                    b.Register(EventTypeId, StopOrderJsonContext.Default.StopOrderEvent);
                    b.UseInMemoryIdempotency();
                });
                services.AddKunEventsBroker(_ => "amqp://localhost/");
                services.AddKunEventsSubscriber("stop-order", b => b.Subscribe<StopOrderEvent, StopOrderHandler>("work", acceptPlatformEvents: true));
            })
            .Build();

    /// <summary>
    /// 假连接与假通道。第一次建通道给消费通道 <see cref="Channel"/>，之后给 <see cref="SecondChannel"/>：
    /// 处理器失败时它是重试发布用的通道，会话出错重连时它是下一次会话的消费通道。
    /// </summary>
    private sealed class FakeBroker
    {
        private TaskCompletionSource<string>? _heldSubscription;

        public FakeBroker()
        {
            Connection.IsOpen.Returns(true);
            var setup = Substitute.For<IChannel>();
            Channel.IsOpen.Returns(true);
            SecondChannel.IsOpen.Returns(true);
            Connection.CreateChannelAsync(default, default).ReturnsForAnyArgs(
                _ => Task.FromResult(setup),
                _ => Task.FromResult(Channel),
                _ => Task.FromResult(setup),
                call => CreateSecondChannel(call.ArgAt<CancellationToken>(1)));
            Channel.BasicConsumeAsync(default!, default, default!, default, default, default, default!, default)
                .ReturnsForAnyArgs(call =>
                {
                    Consumer.TrySetResult(call.ArgAt<IAsyncBasicConsumer>(6));
                    return _heldSubscription?.Task ?? Task.FromResult("consumer-1");
                });
            Channel.CloseAsync(default, default!, default, default)
                .ReturnsForAnyArgs(_ =>
                {
                    CloseRequested.TrySetResult();
                    return CloseCompletion.Task;
                });
            SecondChannel.BasicConsumeAsync(default!, default, default!, default, default, default, default!, default)
                .ReturnsForAnyArgs(call =>
                {
                    SecondConsumer.TrySetResult(call.ArgAt<IAsyncBasicConsumer>(6));
                    return Task.FromResult("consumer-2");
                });
        }

        public IConnection Connection { get; } = Substitute.For<IConnection>();

        public IChannel Channel { get; } = Substitute.For<IChannel>();

        public IChannel SecondChannel { get; } = Substitute.For<IChannel>();

        public TaskCompletionSource<IAsyncBasicConsumer> Consumer { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<IAsyncBasicConsumer> SecondConsumer { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// 建重试通道时停止已开始：像真实客户端一样抛取消（客户端建通道要拿这个已取消的令牌等通道的 RPC 信号量）。
        /// </summary>
        public bool RetryChannelCancelled { get; init; }

        public TaskCompletionSource CloseRequested { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource CloseCompletion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>消费通道上设置预取就失败，会话在开始消费之前出错。</summary>
        public void FailPrefetch(Exception failure) =>
            Channel.BasicQosAsync(default, default, default, default).ReturnsForAnyArgs(Task.FromException(failure));

        /// <summary>消费通道上的订阅迟迟不完成：消费者照样登记、可以往里投递，订阅以什么结束由用例决定。</summary>
        public TaskCompletionSource<string> HoldSubscription() =>
            _heldSubscription = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>一条通道上收到的确认与拒绝。</summary>
        public static IReadOnlyList<string> Settlements(IChannel channel) =>
            [.. channel.ReceivedCalls().Select(call => call.GetMethodInfo().Name).Where(IsSettlement)];

        /// <summary>消费通道上、关通道之后收到的确认与拒绝。</summary>
        public IReadOnlyList<string> SettlementsAfterClose()
        {
            var calls = Channel.ReceivedCalls().Select(call => call.GetMethodInfo().Name).ToList();
            var close = calls.IndexOf(nameof(IChannel.CloseAsync));
            close.Should().BeGreaterThanOrEqualTo(0, "停止时要关消费通道");
            return [.. calls.Skip(close + 1).Where(IsSettlement)];
        }

        private static bool IsSettlement(string name) => name is nameof(IChannel.BasicAckAsync) or nameof(IChannel.BasicNackAsync);

        private Task<IChannel> CreateSecondChannel(CancellationToken cancellationToken)
        {
            if (!RetryChannelCancelled)
            {
                return Task.FromResult(SecondChannel);
            }

            return Task.FromException<IChannel>(new OperationCanceledException(cancellationToken));
        }
    }
}
