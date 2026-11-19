using System.Diagnostics;
using Kun.Core.MultiTenancy;
using Kun.EventBus;
using Kun.EventBus.DependencyInjection;
using Kun.EventBus.EntityFrameworkCore;
using Kun.EventBus.Outbox;
using Kun.IntegrationTests.Fixtures;
using Kun.IntegrationTests.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Kun.IntegrationTests.EventBus;

public sealed class OutboxPublisherResilienceTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task OrdinaryContext_RetriesAndPublishesWithOriginalTrace()
    {
        var ct = TestContext.Current.CancellationToken;
        var builder = Host.CreateApplicationBuilder();
        var services = builder.Services;
        var probe = new FlakyPublisher();
        services.AddSingleton<ITenantContext>(NullTenantContext.Instance);
        services.AddDbContext<PublisherDbContext>(o => o.UseNpgsql(postgres.ConnectionString));
        services.AddKunEventBus();
        services.AddSingleton<IIntegrationEventPublisher>(probe);
        services.AddKunEvents(o =>
        {
            o.PollInterval = TimeSpan.FromMilliseconds(25);
            o.RetryBaseDelay = TimeSpan.FromMilliseconds(50);
        });
        services.AddKunOutboxRelay<PublisherDbContext>();
        services.AddKunOutboxRelay<PublisherDbContext>();
        using var host = builder.Build();
        var id = Guid.CreateVersion7();
        var row = EventMessages.Create(id, "tests.event.v1", new TestEventBV1(id, "retry"), VersioningJsonContext.Default.TestEventBV1);
        await using var scope = host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PublisherDbContext>();
        await db.Database.EnsureCreatedAsync(ct);
        db.OutboxMessages.Add(row);
        await db.SaveChangesAsync(ct);
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Kun.EventBus.EntityFrameworkCore",
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(TimeSpan.FromSeconds(20));
        await host.StartAsync(ct);
        while (await db.OutboxMessages.AsNoTracking().SingleAsync(m => m.Id == id, deadline.Token) is { Status: OutboxStatus.Pending })
            await Task.Delay(25, deadline.Token);
        await host.StopAsync(ct);
        var persisted = await db.OutboxMessages.AsNoTracking().SingleAsync(m => m.Id == id, ct);
        Assert.Equal(OutboxStatus.Published, persisted.Status);
        Assert.Equal(1, persisted.AttemptCount);
        Assert.Equal(2, probe.Attempts);
        Assert.Equal("outbox.publish", probe.Activity?.OperationName);
        Assert.Equal(ActivityKind.Producer, probe.Activity?.Kind);
        Assert.Equal(row.TraceParent, probe.Activity?.ParentId);
    }

    private sealed class FlakyPublisher : IIntegrationEventPublisher
    {
        public int Attempts { get; private set; }
        public Activity? Activity { get; private set; }

        public ValueTask PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            Activity = System.Diagnostics.Activity.Current;
            if (++Attempts == 1) throw new IOException("temporary broker failure");
            return ValueTask.CompletedTask;
        }
    }
}
