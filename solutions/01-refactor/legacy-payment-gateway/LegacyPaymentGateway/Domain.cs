namespace LegacyPaymentGateway;

/// <summary>Card details as our own app models them.</summary>
public record Card(string Number, int ExpiryMonth, int ExpiryYear);

public record CheckoutResult(bool Success, string? TransactionId, string? ErrorMessage)
{
    public static CheckoutResult Ok(string transactionId) => new(true, transactionId, null);
    public static CheckoutResult Fail(string message) => new(false, null, message);
}

public record RefundResult(bool Success, string? ErrorMessage)
{
    public static RefundResult Ok() => new(true, null);
    public static RefundResult Fail(string message) => new(false, message);
}
