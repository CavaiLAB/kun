using System.Diagnostics;
using Kun.AspNetCore.Endpoints;
using Kun.Audit.DependencyInjection;
using Kun.Core.Domain;
using Kun.EntityFrameworkCore;
using Kun.EntityFrameworkCore.Behaviors;
using Kun.EntityFrameworkCore.DependencyInjection;
using Kun.EntityFrameworkCore.Extensions;
using Kun.Errors.AspNetCore;
using Kun.EventBus.DependencyInjection;
using Kun.Grpc.DependencyInjection;
using Kun.Mediator.DependencyInjection;
using Kun.MultiTenancy.DependencyInjection;
using Kun.Observability.Enrichment;
using Kun.Observability.Extensions;
using Kun.Sample.Catalog;
using Kun.Sample.Catalog.Application.Commands;
using Kun.Sample.Catalog.Application.DomainEventHandlers;
using Kun.Sample.Catalog.Application.Queries;
using Kun.Sample.Catalog.Contracts;
using Kun.Sample.Catalog.Grpc;
using Kun.Sample.Catalog.Infrastructure;
using Kun.Security.Authentication;
using Kun.Validation.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ── OpenTelemetry (traces + metrics + logs via OTLP, with baggage enrichment) ─
builder.Services.AddKunObservability(o =>
{
    o.ServiceName = "catalog";
    o.EnrichActivitiesFromBaggage = true;
    o.EnrichLogsFromBaggage = true;
});

// ── Connection strings ────────────────────────────────────────────────────────
var writeCs = builder.Configuration.GetConnectionString("CatalogDb")
    ?? "Host=localhost;Port=5432;Database=catalog;Username=postgres;Password=postgres";

var readCs = builder.Configuration.GetConnectionString("CatalogDbRead") ?? writeCs;

// ── Structured errors (problem+json ErrorDetail; validation failures → INPUT_INVALID) ─
builder.Services.AddKunErrors();

// ── Tenant context for authenticated service calls ───────────────────────
builder.Services.AddKunMultiTenancy();

// ── Domain event dispatcher (must be registered before AddKunEntityFrameworkCore
//    so TryAddScoped does not overwrite the sample-specific dispatcher) ──────────
builder.Services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

// ── EF Core (write side) ─────────────────────────────────────────────────────
// Register CatalogDbContext so it can be injected directly by command handlers.
builder.Services.AddDbContext<CatalogDbContext>(opt =>
    opt.UseNpgsql(writeCs)
       .UseKunSchema("catalog"));

// EfUnitOfWork resolves KunDbContext, so forward the scoped registration.
builder.Services.AddScoped<KunDbContext>(sp => sp.GetRequiredService<CatalogDbContext>());

builder.Services.AddKunEntityFrameworkCore();

// ── Read side: Npgsql data source + Dapper ────────────────────────────────────
// Query handlers open connections from this data source and run SQL with Dapper.
// The container disposes the data source on shutdown.
builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(readCs));

// ── Event bus + Outbox ────────────────────────────────────────────────────────
builder.Services.AddKunEventBus(eb =>
{
    eb.Register("product.published.v1", CatalogEventJson.Default.ProductPublishedIntegrationEvent);
    eb.Register("inventory.changed.v1", CatalogEventJson.Default.InventoryChangedIntegrationEvent);
    eb.UseInMemoryIdempotency();
});

// ── Mediator ──────────────────────────────────────────────────────────────────
builder.Services.AddKunMediator(m =>
{
    // Validation behavior runs before the handler; UoW behavior commits after.
    // Behaviors are applied outermost-first: Validation → UoW → Handler.
    m.AddPipelineBehavior<CreateProductCommand, Guid,
        Kun.Validation.Behaviors.ValidationPipelineBehavior<CreateProductCommand, Guid>>();
    m.AddPipelineBehavior<CreateProductCommand, Guid,
        UnitOfWorkPipelineBehavior<CreateProductCommand, Guid>>();

    m.AddRequestHandler<CreateProductCommand, Guid, CreateProductCommandHandler>();

    m.AddPipelineBehavior<AdjustStockCommand, Kun.Core.Results.Result,
        UnitOfWorkPipelineBehavior<AdjustStockCommand, Kun.Core.Results.Result>>();

    m.AddRequestHandler<AdjustStockCommand, Kun.Core.Results.Result, AdjustStockCommandHandler>();

    m.AddRequestHandler<GetProductByIdQuery, ProductDto?, GetProductByIdQueryHandler>();

    // Domain event notification handler
    m.AddNotificationHandler<ProductCreatedNotification, ProductCreatedHandler>();
});

// Validator for CreateProductCommand
builder.Services.AddValidator<CreateProductCommand, CreateProductCommandValidator>();

// ── Audit (Kun.Audit) ─────────────────────────────────────────────────────────
// Wires IAuditWriter with the NullAuditWriter fallback. Call
// .UseEventBus() to route audit entries through the Outbox, or register
// Kun.Audit.EntityFrameworkCore to persist them to a dedicated table.
// This sample sticks with the null backend so the minimal wiring shape
// is visible without a schema migration.
builder.Services.AddKunAudit();

// ── gRPC ──────────────────────────────────────────────────────────────────────
builder.Services.AddKunGrpc();
if (builder.Configuration["CatalogAuth:PublicJwksPath"] is { Length: > 0 } jwksPath)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddKunServiceAuthentication(options =>
        {
            options.Issuer = "kun-sample";
            options.Audience = "kun-sample-catalog";
            options.StaticKeySet = new JsonWebKeySet(File.ReadAllText(jwksPath));
        });
}

// ── OpenAPI (ASP.NET Core 10 built-in, source-gen-friendly) ──────────────────
// Exposed at /openapi/v1.json; gated by environment below so production hosts
// don't leak the document without an explicit opt-in.
builder.Services.AddOpenApi();

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseKunErrors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ── Database initialisation (apply EF migrations on startup) ─────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.MigrateAsync();
}

// ── Endpoints ─────────────────────────────────────────────────────────────────
// MapCommand/MapQuery require JsonTypeInfo for AOT-safe serialisation.
// This sample is NOT AOT (no IsAotCompatible / PublishAot) but we still supply a
// JsonSerializerContext to satisfy the method signature.
app.MapCommand<CreateProductCommand, Guid>(
    "/products",
    CatalogJsonContext.Default.CreateProductCommand,
    CatalogJsonContext.Default.Guid);

// MapQuery cannot express JsonTypeInfo<ProductDto?> via source-gen attributes,
// so this endpoint is wired manually using ISender directly.
app.MapGet("/products/{id:guid}", async (Guid id, Kun.Mediator.ISender sender, CancellationToken ct) =>
{
    var dto = await sender.Send(new GetProductByIdQuery(id), ct);
    return dto is null ? Results.NotFound() : Results.Ok(dto);
});

// Demo: set tenant.id + user.id baggage so Kun.Observability's activity enricher
// mirrors them onto span tags — visible in the Aspire Dashboard trace view.
// Real services populate these from the S2S JWT claims in a middleware.
app.MapGet("/health", () =>
{
    Activity.Current?.SetBaggage(KunBaggageKeys.TenantId, "01900000-0000-7000-8000-000000000001");
    Activity.Current?.SetBaggage(KunBaggageKeys.UserId, "demo-user-0001");
    return Results.Ok(new { status = "healthy" });
});

app.MapGrpcService<CatalogGrpcService>();

app.Run();

/// <summary>Test affordance — exposes the top-level Program for WebApplicationFactory.</summary>
public partial class Program { }

