using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Kun.EventBus;
using Kun.EventBus.DependencyInjection;
using Kun.EventBus.Idempotency;
using Kun.EventBus.Outbox;
using Kun.EventBus.RabbitMQ;
using Kun.EventBus.RabbitMQ.Consuming;
using Kun.EventBus.RabbitMQ.DependencyInjection;
using Kun.EventBus.RabbitMQ.Options;
using Kun.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace Kun.IntegrationTests.EventBus;

internal sealed record StopWhileHandlingEvent(Guid Id);

[JsonSerializable(typeof(StopWhileHandlingEvent))]
internal sealed partial class StopWhileHandlingJsonContext : JsonSerializerContext { }

internal sealed class StopWhileHandlingProbe
{
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>订阅端的消费通道关掉了（客户端收到 close-ok 时发出）。</summary>
    public TaskCompletionSource ConsumerChannelClosed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public int Completed;
}

/// <summary>
/// 处理到一半遇上停机：等订阅端的消费通道关掉之后才把活干完。客户端收到 close-ok 时先发通道关闭事件，
/// 之后才等正在执行的处理器返回，所以这里不会与关通道互相等待。
/// </summary>
internal sealed class StopWhileHandlingHandler(StopWhileHandlingProbe probe) : IIntegrationEventHandler<StopWhileHandlingEvent>
{
    public async ValueTask HandleAsync(StopWhileHandlingEvent @event, MessageContext context, CancellationToken cancellationToken)
    {
        probe.Entered.TrySetResult();
        await probe.ConsumerChannelClosed.Task;
        Interlocked.Increment(ref probe.Completed);
    }
}

/// <summary>两个宿主共用的幂等记录；能等下一次查询，用来确认消息重新投递到了第二个宿主。</summary>
internal sealed class SharedProcessedStore : IProcessedMessageStore
{
    private readonly ConcurrentDictionary<(string Consumer, Guid MessageId), byte> _processed = new();
    private TaskCompletionSource<Guid>? _nextCheck;

    public Task<Guid> NextCheckAsync()
    {
        var signal = new TaskCompletionSource<Guid>(TaskCreationOptions.RunContinuationsAsynchronously);
        Volatile.Write(ref _nextCheck, signal);
        return signal.Task;
    }

    public bool Contains(string consumerName, Guid messageId) => _processed.ContainsKey((consumerName, messageId));

    public ValueTask<bool> IsProcessedAsync(string consumerName, Guid messageId, CancellationToken cancellationToken = default)
    {
        var processed = _processed.ContainsKey((consumerName, messageId));
        Volatile.Read(ref _nextCheck)?.TrySetResult(messageId);
        return ValueTask.FromResult(processed);
    }

    public ValueTask<bool> TryMarkProcessedAsync(string consumerName, Guid messageId, TimeSpan ttl, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(_processed.TryAdd((consumerName, messageId), 0));
}

/// <summary>
/// 真实 RabbitMQ 上的停止语义：消息处理到一半时停止宿主，处理器在消费通道关掉之后才结束。
/// 不复现竞态，只钉住结果：订阅端不再确认这条消息，连接由应用关闭（不是对端以 504 关掉）；
/// RabbitMQ 把消息重新投递给重启的消费者，按幂等记录跳过，只处理一次。
/// </summary>
public sealed class SubscriberStopWhileHandlingTests : IAsyncLifetime
{
    private const string EventTypeId = "integration.test.stop-while-handling.v1";
    private const string ConsumerName = "test-consumer-stop-while-handling";
    private const int LeftUnsettled = 36; // RabbitMqIntegrationEventSubscriber 的日志事件号
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(20);

    private readonly RabbitMqFixture _rabbitmq = new();

    public ValueTask InitializeAsync() => _rabbitmq.InitializeAsync();

    public ValueTask DisposeAsync() => _rabbitmq.DisposeAsync();

