namespace GlobalCard;

// ⚠️ THIRD-PARTY CODE (pretend NuGet package). Don't change it.
//
// GlobalCard, an international provider.
//  - Async.
//  - Amounts in cents as int. Currency as an enum (SEK, EUR, USD only).
//  - Expiry as separate month and 4-digit year.
//  - Failures are EXCEPTIONS:
//      CardDeclinedException with a DeclineCode
//      GatewayUnavailableException when the service is down
//      UnknownTransactionException / RefundExceedsCaptureException for refunds
//  - Test cards: 4000000000000002 = InsufficientFunds, 4000000000000069 = Expired,
//    4000000000000119 = gateway unavailable, 4100000000000019 = SuspectedFraud.
//    Any other 16-digit number is approved.

public enum Iso4217 { SEK = 752, EUR = 978, USD = 840 }

public enum DeclineCode { InsufficientFunds, Expired, InvalidNumber, SuspectedFraud }

public class CardDeclinedException(DeclineCode code) : Exception($"Card declined: {code}")
{
    public DeclineCode Code { get; } = code;
}

public class GatewayUnavailableException() : Exception("GlobalCard gateway is unavailable.");
public class UnknownTransactionException(string id) : Exception($"Unknown transaction {id}.");
public class RefundExceedsCaptureException() : Exception("Refund exceeds captured amount.");

public class GlobalCardGateway
{
    private readonly Dictionary<string, int> _captures = [];

    public async Task<string> AuthorizeAndCaptureAsync(
        string cardNumber, int expiryMonth, int expiryYear, string securityCode,
        int amountInCents, Iso4217 currency, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);

        if (cardNumber.Length != 16 || !cardNumber.All(char.IsDigit)) throw new CardDeclinedException(DeclineCode.InvalidNumber);
        if (cardNumber == "4000000000000002") throw new CardDeclinedException(DeclineCode.InsufficientFunds);
        if (cardNumber == "4000000000000069") throw new CardDeclinedException(DeclineCode.Expired);
        if (cardNumber == "4100000000000019") throw new CardDeclinedException(DeclineCode.SuspectedFraud);
        if (cardNumber == "4000000000000119") throw new GatewayUnavailableException();

        var id = $"gc_{Guid.NewGuid():N}";
        _captures[id] = amountInCents;
        return id;
    }

    public async Task RefundAsync(string transactionId, int amountInCents, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);

        if (!_captures.TryGetValue(transactionId, out var remaining)) throw new UnknownTransactionException(transactionId);
        if (amountInCents > remaining) throw new RefundExceedsCaptureException();

        _captures[transactionId] = remaining - amountInCents;
    }
}
