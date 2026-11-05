using Swish.Sdk;

namespace SecondPaymentProvider;

/// <summary>Country is an ISO code ("SE", "NO", "DE"...). Phone is "+46...". CardNumber is only needed for card payments.</summary>
public record CheckoutRequest(string OrderId, string Country, decimal Amount, string Currency, string? Phone, string? CardNumber);

public record CheckoutResult(bool Paid, string? Reference, string? Error)
{
    public static CheckoutResult Success(string reference) => new(true, reference, null);
    public static CheckoutResult Failure(string error) => new(false, null, error);
}

// Today the shop only sells in Sweden, so checkout talks to Swish directly.
public class CheckoutService(SwishClient swish)
{
    public Task<CheckoutResult> CheckoutAsync(CheckoutRequest request)
    {
        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(request));

        if (request.Country != "SE")
            throw new NotSupportedException($"We don't sell to {request.Country} yet.");

        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new ArgumentException("Swish needs a phone number.", nameof(request));

        var payment = swish.CreatePaymentRequest(
            request.Phone.TrimStart('+').Replace(" ", ""),
            request.Amount,
            $"Order {request.OrderId}");

        return Task.FromResult(payment.Status switch
        {
            "PAID" => CheckoutResult.Success(payment.Id),
            "DECLINED" => CheckoutResult.Failure("Betalningen nekades."),
            _ => CheckoutResult.Failure("Tekniskt fel, försök igen.")
        });
    }
}
