namespace OrderNotifications;

public record OrderPlacedEventArgs(Order Order);

/// <summary>
/// The SUBJECT. It does what is part of placing an order (validate, reserve stock, store)
/// and then announces that it happened. It no longer knows who's listening.
/// </summary>
public class OrderService(Inventory inventory)
{
    private readonly List<Order> _orders = [];

    public IReadOnlyList<Order> Orders => _orders;

    public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;

    public void PlaceOrder(Order order)
    {
        if (order.Lines.Count == 0)
            throw new ArgumentException("An order must have at least one line.", nameof(order));

        // A STEP, not a reaction: it can stop the order, so it is called directly.
        inventory.ReserveAll(order.Lines);

        _orders.Add(order);

        OrderPlaced?.Invoke(this, new OrderPlacedEventArgs(order));
    }
}
