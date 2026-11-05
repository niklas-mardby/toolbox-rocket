namespace SubscriptionReminders;

/// <summary>
/// The composition root: the place where the application's object graph is built.
/// Right now it has almost nothing to do, because ReminderService creates everything itself.
/// You may change this file freely. The existing tests only call CreateReminderService().
/// </summary>
public static class Composition
{
    public static ReminderService CreateReminderService()
    {
        return new ReminderService();
    }
}
