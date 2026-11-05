using CardGateway.Sdk;
using Swish.Sdk;
using Vipps.Sdk;

namespace SecondPaymentProvider;

/// <summary>
/// The composition root. The tests only use CreateCheckout, so you may change how things are wired.
/// All three SDK clients are passed in: the contracts with Vipps and the card gateway are already signed.
/// </summary>
public static class Composition
{
    public static CheckoutService CreateCheckout(SwishClient swish, VippsClient vipps, CardProcessor cards) =>
        new(swish);
}
