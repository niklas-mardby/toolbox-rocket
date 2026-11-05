namespace LegacyPaymentGateway.Tests;

// These tests must stay green before AND after your refactoring. Don't change them.
// They only use Composition.CreatePaymentServices(), which you may change.
public class PaymentTests
{
    private static readonly Card ValidCard = new("4111111111111111", 12, 2099);
    private readonly PaymentServices _services = Composition.CreatePaymentServices();

    [Fact]
    public async Task Successful_checkout_returns_a_transaction_id()
    {
        var result = await _services.Checkout.CheckoutAsync(499.90m, "SEK", ValidCard);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.TransactionId));
        Assert.Null(result.ErrorMessage);
    }

    [Theory]
    [InlineData("4000000000000002", 12, 2099, "Kortet saknar täckning.")]
    [InlineData("4111111111111111", 1, 2020, "Kortet har gått ut.")]
    [InlineData("1234", 12, 2099, "Ogiltigt kortnummer.")]
    [InlineData("4000000000000119", 12, 2099, "Betalningen kunde inte genomföras. Försök igen senare.")]
    public async Task Failed_checkout_gives_a_swedish_error_message(string number, int month, int year, string expected)
    {
        var result = await _services.Checkout.CheckoutAsync(100m, "SEK", new Card(number, month, year));

        Assert.False(result.Success);
        Assert.Null(result.TransactionId);
        Assert.Equal(expected, result.ErrorMessage);
    }

    [Fact]
    public async Task Temporary_errors_are_retried_once()
    {
        // This test card fails with a temporary error the first time and succeeds the second.
        var result = await _services.Checkout.CheckoutAsync(100m, "SEK", new Card("4000000000000127", 12, 2099));

        Assert.True(result.Success);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Checkout_rejects_non_positive_amounts(double amount)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _services.Checkout.CheckoutAsync((decimal)amount, "SEK", ValidCard));
    }

    [Fact]
    public async Task Partial_refunds_work_until_the_charge_is_used_up()
    {
        var purchase = await _services.Checkout.CheckoutAsync(199.99m, "SEK", ValidCard);

        var first = await _services.Refunds.RefundAsync(purchase.TransactionId!, 100m);
        var second = await _services.Refunds.RefundAsync(purchase.TransactionId!, 99.99m);
        var third = await _services.Refunds.RefundAsync(purchase.TransactionId!, 0.01m);

        Assert.True(first.Success);
        Assert.True(second.Success);
        Assert.False(third.Success);
        Assert.Equal("Beloppet är större än köpet.", third.ErrorMessage);
    }

    [Fact]
    public async Task Refunding_an_unknown_transaction_fails()
    {
        var result = await _services.Refunds.RefundAsync("does-not-exist", 10m);

        Assert.False(result.Success);
        Assert.Equal("Köpet hittades inte.", result.ErrorMessage);
    }

    [Fact]
    public async Task Refund_rejects_non_positive_amounts()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _services.Refunds.RefundAsync("any", 0m));
    }
}
