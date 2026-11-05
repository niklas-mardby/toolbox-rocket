namespace SecondPaymentProvider.Tests;

// ✨ The new requirement. Switch these on with Todo.Skip = null. Don't change these tests.
public class NewMarketsAcceptanceTests : CheckoutTestBase
{
    // --- Norway: Vipps ---

    [Fact(Skip = Todo.Skip)]
    public async Task Norwegian_customers_pay_with_vipps()
    {
        var result = await Checkout.CheckoutAsync(Request("NO", 499.50m, "NOK", phone: "+47 900 00 001"));

        Assert.True(result.Paid);
        Assert.StartsWith("VP-", result.Reference);
        var sent = Assert.Single(Vipps.Payments);
        Assert.Equal("4790000001", sent.MobileNumber);
        Assert.Equal(49950, sent.AmountInOre);
        Assert.Equal("1001", sent.OrderId);
        Assert.Equal("Order 1001", sent.Text);
        Assert.Empty(Swish.Requests);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Rejected_vipps_payment()
    {
        var result = await Checkout.CheckoutAsync(Request("NO", 499m, "NOK", phone: "+4790000000"));

        Assert.Equal(CheckoutResult.Failure("Betalningen nekades."), result);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Vipps_needs_a_phone_number()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Checkout.CheckoutAsync(Request("NO", 499m, "NOK")));
    }

    // --- Rest of Europe: card ---

    [Theory(Skip = Todo.Skip)]
    [InlineData("DE", "EUR")]
    [InlineData("FI", "EUR")]
    [InlineData("DK", "DKK")]
    public async Task Other_european_customers_pay_by_card(string country, string currency)
    {
        var result = await Checkout.CheckoutAsync(Request(country, 19.99m, currency, card: "4111111111111111"));

        Assert.True(result.Paid);
        Assert.StartsWith("CD-", result.Reference);
        var auth = Assert.Single(Cards.Authorizations);
        Assert.Equal(1999, auth.AmountInCents);
        Assert.Equal(currency, auth.Currency);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Declined_card()
    {
        var result = await Checkout.CheckoutAsync(Request("DE", 19.99m, "EUR", card: "4000000000000002"));

        Assert.Equal(CheckoutResult.Failure("Betalningen nekades."), result);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Card_payments_need_a_card_number()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Checkout.CheckoutAsync(Request("DE", 19.99m, "EUR")));
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Each_payment_goes_to_exactly_one_provider()
    {
        await Checkout.CheckoutAsync(Request("SE", 1m, "SEK", phone: "+46701234567"));
        await Checkout.CheckoutAsync(Request("NO", 1m, "NOK", phone: "+4790000001"));
        await Checkout.CheckoutAsync(Request("DE", 1m, "EUR", card: "4111111111111111"));

        Assert.Single(Swish.Requests);
        Assert.Single(Vipps.Payments);
        Assert.Single(Cards.Authorizations);
    }
}
