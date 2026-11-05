namespace SubscriptionReminders.Tests;

// Fast, isolated unit tests. Impossible before the refactoring, easy after it.

public class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

public class FakeCustomerRepository(params Customer[] customers) : ICustomerRepository
{
    public IReadOnlyList<Customer> GetAllCustomers() => customers;
}

public class FakeEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Body)> Sent { get; } = [];
    public void Send(string to, string subject, string body) => Sent.Add((to, subject, body));
}

public class ReminderServiceTests
{
    private readonly FakeEmailSender _email = new();

    private ReminderResult RunOn(DateOnly today, params Customer[] customers) =>
        new ReminderService(
                new FakeCustomerRepository(customers),
                _email,
                new FakeTimeProvider(new DateTimeOffset(today.ToDateTime(new TimeOnly(8, 0)), TimeSpan.Zero)))
            .SendRenewalReminders();

    private static readonly DateOnly Today = new(2026, 11, 5);

    private static Customer EndingOn(DateOnly date, bool autoRenew = false, string email = "a@x.se") =>
        new(email, "Anna", date, autoRenew);

    [Fact]
    public void Exactly_seven_days_gets_a_reminder_but_eight_does_not()
    {
        var result = RunOn(Today,
            EndingOn(Today.AddDays(7), email: "seven@x.se"),
            EndingOn(Today.AddDays(8), email: "eight@x.se"));

        Assert.Equal(new[] { "seven@x.se" }, result.Recipients);
    }

    [Fact]
    public void Auto_renew_customers_get_no_reminder()
    {
        Assert.Empty(RunOn(Today, EndingOn(Today.AddDays(3), autoRenew: true)).Recipients);
    }

    [Fact]
    public void Expired_subscriptions_get_no_reminder()
    {
        Assert.Empty(RunOn(Today, EndingOn(Today.AddDays(-1))).Recipients);
    }

    [Fact]
    public void Ending_today_says_so_in_the_email()
    {
        RunOn(Today, EndingOn(Today));

        var mail = Assert.Single(_email.Sent);
        Assert.Equal("Dags att förnya!", mail.Subject);
        Assert.Equal("Hej Anna! Din prenumeration går ut idag.", mail.Body);
    }

    [Fact]
    public void Reminder_across_the_leap_day()
    {
        var result = RunOn(new DateOnly(2028, 2, 25), EndingOn(new DateOnly(2028, 3, 3)));   // 26, 27, 28, 29 Feb, 1, 2, 3 Mar

        Assert.Single(result.Recipients);
        Assert.Equal("Hej Anna! Din prenumeration går ut om 7 dagar, den 2028-03-03.", _email.Sent.Single().Body);
    }
}
