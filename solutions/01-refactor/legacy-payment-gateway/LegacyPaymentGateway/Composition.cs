using LegacyPay.Sdk;
using NovaPay;

namespace LegacyPaymentGateway;

public record PaymentServices(CheckoutService Checkout, RefundService Refunds);

public static class Composition
{
    /// <summary>
    /// The ONE place that knows which provider we use. Switching to NovaPay is changing this line:
    ///   IPaymentGateway gateway = new NovaPayAdapter(new NovaPayApi(apiKey: "..."));
    /// </summary>
    public static PaymentServices CreatePaymentServices()
    {
        IPaymentGateway gateway = new LegacyPayAdapter(new LegacyPayClient(merchantId: "TOOLBOX-0042"));
        return Create(gateway);
    }

    public static PaymentServices CreateWithNovaPay() => Create(new NovaPayAdapter(new NovaPayApi(apiKey: "test-key")));

    // Checkout and refunds share ONE gateway, because the provider remembers charges per client.
    private static PaymentServices Create(IPaymentGateway gateway) => new(new CheckoutService(gateway), new RefundService(gateway));
}
