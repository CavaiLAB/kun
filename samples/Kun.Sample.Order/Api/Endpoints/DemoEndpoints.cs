using Kun.Mediator;
using Kun.Sample.Order.Application.Commands;
using Kun.Sample.Order.Application.Queries;

namespace Kun.Sample.Order.Api.Endpoints;

/// <summary>
/// Demo endpoints for the order command flow: place an order (reserve stock over gRPC, then commit
/// the order and its <c>order.completed.v1</c> Outbox row together) and read it back.
/// </summary>
public static class DemoEndpoints
{
    /// <summary>Maps all demo routes onto the given <paramref name="app"/>.</summary>
    public static IEndpointRouteBuilder MapDemoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/demo/orders", async (OrderLineDto? line, ISender sender, CancellationToken ct) =>
        {
            var command = line is null
                ? BuildDemoCommand()
                : new PlaceOrderCommand(Guid.NewGuid(), [line]);
            var orderId = await sender.Send(command, ct);
            return Results.Ok(new { orderId });
        })
        .WithName("PlaceDemoOrder")
        .WithSummary("Place a demo order: reserve stock in Catalog, then commit the order and its Outbox row.");

        // GET /demo/orders/{id} — read the order back
        app.MapGet("/demo/orders/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetOrderByIdQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetDemoOrder")
        .WithSummary("Get an order by its ID.");

        return app;
    }

    private static PlaceOrderCommand BuildDemoCommand() =>
        new(
            CustomerId: Guid.NewGuid(),
            Items: new List<OrderLineDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 49.99m },
                new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 19.99m },
            });
}
