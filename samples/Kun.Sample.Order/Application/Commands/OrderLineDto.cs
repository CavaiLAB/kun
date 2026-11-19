namespace Kun.Sample.Order.Application.Commands;

/// <summary>One requested line of a <see cref="PlaceOrderCommand"/>.</summary>
public sealed class OrderLineDto
{
    /// <summary>Gets or sets the product identifier.</summary>
    public Guid ProductId { get; init; }

    /// <summary>Gets or sets the quantity ordered.</summary>
    public int Quantity { get; init; }

    /// <summary>Gets or sets the unit price.</summary>
    public decimal UnitPrice { get; init; }
}
