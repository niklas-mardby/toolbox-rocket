namespace NotificationObserver;

/// <summary>
/// 🛠️ YOUR JOB: the SUBJECT. It keeps track of every package's status and tells
/// interested parties when a status changes, in two ways:
///   1. a classic C# event (StatusChanged)
///   2. the formal .NET Observer interface (IObservable&lt;StatusChange&gt;)
/// </summary>
public class PackageTracker(TimeProvider time) : IObservable<StatusChange>
{
    public event EventHandler<StatusChangedEventArgs>? StatusChanged;

    /// <summary>The current status, or null if the package has never been seen.</summary>
    public PackageStatus? StatusOf(string trackingNumber) =>
        throw new NotImplementedException();

    /// <summary>
    /// Sets a new status and notifies BOTH the event subscribers and the IObservable
    /// subscribers, but ONLY if the status actually changed. The time comes from TimeProvider.
    /// </summary>
    public void UpdateStatus(string trackingNumber, PackageStatus newStatus) =>
        throw new NotImplementedException();

    /// <summary>Subscribe an IObserver. Disposing the returned object unsubscribes it.</summary>
    public IDisposable Subscribe(IObserver<StatusChange> observer) =>
        throw new NotImplementedException();

    /// <summary>
    /// The tracker is shutting down. All IObservable subscribers get OnCompleted
    /// and are removed. Further UpdateStatus calls throw InvalidOperationException.
    /// </summary>
    public void Shutdown() =>
        throw new NotImplementedException();
}
