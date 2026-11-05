namespace PaymentProviderAdapter;

// ✅ OUR domain's view of payments. Finished. You don't need to change these.

public record Money(decimal Amount, string Currency);

public record CardDetails(string Number, int ExpiryMonth, int ExpiryYear, string Cvc);

public enum FailureReason
{
    InsufficientFunds,
    CardExpired,
    InvalidCard,
    UnsupportedCurrency,
    UnknownPayment,
    RefundTooLarge,
    ProviderUnavailable,
    Other
}

public record PaymentResult(bool Succeeded, string? PaymentId, FailureReason? Failure)
{
    public static PaymentResult Success(string paymentId) => new(true, paymentId, null);
    public static PaymentResult Failed(FailureReason reason) => new(false, null, reason);
}

/// <summary>The interface the rest of our app uses. It knows nothing about any SDK.</summary>
public interface IPaymentProvider
{
    string Name { get; }
    Task<PaymentResult> ChargeAsync(Money amount, CardDetails card, CancellationToken ct = default);
    Task<PaymentResult> RefundAsync(string paymentId, Money amount, CancellationToken ct = default);
}
