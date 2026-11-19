using System.Text.Json.Serialization;
using FluentAssertions;
using Kun.Core.MultiTenancy;
using Kun.EventBus;
using Kun.EventBus.DependencyInjection;
using Kun.EventBus.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Kun.IntegrationTests.EventBus;

internal sealed record ResilienceProbeEvent(Guid Id);

internal sealed class ResilienceProbeHandler : IIntegrationEventHandler<ResilienceProbeEvent>
{
    public ValueTask HandleAsync(ResilienceProbeEvent @event, MessageContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}

[JsonSerializable(typeof(ResilienceProbeEvent))]
internal sealed partial class ResilienceProbeJsonContext : JsonSerializerContext { }

/// <summary>
/// An unreachable broker must never fault the subscriber's ExecuteAsync — the
/// .NET host default (BackgroundServiceExceptionBehavior.StopHost) would stop
/// the entire consumer host with it. No container needed: the point is the
/// retry loop, not a successful connection.
/// </summary>
public sealed class SubscriberResilienceTests
{
    [Fact]
    public async Task UnreachableBroker_KeepsRetrying_WithoutFaultingTheHost()
    {
        var ct = TestContext.Current.CancellationToken;

        using var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(l => l.SetMinimumLevel(LogLevel.Critical))
            .ConfigureServices(services =>
            {
                services.AddSingleton<ITenantContext>(NullTenantContext.Instance);
                services.AddKunEventBus(b =>
                {
                    b.Register("integration.test.resilience-probe.v1", ResilienceProbeJsonContext.Default.ResilienceProbeEvent);
                    b.UseInMemoryIdempotency();
                });
                services.AddKunEventsBroker(_ => "amqp://127.0.0.1:1/");
                services.AddKunEventsSubscriber("resilience-probe", b => b.Subscribe<ResilienceProbeEvent, ResilienceProbeHandler>("work"));
            })
            .Build();

        await host.StartAsync(ct);

        // Give the subscriber time for the initial connect failure plus at
        // least one backoff retry.
        await Task.Delay(TimeSpan.FromSeconds(3.5), ct);

        var subscriber = host.Services.GetServices<IHostedService>().OfType<KunEventSubscriber>().Single();
        subscriber.ExecuteTask.Should().NotBeNull();
        subscriber.ExecuteTask!.IsFaulted.Should().BeFalse(
            "an unreachable broker must be retried, not fault the BackgroundService (StopHost)");
        host.Services.GetRequiredService<IHostApplicationLifetime>()
            .ApplicationStopping.IsCancellationRequested.Should().BeFalse();

        // And the loop must exit promptly on shutdown.
        await host.StopAsync(ct);
    }
}
