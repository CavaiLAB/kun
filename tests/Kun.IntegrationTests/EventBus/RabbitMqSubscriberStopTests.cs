using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Kun.EventBus;
using Kun.EventBus.DependencyInjection;
using Kun.EventBus.RabbitMQ;
using Kun.EventBus.RabbitMQ.Consuming;
using Kun.EventBus.RabbitMQ.DependencyInjection;
using Kun.EventBus.RabbitMQ.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    /// <summary>抛出异常，订阅端走重试或死信，再确认原消息。</summary>
    Fail,

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
                throw new InvalidOperationException("处理失败");
            case StopOrderOutcome.HonourCancellation:
                cancellationToken.ThrowIfCancellationRequested();
                break;
        }
    }
}

/// <summary>
/// 订阅端停止时的确认顺序，用假连接、假通道，不起 RabbitMQ，不靠时序：用例自己投递消息、
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
    [InlineData(StopOrderOutcome.HonourCancellation)]
    public async Task Stop_WhileADeliveryIsBeingHandled_NothingIsSettledAfterTheChannelClose(StopOrderOutcome outcome)
    {
        var ct = TestContext.Current.CancellationToken;
        var broker = new FakeBroker();
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
        broker.Channel.Received(1).Dispose();
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

        broker.Channel.Received(1).Dispose();
        host.Services.GetRequiredService<RabbitMqIntegrationEventSubscriber>().ExecuteTask!.IsCompletedSuccessfully.Should().BeTrue();
        logs.Entries.Should().NotContain(entry => entry.EventId == ConsumeSessionFailed,
            "连接已被对端关掉时关通道，不算消费会话失败");
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
        broker.Channel.Received(1).Dispose();
    }

    // RabbitMqIntegrationEventSubscriber 的日志事件号。
    private const int ConsumeSessionFailed = 35;
    private const int ChannelCloseFailed = 38;

    private static Task DeliverAsync(IAsyncBasicConsumer consumer, ulong deliveryTag)
    {
        var id = Guid.NewGuid();
        var properties = new BasicProperties { Type = EventTypeId, MessageId = id.ToString() };
        var body = JsonSerializer.SerializeToUtf8Bytes(new StopOrderEvent(id), StopOrderJsonContext.Default.StopOrderEvent);
        return consumer.HandleBasicDeliverAsync("consumer-1", deliveryTag, redelivered: false, "kun.events", EventTypeId,
            properties, body, CancellationToken.None);
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
                services.AddSingleton<IOptions<RabbitMqOptions>>(Options.Create(new RabbitMqOptions
                {
                    ConnectionString = "amqp://localhost:5672/",
                    ConsumerName = "stop-order",
                }));
                services.AddSingleton(probe);
                // 先登记连接提供器，AddRabbitMq 里的 TryAddSingleton 就不再登记默认的那个。
                services.AddSingleton(sp => new RabbitMqConnectionProvider(
                    sp.GetRequiredService<IOptions<RabbitMqOptions>>(),
                    sp.GetRequiredService<ILogger<RabbitMqConnectionProvider>>(),
                    (_, _) => Task.FromResult(broker.Connection)));
                services.AddKunEventBus(b =>
                {
                    b.Register<StopOrderEvent>(EventTypeId);
                    b.UseInMemoryIdempotency();
                });
                services.AddRabbitMq(
                    _ => { },
                    builder => builder.Subscribe<StopOrderEvent, StopOrderHandler>(EventTypeId, StopOrderJsonContext.Default.StopOrderEvent));
            })
            .Build();

    /// <summary>假连接与假通道。第一次建通道给消费通道，之后（重试发布）给另一条通道。</summary>
    private sealed class FakeBroker
    {
        public FakeBroker()
        {
            Connection.IsOpen.Returns(true);
            Connection.CreateChannelAsync(default, default).ReturnsForAnyArgs(Task.FromResult(Channel), Task.FromResult(RetryChannel));
            Channel.BasicConsumeAsync(default!, default, default!, default, default, default, default!, default)
                .ReturnsForAnyArgs(call =>
                {
                    Consumer.TrySetResult(call.ArgAt<IAsyncBasicConsumer>(6));
                    return Task.FromResult("consumer-1");
                });
            Channel.CloseAsync(default, default!, default, default)
                .ReturnsForAnyArgs(_ =>
                {
                    CloseRequested.TrySetResult();
                    return CloseCompletion.Task;
                });
        }

        public IConnection Connection { get; } = Substitute.For<IConnection>();

        public IChannel Channel { get; } = Substitute.For<IChannel>();

        public IChannel RetryChannel { get; } = Substitute.For<IChannel>();

        public TaskCompletionSource<IAsyncBasicConsumer> Consumer { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource CloseRequested { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource CloseCompletion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>消费通道上设置预取就失败，会话在开始消费之前出错。</summary>
        public void FailPrefetch(Exception failure) =>
            Channel.BasicQosAsync(default, default, default, default).ReturnsForAnyArgs(Task.FromException(failure));

        /// <summary>消费通道上、关通道之后收到的确认与拒绝。</summary>
        public IReadOnlyList<string> SettlementsAfterClose()
        {
            var calls = Channel.ReceivedCalls().Select(call => call.GetMethodInfo().Name).ToList();
            var close = calls.IndexOf(nameof(IChannel.CloseAsync));
            close.Should().BeGreaterThanOrEqualTo(0, "停止时要关消费通道");
            return [.. calls.Skip(close + 1).Where(name => name is nameof(IChannel.BasicAckAsync) or nameof(IChannel.BasicNackAsync))];
        }
    }
}
