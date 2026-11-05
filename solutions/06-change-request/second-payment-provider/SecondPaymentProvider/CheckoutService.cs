namespace SecondPaymentProvider;

/// <summary>Country is an ISO code ("SE", "NO", "DE"...). Phone is "+46...". CardNumber is only needed for card payments.</summary>
public record CheckoutRequest(string OrderId, string Country, decimal Amount, string Currency, string? Phone, string? CardNumber);

public record CheckoutResult(bool Paid, string? Reference, string? Error)
{
    public static CheckoutResult Success(string reference) => new(true, reference, null);
    public static CheckoutResult Failure(string error) => new(false, null, error);

    // The customer-facing messages, defined ONCE for every provider.
    public static CheckoutResult Declined() => Failure("Betalningen nekades.");
    public static CheckoutResult TechnicalError() => Failure("Tekniskt fel, försök igen.");
}

/// <summary>The checkout no longer knows which providers exist. It asks the selector and pays.</summary>
public class CheckoutService(IPaymentProviderSelector providers)
{
    public Task<CheckoutResult> CheckoutAsync(CheckoutRequest request)
    {
        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(request));

        return providers.For(request.Country).PayAsync(request);
    }
}
