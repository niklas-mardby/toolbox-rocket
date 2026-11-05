namespace QuickPay.Sdk;

// ⚠️ THIRD-PARTY CODE (pretend NuGet package). Don't change it.
//
// QuickPay, a Nordic provider.
//  - Synchronous.
//  - Amounts in öre/øre as long. Supports SEK, NOK and DKK only.
//  - Expiry as "YYMM". Card number is called "Pan".
//  - Response.Status: "OK", "DECLINED" or "ERROR".
//  - Response.ReasonCode: "NSF" (insufficient funds), "EXP" (expired), "PAN" (invalid card),
//    "CUR" (unsupported currency), "REF" (unknown reference), "AMT" (refund too large), "SYS" (system down).
//  - Test cards: 4000000000000002 = NSF, 4000000000000069 = EXP, 4000000000000119 = SYS.
//    Any other 16-digit number is approved.

public class QuickPayDebit
{
    public required string Pan { get; init; }
    public required string ExpiryYYMM { get; init; }
    public required string Cvc { get; init; }
    public required long AmountMinor { get; init; }
    public required string Currency { get; init; }
}

public class QuickPayResponse
{
    public required string Status { get; init; }
    public string? Reference { get; init; }
    public string? ReasonCode { get; init; }
}

public class QuickPayClient
{
    private static readonly HashSet<string> SupportedCurrencies = ["SEK", "NOK", "DKK"];
    private readonly Dictionary<string, long> _debits = [];
    private int _next = 1;

    public QuickPayResponse Debit(QuickPayDebit request)
    {
        if (!SupportedCurrencies.Contains(request.Currency)) return Fail("ERROR", "CUR");
        if (request.Pan.Length != 16 || !request.Pan.All(char.IsDigit)) return Fail("DECLINED", "PAN");
        if (request.ExpiryYYMM.Length != 4) return Fail("DECLINED", "PAN");
        if (request.Pan == "4000000000000002") return Fail("DECLINED", "NSF");
        if (request.Pan == "4000000000000069") return Fail("DECLINED", "EXP");
        if (request.Pan == "4000000000000119") return Fail("ERROR", "SYS");

        var reference = $"QP{_next++:D6}";
        _debits[reference] = request.AmountMinor;
        return new QuickPayResponse { Status = "OK", Reference = reference };
    }

    public QuickPayResponse Credit(string reference, long amountMinor)
    {
        if (!_debits.TryGetValue(reference, out var remaining)) return Fail("DECLINED", "REF");
        if (amountMinor > remaining) return Fail("DECLINED", "AMT");

        _debits[reference] = remaining - amountMinor;
        return new QuickPayResponse { Status = "OK", Reference = reference };
    }

    private static QuickPayResponse Fail(string status, string reason) => new() { Status = status, ReasonCode = reason };
}
