namespace FactoryOfFactories;

public record WelcomeMessage(string To, string Subject, string Body)
{
    /// <summary>The ONE place that knows what a welcome message says. A static factory method is plenty.</summary>
    public static WelcomeMessage For(string email, string name) => new(
        email,
        $"Välkommen, {name}!",
        $"Hej {name}! Ditt konto {email} är nu skapat. Välkommen till Verktygsboden.");
}

public class Outbox
{
    private readonly List<WelcomeMessage> _messages = [];
    public IReadOnlyList<WelcomeMessage> Messages => _messages;
    public void Add(WelcomeMessage message) => _messages.Add(message);
}
