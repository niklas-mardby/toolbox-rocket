using MessageSenderDecorators;

// A small demo. It throws NotImplementedException until you've built the decorators.
IMessageSender sender = new ConsoleMessageSender()
    .WithProfanityFilter("fan", "jäkla")
    .WithRetry(3)
    .WithRateLimit(maxMessages: 2, window: TimeSpan.FromSeconds(10), TimeProvider.System)
    .WithLogging(new ConsoleLogSink());

await sender.SendAsync(new Message("anna@example.com", "Fantastiskt!", "Vilken jäkla bra dag."));
await sender.SendAsync(new Message("bo@example.com", "Hej", "Fan vad kul."));

try
{
    await sender.SendAsync(new Message("cilla@example.com", "Ett till", "Det här blir ett för mycket."));
}
catch (RateLimitExceededException ex)
{
    Console.WriteLine($"🛑 {ex.Message}");
}
