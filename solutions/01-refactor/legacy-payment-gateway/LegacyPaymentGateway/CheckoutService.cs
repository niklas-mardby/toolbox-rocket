namespace LegacyPaymentGateway;

/// <summary>Business code only: validation, the retry policy and what the CUSTOMER should read.</summary>
public class CheckoutService(IPaymentGateway gateway)
{
    public async Task<CheckoutResult> CheckoutAsync(decimal amount, string currency, Card card)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        var result = await gateway.ChargeAsync(amount, currency, card);

        // Temporary error: try once more.
        if (result.Failure == PaymentFailure.TemporaryError)
            result = await gateway.ChargeAsync(amount, currency, card);

        return result.Success
            ? CheckoutResult.Ok(result.TransactionId!)
            : CheckoutResult.Fail(result.Failure switch
            {
                PaymentFailure.InvalidCard => "Ogiltigt kortnummer.",
                PaymentFailure.InsufficientFunds => "Kortet saknar täckning.",
                PaymentFailure.CardExpired => "Kortet har gått ut.",
                _ => "Betalningen kunde inte genomföras. Försök igen senare."
            });
    }
}
