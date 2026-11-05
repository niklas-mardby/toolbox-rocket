namespace NotificationChannels;

/// <summary>WHAT to say. Independent of HOW it's delivered.</summary>
public record Notification(string Subject, string Body, string ShortText)
{
    public static Notification Shipped(Customer customer, string orderId, string trackingUrl) => new(
        $"Din order {orderId} är skickad",
        $"Hej {customer.Name}! Följ paketet här: {trackingUrl}",
        $"Din order {orderId} är skickad. Spåra: {trackingUrl}");

    public static Notification Delivered(Customer customer, string orderId) => new(
        $"Din order {orderId} har levererats",
        $"Hej {customer.Name}! Ditt paket har kommit fram. Hoppas du blir nöjd!",
        $"Din order {orderId} har levererats.");
}

/// <summary>Strategy: HOW to deliver a notification through one channel.</summary>
public interface INotificationChannel
{
    NotificationChannel Kind { get; }
    bool CanReach(Customer customer);
    void Send(Customer customer, Notification notification);
}

public class EmailChannel(IEmailGateway email) : INotificationChannel
{
    public NotificationChannel Kind => NotificationChannel.Email;
    public bool CanReach(Customer customer) => !string.IsNullOrWhiteSpace(customer.Email);
    public void Send(Customer customer, Notification n) => email.Send(customer.Email, n.Subject, n.Body);
}

public class SmsChannel(ISmsGateway sms) : INotificationChannel
{
    public NotificationChannel Kind => NotificationChannel.Sms;
    public bool CanReach(Customer customer) => !string.IsNullOrWhiteSpace(customer.Phone);
    public void Send(Customer customer, Notification n) => sms.Send(customer.Phone!, $"Toolbox: {n.ShortText}");
}

public class PushChannel(IPushGateway push) : INotificationChannel
{
    public NotificationChannel Kind => NotificationChannel.Push;
    public bool CanReach(Customer customer) => !string.IsNullOrWhiteSpace(customer.PushToken);
    public void Send(Customer customer, Notification n) => push.Send(customer.PushToken!, n.Subject, n.ShortText);
}

/// <summary>
/// Decides WHICH channels to use (the customer's preferences, with email as the fallback)
/// and leaves the HOW to the channel strategies. Adding a channel never touches this class.
/// </summary>
public class NotificationService(ICustomerDirectory customers, IEnumerable<INotificationChannel> channels)
{
    private readonly Dictionary<NotificationChannel, INotificationChannel> _channels = channels.ToDictionary(c => c.Kind);

    public void NotifyOrderShipped(string customerId, string orderId, string trackingUrl)
    {
        var customer = FindCustomer(customerId);
        Deliver(customer, Notification.Shipped(customer, orderId, trackingUrl));
    }

    public void NotifyOrderDelivered(string customerId, string orderId)
    {
        var customer = FindCustomer(customerId);
        Deliver(customer, Notification.Delivered(customer, orderId));
    }

    private Customer FindCustomer(string customerId) =>
        customers.Find(customerId) ?? throw new KeyNotFoundException($"Customer {customerId} not found.");

    private void Deliver(Customer customer, Notification notification)
    {
        var reachable = customer.PreferredChannels
            .Distinct()
            .Select(kind => _channels.GetValueOrDefault(kind))
            .OfType<INotificationChannel>()
            .Where(channel => channel.CanReach(customer))
            .ToList();

        if (reachable.Count == 0)
            reachable.Add(_channels[NotificationChannel.Email]);   // fallback: everyone has an email

        foreach (var channel in reachable)
            channel.Send(customer, notification);
    }
}
