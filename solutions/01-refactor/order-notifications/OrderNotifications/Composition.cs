namespace OrderNotifications;

/// <summary>Everything the tests need to look at after placing an order.</summary>
public record Shop(
    OrderService Orders,
    EmailOutbox Outbox,
    Inventory Inventory,
    LoyaltyProgram Loyalty,
    SalesStatistics Statistics,
    PickingQueue Picking,
    SlackChannel Slack);

/// <summary>The composition root: objects are created AND observers are subscribed here, in one place.</summary>
public static class Composition
{
    public static Shop CreateShop(Dictionary<string, int> initialStock)
    {
        var outbox = new EmailOutbox();
        var inventory = new Inventory(initialStock);
        var loyalty = new LoyaltyProgram();
        var statistics = new SalesStatistics();
        var picking = new PickingQueue();
        var slack = new SlackChannel();

        var orders = new OrderService(inventory);

        var confirmation = new ConfirmationEmailHandler(outbox);
        var points = new LoyaltyPointsHandler(loyalty);
        var bigOrders = new BigOrderAlertHandler(outbox);

        // Every reaction to a new order, readable top to bottom.
        orders.OrderPlaced += (_, e) => confirmation.Handle(e.Order);
        orders.OrderPlaced += (_, e) => points.Handle(e.Order);
        orders.OrderPlaced += (_, e) => bigOrders.Handle(e.Order);
        orders.OrderPlaced += (_, e) => statistics.Record(e.Order);
        orders.OrderPlaced += (_, e) => picking.Enqueue(e.Order);
        orders.OrderPlaced += (_, e) => slack.Post($"Ny order: {e.Order.Total:0.00} kr");   // the bonus: zero changes to OrderService

        return new Shop(orders, outbox, inventory, loyalty, statistics, picking, slack);
    }
}
