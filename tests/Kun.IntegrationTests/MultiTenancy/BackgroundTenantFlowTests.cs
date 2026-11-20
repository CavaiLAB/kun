using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Kun.Core.Domain;
using Kun.Core.MultiTenancy;
using Kun.EntityFrameworkCore;
using Kun.EntityFrameworkCore.DependencyInjection;
using Kun.EntityFrameworkCore.MultiTenancy;
using Kun.EventBus;
using Kun.EventBus.DependencyInjection;
using Kun.EventBus.RabbitMQ;
using Kun.IntegrationTests.EventBus;
using Kun.IntegrationTests.Fixtures;
using Kun.MultiTenancy.DependencyInjection;
using Kun.Testing.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using RabbitMQ.Client;
using Xunit;

namespace Kun.IntegrationTests.MultiTenancy;

internal sealed class TenantProbe : IMultiTenant
{
    public int Id { get; set; }
    public string? TenantId { get; set; }
}

internal sealed class TenantProbeDbContext(DbContextOptions<TenantProbeDbContext> options, ITenantContext tenant)
    : KunDbContext(options, tenant)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantProbe>(entity =>
        {
            entity.ToTable("tenant_probe");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
        });
        base.OnModelCreating(modelBuilder);
    }
}

internal sealed record TenantProbeEvent(Guid Id);
internal sealed record TenantObservation(string? EnvelopeTenant, string? ConstructorTenant, string? HandlerTenant,
    string Sql, string?[] EfTenants, string?[] RlsTenants, string? TransactionTenant, bool CacheFactory);

internal sealed class TenantProbeHandler : IIntegrationEventHandler<TenantProbeEvent>
{
    private readonly ITenantContext _tenant;
    private readonly TenantProbeDbContext _db;
    private readonly string? _constructorTenant;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<TenantObservation>> _received;

    public TenantProbeHandler(ITenantContext tenant, TenantProbeDbContext db,
        ConcurrentDictionary<Guid, TaskCompletionSource<TenantObservation>> received)
    {
        _tenant = tenant;
        _db = db;
        _constructorTenant = tenant.TenantId?.ToString("D");
        _received = received;
    }

    public async ValueTask HandleAsync(TenantProbeEvent @event, MessageContext context, CancellationToken cancellationToken)
    {
        try
        {
            var query = _db.Set<TenantProbe>().OrderBy(x => x.Id);
            var sql = query.ToQueryString();
            var efTenants = await query.Select(x => x.TenantId).ToArrayAsync(cancellationToken);
            // 绕过 EF 全局过滤器，单独验证连接版 RLS。
            var rlsTenants = await _db.Set<TenantProbe>().IgnoreQueryFilters()
                .Select(x => x.TenantId).ToArrayAsync(cancellationToken);
            await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
            var transactionTenant = await _db.Database.SqlQueryRaw<string>(
                "SELECT current_setting('app.current_tenant_id', true) AS \"Value\"").SingleAsync(cancellationToken);
            _received[@event.Id].TrySetResult(new TenantObservation(context.TenantId, _constructorTenant,
                _tenant.TenantId?.ToString("D"), sql, efTenants, rlsTenants, transactionTenant,
                _db.GetService<IModelCacheKeyFactory>() is KunMultiTenantModelCacheKeyFactory));
        }
        catch (Exception ex)
        {
            _received[@event.Id].TrySetException(ex);
            throw;
        }
    }
}

[JsonSerializable(typeof(TenantProbeEvent))]
internal sealed partial class TenantProbeJsonContext : JsonSerializerContext;

