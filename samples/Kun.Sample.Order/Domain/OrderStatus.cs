namespace Kun.Sample.Order.Domain;

/// <summary>Lifecycle states of an <see cref="Order"/>.</summary>
public enum OrderStatus
{
    /// <summary>Order has been created and is awaiting processing.</summary>
    Pending,

    /// <summary>Inventory has been reserved for the order.</summary>
    Reserved,

    /// <summary>Stock is reserved and the <c>order.completed.v1</c> event is queued in the Outbox.</summary>
    Completed,

    /// <summary>The order was cancelled.</summary>
    Cancelled,

    /// <summary>The order failed in a non-recoverable way.</summary>
    Failed,
}
