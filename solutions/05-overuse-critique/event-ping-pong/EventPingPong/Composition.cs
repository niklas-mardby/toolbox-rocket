namespace EventPingPong;

public record Shop(
    CheckoutService Checkout,
    InventoryService Inventory,
    PaymentService Payments,
    ShippingService Shipping,
    LoyaltyService Loyalty,
    EmailService Emails);

public static class Composition
{
    public static Shop CreateShop(Dictionary<string, int> stock, Dictionary<string, string> addresses)
    {
        var inventory = new InventoryService(stock);
        var payments = new PaymentService();
        var shipping = new ShippingService(addresses);
        var loyalty = new LoyaltyService();
        var emails = new EmailService();

        var checkout = new CheckoutService(inventory, payments, shipping, loyalty, emails);
        return new Shop(checkout, inventory, payments, shipping, loyalty, emails);
    }
}
