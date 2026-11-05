using System.Text.RegularExpressions;

namespace MessageSenderDecorators;

/// <summary>Logs before, after and on failure. Rethrows with `throw;` to keep the original exception.</summary>
public class LoggingMessageSender(IMessageSender inner, ILogSink log) : IMessageSender
{
    public async Task SendAsync(Message message, CancellationToken ct = default)
    {
        log.Write($"Sending to {message.To}: {message.Subject}");
        try
        {
            await inner.SendAsync(message, ct);
            log.Write($"Sent to {message.To}");
        }
        catch (Exception ex)
        {
            log.Write($"Failed to {message.To}: {ex.GetType().Name}");
            throw;
        }
    }
}

/// <summary>Retries transient failures only. Other exceptions pass straight through.</summary>
public class RetryingMessageSender : IMessageSender
{
    private readonly IMessageSender _inner;
    private readonly int _maxAttempts;

    public RetryingMessageSender(IMessageSender inner, int maxAttempts)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        _inner = inner;
        _maxAttempts = maxAttempts;
    }

    public async Task SendAsync(Message message, CancellationToken ct = default)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await _inner.SendAsync(message, ct);
                return;
            }
            catch (TransientSendException) when (attempt < _maxAttempts)
            {
                // Swallow and try again. On the LAST attempt the filter is false,
                // so the exception propagates unchanged.
                // A real implementation would wait here: await Task.Delay(backoff, ct);
            }
        }
    }
}

/// <summary>Changes the message on its way through, then passes it on.</summary>
public class ProfanityFilterMessageSender(IMessageSender inner, IEnumerable<string> bannedWords) : IMessageSender
{
    private readonly Regex _pattern = new(
        $@"\b({string.Join("|", bannedWords.Select(Regex.Escape))})\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public Task SendAsync(Message message, CancellationToken ct = default) =>
        inner.SendAsync(message with { Subject = Clean(message.Subject), Body = Clean(message.Body) }, ct);

    private string Clean(string text) => _pattern.Replace(text, "***");
}

/// <summary>Sliding-window rate limit. Stops the call BEFORE it reaches inner (short-circuits).</summary>
public class RateLimitingMessageSender(IMessageSender inner, int maxMessages, TimeSpan window, TimeProvider time) : IMessageSender
{
    private readonly Queue<DateTimeOffset> _sentAt = new();
    private readonly Lock _lock = new();

    public Task SendAsync(Message message, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var now = time.GetUtcNow();
            while (_sentAt.Count > 0 && now - _sentAt.Peek() >= window)
                _sentAt.Dequeue();

            if (_sentAt.Count >= maxMessages)
                throw new RateLimitExceededException();

            _sentAt.Enqueue(now);
        }

        return inner.SendAsync(message, ct);
    }
}

/// <summary>Fluent stacking. Each call wraps the current sender, so the last call is the outermost layer.</summary>
public static class MessageSenderExtensions
{
    public static IMessageSender WithLogging(this IMessageSender sender, ILogSink log) =>
        new LoggingMessageSender(sender, log);

    public static IMessageSender WithRetry(this IMessageSender sender, int maxAttempts) =>
        new RetryingMessageSender(sender, maxAttempts);

    public static IMessageSender WithProfanityFilter(this IMessageSender sender, params string[] bannedWords) =>
        new ProfanityFilterMessageSender(sender, bannedWords);

    public static IMessageSender WithRateLimit(this IMessageSender sender, int maxMessages, TimeSpan window, TimeProvider time) =>
        new RateLimitingMessageSender(sender, maxMessages, window, time);
}
