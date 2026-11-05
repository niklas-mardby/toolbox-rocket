using CardGateway.Sdk;
using Swish.Sdk;
using Vipps.Sdk;

namespace SecondPaymentProvider;

public static class Composition
{
    public static CheckoutService CreateCheckout(SwishClient swish, VippsClient vipps, CardProcessor cards) =>
        new(new PaymentProviderSelector(
            swish: new SwishAdapter(swish),
            vipps: new VippsAdapter(vipps),
            card: new CardAdapter(cards)));
}
