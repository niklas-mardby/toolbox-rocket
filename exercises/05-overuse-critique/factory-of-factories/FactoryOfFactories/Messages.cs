namespace FactoryOfFactories;

public record WelcomeMessage(string To, string Subject, string Body);

public class Outbox
{
    private readonly List<WelcomeMessage> _messages = [];
    public IReadOnlyList<WelcomeMessage> Messages => _messages;
    public void Add(WelcomeMessage message) => _messages.Add(message);
}
