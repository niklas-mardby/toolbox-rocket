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

/// <summary>
/// The composition root: the ONE place where objects are created and wired together.
/// You may change this file freely during the refactoring. The tests only use Composition.CreateShop.
/// </summary>
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

        var orders = new OrderService(outbox, inventory, loyalty, statistics, picking);

        return new Shop(orders, outbox, inventory, loyalty, statistics, picking, slack);
    }
}
