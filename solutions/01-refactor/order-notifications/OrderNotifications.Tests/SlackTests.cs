namespace OrderNotifications.Tests;

// The bonus test: a new reaction added without touching OrderService.
public class SlackTests
{
    [Fact]
    public void Every_order_is_posted_to_slack()
    {
        var shop = Composition.CreateShop(new Dictionary<string, int> { ["HAMMER"] = 10 });

        shop.Orders.PlaceOrder(new Order(Guid.NewGuid(), "anna", "anna@example.com", [new OrderLine("HAMMER", 2, 199m)]));

        Assert.Equal(new[] { $"Ny order: {398m:0.00} kr" }, shop.Slack.Messages);   // same (culture-aware) formatting as the code
    }
}
