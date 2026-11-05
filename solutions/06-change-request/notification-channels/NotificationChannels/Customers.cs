namespace NotificationChannels;

public enum NotificationChannel { Email, Sms, Push }

/// <summary>
/// PreferredChannels was added by the CRM team last sprint. Customers can already pick channels
/// in "My account", but nothing in the notification code reads it yet.
/// </summary>
public record Customer(
    string Id,
    string Name,
    string Email,
    string? Phone,
    string? PushToken,
    IReadOnlyList<NotificationChannel> PreferredChannels);

public interface ICustomerDirectory
{
    Customer? Find(string customerId);
}

public class InMemoryCustomerDirectory(params Customer[] customers) : ICustomerDirectory
{
    public Customer? Find(string customerId) => customers.FirstOrDefault(c => c.Id == customerId);
}
