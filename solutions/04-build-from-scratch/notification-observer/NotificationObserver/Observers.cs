namespace NotificationObserver;

/// <summary>An observer that uses the C# event.</summary>
public class SmsNotifier(ISmsGateway sms, IReadOnlyDictionary<string, string> phoneByTrackingNumber)
{
    public void Attach(PackageTracker tracker) => tracker.StatusChanged += OnStatusChanged;

    // -= needs the SAME delegate as +=. A method group works; a new lambda would not.
    public void Detach(PackageTracker tracker) => tracker.StatusChanged -= OnStatusChanged;

    private void OnStatusChanged(object? sender, StatusChangedEventArgs e)
    {
        var change = e.Change;
        if (!phoneByTrackingNumber.TryGetValue(change.TrackingNumber, out var phone))
            return;

        var text = change.NewStatus switch
        {
            PackageStatus.OutForDelivery => $"Ditt paket {change.TrackingNumber} är på väg och levereras idag.",
            PackageStatus.Delivered => $"Ditt paket {change.TrackingNumber} har levererats.",
            _ => null
        };

        if (text is not null)
            sms.Send(phone, text);
    }
}

/// <summary>An observer that uses IObserver&lt;StatusChange&gt;.</summary>
public class HistoryLog : IObserver<StatusChange>
{
    private readonly Dictionary<string, List<StatusChange>> _history = [];

    public bool IsCompleted { get; private set; }

    public IReadOnlyList<StatusChange> For(string trackingNumber) =>
        _history.TryGetValue(trackingNumber, out var list) ? list : [];

    public void OnNext(StatusChange value)
    {
        if (!_history.TryGetValue(value.TrackingNumber, out var list))
            _history[value.TrackingNumber] = list = [];
        list.Add(value);
    }

    public void OnError(Exception error)
    {
        // Nothing sends errors in this exercise. A real log might record it.
    }

    public void OnCompleted() => IsCompleted = true;
}
