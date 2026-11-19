using Kun.EventBus;

namespace Kun.Sample.Order.Contracts;

/// <summary>
/// Integration event published through the Outbox when an order is placed and its stock is reserved.
/// </summary>
[IntegrationEventId("order.completed.v1")]
public sealed record OrderCompletedIntegrationEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    DateTimeOffset CompletedAt);
