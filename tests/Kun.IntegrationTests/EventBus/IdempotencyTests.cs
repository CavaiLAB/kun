using System.Text.Json.Serialization;
using FluentAssertions;
using Kun.Core.MultiTenancy;
using Kun.EventBus;
using Kun.EventBus.DependencyInjection;
using Kun.EventBus.Idempotency;
using Kun.EventBus.RabbitMQ;
using Kun.IntegrationTests.Fixtures;
using Kun.Testing.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using StackExchange.Redis;
using Xunit;

namespace Kun.IntegrationTests.EventBus;

// ---------------------------------------------------------------------------
// Internal types — JSON source generator requires non-file-scoped visibility
// ---------------------------------------------------------------------------

internal sealed record IdempotencyTestEvent(Guid Id, string Data);

internal sealed class CountingHandler : IIntegrationEventHandler<IdempotencyTestEvent>
{
    public int InvocationCount;
    public int Attempts;
    public int FailuresRemaining;
    public TaskCompletionSource? FirstAttemptGate;

    public async ValueTask HandleAsync(IdempotencyTestEvent @event, MessageContext context, CancellationToken cancellationToken)
    {
        var attempt = Interlocked.Increment(ref Attempts);
        if (attempt == 1 && FirstAttemptGate is not null) await FirstAttemptGate.Task.WaitAsync(cancellationToken);
        if (Interlocked.Decrement(ref FailuresRemaining) >= 0) throw new IOException("临时依赖不可用");
        Interlocked.Increment(ref InvocationCount);
    }
}

[JsonSerializable(typeof(IdempotencyTestEvent))]
internal sealed partial class IdempotencyJsonContext : JsonSerializerContext { }

// ---------------------------------------------------------------------------
// Test class
// ---------------------------------------------------------------------------

public sealed class IdempotencyTests : IAsyncLifetime
{
    private readonly RabbitMq313Fixture _rabbitmq = new() { PreservePortAcrossRestarts = true };
    private readonly RedisFixture _redis = new();
    private IHost? _host;
    private CountingHandler? _handler;

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(
            _rabbitmq.InitializeAsync().AsTask(),
            _redis.InitializeAsync().AsTask());

        if (string.IsNullOrEmpty(_rabbitmq.ConnectionString) || string.IsNullOrEmpty(_redis.ConnectionString))
        {
            DockerRequirement.ReportUnavailable("Docker not available.");
            return;
        }

        _handler = new CountingHandler();
        var capturedHandler = _handler;

