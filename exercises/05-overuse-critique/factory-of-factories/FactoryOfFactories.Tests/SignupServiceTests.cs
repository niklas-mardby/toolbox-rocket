namespace FactoryOfFactories.Tests;

// These tests must stay green before AND after your simplification. Don't change them.
public class SignupServiceTests
{
    private readonly Outbox _outbox = new();
    private readonly SignupService _service;

    public SignupServiceTests() => _service = new SignupService(_outbox);

    [Fact]
    public void Registering_sends_a_welcome_message()
    {
        _service.Register("anna@example.com", "Anna");

        var message = Assert.Single(_outbox.Messages);
        Assert.Equal(
            new WelcomeMessage(
                "anna@example.com",
                "Välkommen, Anna!",
                "Hej Anna! Ditt konto anna@example.com är nu skapat. Välkommen till Verktygsboden."),
            message);
    }

    [Fact]
    public void Email_and_name_are_trimmed()
    {
        _service.Register("  bo@example.com ", " Bo ");

        var message = Assert.Single(_outbox.Messages);
        Assert.Equal("bo@example.com", message.To);
        Assert.Equal("Välkommen, Bo!", message.Subject);
    }

    [Theory]
    [InlineData("not-an-email", "Anna")]
    [InlineData("anna@example.com", "  ")]
    public void Invalid_input_is_rejected_and_nothing_is_sent(string email, string name)
    {
        Assert.Throws<ArgumentException>(() => _service.Register(email, name));
        Assert.Empty(_outbox.Messages);
    }

    [Fact]
    public void The_same_email_cannot_register_twice_regardless_of_case()
    {
        _service.Register("anna@example.com", "Anna");

        Assert.Throws<InvalidOperationException>(() => _service.Register("ANNA@example.com", "Anna igen"));
        Assert.Single(_outbox.Messages);
    }
}
