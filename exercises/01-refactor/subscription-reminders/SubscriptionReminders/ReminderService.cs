namespace SubscriptionReminders;

// Sends a reminder to customers whose subscription ends within 7 days
// (and who don't have auto-renew). It works... but try writing a unit test
// for "a subscription ending exactly 7 days from now gets a reminder"
// without touching the real database, the real mail server and the real clock.
public class ReminderService
{
    public ReminderResult SendRenewalReminders()
    {
        var repository = new SqlCustomerRepository("Server=prod-db;Database=Shop;Trusted_Connection=True");
        var emailSender = new SmtpEmailSender("smtp.toolbox-shop.se", 587);
        var today = DateOnly.FromDateTime(DateTime.Now);

        var customersToRemind = repository.GetAllCustomers()
            .Where(c => !c.AutoRenew)
            .Where(c => c.SubscriptionEndsOn >= today && c.SubscriptionEndsOn <= today.AddDays(7))
            .OrderBy(c => c.SubscriptionEndsOn)
            .ThenBy(c => c.Email)
            .ToList();

        foreach (var customer in customersToRemind)
        {
            var daysLeft = customer.SubscriptionEndsOn.DayNumber - today.DayNumber;
            var body = daysLeft == 0
                ? $"Hej {customer.Name}! Din prenumeration går ut idag."
                : $"Hej {customer.Name}! Din prenumeration går ut om {daysLeft} dagar, den {customer.SubscriptionEndsOn:yyyy-MM-dd}.";

            emailSender.Send(customer.Email, "Dags att förnya!", body);
        }

        return new ReminderResult(customersToRemind.Select(c => c.Email).ToList());
    }
}
