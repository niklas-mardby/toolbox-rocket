namespace NotificationChannels;

// The gateways to the outside world. They already exist (the marketing module uses SMS and push).
// You don't need to change these.

public interface IEmailGateway
{
    void Send(string to, string subject, string body);
}

public interface ISmsGateway
{
    void Send(string phoneNumber, string text);
}

public interface IPushGateway
{
    void Send(string deviceToken, string title, string body);
}

public class ConsoleEmailGateway : IEmailGateway
{
    public void Send(string to, string subject, string body) => Console.WriteLine($"📧 {to} | {subject} | {body}");
}

public class ConsoleSmsGateway : ISmsGateway
{
    public void Send(string phoneNumber, string text) => Console.WriteLine($"📱 {phoneNumber} | {text}");
}

public class ConsolePushGateway : IPushGateway
{
    public void Send(string deviceToken, string title, string body) => Console.WriteLine($"🔔 {deviceToken} | {title} | {body}");
}
