namespace SubscriptionReminders;

// What ReminderService NEEDS, described in its own terms. (The clock uses .NET's built-in TimeProvider.)

public interface ICustomerRepository
{
    IReadOnlyList<Customer> GetAllCustomers();
}

public interface IEmailSender
{
    void Send(string to, string subject, string body);
}
