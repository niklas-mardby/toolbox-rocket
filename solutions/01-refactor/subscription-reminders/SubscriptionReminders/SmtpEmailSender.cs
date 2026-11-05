namespace SubscriptionReminders;

/// <summary>Pretends to send real email.</summary>
public class SmtpEmailSender(string host, int port) : IEmailSender
{
    public void Send(string to, string subject, string body)
    {
        Console.WriteLine($"   📧 [{host}:{port}] To: {to} | {subject} | {body}");
    }
}
