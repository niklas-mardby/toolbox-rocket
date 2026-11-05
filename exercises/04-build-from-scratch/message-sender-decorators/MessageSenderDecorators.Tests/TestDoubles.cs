namespace MessageSenderDecorators.Tests;

/// <summary>Records every message that reaches it.</summary>
public class RecordingSender : IMessageSender
{
    public List<Message> Sent { get; } = [];

    public Task SendAsync(Message message, CancellationToken ct = default)
    {
        Sent.Add(message);
        return Task.CompletedTask;
    }
}

/// <summary>Fails with a transient error the first N times, then succeeds.</summary>
public class FlakySender(int failuresBeforeSuccess) : IMessageSender
{
    public int Attempts { get; private set; }

    public Task SendAsync(Message message, CancellationToken ct = default)
    {
        Attempts++;
        if (Attempts <= failuresBeforeSuccess)
            throw new TransientSendException($"Attempt {Attempts} failed");
        return Task.CompletedTask;
    }
}

/// <summary>Always throws the given exception.</summary>
public class FailingSender(Exception exception) : IMessageSender
{
    public int Attempts { get; private set; }

    public Task SendAsync(Message message, CancellationToken ct = default)
    {
        Attempts++;
        throw exception;
    }
}

public class ManualTime(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;
    public override DateTimeOffset GetUtcNow() => _now;
    public void Advance(TimeSpan by) => _now += by;
}
