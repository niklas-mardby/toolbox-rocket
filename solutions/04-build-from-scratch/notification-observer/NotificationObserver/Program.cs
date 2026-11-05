using NotificationObserver;

// A small demo. It throws NotImplementedException until you've built the classes.
var tracker = new PackageTracker(TimeProvider.System);
var history = new HistoryLog();
var sms = new SmsNotifier(new ConsoleSms(), new Dictionary<string, string> { ["PKG-1"] = "0701234567" });

using var subscription = tracker.Subscribe(history);
sms.Attach(tracker);

tracker.UpdateStatus("PKG-1", PackageStatus.Registered);
tracker.UpdateStatus("PKG-1", PackageStatus.InTransit);
tracker.UpdateStatus("PKG-1", PackageStatus.InTransit); // no change, so no notifications
tracker.UpdateStatus("PKG-1", PackageStatus.OutForDelivery);
tracker.UpdateStatus("PKG-1", PackageStatus.Delivered);

Console.WriteLine("📜 Historik för PKG-1:");
foreach (var change in history.For("PKG-1"))
    Console.WriteLine($"   {change.OldStatus?.ToString() ?? "-",-15} -> {change.NewStatus}");

class ConsoleSms : ISmsGateway
{
    public void Send(string phoneNumber, string text) => Console.WriteLine($"📱 SMS till {phoneNumber}: {text}");
}
