namespace NotificationObserver;

/// <summary>
/// 🛠️ YOUR JOB: an observer that uses the C# EVENT.
/// Sends an SMS when a package is OutForDelivery or Delivered, if we know the
/// recipient's phone number. Other statuses don't send anything.
/// Texts: "Ditt paket {nr} är på väg och levereras idag." / "Ditt paket {nr} har levererats."
/// </summary>
public class SmsNotifier(ISmsGateway sms, IReadOnlyDictionary<string, string> phoneByTrackingNumber)
{
    /// <summary>Start listening to the tracker's StatusChanged event.</summary>
    public void Attach(PackageTracker tracker) => throw new NotImplementedException();

    /// <summary>Stop listening. After this, no more SMS are sent for that tracker.</summary>
    public void Detach(PackageTracker tracker) => throw new NotImplementedException();
}

/// <summary>
/// 🛠️ YOUR JOB: an observer that uses IObserver&lt;StatusChange&gt;.
/// Records every change, and knows whether the tracker has completed.
/// </summary>
public class HistoryLog : IObserver<StatusChange>
{
    public bool IsCompleted => throw new NotImplementedException();

    /// <summary>All changes for one package, oldest first.</summary>
    public IReadOnlyList<StatusChange> For(string trackingNumber) => throw new NotImplementedException();

    public void OnNext(StatusChange value) => throw new NotImplementedException();
    public void OnError(Exception error) => throw new NotImplementedException();
    public void OnCompleted() => throw new NotImplementedException();
}
