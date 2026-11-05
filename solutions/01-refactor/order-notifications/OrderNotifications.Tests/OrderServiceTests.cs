namespace OrderNotifications.Tests;

// These tests must stay green before AND after your refactoring. Don't change them.
// They only create objects through Composition.CreateShop, which you may change.
public class OrderServiceTests
{
    private static Shop CreateShop() => Composition.CreateShop(new Dictionary<string, int>
    {
        ["HAMMER"] = 10,
        ["DRILL"] = 2,
    });

    private static Order SmallOrder() =>
        new(Guid.NewGuid(), "anna", "anna@example.com", [new OrderLine("HAMMER", 2, 199m)]);

    private static Order BigOrder() =>
        new(Guid.NewGuid(), "bo", "bo@example.com", [new OrderLine("DRILL", 1, 1499m)]);

    [Fact]
    public void Placing_an_order_stores_it()
    {
        var shop = CreateShop();
        var order = SmallOrder();

        shop.Orders.PlaceOrder(order);

        Assert.Contains(order, shop.Orders.Orders);
    }

    [Fact]
    public void Placing_an_order_reserves_stock()
    {
        var shop = CreateShop();

        shop.Orders.PlaceOrder(SmallOrder());

        Assert.Equal(8, shop.Inventory.StockOf("HAMMER"));
    }

    [Fact]
    public void Placing_an_order_sends_a_confirmation_to_the_customer()
    {
        var shop = CreateShop();
        var order = SmallOrder();

        shop.Orders.PlaceOrder(order);

        var mail = Assert.Single(shop.Outbox.Sent);
        Assert.Equal("anna@example.com", mail.To);
        Assert.Equal("Tack för din order!", mail.Subject);
        Assert.Contains(order.Id.ToString(), mail.Body);
    }

    [Fact]
    public void Placing_an_order_gives_one_loyalty_point_per_full_10_kr()
    {
        var shop = CreateShop();

        shop.Orders.PlaceOrder(SmallOrder()); // 398 kr

        Assert.Equal(39, shop.Loyalty.PointsFor("anna"));
    }

    [Fact]
    public void Placing_an_order_updates_statistics()
    {
        var shop = CreateShop();

        shop.Orders.PlaceOrder(SmallOrder());
        shop.Orders.PlaceOrder(BigOrder());

        Assert.Equal(2, shop.Statistics.OrderCount);
        Assert.Equal(398m + 1499m, shop.Statistics.Revenue);
    }

    [Fact]
    public void Placing_an_order_queues_it_for_picking()
    {
        var shop = CreateShop();
        var order = SmallOrder();

        shop.Orders.PlaceOrder(order);

        Assert.Equal(new[] { order.Id }, shop.Picking.Pending);
    }

    [Fact]
    public void Big_orders_notify_the_sales_team()
    {
        var shop = CreateShop();

        shop.Orders.PlaceOrder(BigOrder());

        Assert.Equal(2, shop.Outbox.Sent.Count);
        Assert.Contains(shop.Outbox.Sent, m => m.To == "sales@toolbox-shop.se" && m.Subject == "Stor order!");
    }

    [Fact]
    public void Small_orders_do_not_notify_the_sales_team()
    {
        var shop = CreateShop();

        shop.Orders.PlaceOrder(SmallOrder());

        Assert.DoesNotContain(shop.Outbox.Sent, m => m.To == "sales@toolbox-shop.se");
    }

    [Fact]
    public void Out_of_stock_order_fails_and_nothing_else_happens()
    {
        var shop = CreateShop();
        var order = new Order(Guid.NewGuid(), "anna", "anna@example.com",
            [new OrderLine("HAMMER", 1, 199m), new OrderLine("DRILL", 5, 1499m)]);

        Assert.Throws<InsufficientStockException>(() => shop.Orders.PlaceOrder(order));

        Assert.Empty(shop.Orders.Orders);
        Assert.Empty(shop.Outbox.Sent);
        Assert.Equal(0, shop.Loyalty.PointsFor("anna"));
        Assert.Equal(0, shop.Statistics.OrderCount);
        Assert.Empty(shop.Picking.Pending);
        Assert.Equal(10, shop.Inventory.StockOf("HAMMER"));
    }

    [Fact]
    public void Empty_order_is_rejected()
    {
        var shop = CreateShop();
        var order = new Order(Guid.NewGuid(), "anna", "anna@example.com", []);

        Assert.Throws<ArgumentException>(() => shop.Orders.PlaceOrder(order));
        Assert.Empty(shop.Outbox.Sent);
    }
}
