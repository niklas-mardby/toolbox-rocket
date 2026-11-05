namespace SubscriptionReminders;

/// <summary>
/// Only the business rule is left. No connection strings, no SMTP host, no DateTime.Now:
/// every dependency on the outside world comes in through the constructor.
/// </summary>
public class ReminderService(ICustomerRepository customers, IEmailSender email, TimeProvider time)
{
    public const int DaysAhead = 7;

    public ReminderResult SendRenewalReminders()
    {
        var today = DateOnly.FromDateTime(time.GetLocalNow().DateTime);

        var customersToRemind = customers.GetAllCustomers()
            .Where(c => !c.AutoRenew)
            .Where(c => c.SubscriptionEndsOn >= today && c.SubscriptionEndsOn <= today.AddDays(DaysAhead))
            .OrderBy(c => c.SubscriptionEndsOn)
            .ThenBy(c => c.Email)
            .ToList();

        foreach (var customer in customersToRemind)
        {
            var daysLeft = customer.SubscriptionEndsOn.DayNumber - today.DayNumber;
            var body = daysLeft == 0
                ? $"Hej {customer.Name}! Din prenumeration går ut idag."
                : $"Hej {customer.Name}! Din prenumeration går ut om {daysLeft} dagar, den {customer.SubscriptionEndsOn:yyyy-MM-dd}.";

            email.Send(customer.Email, "Dags att förnya!", body);
        }

        return new ReminderResult(customersToRemind.Select(c => c.Email).ToList());
    }
}