        _host = Host.CreateDefaultBuilder()
            .ConfigureLogging(l => l.SetMinimumLevel(LogLevel.Warning))
            .ConfigureServices(services =>
            {
                services.AddSingleton<ITenantContext>(NullTenantContext.Instance);
                services.AddSingleton(capturedHandler);
                services.AddKunEventBus(b =>
                {
                    b.Register("integration.test.idempotency.v1", IdempotencyJsonContext.Default.IdempotencyTestEvent);
                    b.UseRedisIdempotency();
                });
                services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(_redis.ConnectionString));
                services.AddKunEventsBroker(_ => _rabbitmq.ConnectionString);
                services.AddKunEventsSubscriber("idempotency", b => b.Subscribe<IdempotencyTestEvent, CountingHandler>("work", acceptPlatformEvents: true),
                    o => { o.MaxRetries = 4; o.RetryBaseDelay = TimeSpan.FromMilliseconds(100); });
            })
            .Build();

        await _host.StartAsync(TestContext.Current.CancellationToken);
        await _host.Services.GetServices<IHostedService>().OfType<KunEventSubscriber>().Single().Ready.WaitAsync(TimeSpan.FromSeconds(20), TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.StopAsync(CancellationToken.None);
            _host.Dispose();
        }

        await _rabbitmq.DisposeAsync();
        await _redis.DisposeAsync();
    }

    [Fact]
    public async Task SameMessageId_PublishedTwice_HandlerInvokedOnlyOnce()
    {
        var ct = TestContext.Current.CancellationToken;

        if (_host is null)
        {
            DockerRequirement.ReportUnavailable("Docker not available.");
            return;
        }

        var messageId = Guid.CreateVersion7();
        var publisher = _host.Services.GetRequiredService<IIntegrationEventPublisher>();
        var message = EventMessages.Create(messageId, "integration.test.idempotency.v1", new IdempotencyTestEvent(messageId, "dedup-test"), IdempotencyJsonContext.Default.IdempotencyTestEvent);

        // Publish once, wait for handler to process and mark it.
        await publisher.PublishAsync(message, ct);

        // Poll up to 10s for the first delivery to land.
        var deadline = DateTimeOffset.UtcNow.AddSeconds(10);
        while (_handler!.InvocationCount == 0 && DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(100, ct);
        }

        _handler.InvocationCount.Should().Be(1, "first delivery must invoke the handler");

        // Publish the SAME message ID a second time — idempotency store should dedup it.
        await publisher.PublishAsync(message, ct);
        await Task.Delay(TimeSpan.FromSeconds(3), ct);

        _handler.InvocationCount.Should().Be(1, "idempotency must deduplicate the second delivery");
    }
    [Fact]
    public async Task HandlerFailsOnce_OnlySuccessIsMarked_AndRetrySurvivesInBroker()
    {
        _handler!.FailuresRemaining = 1;
        var id = Guid.CreateVersion7();
        var message = EventMessages.Create(id, "integration.test.idempotency.v1", new IdempotencyTestEvent(id, "retry"), IdempotencyJsonContext.Default.IdempotencyTestEvent);
        await _host!.Services.GetRequiredService<IIntegrationEventPublisher>().PublishAsync(message, TestContext.Current.CancellationToken);
        var deadline = DateTimeOffset.UtcNow.AddSeconds(20);
        while (_handler.InvocationCount == 0 && DateTimeOffset.UtcNow < deadline)
            await Task.Delay(50, TestContext.Current.CancellationToken);
        Assert.Equal(1, _handler.InvocationCount);
        Assert.Equal(2, _handler.Attempts);
        Assert.True(await WaitUntilProcessedAsync(id, TestContext.Current.CancellationToken), ProcessedMarkMissing);
        var redis = _host.Services.GetRequiredService<IConnectionMultiplexer>().GetDatabase();
        Assert.Equal("1", (string?)await redis.StringGetAsync($"idemp:idempotency.work:{id}"));
        await _host.Services.GetRequiredService<IIntegrationEventPublisher>().PublishAsync(message, TestContext.Current.CancellationToken);
        await Task.Delay(300, TestContext.Current.CancellationToken);
        Assert.Equal(2, _handler.Attempts);
    }

    [Fact]
    public async Task BrokerRestartsWithUnacknowledgedMessage_RedeliversUntilHandlerSucceeds()
    {
        var ct = TestContext.Current.CancellationToken;
        _handler!.FailuresRemaining = 1;
        _handler.FirstAttemptGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var id = Guid.CreateVersion7();
        var message = EventMessages.Create(id, "integration.test.idempotency.v1", new IdempotencyTestEvent(id, "restart"), IdempotencyJsonContext.Default.IdempotencyTestEvent);
        await _host!.Services.GetRequiredService<IIntegrationEventPublisher>().PublishAsync(message, ct);
        var deadline = DateTimeOffset.UtcNow.AddSeconds(15);
        while (_handler.Attempts == 0 && DateTimeOffset.UtcNow < deadline) await Task.Delay(50, ct);
        Assert.Equal(1, _handler.Attempts);
        await _rabbitmq.Container.StopAsync(ct);
        _handler.FirstAttemptGate.SetResult();
        await _rabbitmq.Container.StartAsync(ct);
        Assert.Equal(new Uri(_rabbitmq.ConnectionString).Port, new Uri(_rabbitmq.Container.GetConnectionString()).Port);
        deadline = DateTimeOffset.UtcNow.AddSeconds(30);
        while (_handler.InvocationCount == 0 && DateTimeOffset.UtcNow < deadline) await Task.Delay(100, ct);
        Assert.Equal(1, _handler.InvocationCount);
        Assert.Equal(2, _handler.Attempts);
        Assert.True(await WaitUntilProcessedAsync(id, ct), ProcessedMarkMissing);
    }

    [Fact]
    public async Task HandlerAlwaysFails_RetriesBoundedlyThenConfirmsDeadLetter_WithoutSuccessMark()
    {
        var ct = TestContext.Current.CancellationToken;
        _handler!.FailuresRemaining = 100;
        await _host!.Services.GetServices<IHostedService>().OfType<KunEventSubscriber>().Single().Ready.WaitAsync(TimeSpan.FromSeconds(20), ct);
        var id = Guid.CreateVersion7();
        var message = EventMessages.Create(id, "integration.test.idempotency.v1", new IdempotencyTestEvent(id, "poison"), IdempotencyJsonContext.Default.IdempotencyTestEvent);
        await _host.Services.GetRequiredService<IIntegrationEventPublisher>().PublishAsync(message, ct);
        await using var connection = await new ConnectionFactory { Uri = new Uri(_rabbitmq.ConnectionString) }.CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
        BasicGetResult? dead = null;
        var deadline = DateTimeOffset.UtcNow.AddSeconds(20);
        while (dead is null && DateTimeOffset.UtcNow < deadline)
        {
            dead = await channel.BasicGetAsync(KunEventTopology.DeadLetterQueue("idempotency", "work"), autoAck: true, ct);
            if (dead is null) await Task.Delay(100, ct);
        }
        Assert.NotNull(dead);
        Assert.Equal(id.ToString(), dead.BasicProperties.MessageId);
        Assert.Equal(5, _handler.Attempts);
        Assert.Equal(0, _handler.InvocationCount);
        Assert.False(await _host.Services.GetRequiredService<IProcessedMessageStore>().IsProcessedAsync("idempotency.work", id, ct));
    }

    private const string ProcessedMarkMissing = "处理器成功之后 10 秒内没有写下已处理标记";

    // 处理器里的计数一到，处理器才刚要返回：已处理标记是 IdempotentConsumerDecorator 在处理器返回之后才写的。
    // 有上限地等标记出现，等不到才算失败；标记真的不写，用例照样失败。
    private async Task<bool> WaitUntilProcessedAsync(Guid id, CancellationToken ct)
    {
        var store = _host!.Services.GetRequiredService<IProcessedMessageStore>();
        var deadline = DateTimeOffset.UtcNow.AddSeconds(10);
        while (!await store.IsProcessedAsync("idempotency.work", id, ct))
        {
            if (DateTimeOffset.UtcNow >= deadline) return false;
            await Task.Delay(50, ct);
        }
        return true;
    }
}
