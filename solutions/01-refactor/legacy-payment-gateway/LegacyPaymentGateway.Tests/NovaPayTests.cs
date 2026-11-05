namespace LegacyPaymentGateway.Tests;

// The bonus: the SAME behaviour with NovaPay behind the interface.
// (A nicer variant: make PaymentTests abstract with one subclass per provider.)
public class NovaPayTests
{
    private static readonly Card ValidCard = new("4111111111111111", 12, 2099);
    private readonly PaymentServices _services = Composition.CreateWithNovaPay();

    [Fact]
    public async Task Successful_checkout_and_partial_refunds()
    {
        var purchase = await _services.Checkout.CheckoutAsync(199.99m, "SEK", ValidCard);

        Assert.True(purchase.Success);
        Assert.True((await _services.Refunds.RefundAsync(purchase.TransactionId!, 199.99m)).Success);
        Assert.Equal("Beloppet är större än köpet.", (await _services.Refunds.RefundAsync(purchase.TransactionId!, 0.01m)).ErrorMessage);
    }

    [Theory]
    [InlineData("4000000000000002", 12, 2099, "Kortet saknar täckning.")]
    [InlineData("4111111111111111", 1, 2020, "Kortet har gått ut.")]
    [InlineData("1234", 12, 2099, "Ogiltigt kortnummer.")]
    [InlineData("4000000000000119", 12, 2099, "Betalningen kunde inte genomföras. Försök igen senare.")]
    public async Task Same_customer_messages_as_with_LegacyPay(string number, int month, int year, string expected)
    {
        var result = await _services.Checkout.CheckoutAsync(100m, "SEK", new Card(number, month, year));

        Assert.Equal(expected, result.ErrorMessage);
    }

    [Fact]
    public async Task Temporary_errors_are_retried_once()
    {
        Assert.True((await _services.Checkout.CheckoutAsync(100m, "SEK", new Card("4000000000000127", 12, 2099))).Success);
    }

    [Fact]
    public async Task Unknown_transaction()
    {
        Assert.Equal("Köpet hittades inte.", (await _services.Refunds.RefundAsync("nope", 1m)).ErrorMessage);
    }
}
