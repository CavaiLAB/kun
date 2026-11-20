using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore;
using Kun.EventBus;
using Kun.EventBus.DependencyInjection;
using Kun.EventBus.EntityFrameworkCore;
using Kun.EventBus.Outbox;
using Kun.EventBus.RabbitMQ;
using Kun.IntegrationTests.EventBus;
using Kun.IntegrationTests.Fixtures;
using Kun.Testing.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Xunit;

namespace Kun.IntegrationTests.Outbox;

internal sealed class PublisherDbContext(DbContextOptions<PublisherDbContext> options)
    : KunDbContext(options, NullTenantContext.Instance);

public sealed class OutboxReplicasTests(PostgresFixture postgres, RabbitMq313Fixture rabbit)
    : IClassFixture<PostgresFixture>, IClassFixture<RabbitMq313Fixture>
{
    private IHost Build()
    {
        var builder = Host.CreateApplicationBuilder();
        var services = builder.Services;
        services.AddSingleton<ITenantContext>(NullTenantContext.Instance);
        services.AddDbContext<PublisherDbContext>(options => options.UseNpgsql(postgres.ConnectionString));
        services.AddKunEventBus();
        services.AddKunEventsBroker(_ => rabbit.ConnectionString);
        services.AddKunEvents(options => { options.BatchSize = 5; options.PollInterval = TimeSpan.FromMilliseconds(25); });
        services.AddKunOutboxRelay<PublisherDbContext>();
        return builder.Build();
    }

    [Fact]
    public async Task TwoPublishers_DefaultRowLocks_PublishEveryMessageOnceWithConfirms()
    {
        var ct = TestContext.Current.CancellationToken;
        using var first = Build();
        using var second = Build();
        var eventType = "replicas.test.v1";
        await using var connection = await new ConnectionFactory { Uri = new Uri(rabbit.ConnectionString) }.CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
        await channel.ExchangeDeclareAsync(KunEventTopology.Exchange, "topic", durable: true, cancellationToken: ct);
        var queue = "replicas." + Guid.NewGuid().ToString("N");
        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await channel.QueueBindAsync(queue, KunEventTopology.Exchange, IntegrationEventTypes.SubjectOf(eventType), cancellationToken: ct);
        await using (var setup = first.Services.CreateAsyncScope())
        {
            var db = setup.ServiceProvider.GetRequiredService<PublisherDbContext>();
            await db.Database.EnsureCreatedAsync(ct);
            await db.OutboxMessages.ExecuteDeleteAsync(ct);
            for (var i = 0; i < 40; i++)
            {
                var id = Guid.CreateVersion7();
                db.OutboxMessages.Add(EventMessages.Create(id, eventType, new TestEventBV1(id, "replicas"), VersioningJsonContext.Default.TestEventBV1));
            }
            await db.SaveChangesAsync(ct);
        }
        await Task.WhenAll(first.StartAsync(ct), second.StartAsync(ct));
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(TimeSpan.FromSeconds(30));
        while ((await channel.QueueDeclarePassiveAsync(queue, deadline.Token)).MessageCount != 40)
            await Task.Delay(50, deadline.Token);
        await Task.WhenAll(first.StopAsync(ct), second.StopAsync(ct));
        var delivered = new List<string>();
        while (await channel.BasicGetAsync(queue, autoAck: true, ct) is { } message)
            delivered.Add(message.BasicProperties.MessageId!);
        Assert.Equal(40, delivered.Count);
        Assert.Equal(40, delivered.Distinct().Count());
        await using var verify = first.Services.CreateAsyncScope();
        Assert.Equal(40, await verify.ServiceProvider.GetRequiredService<PublisherDbContext>().OutboxMessages
            .CountAsync(m => m.Status == OutboxStatus.Published, ct));
    }
}
