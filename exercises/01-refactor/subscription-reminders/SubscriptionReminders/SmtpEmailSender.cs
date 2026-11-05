namespace SubscriptionReminders;

/// <summary>Pretends to send real email. You definitely don't want this to run in your tests.</summary>
public class SmtpEmailSender(string host, int port)
{
    public void Send(string to, string subject, string body)
    {
        Console.WriteLine($"   📧 [{host}:{port}] To: {to} | {subject} | {body}");
    }
}
