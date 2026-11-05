namespace SubscriptionReminders;

public record Customer(string Email, string Name, DateOnly SubscriptionEndsOn, bool AutoRenew);

public record ReminderResult(IReadOnlyList<string> Recipients);
