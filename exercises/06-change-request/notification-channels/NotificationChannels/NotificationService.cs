namespace NotificationChannels;

public class NotificationService(ICustomerDirectory customers, IEmailGateway email)
{
    public void NotifyOrderShipped(string customerId, string orderId, string trackingUrl)
    {
        var customer = customers.Find(customerId)
            ?? throw new KeyNotFoundException($"Customer {customerId} not found.");

        email.Send(
            customer.Email,
            $"Din order {orderId} är skickad",
            $"Hej {customer.Name}! Följ paketet här: {trackingUrl}");
    }

    public void NotifyOrderDelivered(string customerId, string orderId)
    {
        var customer = customers.Find(customerId)
            ?? throw new KeyNotFoundException($"Customer {customerId} not found.");

        email.Send(
            customer.Email,
            $"Din order {orderId} har levererats",
            $"Hej {customer.Name}! Ditt paket har kommit fram. Hoppas du blir nöjd!");
    }
}
