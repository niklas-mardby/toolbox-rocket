namespace NovaPay;

// ⚠️ THIRD-PARTY CODE. Pretend this is a NuGet package you can't change.
// The modern payment provider the company plans to switch to. Used in the bonus task.
//
// - Async API, amounts as decimal, expiry as separate month and year.
// - Failures are thrown as NovaPayException with a string code:
//   "invalid_card", "card_declined", "expired_card", "rate_limited", "charge_not_found", "amount_too_large".
// - Test cards: same numbers as LegacyPay, for convenience.

public record NovaChargeRequest(string CardNumber, int ExpiryMonth, int ExpiryYear, decimal Amount, string Currency);

public record NovaCharge(string Id, decimal Amount, string Currency, DateTimeOffset CreatedAt);

public class NovaPayException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

public class NovaPayApi(string apiKey)
{
    private readonly Dictionary<string, decimal> _charges = [];
    private readonly HashSet<string> _flakyCardsSeen = [];

    public string ApiKey { get; } = apiKey;

    public async Task<NovaCharge> CreateChargeAsync(NovaChargeRequest request)
    {
        await Task.Yield();

        if (request.CardNumber.Length != 16 || !request.CardNumber.All(char.IsDigit))
            throw new NovaPayException("invalid_card", "The card number is not valid.");

        var lastValidDay = new DateOnly(request.ExpiryYear, request.ExpiryMonth, 1).AddMonths(1).AddDays(-1);
        if (lastValidDay < DateOnly.FromDateTime(DateTime.Today))
            throw new NovaPayException("expired_card", "The card has expired.");

        if (request.CardNumber == "4000000000000002")
            throw new NovaPayException("card_declined", "Insufficient funds.");

        if (request.CardNumber == "4000000000000119" || (request.CardNumber == "4000000000000127" && _flakyCardsSeen.Add(request.CardNumber)))
            throw new NovaPayException("rate_limited", "Too many requests, try again.");

        var charge = new NovaCharge($"nova_{Guid.NewGuid():N}", request.Amount, request.Currency, DateTimeOffset.UtcNow);
        _charges[charge.Id] = request.Amount;
        return charge;
    }

    public async Task RefundAsync(string chargeId, decimal amount)
    {
        await Task.Yield();

        if (!_charges.TryGetValue(chargeId, out var charged))
            throw new NovaPayException("charge_not_found", $"No charge with id {chargeId}.");
        if (amount > charged)
            throw new NovaPayException("amount_too_large", "Refund exceeds the charged amount.");

        _charges[chargeId] = charged - amount;
    }
}
