namespace FactoryOfFactories;

// "Enterprise-grade, pluggable message creation infrastructure." – the original author
//
// The app sends exactly one kind of message: a welcome message when someone signs up.

// --- Layer 1: builders ---

public interface IMessageBuilder
{
    IMessageBuilder SetRecipient(string recipient);
    IMessageBuilder SetSubject(string subject);
    IMessageBuilder SetBody(string body);
    WelcomeMessage Build();
}

public class WelcomeMessageBuilder : IMessageBuilder
{
    private string? _recipient;
    private string? _subject;
    private string? _body;

    public IMessageBuilder SetRecipient(string recipient) { _recipient = recipient; return this; }
    public IMessageBuilder SetSubject(string subject) { _subject = subject; return this; }
    public IMessageBuilder SetBody(string body) { _body = body; return this; }

    public WelcomeMessage Build() =>
        new(_recipient ?? throw new InvalidOperationException("Recipient not set."),
            _subject ?? throw new InvalidOperationException("Subject not set."),
            _body ?? throw new InvalidOperationException("Body not set."));
}

// --- Layer 2: a factory for builders ---

public interface IMessageBuilderFactory
{
    IMessageBuilder CreateBuilder();
}

public class WelcomeMessageBuilderFactory : IMessageBuilderFactory
{
    public IMessageBuilder CreateBuilder() => new WelcomeMessageBuilder();
}

// --- Layer 3: a factory for messages ---

public interface IMessageFactory
{
    WelcomeMessage Create(string email, string name);
}

public class WelcomeMessageFactory(IMessageBuilderFactory builderFactory) : IMessageFactory
{
    public WelcomeMessage Create(string email, string name) =>
        builderFactory.CreateBuilder()
            .SetRecipient(email)
            .SetSubject($"Välkommen, {name}!")
            .SetBody($"Hej {name}! Ditt konto {email} är nu skapat. Välkommen till Verktygsboden.")
            .Build();
}

// --- Layer 4: a factory for message factories ---

public interface IMessageFactoryFactory
{
    IMessageFactory CreateFactory(string messageType);
}

public class DefaultMessageFactoryFactory : IMessageFactoryFactory
{
    public IMessageFactory CreateFactory(string messageType) => messageType switch
    {
        "welcome" => new WelcomeMessageFactory(new WelcomeMessageBuilderFactory()),
        _ => throw new NotSupportedException($"No factory for message type '{messageType}'.")
    };
}

// --- Layer 5: a singleton that provides the factory factory ---

public sealed class MessageFactoryProvider
{
    private static readonly Lazy<MessageFactoryProvider> LazyInstance = new(() => new MessageFactoryProvider());
    private readonly IMessageFactoryFactory _factoryFactory = new DefaultMessageFactoryFactory();

    private MessageFactoryProvider() { }

    public static MessageFactoryProvider Instance => LazyInstance.Value;

    public IMessageFactoryFactory GetFactoryFactory() => _factoryFactory;
}
