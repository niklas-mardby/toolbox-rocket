namespace SubscriptionReminders.Tests;

// Characterization tests: they describe what the app does TODAY, end to end,
// using the real (pretend) database, mail server and clock.
// They must stay green before AND after your refactoring. Don't change them.
//
// Notice how little they can check. That's the problem you're about to solve.
public class CharacterizationTests
{
    [Fact]
    public void Sends_reminders_to_customers_whose_subscription_ends_within_a_week()
    {
        var service = Composition.CreateReminderService();

        var result = service.SendRenewalReminders();

        Assert.Equal(
            new[] { "fatima@example.com", "anna@example.com", "bo@example.com" },
            result.Recipients);
    }
}
