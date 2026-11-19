using Kun.Core.Cqrs;
using Kun.Errors;
using Kun.EventBus;
using Kun.Sample.Catalog.Grpc;
using Kun.Sample.Order.Contracts;
using Kun.Sample.Order.Infrastructure;
using OrderAggregate = Kun.Sample.Order.Domain.Order;
using OrderLine = Kun.Sample.Order.Domain.OrderLine;

namespace Kun.Sample.Order.Application.Commands;

/// <summary>
/// Handles <see cref="PlaceOrderCommand"/>: reserves stock for every line through the Catalog gRPC
/// service, then adds the <see cref="OrderAggregate"/> and publishes
/// <see cref="OrderCompletedIntegrationEvent"/>. The <c>UnitOfWorkPipelineBehavior</c> registered
/// for this command commits the order row and the Outbox row in one transaction after the handler
/// returns; if a reservation is rejected the handler throws and nothing is committed.
/// </summary>
public sealed partial class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Guid>
{
    private readonly OrderDbContext _db;
    private readonly CatalogService.CatalogServiceClient _catalogClient;
    private readonly IIntegrationEventBus _eventBus;
    private readonly ILogger<PlaceOrderCommandHandler> _logger;

    /// <summary>Creates a new <see cref="PlaceOrderCommandHandler"/>.</summary>
    public PlaceOrderCommandHandler(
        OrderDbContext db,
        CatalogService.CatalogServiceClient catalogClient,
        IIntegrationEventBus eventBus,
        ILogger<PlaceOrderCommandHandler> logger)
    {
        _db = db;
        _catalogClient = catalogClient;
        _eventBus = eventBus;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async ValueTask<Guid> Handle(PlaceOrderCommand cmd, CancellationToken cancellationToken)
    {
        var lines = cmd.Items.Select(i => new OrderLine(i.ProductId, i.Quantity, i.UnitPrice));
        var order = OrderAggregate.Create(cmd.CustomerId, lines);

        // NOTE: demo-grade — lines reserved before a rejected one are not released. A real system
        // would reserve all lines in one call or release them when a later line is rejected.
        foreach (var item in cmd.Items)
        {
            var reply = await _catalogClient.ReserveStockAsync(
                new ReserveStockRequest
                {
                    ProductId = item.ProductId.ToString(),
                    Quantity = item.Quantity,
                },
                cancellationToken: cancellationToken);

            if (!reply.Success)
            {
                LogReservationRejected(_logger, item.ProductId, reply.FailureReason);
                throw new ErrorException(new Error(PlatformErrors.BusinessRuleViolated));
            }
        }

        order.MarkReserved();
        order.MarkCompleted();
        await _db.Orders.AddAsync(order, cancellationToken);

        _eventBus.Publish(new OrderCompletedIntegrationEvent(
            OrderId: order.Id,
            CustomerId: order.CustomerId,
            TotalAmount: order.TotalAmount,
            CompletedAt: DateTimeOffset.UtcNow));

        LogOrderPlaced(_logger, order.Id);
        return order.Id;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Order {OrderId} placed: stock reserved, order.completed.v1 queued in the Outbox.")]
    private static partial void LogOrderPlaced(ILogger logger, Guid orderId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Catalog rejected the stock reservation for product {ProductId}: {Reason}")]
    private static partial void LogReservationRejected(ILogger logger, Guid productId, string reason);
}
