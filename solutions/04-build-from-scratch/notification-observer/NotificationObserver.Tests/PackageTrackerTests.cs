namespace NotificationObserver.Tests;

public class FixedTime(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

public class RecordingSms : ISmsGateway
{
    public List<(string Phone, string Text)> Sent { get; } = [];
    public void Send(string phoneNumber, string text) => Sent.Add((phoneNumber, text));
}

public class PackageTrackerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 10, 0, 0, TimeSpan.Zero);
    private readonly PackageTracker _tracker = new(new FixedTime(Now));

    // --- Part 1: the C# event ---

    [Fact(Skip = Todo.Skip)]
    public void Unknown_package_has_no_status()
    {
        Assert.Null(_tracker.StatusOf("NOPE"));
    }

    [Fact(Skip = Todo.Skip)]
    public void UpdateStatus_stores_the_status()
    {
        _tracker.UpdateStatus("PKG-1", PackageStatus.InTransit);

        Assert.Equal(PackageStatus.InTransit, _tracker.StatusOf("PKG-1"));
    }

    [Fact(Skip = Todo.Skip)]
    public void Event_is_raised_with_old_and_new_status()
    {
        var changes = new List<StatusChange>();
        _tracker.StatusChanged += (_, e) => changes.Add(e.Change);

        _tracker.UpdateStatus("PKG-1", PackageStatus.Registered);
        _tracker.UpdateStatus("PKG-1", PackageStatus.InTransit);

        Assert.Equal(
            new[]
            {
                new StatusChange("PKG-1", null, PackageStatus.Registered, Now),
                new StatusChange("PKG-1", PackageStatus.Registered, PackageStatus.InTransit, Now),
            },
            changes);
    }

    [Fact(Skip = Todo.Skip)]
    public void Event_is_not_raised_when_status_is_unchanged()
    {
        var count = 0;
        _tracker.UpdateStatus("PKG-1", PackageStatus.InTransit);
        _tracker.StatusChanged += (_, _) => count++;

        _tracker.UpdateStatus("PKG-1", PackageStatus.InTransit);

        Assert.Equal(0, count);
    }

    [Fact(Skip = Todo.Skip)]
    public void Works_without_any_subscribers()
    {
        var exception = Record.Exception(() => _tracker.UpdateStatus("PKG-1", PackageStatus.InTransit));

        Assert.Null(exception);
    }

    // --- Part 1: SmsNotifier ---

    [Fact(Skip = Todo.Skip)]
    public void SmsNotifier_texts_on_out_for_delivery_and_delivered_only()
    {
        var sms = new RecordingSms();
        var notifier = new SmsNotifier(sms, new Dictionary<string, string> { ["PKG-1"] = "0701234567" });
        notifier.Attach(_tracker);

        foreach (var status in Enum.GetValues<PackageStatus>())
            _tracker.UpdateStatus("PKG-1", status);

        Assert.Equal(
            new[]
            {
                ("0701234567", "Ditt paket PKG-1 är på väg och levereras idag."),
                ("0701234567", "Ditt paket PKG-1 har levererats."),
            },
            sms.Sent);
    }

    [Fact(Skip = Todo.Skip)]
    public void SmsNotifier_ignores_packages_without_a_phone_number()
    {
        var sms = new RecordingSms();
        var notifier = new SmsNotifier(sms, new Dictionary<string, string>());
        notifier.Attach(_tracker);

        _tracker.UpdateStatus("PKG-9", PackageStatus.Delivered);

        Assert.Empty(sms.Sent);
    }

    [Fact(Skip = Todo.Skip)]
    public void Detached_SmsNotifier_sends_nothing()
    {
        var sms = new RecordingSms();
        var notifier = new SmsNotifier(sms, new Dictionary<string, string> { ["PKG-1"] = "0701234567" });
        notifier.Attach(_tracker);
        notifier.Detach(_tracker);

        _tracker.UpdateStatus("PKG-1", PackageStatus.Delivered);

        Assert.Empty(sms.Sent);
    }

    // --- Part 2: IObservable / IObserver ---

    [Fact(Skip = Todo.Skip)]
    public void HistoryLog_receives_every_change_per_package()
    {
        var history = new HistoryLog();
        _tracker.Subscribe(history);

        _tracker.UpdateStatus("PKG-1", PackageStatus.Registered);
        _tracker.UpdateStatus("PKG-2", PackageStatus.Registered);
        _tracker.UpdateStatus("PKG-1", PackageStatus.InTransit);

        Assert.Equal(
            new[] { PackageStatus.Registered, PackageStatus.InTransit },
            history.For("PKG-1").Select(c => c.NewStatus));
        Assert.Single(history.For("PKG-2"));
        Assert.Empty(history.For("PKG-3"));
    }

    [Fact(Skip = Todo.Skip)]
    public void Disposing_the_subscription_unsubscribes()
    {
        var history = new HistoryLog();
        var subscription = _tracker.Subscribe(history);

        _tracker.UpdateStatus("PKG-1", PackageStatus.Registered);
        subscription.Dispose();
        _tracker.UpdateStatus("PKG-1", PackageStatus.InTransit);

        Assert.Single(history.For("PKG-1"));
    }

    [Fact(Skip = Todo.Skip)]
    public void Disposing_twice_is_harmless()
    {
        var subscription = _tracker.Subscribe(new HistoryLog());

        subscription.Dispose();
        var exception = Record.Exception(subscription.Dispose);

        Assert.Null(exception);
    }

    [Fact(Skip = Todo.Skip)]
    public void Event_and_observable_subscribers_are_both_notified()
    {
        var history = new HistoryLog();
        var eventCount = 0;
        _tracker.Subscribe(history);
        _tracker.StatusChanged += (_, _) => eventCount++;

        _tracker.UpdateStatus("PKG-1", PackageStatus.Registered);

        Assert.Equal(1, eventCount);
        Assert.Single(history.For("PKG-1"));
    }

    [Fact(Skip = Todo.Skip)]
    public void Shutdown_completes_observers_and_blocks_further_updates()
    {
        var history = new HistoryLog();
        _tracker.Subscribe(history);

        _tracker.Shutdown();

        Assert.True(history.IsCompleted);
        Assert.Throws<InvalidOperationException>(() => _tracker.UpdateStatus("PKG-1", PackageStatus.Registered));
    }

    [Fact(Skip = Todo.Skip)]
    public void An_observer_may_unsubscribe_while_being_notified()
    {
        IDisposable? subscription = null;
        var selfRemoving = new SelfRemovingObserver(() => subscription!.Dispose());
        var history = new HistoryLog();
        subscription = _tracker.Subscribe(selfRemoving);
        _tracker.Subscribe(history);

        _tracker.UpdateStatus("PKG-1", PackageStatus.Registered);
        _tracker.UpdateStatus("PKG-1", PackageStatus.InTransit);

        Assert.Equal(1, selfRemoving.Calls);
        Assert.Equal(2, history.For("PKG-1").Count);
    }

    private class SelfRemovingObserver(Action unsubscribe) : IObserver<StatusChange>
    {
        public int Calls { get; private set; }
        public void OnNext(StatusChange value) { Calls++; unsubscribe(); }
        public void OnError(Exception error) { }
        public void OnCompleted() { }
    }
}
