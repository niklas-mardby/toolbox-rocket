namespace NotificationObserver;

/// <summary>
/// The SUBJECT. Keeps track of every package's status and tells interested parties
/// when a status changes, both via a C# event and via IObservable&lt;StatusChange&gt;.
/// </summary>
public class PackageTracker(TimeProvider time) : IObservable<StatusChange>
{
    private readonly Dictionary<string, PackageStatus> _statuses = [];
    private readonly List<IObserver<StatusChange>> _observers = [];
    private bool _isShutDown;

    public event EventHandler<StatusChangedEventArgs>? StatusChanged;

    public PackageStatus? StatusOf(string trackingNumber) =>
        _statuses.TryGetValue(trackingNumber, out var status) ? status : null;

    public void UpdateStatus(string trackingNumber, PackageStatus newStatus)
    {
        if (_isShutDown)
            throw new InvalidOperationException("The tracker has been shut down.");

        var oldStatus = StatusOf(trackingNumber);
        if (oldStatus == newStatus)
            return; // no change, no notification

        _statuses[trackingNumber] = newStatus;
        var change = new StatusChange(trackingNumber, oldStatus, newStatus, time.GetUtcNow());

        // 1. The C# event
        StatusChanged?.Invoke(this, new StatusChangedEventArgs(change));

        // 2. IObservable subscribers. Iterate over a COPY, so an observer
        //    may unsubscribe while it is being notified.
        foreach (var observer in _observers.ToList())
            observer.OnNext(change);
    }

    public IDisposable Subscribe(IObserver<StatusChange> observer)
    {
        _observers.Add(observer);
        return new Unsubscriber(() => _observers.Remove(observer));
    }

    public void Shutdown()
    {
        _isShutDown = true;
        foreach (var observer in _observers.ToList())
            observer.OnCompleted();
        _observers.Clear();
    }

    /// <summary>Removes the observer the first time it's disposed. Safe to dispose many times.</summary>
    private sealed class Unsubscriber(Action unsubscribe) : IDisposable
    {
        private Action? _unsubscribe = unsubscribe;

        public void Dispose()
        {
            _unsubscribe?.Invoke();
            _unsubscribe = null;
        }
    }
}
