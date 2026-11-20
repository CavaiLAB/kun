using Kun.Core.Time;
using Kun.EntityFrameworkCore;
using Kun.EntityFrameworkCore.Behaviors;
using Kun.EntityFrameworkCore.DependencyInjection;
using Kun.EntityFrameworkCore.Extensions;
using Kun.Errors.AspNetCore;
using Kun.EventBus;
using Kun.EventBus.DependencyInjection;
using Kun.EventBus.EntityFrameworkCore;
using Kun.Mediator.DependencyInjection;
using Kun.MultiTenancy.DependencyInjection;
using Kun.Sample.Order.Api.Endpoints;
using Kun.Sample.Order.Application.Commands;
using Kun.Sample.Order.Application.Queries;
using Kun.Sample.Order.Contracts;
using Kun.Sample.Order.Infrastructure;
using Kun.Sample.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── OpenTelemetry (traces + metrics via OTLP) ─────────────────────────────────
builder.Services.AddKunObservability("order");

// ── Logging ──────────────────────────────────────────────────────────────────
builder.Logging.AddConsole();

// ── Core infrastructure ───────────────────────────────────────────────────────
builder.Services.AddSingleton<ISystemClock>(SystemClock.Instance);
// Service calls can open a tenant scope from their authenticated identity.
builder.Services.AddKunMultiTenancy();

// ── EF Core (write side) ──────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("OrderDb")
    ?? "Host=localhost;Database=order_sample;Username=postgres;Password=postgres";

builder.Services.AddDbContext<OrderDbContext>(opt =>
    opt.UseNpgsql(connectionString)
       .UseKunSchema("order"));

// Register OrderDbContext as KunDbContext so framework infrastructure can resolve it.
builder.Services.AddScoped<KunDbContext>(sp => sp.GetRequiredService<OrderDbContext>());

builder.Services.AddKunEntityFrameworkCore();

// ── Event Bus + Outbox Publisher ──────────────────────────────────────────────
builder.Services.AddKunEventBus(eb =>
{
    eb.Register("order.completed.v1", OrderEventJson.Default.OrderCompletedIntegrationEvent);
    eb.UseInMemoryIdempotency();
});

// The sample logs confirmed publications locally. Configure the RabbitMQ broker in a messaging host.
builder.Services.AddSingleton<IIntegrationEventPublisher, NoOpIntegrationEventPublisher>();
builder.Services.AddKunOutboxRelay<OrderDbContext>();

// ── Catalog gRPC client ───────────────────────────────────────────────────────
// PlaceOrderCommandHandler reserves stock through this client.
var catalogClient = builder.Services
    .AddGrpcClient<Kun.Sample.Catalog.Grpc.CatalogService.CatalogServiceClient>(o =>
    {
        // Aspire service discovery: services__catalog__grpc__0 / services__catalog__http__0 are
        // injected by the AppHost when running under .NET Aspire.
        // For standalone runs, fall back to CatalogGrpc:Address from appsettings.json.
        var catalogUrl = builder.Configuration["services:catalog:grpc:0"]
            ?? builder.Configuration["services:catalog:http:0"]
            ?? builder.Configuration["CatalogGrpc:Address"]
            ?? "http://localhost:5001";
        o.Address = new Uri(catalogUrl);
    });

if (builder.Configuration["CatalogGrpc:TokenPath"] is { Length: > 0 } tokenPath)
{
    // gRPC call credentials are sent over TLS. Read the short-lived token again on each call.
    catalogClient.AddCallCredentials(async (context, metadata) =>
    {
        var token = await File.ReadAllTextAsync(tokenPath, context.CancellationToken);
        metadata.Add("Authorization", $"Bearer {token.Trim()}");
    });
}

// ── Mediator ──────────────────────────────────────────────────────────────────
builder.Services.AddKunMediator(m =>
{
    // Commands
    m.AddRequestHandler<PlaceOrderCommand, Guid, PlaceOrderCommandHandler>();
    m.AddPipelineBehavior<PlaceOrderCommand, Guid, UnitOfWorkPipelineBehavior<PlaceOrderCommand, Guid>>();

    // Queries
    m.AddRequestHandler<GetOrderByIdQuery, OrderDto?, GetOrderByIdQueryHandler>();
});

// ── ASP.NET Core ──────────────────────────────────────────────────────────────
builder.Services.AddKunErrors();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ── Database bootstrap (apply EF migrations on startup) ──────────────────────
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.MigrateAsync();
}

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseKunErrors();

// ── Endpoints ─────────────────────────────────────────────────────────────────
app.MapDemoEndpoints();

app.MapGet("/", static () => Results.Ok(new
{
    service = "Kun.Sample.Order",
    description = "Order command flow demo: gRPC stock reservation + Outbox",
    endpoints = DemoRoutes.All,
}));

// ── Route manifest (static readonly avoids CA1861 on repeated array allocation) ─

app.Run();

/// <summary>Test affordance — exposes the top-level Program for WebApplicationFactory.</summary>
public partial class Program { }

// ── Route manifest ────────────────────────────────────────────────────────────
file static class DemoRoutes
{
    internal static readonly string[] All =
    [
        "POST /demo/orders",
        "GET  /demo/orders/{id}",
    ];
}

// ── No-op integration event publisher (sample only — no RabbitMQ) ────────────
file sealed class NoOpIntegrationEventPublisher : IIntegrationEventPublisher
{
    public ValueTask PublishAsync(
        Kun.EventBus.Outbox.OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        // In production replace with RabbitMQ publisher from Kun.EventBus.RabbitMQ.
        Console.WriteLine($"[Outbox] Would publish event '{message.EventTypeId}' (no-op in sample).");
        return ValueTask.CompletedTask;
    }
}
