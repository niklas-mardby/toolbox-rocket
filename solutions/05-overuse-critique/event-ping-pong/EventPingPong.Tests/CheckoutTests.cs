namespace EventPingPong.Tests;

// These tests must stay green before AND after your simplification. Don't change them.
public class CheckoutTests
{
    private readonly Shop _shop = Composition.CreateShop(
        new Dictionary<string, int> { ["HAMMER"] = 5, ["DRILL"] = 1 },
        new Dictionary<string, string> { ["anna"] = "Storgatan 1, Örebro" });

    private static Order OrderFor(string customer, string card, params OrderLine[] lines) => new()
    {
        CustomerId = customer,
        Email = $"{customer}@example.com",
        CardNumber = card,
        Lines = lines,
    };

    private const string GoodCard = "4111111111111111";
    private const string DeclinedCard = "4000000000000002";

    [Fact]
    public void Successful_checkout_reserves_charges_ships_awards_points_and_confirms()
    {
        var order = OrderFor("anna", GoodCard, new OrderLine("HAMMER", 2, 199m));

        var result = _shop.Checkout.Checkout(order);

        Assert.Equal(CheckoutResult.Completed, result);
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Equal(3, _shop.Inventory.StockOf("HAMMER"));
        Assert.Equal(new[] { new Charge(order.Id, 398m) }, _shop.Payments.Charges);
        Assert.Equal(new[] { new Shipment(order.Id, "Storgatan 1, Örebro") }, _shop.Shipping.Shipments);
        Assert.Equal(39, _shop.Loyalty.PointsFor("anna"));
        Assert.Equal(new[] { new SentEmail("anna@example.com", "Tack för din order! Du fick 39 poäng.") }, _shop.Emails.Sent);
    }

    [Fact]
    public void Customers_without_an_address_pick_up_in_store()
    {
        var order = OrderFor("bo", GoodCard, new OrderLine("HAMMER", 1, 199m));

        _shop.Checkout.Checkout(order);

        Assert.Equal("Hämtas i butik", _shop.Shipping.Shipments.Single().Address);
    }

    [Fact]
    public void Out_of_stock_rejects_the_order_and_nothing_else_happens()
    {
        var order = OrderFor("anna", GoodCard, new OrderLine("HAMMER", 1, 199m), new OrderLine("DRILL", 2, 999m));

        var result = _shop.Checkout.Checkout(order);

        Assert.Equal(CheckoutResult.OutOfStock, result);
        Assert.Equal(OrderStatus.Rejected, order.Status);
        Assert.Equal(5, _shop.Inventory.StockOf("HAMMER"));
        Assert.Empty(_shop.Payments.Charges);
        Assert.Empty(_shop.Shipping.Shipments);
        Assert.Equal(0, _shop.Loyalty.PointsFor("anna"));
        Assert.Equal(new[] { new SentEmail("anna@example.com", "Tyvärr, en vara är slut i lager") }, _shop.Emails.Sent);
    }

    [Fact]
    public void Declined_payment_releases_the_stock_again()
    {
        var order = OrderFor("anna", DeclinedCard, new OrderLine("HAMMER", 2, 199m));

        var result = _shop.Checkout.Checkout(order);

        Assert.Equal(CheckoutResult.PaymentDeclined, result);
        Assert.Equal(OrderStatus.PaymentFailed, order.Status);
        Assert.Equal(5, _shop.Inventory.StockOf("HAMMER"));
        Assert.Empty(_shop.Payments.Charges);
        Assert.Empty(_shop.Shipping.Shipments);
        Assert.Equal(new[] { new SentEmail("anna@example.com", "Betalningen nekades") }, _shop.Emails.Sent);
    }

    [Fact]
    public void Several_orders_add_up()
    {
        _shop.Checkout.Checkout(OrderFor("anna", GoodCard, new OrderLine("HAMMER", 1, 199m)));
        _shop.Checkout.Checkout(OrderFor("anna", GoodCard, new OrderLine("HAMMER", 1, 199m)));

        Assert.Equal(3, _shop.Inventory.StockOf("HAMMER"));
        Assert.Equal(38, _shop.Loyalty.PointsFor("anna"));
        Assert.Equal(2, _shop.Emails.Sent.Count);
    }
}
