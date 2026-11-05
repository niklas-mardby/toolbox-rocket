using GlobalCard;
using QuickPay.Sdk;

namespace PaymentProviderAdapter;

/// <summary>
/// 🛠️ YOUR JOB: make QuickPayClient look like an IPaymentProvider.
/// Translate money to öre, expiry to "YYMM", and status/reason codes to PaymentResult.
/// </summary>
public class QuickPayAdapter(QuickPayClient client) : IPaymentProvider
{
    public string Name => "QuickPay";

    public Task<PaymentResult> ChargeAsync(Money amount, CardDetails card, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<PaymentResult> RefundAsync(string paymentId, Money amount, CancellationToken ct = default) =>
        throw new NotImplementedException();
}

/// <summary>
/// 🛠️ YOUR JOB: make GlobalCardGateway look like an IPaymentProvider.
/// Translate money to cents and Iso4217, and EXCEPTIONS to PaymentResult.
/// </summary>
public class GlobalCardAdapter(GlobalCardGateway gateway) : IPaymentProvider
{
    public string Name => "GlobalCard";

    public Task<PaymentResult> ChargeAsync(Money amount, CardDetails card, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<PaymentResult> RefundAsync(string paymentId, Money amount, CancellationToken ct = default) =>
        throw new NotImplementedException();
}
