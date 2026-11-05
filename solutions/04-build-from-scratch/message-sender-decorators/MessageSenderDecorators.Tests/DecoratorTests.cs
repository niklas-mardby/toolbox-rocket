namespace MessageSenderDecorators.Tests;

public class DecoratorTests
{
    private static readonly Message Hello = new("anna@example.com", "Hej", "Hej Anna!");

    // --- Logging ---

    [Fact(Skip = Todo.Skip)]
    public async Task Logging_writes_before_and_after_and_passes_the_message_on()
    {
        var inner = new RecordingSender();
        var log = new InMemoryLogSink();

        await new LoggingMessageSender(inner, log).SendAsync(Hello);

        Assert.Equal(new[] { Hello }, inner.Sent);
        Assert.Equal(new[] { "Sending to anna@example.com: Hej", "Sent to anna@example.com" }, log.Lines);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Logging_logs_failures_and_rethrows_the_same_exception()
    {
        var original = new InvalidOperationException("boom");
        var log = new InMemoryLogSink();
        var sender = new LoggingMessageSender(new FailingSender(original), log);

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendAsync(Hello));

        Assert.Same(original, thrown);
        Assert.Equal(new[] { "Sending to anna@example.com: Hej", "Failed to anna@example.com: InvalidOperationException" }, log.Lines);
    }

    // --- Retry ---

    [Fact(Skip = Todo.Skip)]
    public async Task Retry_tries_again_after_transient_failures()
    {
        var flaky = new FlakySender(failuresBeforeSuccess: 2);

        await new RetryingMessageSender(flaky, maxAttempts: 3).SendAsync(Hello);

        Assert.Equal(3, flaky.Attempts);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Retry_gives_up_after_max_attempts_and_rethrows()
    {
        var flaky = new FlakySender(failuresBeforeSuccess: 10);
        var sender = new RetryingMessageSender(flaky, maxAttempts: 3);

        var ex = await Assert.ThrowsAsync<TransientSendException>(() => sender.SendAsync(Hello));

        Assert.Equal(3, flaky.Attempts);
        Assert.Equal("Attempt 3 failed", ex.Message);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Retry_does_not_retry_other_exceptions()
    {
        var failing = new FailingSender(new ArgumentException("bad address"));
        var sender = new RetryingMessageSender(failing, maxAttempts: 3);

        await Assert.ThrowsAsync<ArgumentException>(() => sender.SendAsync(Hello));
        Assert.Equal(1, failing.Attempts);
    }

    [Fact(Skip = Todo.Skip)]
    public void Retry_needs_at_least_one_attempt()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RetryingMessageSender(new RecordingSender(), 0));
    }

    // --- Profanity filter ---

    [Fact(Skip = Todo.Skip)]
    public async Task Filter_replaces_whole_banned_words_case_insensitively()
    {
        var inner = new RecordingSender();
        var sender = new ProfanityFilterMessageSender(inner, ["fan", "jäkla"]);

        await sender.SendAsync(new Message("bo@example.com", "Fan, en FANTASTISK dag", "Vilken jäkla tur. FAN!"));

        var sent = Assert.Single(inner.Sent);
        Assert.Equal("bo@example.com", sent.To);
        Assert.Equal("***, en FANTASTISK dag", sent.Subject);
        Assert.Equal("Vilken *** tur. ***!", sent.Body);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Filter_leaves_clean_messages_untouched()
    {
        var inner = new RecordingSender();

        await new ProfanityFilterMessageSender(inner, ["fan"]).SendAsync(Hello);

        Assert.Equal(Hello, inner.Sent.Single());
    }

    // --- Rate limit ---

    [Fact(Skip = Todo.Skip)]
    public async Task Rate_limit_blocks_the_message_over_the_limit_without_sending_it()
    {
        var inner = new RecordingSender();
        var time = new ManualTime(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var sender = new RateLimitingMessageSender(inner, maxMessages: 2, TimeSpan.FromMinutes(1), time);

        await sender.SendAsync(Hello);
        await sender.SendAsync(Hello);
        await Assert.ThrowsAsync<RateLimitExceededException>(() => sender.SendAsync(Hello));

        Assert.Equal(2, inner.Sent.Count);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Rate_limit_window_slides()
    {
        var inner = new RecordingSender();
        var time = new ManualTime(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var sender = new RateLimitingMessageSender(inner, maxMessages: 2, TimeSpan.FromMinutes(1), time);

        await sender.SendAsync(Hello);                 // 12:00:00
        time.Advance(TimeSpan.FromSeconds(30));
        await sender.SendAsync(Hello);                 // 12:00:30
        time.Advance(TimeSpan.FromSeconds(30));
        await sender.SendAsync(Hello);                 // 12:01:00, the first one no longer counts
        await Assert.ThrowsAsync<RateLimitExceededException>(() => sender.SendAsync(Hello));

        Assert.Equal(3, inner.Sent.Count);
    }

    // --- Stacking ---

    [Fact(Skip = Todo.Skip)]
    public async Task Extension_methods_stack_decorators_with_the_last_call_outermost()
    {
        var inner = new RecordingSender();
        var log = new InMemoryLogSink();

        var sender = inner.WithProfanityFilter("fan").WithLogging(log);
        await sender.SendAsync(new Message("bo@example.com", "fan", "hej"));

        Assert.IsType<LoggingMessageSender>(sender);
        Assert.Equal("***", inner.Sent.Single().Subject);
        Assert.Equal("Sending to bo@example.com: fan", log.Lines[0]); // logging is OUTSIDE the filter
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Logging_outside_retry_hides_the_retries()
    {
        var log = new InMemoryLogSink();
        var sender = new FlakySender(failuresBeforeSuccess: 2).WithRetry(3).WithLogging(log);

        await sender.SendAsync(Hello);

        Assert.Equal(new[] { "Sending to anna@example.com: Hej", "Sent to anna@example.com" }, log.Lines);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Logging_inside_retry_shows_every_attempt()
    {
        var log = new InMemoryLogSink();
        var sender = new FlakySender(failuresBeforeSuccess: 2).WithLogging(log).WithRetry(3);

        await sender.SendAsync(Hello);

        Assert.Equal(
            new[]
            {
                "Sending to anna@example.com: Hej", "Failed to anna@example.com: TransientSendException",
                "Sending to anna@example.com: Hej", "Failed to anna@example.com: TransientSendException",
                "Sending to anna@example.com: Hej", "Sent to anna@example.com",
            },
            log.Lines);
    }
}
