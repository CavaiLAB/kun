extern alias OrderApp;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Grpc.Core;
using Kun.Sample.Tests.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
// The gRPC client code (CatalogService.CatalogServiceClient) is generated into the Order
// assembly (GrpcServices="Client"), not the Catalog assembly (GrpcServices="Server").
using OrderApp::Kun.Sample.Catalog.Grpc;
using Xunit;
using OrderDbContext = OrderApp::Kun.Sample.Order.Infrastructure.OrderDbContext;

namespace Kun.Sample.Tests;

// ── Limitations documented ────────────────────────────────────────────────────
// PlaceOrderCommandHandler reserves stock with a live gRPC call to the Catalog
// service. Running both WebApplicationFactory<Catalog> and WebApplicationFactory<Order>
// simultaneously in the same process is feasible but requires wiring the Order
// factory's gRPC client address to the in-process Catalog test server, which requires
// HTTP/2 h2c support in the test host and cross-process port discovery.
//
// Chosen approach: replace CatalogService.CatalogServiceClient with a fake backed
// by a no-op CallInvoker so there is no actual network call. This keeps tests fast,
// deterministic, and free of inter-service networking while still exercising the
// command handler, the unit of work, the Outbox write, and DB persistence.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A <see cref="CallInvoker"/> that never makes network calls.
/// Used only as a base-class constructor argument — the overridden methods are never reached.
/// </summary>
internal sealed class NullCallInvoker : CallInvoker
{
    internal static readonly NullCallInvoker Instance = new();

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
        => throw new NotSupportedException("NullCallInvoker is a stub.");

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
        => throw new NotSupportedException("NullCallInvoker is a stub.");

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
        => throw new NotSupportedException();

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options)
        => throw new NotSupportedException();

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options)
        => throw new NotSupportedException();
}

/// <summary>
/// Fake gRPC client that returns configurable in-memory responses and counts reservations.
/// </summary>
internal sealed class FakeCatalogServiceClient : CatalogService.CatalogServiceClient
{
    private readonly bool _reserveSuccess;
    private readonly string _failureReason;
    private int _reserveCalls;

    internal FakeCatalogServiceClient(bool reserveSuccess = true, string failureReason = "")
        : base(NullCallInvoker.Instance)
    {
        _reserveSuccess = reserveSuccess;
        _failureReason = failureReason;
    }

    /// <summary>Gets how many ReserveStock calls the Order service made.</summary>
    internal int ReserveCalls => Volatile.Read(ref _reserveCalls);

    /// <inheritdoc/>
    public override AsyncUnaryCall<ReserveStockReply> ReserveStockAsync(
        ReserveStockRequest request,
        CallOptions options)
    {
        Interlocked.Increment(ref _reserveCalls);
        var reply = new ReserveStockReply
        {
            Success = _reserveSuccess,
            FailureReason = _failureReason,
        };
        return new AsyncUnaryCall<ReserveStockReply>(
            Task.FromResult(reply),
            Task.FromResult(Metadata.Empty),
            () => Status.DefaultSuccess,
            () => Metadata.Empty,
            () => { });
    }

    /// <inheritdoc/>
    public override AsyncUnaryCall<ReserveStockReply> ReserveStockAsync(
        ReserveStockRequest request,
        Metadata? headers = null,
        DateTime? deadline = null,
        CancellationToken cancellationToken = default)
        => ReserveStockAsync(request, new CallOptions(headers, deadline, cancellationToken));
}

/// <summary>
/// Order service on its own Postgres container, with the Catalog gRPC client replaced by a fake.
/// </summary>
public abstract class OrderWebFactoryBase : IAsyncLifetime
{
    private readonly PostgresFixture _postgres = new();
    private WebApplicationFactory<OrderApp::Program>? _factory;

    /// <summary>Creates the factory with a fake Catalog client that accepts or rejects every reservation.</summary>
    private protected OrderWebFactoryBase(bool reserveSuccess)
    {
        Catalog = reserveSuccess
            ? new FakeCatalogServiceClient(reserveSuccess: true)
            : new FakeCatalogServiceClient(reserveSuccess: false, failureReason: "out of stock");
    }

    /// <summary>Gets the configured HttpClient for the Order service.</summary>
    public HttpClient Client { get; private set; } = null!;

    /// <summary>Gets the Order service's root service provider.</summary>
    public IServiceProvider Services => _factory!.Services;

    internal FakeCatalogServiceClient Catalog { get; }

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        await _postgres.InitializeAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__OrderDb", _postgres.ConnectionString);
        Environment.SetEnvironmentVariable("OTEL_SDK_DISABLED", "true");
        Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT", "");

        var cs = _postgres.ConnectionString;

        _factory = new WebApplicationFactory<OrderApp::Program>()
            .WithWebHostBuilder(builder =>
            {
                // Development turns on DI scope validation, so a lifetime mistake in the
                // sample's registrations fails the test instead of surfacing at runtime.
                builder.UseEnvironment("Development");

                builder.ConfigureAppConfiguration((_, cfg) =>
                {
                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:OrderDb"] = cs,
                        ["OTEL_SDK_DISABLED"] = "true",
                    });
                });
                builder.UseSetting("ConnectionStrings:OrderDb", cs);

                builder.ConfigureTestServices(services =>
                {
                    // Remove ALL registrations for the typed gRPC client and replace with the fake.
                    var toRemove = services
                        .Where(d => d.ServiceType == typeof(CatalogService.CatalogServiceClient))
                        .ToList();
                    foreach (var d in toRemove)
                        services.Remove(d);

                    services.AddSingleton<CatalogService.CatalogServiceClient>(Catalog);
                });

