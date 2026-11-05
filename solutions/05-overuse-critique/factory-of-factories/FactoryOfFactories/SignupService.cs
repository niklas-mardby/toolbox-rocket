namespace FactoryOfFactories;

public class SignupService(Outbox outbox)
{
    private readonly HashSet<string> _registeredEmails = new(StringComparer.OrdinalIgnoreCase);

    public void Register(string email, string name)
    {
        email = email.Trim();
        name = name.Trim();

        if (!email.Contains('@'))
            throw new ArgumentException("Invalid email address.", nameof(email));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (!_registeredEmails.Add(email))
            throw new InvalidOperationException($"{email} is already registered.");

        outbox.Add(WelcomeMessage.For(email, name));
    }
}
