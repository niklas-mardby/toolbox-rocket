namespace OrderNotifications;

// OrderService works. But it knows about EVERYONE who cares about a new order.
// Product wants a Slack message too, and next month a "review request" email
// two days later... Each new wish means editing this class again.
public class OrderService(
    EmailOutbox emailOutbox,
    Inventory inventory,
    LoyaltyProgram loyaltyProgram,
    SalesStatistics salesStatistics,
    PickingQueue pickingQueue)
{
    private readonly List<Order> _orders = [];

    public IReadOnlyList<Order> Orders => _orders;

    public void PlaceOrder(Order order)
    {
        if (order.Lines.Count == 0)
            throw new ArgumentException("An order must have at least one line.", nameof(order));

        // Reserve stock. Throws InsufficientStockException if anything is missing.
        inventory.ReserveAll(order.Lines);

        _orders.Add(order);

        // Confirmation email to the customer
        emailOutbox.Send(
            order.CustomerEmail,
            "Tack för din order!",
            $"Din order {order.Id} på {order.Total:0.00} kr är mottagen.");

        // One loyalty point per full 10 kr
        var points = (int)Math.Floor(order.Total / 10m);
        loyaltyProgram.AddPoints(order.CustomerId, points);

        // Sales team wants to know about big orders
        if (order.Total >= 1000m)
        {
            emailOutbox.Send(
                "sales@toolbox-shop.se",
                "Stor order!",
                $"Kund {order.CustomerId} lade en order på {order.Total:0.00} kr.");
        }

        // Statistics for the dashboard
        salesStatistics.Record(order);

        // Tell the warehouse to start picking
        pickingQueue.Enqueue(order);
    }
}
