namespace MessageSenderDecorators;

/// <summary>
/// 🛠️ YOUR JOB. Writes to the log:
///   before:  "Sending to {To}: {Subject}"
///   after:   "Sent to {To}"
///   failure: "Failed to {To}: {ExceptionTypeName}"  and then rethrows the SAME exception.
/// </summary>
public class LoggingMessageSender(IMessageSender inner, ILogSink log) : IMessageSender
{
    public Task SendAsync(Message message, CancellationToken ct = default) => throw new NotImplementedException();
}

/// <summary>
/// 🛠️ YOUR JOB. Tries up to maxAttempts times IN TOTAL, but only for TransientSendException.
/// Other exceptions are not retried. When all attempts fail, the last TransientSendException is rethrown.
/// maxAttempts below 1 -> ArgumentOutOfRangeException (in the constructor).
/// </summary>
public class RetryingMessageSender(IMessageSender inner, int maxAttempts) : IMessageSender
{
    public Task SendAsync(Message message, CancellationToken ct = default) => throw new NotImplementedException();
}

/// <summary>
/// 🛠️ YOUR JOB. Replaces banned words with "***" in Subject and Body before passing the message on.
/// Case-insensitive, whole words only: banning "fan" must not change "fantastisk".
/// </summary>
public class ProfanityFilterMessageSender(IMessageSender inner, IEnumerable<string> bannedWords) : IMessageSender
{
    public Task SendAsync(Message message, CancellationToken ct = default) => throw new NotImplementedException();
}

/// <summary>
/// 🛠️ YOUR JOB. Allows at most maxMessages within any sliding time window.
/// If the limit is reached, throws RateLimitExceededException WITHOUT calling inner.
/// A message sent exactly `window` ago no longer counts.
/// </summary>
public class RateLimitingMessageSender(IMessageSender inner, int maxMessages, TimeSpan window, TimeProvider time) : IMessageSender
{
    public Task SendAsync(Message message, CancellationToken ct = default) => throw new NotImplementedException();
}

/// <summary>
/// 🛠️ YOUR JOB. Extension methods so decorators can be stacked fluently:
///   sender.WithProfanityFilter(words).WithRetry(3).WithLogging(log)
/// Each method wraps the sender it's called on. The LAST call becomes the OUTERMOST layer.
/// </summary>
public static class MessageSenderExtensions
{
    public static IMessageSender WithLogging(this IMessageSender sender, ILogSink log) => throw new NotImplementedException();

    public static IMessageSender WithRetry(this IMessageSender sender, int maxAttempts) => throw new NotImplementedException();

    public static IMessageSender WithProfanityFilter(this IMessageSender sender, params string[] bannedWords) => throw new NotImplementedException();

    public static IMessageSender WithRateLimit(this IMessageSender sender, int maxMessages, TimeSpan window, TimeProvider time) =>
        throw new NotImplementedException();
}
