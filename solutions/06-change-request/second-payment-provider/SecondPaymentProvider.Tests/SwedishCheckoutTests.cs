namespace SecondPaymentProvider.Tests;

// Existing behaviour. Must stay green throughout. Don't change these tests.
public class SwedishCheckoutTests : CheckoutTestBase
{
    [Fact]
    public async Task Swedish_customers_pay_with_swish()
    {
        var result = await Checkout.CheckoutAsync(Request("SE", 499m, "SEK", phone: "+46 701234567"));

        Assert.True(result.Paid);
        Assert.StartsWith("SW-", result.Reference);
        var sent = Assert.Single(Swish.Requests);
        Assert.Equal("46701234567", sent.PayerAlias);
        Assert.Equal(499m, sent.Amount);
        Assert.Equal("Order 1001", sent.Message);
    }

    [Fact]
    public async Task Declined_swish_payment()
    {
        var result = await Checkout.CheckoutAsync(Request("SE", 499m, "SEK", phone: "+46700000000"));

        Assert.Equal(CheckoutResult.Failure("Betalningen nekades."), result);
    }

    [Fact]
    public async Task Swish_errors_are_technical_errors()
    {
        var result = await Checkout.CheckoutAsync(Request("SE", 200_000m, "SEK", phone: "+46701234567"));

        Assert.Equal(CheckoutResult.Failure("Tekniskt fel, försök igen."), result);
    }

    [Fact]
    public async Task Swish_needs_a_phone_number()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Checkout.CheckoutAsync(Request("SE", 499m, "SEK")));
    }

    [Fact]
    public async Task Amount_must_be_positive()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Checkout.CheckoutAsync(Request("SE", 0m, "SEK", phone: "+46701234567")));
    }

    [Fact]
    public async Task Countries_outside_europe_are_not_supported()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() => Checkout.CheckoutAsync(Request("US", 10m, "USD", card: "4111111111111111")));
    }
}
