using Kun.Core.Cqrs;

namespace Kun.Sample.Order.Application.Commands;

/// <summary>
/// Places a new order: reserves stock in the Catalog service, then stores the order and its
/// <c>order.completed.v1</c> integration event in one unit of work.
/// </summary>
/// <param name="CustomerId">The customer placing the order.</param>
/// <param name="Items">The line items requested.</param>
public sealed record PlaceOrderCommand(
    Guid CustomerId,
    List<OrderLineDto> Items) : ICommand<Guid>;
