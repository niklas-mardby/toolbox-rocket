using CardGateway.Sdk;
using Swish.Sdk;
using Vipps.Sdk;

namespace SecondPaymentProvider.Tests;

public abstract class CheckoutTestBase
{
    protected readonly SwishClient Swish = new();
    protected readonly VippsClient Vipps = new();
    protected readonly CardProcessor Cards = new();
    protected readonly CheckoutService Checkout;

    protected CheckoutTestBase() => Checkout = Composition.CreateCheckout(Swish, Vipps, Cards);

    protected static CheckoutRequest Request(string country, decimal amount, string currency, string? phone = null, string? card = null) =>
        new("1001", country, amount, currency, phone, card);
}
