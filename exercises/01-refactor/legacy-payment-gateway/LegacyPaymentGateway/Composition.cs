using LegacyPay.Sdk;

namespace LegacyPaymentGateway;

public record PaymentServices(CheckoutService Checkout, RefundService Refunds);

/// <summary>
/// The composition root. The tests only use CreatePaymentServices(), so you may change
/// everything about how the services are built.
/// </summary>
public static class Composition
{
    public static PaymentServices CreatePaymentServices()
    {
        var legacyPay = new LegacyPayClient(merchantId: "TOOLBOX-0042");
        return new PaymentServices(new CheckoutService(legacyPay), new RefundService(legacyPay));
    }
}
