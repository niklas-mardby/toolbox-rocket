using GlobalCard;
using QuickPay.Sdk;

namespace PaymentProviderAdapter.Tests;

/// <summary>
/// CONTRACT TESTS: the rules EVERY IPaymentProvider must follow, whatever SDK is behind it.
/// The same tests run once per adapter, via the two small subclasses at the bottom.
/// If you write a third adapter, add one more subclass and you get all these tests for free.
/// </summary>
public abstract class PaymentProviderContractTests
{
    protected abstract IPaymentProvider CreateProvider();

    private static CardDetails Card(string number) => new(number, 12, 2030, "123");
    private static readonly CardDetails GoodCard = Card("4111111111111111");
    private static Money Sek(decimal amount) => new(amount, "SEK");

    [Fact(Skip = Todo.Skip)]
    public async Task Successful_charge_returns_a_payment_id()
    {
        var result = await CreateProvider().ChargeAsync(Sek(249.50m), GoodCard);

        Assert.True(result.Succeeded);
        Assert.False(string.IsNullOrEmpty(result.PaymentId));
        Assert.Null(result.Failure);
    }

    [Theory(Skip = Todo.Skip)]
    [InlineData("4000000000000002", FailureReason.InsufficientFunds)]
    [InlineData("4000000000000069", FailureReason.CardExpired)]
    [InlineData("123", FailureReason.InvalidCard)]
    [InlineData("4000000000000119", FailureReason.ProviderUnavailable)]
    public async Task Failed_charge_is_translated_to_our_failure_reason(string cardNumber, FailureReason expected)
    {
        var result = await CreateProvider().ChargeAsync(Sek(100m), Card(cardNumber));

        Assert.False(result.Succeeded);
        Assert.Null(result.PaymentId);
        Assert.Equal(expected, result.Failure);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Failures_are_results_not_exceptions()
    {
        var provider = CreateProvider();

        var exception = await Record.ExceptionAsync(() => provider.ChargeAsync(Sek(100m), Card("4000000000000119")));

        Assert.Null(exception);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Refunds_can_be_partial_and_are_converted_to_minor_units_exactly()
    {
        var provider = CreateProvider();
        var charge = await provider.ChargeAsync(Sek(199.99m), GoodCard);

        var first = await provider.RefundAsync(charge.PaymentId!, Sek(100.00m));
        var second = await provider.RefundAsync(charge.PaymentId!, Sek(99.99m));
        var tooMuch = await provider.RefundAsync(charge.PaymentId!, Sek(0.01m));

        Assert.True(first.Succeeded);
        Assert.True(second.Succeeded);
        Assert.False(tooMuch.Succeeded);
        Assert.Equal(FailureReason.RefundTooLarge, tooMuch.Failure);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Refunding_an_unknown_payment_fails()
    {
        var result = await CreateProvider().RefundAsync("no-such-payment", Sek(10m));

        Assert.False(result.Succeeded);
        Assert.Equal(FailureReason.UnknownPayment, result.Failure);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Amounts_with_more_than_two_decimals_are_rejected()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CreateProvider().ChargeAsync(Sek(10.005m), GoodCard));
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Non_positive_amounts_are_rejected()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CreateProvider().ChargeAsync(Sek(0m), GoodCard));
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Unsupported_currency_is_a_failure()
    {
        var result = await CreateProvider().ChargeAsync(new Money(10m, "JPY"), GoodCard);

        Assert.Equal(FailureReason.UnsupportedCurrency, result.Failure);
    }
}

public class QuickPayAdapterTests : PaymentProviderContractTests
{
    protected override IPaymentProvider CreateProvider() => new QuickPayAdapter(new QuickPayClient());

    [Fact(Skip = Todo.Skip)]
    public async Task QuickPay_does_not_support_euro()
    {
        var result = await CreateProvider().ChargeAsync(new Money(10m, "EUR"), new CardDetails("4111111111111111", 12, 2030, "123"));

        Assert.Equal(FailureReason.UnsupportedCurrency, result.Failure);
    }
}

public class GlobalCardAdapterTests : PaymentProviderContractTests
{
    protected override IPaymentProvider CreateProvider() => new GlobalCardAdapter(new GlobalCardGateway());

    [Fact(Skip = Todo.Skip)]
    public async Task GlobalCard_does_not_support_norwegian_kroner()
    {
        var result = await CreateProvider().ChargeAsync(new Money(10m, "NOK"), new CardDetails("4111111111111111", 12, 2030, "123"));

        Assert.Equal(FailureReason.UnsupportedCurrency, result.Failure);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Suspected_fraud_is_reported_as_other()
    {
        var result = await CreateProvider().ChargeAsync(new Money(10m, "SEK"), new CardDetails("4100000000000019", 12, 2030, "123"));

        Assert.Equal(FailureReason.Other, result.Failure);
    }
}
