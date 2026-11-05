namespace MessageSenderDecorators;

// ✅ Finished. You don't need to change these.

public record Message(string To, string Subject, string Body);

/// <summary>The component interface. Every decorator implements it AND wraps one.</summary>
public interface IMessageSender
{
    Task SendAsync(Message message, CancellationToken ct = default);
}

/// <summary>A temporary failure (network glitch, server busy). Worth trying again.</summary>
public class TransientSendException(string message) : Exception(message);

/// <summary>Thrown when too many messages are sent too quickly.</summary>
public class RateLimitExceededException() : Exception("Too many messages. Try again later.");

public interface ILogSink
{
    void Write(string line);
}

public class InMemoryLogSink : ILogSink
{
    private readonly List<string> _lines = [];
    public IReadOnlyList<string> Lines => _lines;
    public void Write(string line) => _lines.Add(line);
}

public class ConsoleLogSink : ILogSink
{
    public void Write(string line) => Console.WriteLine($"   📝 {line}");
}

/// <summary>The concrete component: the thing that actually "sends".</summary>
public class ConsoleMessageSender : IMessageSender
{
    public Task SendAsync(Message message, CancellationToken ct = default)
    {
        Console.WriteLine($"✉️  Till {message.To}: {message.Subject} | {message.Body}");
        return Task.CompletedTask;
    }
}
