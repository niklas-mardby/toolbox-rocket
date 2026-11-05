namespace NotificationChannels.Tests;

public class RecordingEmail : IEmailGateway
{
    public List<(string To, string Subject, string Body)> Sent { get; } = [];
    public void Send(string to, string subject, string body) => Sent.Add((to, subject, body));
}

public class RecordingSms : ISmsGateway
{
    public List<(string Phone, string Text)> Sent { get; } = [];
    public void Send(string phoneNumber, string text) => Sent.Add((phoneNumber, text));
}

public class RecordingPush : IPushGateway
{
    public List<(string Token, string Title, string Body)> Sent { get; } = [];
    public void Send(string deviceToken, string title, string body) => Sent.Add((deviceToken, title, body));
}

public abstract class NotificationTestBase
{
    protected readonly RecordingEmail Email = new();
    protected readonly RecordingSms Sms = new();
    protected readonly RecordingPush Push = new();

    protected NotificationService CreateService(params Customer[] customers) =>
        Composition.CreateNotificationService(new InMemoryCustomerDirectory(customers), Email, Sms, Push);

    protected static Customer Customer(
        string id = "anna",
        string? phone = "0701234567",
        string? pushToken = "device-1",
        params NotificationChannel[] channels) =>
        new(id, "Anna", "anna@example.com", phone, pushToken, channels);
}