    [Fact]
    public async Task StopWhileHandling_TheApplicationClosesTheConnection_AndTheMessageIsRedeliveredAndHandledOnce()
    {
        var ct = TestContext.Current.CancellationToken;
        if (string.IsNullOrEmpty(_rabbitmq.ConnectionString))
        {
            ContainerRuntime.ReportUnavailable("Docker not available.");
        }

        var store = new SharedProcessedStore();
        var id = Guid.NewGuid();

        // 第一个宿主：消息处理到一半时停止。
        var first = new StopWhileHandlingProbe();
        var logs = new CapturingLoggerProvider();
        var shutdowns = new ConcurrentQueue<ShutdownEventArgs>();
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using (var host = BuildHost(store, first, logs, connection => connection.ConnectionShutdownAsync += (_, args) =>
               {
                   shutdowns.Enqueue(args);
                   closed.TrySetResult();
                   return Task.CompletedTask;
               }))
        {
            await host.StartAsync(ct);
            await host.Services.GetRequiredService<RabbitMqIntegrationEventSubscriber>().Ready.WaitAsync(Timeout, ct);
            await PublishAsync(host, id, ct);
            await first.Entered.Task.WaitAsync(Timeout, ct);
            await host.StopAsync(CancellationToken.None);
        }

        await closed.Task.WaitAsync(Timeout, ct);
        shutdowns.Should().OnlyContain(args => args.Initiator == ShutdownInitiator.Application && args.ReplyCode == 200,
            "停止时订阅端不能在已关的通道上确认，否则 RabbitMQ 以 504 CHANNEL_ERROR 关掉整条连接");
        first.Completed.Should().Be(1);
        store.Contains(ConsumerName, id).Should().BeTrue("处理器成功之后照常写下已处理标记");
        logs.Entries.Should().Contain(entry => entry.EventId == LeftUnsettled, "消费通道已在关，这条消息不再确认");

        // 第二个宿主：同一个消费者、同一份幂等记录。RabbitMQ 把没确认的那条重新投递过来，按幂等记录跳过。
        var redelivered = store.NextCheckAsync();
        var second = new StopWhileHandlingProbe();
        using (var host = BuildHost(store, second, logs: null, onConnected: null))
        {
            await host.StartAsync(ct);
            (await redelivered.WaitAsync(Timeout, ct)).Should().Be(id);
            await host.StopAsync(CancellationToken.None);
        }

        second.Completed.Should().Be(0, "处理过的消息重新投递时不再处理");
    }

    private static async Task PublishAsync(IHost host, Guid id, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(new StopWhileHandlingEvent(id), StopWhileHandlingJsonContext.Default.StopWhileHandlingEvent);
        await host.Services.GetRequiredService<IIntegrationEventPublisher>().PublishAsync(
            new OutboxMessage(id, EventTypeId, payload, null, null, null, DateTimeOffset.UtcNow), ct);
    }

    private IHost BuildHost(SharedProcessedStore store, StopWhileHandlingProbe probe, CapturingLoggerProvider? logs, Action<IConnection>? onConnected) =>
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
                    ConnectionString = _rabbitmq.ConnectionString,
                    ConsumerName = ConsumerName,
                }));
                services.AddSingleton(probe);
                // 先登记共用的幂等记录，UseInMemoryIdempotency 里的 TryAddSingleton 就不再登记。
                services.AddSingleton<IProcessedMessageStore>(store);
                // 真实连接外面套一层只转发的外壳，记下订阅端建的第一条通道（消费通道）。
                services.AddSingleton(sp => new RabbitMqConnectionProvider(
                    sp.GetRequiredService<IOptions<RabbitMqOptions>>(),
                    sp.GetRequiredService<ILogger<RabbitMqConnectionProvider>>(),
                    async (factory, cancellationToken) =>
                    {
                        var connection = await factory.CreateConnectionAsync(cancellationToken);
                        onConnected?.Invoke(connection);
                        return TapConsumerChannel(connection, probe);
                    }));
                services.AddKunEventBus(b =>
                {
                    b.Register<StopWhileHandlingEvent>(EventTypeId);
                    b.UseInMemoryIdempotency();
                });
                services.AddRabbitMq(
                    _ => { },
                    builder => builder.Subscribe<StopWhileHandlingEvent, StopWhileHandlingHandler>(
                        EventTypeId, StopWhileHandlingJsonContext.Default.StopWhileHandlingEvent));
            })
            .Build();

    // 订阅端、发布器与连接提供器用到的成员原样转发；第一条建出来的通道是订阅端的消费通道（发布在订阅就绪之后）。
    private static IConnection TapConsumerChannel(IConnection real, StopWhileHandlingProbe probe)
    {
        var channels = 0;
        var tapped = Substitute.For<IConnection>();
        tapped.IsOpen.Returns(_ => real.IsOpen);
        tapped.CreateChannelAsync(default, default).ReturnsForAnyArgs(async call =>
        {
            var channel = await real.CreateChannelAsync(call.ArgAt<CreateChannelOptions?>(0), call.ArgAt<CancellationToken>(1));
            if (Interlocked.Increment(ref channels) == 1)
            {
                channel.ChannelShutdownAsync += (_, _) =>
                {
                    probe.ConsumerChannelClosed.TrySetResult();
                    return Task.CompletedTask;
                };
            }

            return channel;
        });
        tapped.CloseAsync(default, default!, default, default, default).ReturnsForAnyArgs(call =>
            real.CloseAsync(call.ArgAt<ushort>(0), call.ArgAt<string>(1), call.ArgAt<TimeSpan>(2), call.ArgAt<bool>(3), call.ArgAt<CancellationToken>(4)));
        tapped.When(connection => connection.Dispose()).Do(_ => real.Dispose());
        return tapped;
    }
}
