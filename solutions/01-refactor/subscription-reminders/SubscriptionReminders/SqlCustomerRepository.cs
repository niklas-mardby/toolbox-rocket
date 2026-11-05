namespace SubscriptionReminders;

/// <summary>
/// Pretends to talk to the production SQL database. The demo data is still generated relative to
/// today's date, so the characterization test and the demo keep working.
/// </summary>
public class SqlCustomerRepository(string connectionString) : ICustomerRepository
{
    public IReadOnlyList<Customer> GetAllCustomers()
    {
        Console.WriteLine($"   🗄️ Connecting to {connectionString} ...");
        var today = DateOnly.FromDateTime(DateTime.Now);

        return
        [
            new Customer("anna@example.com", "Anna", today.AddDays(3), AutoRenew: false),
            new Customer("bo@example.com", "Bo", today.AddDays(7), AutoRenew: false),
            new Customer("cecilia@example.com", "Cecilia", today.AddDays(8), AutoRenew: false),
            new Customer("david@example.com", "David", today.AddDays(2), AutoRenew: true),
            new Customer("erik@example.com", "Erik", today.AddDays(-1), AutoRenew: false),
            new Customer("fatima@example.com", "Fatima", today, AutoRenew: false),
        ];
    }
}
