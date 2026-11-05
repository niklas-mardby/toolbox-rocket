namespace EventPingPong;

public record Shop(
    CheckoutService Checkout,
    InventoryService Inventory,
    PaymentService Payments,
    ShippingService Shipping,
    LoyaltyService Loyalty,
    EmailService Emails);

/// <summary>
/// The composition root. The tests only use CreateShop and the properties on Shop,
/// so you may change how the services are built and connected.
/// </summary>
public static class Composition
{
    public static Shop CreateShop(Dictionary<string, int> stock, Dictionary<string, string> addresses)
    {
        var events = new ShopEvents();

        // Order matters here (does it?), and nothing tells you so.
        var emails = new EmailService(events);
        var inventory = new InventoryService(events, stock);
        var payments = new PaymentService(events);
        var shipping = new ShippingService(events, addresses);
        var loyalty = new LoyaltyService(events);

        return new Shop(new CheckoutService(events), inventory, payments, shipping, loyalty, emails);
    }
}
