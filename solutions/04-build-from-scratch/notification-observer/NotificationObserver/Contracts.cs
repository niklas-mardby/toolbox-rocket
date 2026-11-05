namespace NotificationObserver;

// ✅ These types are finished. You don't need to change them.

public enum PackageStatus
{
    Registered,
    InTransit,
    OutForDelivery,
    Delivered
}

/// <summary>A change of status. OldStatus is null the first time a package is seen.</summary>
public record StatusChange(string TrackingNumber, PackageStatus? OldStatus, PackageStatus NewStatus, DateTimeOffset At);

public class StatusChangedEventArgs(StatusChange change) : EventArgs
{
    public StatusChange Change { get; } = change;
}

public interface ISmsGateway
{
    void Send(string phoneNumber, string text);
}