public sealed class BackgroundTenantFlowTests : IAsyncLifetime
{
    private readonly PostgresFixture _postgres = new();
    private readonly RabbitMq313Fixture _rabbit = new();
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<TenantObservation>> _received = new();
    private IHost _host = null!;
    private const string EventType = "tenant.probe.v1";
    private const string Consumer = "tenant-probe";

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_postgres.InitializeAsync().AsTask(), _rabbit.InitializeAsync().AsTask());
        await using var connection = new NpgsqlConnection(_postgres.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new NpgsqlCommand($"""
            CREATE TABLE tenant_probe (id int PRIMARY KEY, tenant_id text NOT NULL);
            INSERT INTO tenant_probe VALUES (1, '{Tenants.AText}'), (2, '{Tenants.BText}');
            ALTER TABLE tenant_probe ENABLE ROW LEVEL SECURITY;
            ALTER TABLE tenant_probe FORCE ROW LEVEL SECURITY;
            CREATE POLICY tenant_probe_scope ON tenant_probe
              USING (tenant_id = current_setting('app.current_tenant_id', true))
              WITH CHECK (tenant_id = current_setting('app.current_tenant_id', true));
            """, connection);
        await command.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        _host = Host.CreateDefaultBuilder().ConfigureServices(services =>
        {
            services.AddKunMultiTenancy();
            services.AddSingleton(_received);
            services.AddDbContext<TenantProbeDbContext>(options => options.UseNpgsql(_postgres.AppConnectionString));
            services.AddKunTenantRls<TenantProbeDbContext>();
            services.AddKunEventBus(bus =>
            {
                bus.Register(EventType, TenantProbeJsonContext.Default.TenantProbeEvent);
                bus.UseInMemoryIdempotency();
            });
            services.AddKunEventsBroker(_ => _rabbit.ConnectionString);
            services.AddKunEventsSubscriber(Consumer, bus => bus.Subscribe<TenantProbeEvent, TenantProbeHandler>("probe"));
        }).Build();
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
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _rabbit.DisposeAsync().AsTask());
    }

    [Fact]
    public async Task Publish_TwoTenants_ConsumerRestoresScopeAndEfAndRlsIsolation()
    {
        foreach (var tenantId in new[] { Tenants.AText, Tenants.BText, Tenants.AText })
        {
            var id = Guid.CreateVersion7();
            var completion = new TaskCompletionSource<TenantObservation>(TaskCreationOptions.RunContinuationsAsynchronously);
            _received[id] = completion;
            var message = EventMessages.Create(id, EventType, new TenantProbeEvent(id), TenantProbeJsonContext.Default.TenantProbeEvent, tenantId);
            await _host.Services.GetRequiredService<IIntegrationEventPublisher>()
                .PublishAsync(message, TestContext.Current.CancellationToken);
            var observed = await completion.Task.WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);
            Assert.Equal(tenantId, observed.EnvelopeTenant);
            Assert.Equal(tenantId, observed.ConstructorTenant);
            Assert.Equal(tenantId, observed.HandlerTenant);
            Assert.Contains("WHERE", observed.Sql, StringComparison.Ordinal);
            Assert.Contains("tenant_id", observed.Sql, StringComparison.Ordinal);
            Assert.Equal(tenantId, Assert.Single(observed.EfTenants));
            Assert.Equal(tenantId, Assert.Single(observed.RlsTenants));
            Assert.Equal(tenantId, observed.TransactionTenant);
            Assert.True(observed.CacheFactory);
        }
        Assert.Null(_host.Services.GetRequiredService<ITenantContext>().TenantId);
    }

    [Fact]
    public async Task PlatformEvent_IsDeadLetteredBeforeTenantHandler()
    {
        var id = Guid.CreateVersion7();
        var completion = new TaskCompletionSource<TenantObservation>(TaskCreationOptions.RunContinuationsAsynchronously);
        _received[id] = completion;
        var message = EventMessages.Create(id, EventType, new TenantProbeEvent(id), TenantProbeJsonContext.Default.TenantProbeEvent);
        await _host.Services.GetRequiredService<IIntegrationEventPublisher>().PublishAsync(message, TestContext.Current.CancellationToken);
        await using var broker = await new ConnectionFactory { Uri = new Uri(_rabbit.ConnectionString) }.CreateConnectionAsync(TestContext.Current.CancellationToken);
        await using var channel = await broker.CreateChannelAsync(cancellationToken: TestContext.Current.CancellationToken);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        BasicGetResult? rejected;
        while ((rejected = await channel.BasicGetAsync(KunEventTopology.DeadLetterQueue(Consumer, "probe"), true, timeout.Token)) is null)
            await Task.Delay(50, timeout.Token);
        Assert.Equal(id.ToString(), rejected.BasicProperties.MessageId);
        Assert.False(completion.Task.IsCompleted);
        Assert.Null(_host.Services.GetRequiredService<ITenantContext>().TenantId);
    }
}