                builder.ConfigureLogging(lb =>
                {
                    lb.ClearProviders();
                    lb.AddProvider(CapturingLoggerProvider.Instance);
                    lb.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
                });
            });

        Client = _factory.CreateClient();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        if (_factory is not null) await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__OrderDb", null);
        GC.SuppressFinalize(this);
    }
}

/// <summary>Shared fixture: the fake Catalog client accepts every reservation.</summary>
public sealed class OrderWebFactory : OrderWebFactoryBase
{
    /// <summary>Creates the fixture.</summary>
    public OrderWebFactory() : base(reserveSuccess: true) { }
}

/// <summary>Shared fixture: the fake Catalog client rejects every reservation.</summary>
public sealed class OrderWebFactoryWithInventoryFailure : OrderWebFactoryBase
{
    /// <summary>Creates the fixture.</summary>
    public OrderWebFactoryWithInventoryFailure() : base(reserveSuccess: false) { }
}

/// <summary>
/// End-to-end tests for the Order sample's command flow: reserve stock, then commit the order
/// and its <c>order.completed.v1</c> Outbox row in one unit of work.
/// </summary>
public sealed class PlaceOrderTests : IClassFixture<OrderWebFactory>
{
    private readonly OrderWebFactory _factory;

    /// <summary>Injects the shared factory fixture.</summary>
    public PlaceOrderTests(OrderWebFactory factory) => _factory = factory;

    /// <summary>
    /// POST /demo/orders reserves the supplied line, stores the order as Completed and queues
    /// <c>order.completed.v1</c> for it in the Outbox.
    /// </summary>
    [Fact]
    public async Task PlaceOrder_ReservesStock_CompletesOrder_AndQueuesOutboxMessage()
    {
        var ct = TestContext.Current.CancellationToken;

        var line = new { productId = Guid.NewGuid(), quantity = 3, unitPrice = 12.50m };
        var postResponse = await _factory.Client.PostAsJsonAsync("/demo/orders", line, ct);
        postResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            because: $"server response: {await postResponse.Content.ReadAsStringAsync(ct)} | " +
                     $"captured error: {CapturingLoggerProvider.Instance.LastError}");

        using var created = JsonDocument.Parse(await postResponse.Content.ReadAsStringAsync(ct));
        var orderId = created.RootElement.GetProperty("orderId").GetGuid();
        orderId.Should().NotBeEmpty();
        _factory.Catalog.ReserveCalls.Should().Be(1);

        var getResponse = await _factory.Client.GetAsync($"/demo/orders/{orderId}", ct);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var order = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync(ct));
        order.RootElement.GetProperty("status").GetString().Should().Be("Completed");
        order.RootElement.GetProperty("totalAmount").GetDecimal().Should().Be(37.50m);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var payloads = await db.OutboxMessages.AsNoTracking()
            .Where(m => m.EventTypeId == "order.completed.v1")
            .Select(m => m.Payload)
            .ToListAsync(ct);
        payloads.Should().ContainSingle(p => p.Contains(orderId.ToString(), StringComparison.OrdinalIgnoreCase),
            because: "the order and its integration event are committed in the same unit of work");
    }
}

/// <summary>
/// When Catalog rejects a reservation, the command fails with 422 and neither the order nor an
/// Outbox row is committed.
/// </summary>
public sealed class PlaceOrderRejectedTests : IClassFixture<OrderWebFactoryWithInventoryFailure>
{
    private readonly OrderWebFactoryWithInventoryFailure _factory;

    /// <summary>Injects the shared factory fixture.</summary>
    public PlaceOrderRejectedTests(OrderWebFactoryWithInventoryFailure factory) => _factory = factory;

    /// <summary>A rejected reservation maps to 422 BUSINESS_RULE_VIOLATED and commits nothing.</summary>
    [Fact]
    public async Task PlaceOrder_WhenCatalogRejects_Returns422_AndCommitsNothing()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _factory.Client.PostAsync("/demo/orders", null, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity, because: $"server response: {body}");
        using var problem = JsonDocument.Parse(body);
        problem.RootElement.GetProperty("code").GetString().Should().Be("BUSINESS_RULE_VIOLATED");
        _factory.Catalog.ReserveCalls.Should().Be(1, because: "the handler stops at the first rejected line");

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        (await db.Orders.CountAsync(ct)).Should().Be(0);
        (await db.OutboxMessages.CountAsync(ct)).Should().Be(0);
    }
}

/// <summary>
/// Diagnostic logger provider — keeps the most recent error-level entry so the test
/// assertion can show the server-side exception that caused a 500, rather than just
/// the ProblemDetails envelope.
/// </summary>
internal sealed class CapturingLoggerProvider : ILoggerProvider
{
    internal static readonly CapturingLoggerProvider Instance = new();

    internal string? LastError { get; private set; }

    public ILogger CreateLogger(string categoryName) => new Capturing(this, categoryName);

    public void Dispose() { }

    private sealed class Capturing : ILogger
    {
        private readonly CapturingLoggerProvider _owner;
        private readonly string _category;

        public Capturing(CapturingLoggerProvider owner, string category)
        {
            _owner = owner;
            _category = category;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel < LogLevel.Error) return;
            _owner.LastError = $"[{_category}] {formatter(state, exception)}\n{exception}";
        }
    }
}
