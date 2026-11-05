namespace LegacyPay.Sdk;

// ⚠️ THIRD-PARTY CODE. Pretend this is a NuGet package you can't change.
//
// LegacyPay API notes (from their 2009 PDF documentation):
//  - Amounts are in minor units (öre/cent) as int.
//  - Expiry is "MMYY".
//  - Return codes: 0 = OK, 14 = invalid card number, 51 = insufficient funds,
//    54 = expired card, 96 = temporary system error (try again),
//    12 = unknown transaction, 13 = refund larger than charge.
//  - Test cards: 4000000000000002 = always 51, 4000000000000119 = always 96,
//    4000000000000127 = 96 on the first attempt, OK on the next.
public class LegacyPayClient(string merchantId)
{
    private readonly Dictionary<string, int> _charges = [];
    private readonly HashSet<string> _flakyCardsSeen = [];
    private int _nextReference = 1000;

    public string MerchantId { get; } = merchantId;

    public int Charge(string cardNumber, string expiryMmYy, int amountMinorUnits, string currency,
        out string transactionRef, out string errorText)
    {
        transactionRef = "";
        errorText = "";

        if (cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
        {
            errorText = "ERR_CARD_FORMAT";
            return 14;
        }

        if (IsExpired(expiryMmYy))
        {
            errorText = "ERR_CARD_EXPIRED";
            return 54;
        }

        if (cardNumber == "4000000000000002")
        {
            errorText = "ERR_NSF";
            return 51;
        }

        if (cardNumber == "4000000000000119" || (cardNumber == "4000000000000127" && _flakyCardsSeen.Add(cardNumber)))
        {
            errorText = "ERR_SYS_TEMP";
            return 96;
        }

        transactionRef = $"LP-{_nextReference++}";
        _charges[transactionRef] = amountMinorUnits;
        return 0;
    }

    public int Refund(string transactionRef, int amountMinorUnits, out string errorText)
    {
        errorText = "";

        if (!_charges.TryGetValue(transactionRef, out var charged))
        {
            errorText = "ERR_TXN_UNKNOWN";
            return 12;
        }

        if (amountMinorUnits > charged)
        {
            errorText = "ERR_AMOUNT";
            return 13;
        }

        _charges[transactionRef] = charged - amountMinorUnits;
        return 0;
    }

    private static bool IsExpired(string expiryMmYy)
    {
        if (expiryMmYy.Length != 4
            || !int.TryParse(expiryMmYy[..2], out var month)
            || !int.TryParse(expiryMmYy[2..], out var year)
            || month is < 1 or > 12)
            return true;

        var lastValidDay = new DateOnly(2000 + year, month, 1).AddMonths(1).AddDays(-1);
        return lastValidDay < DateOnly.FromDateTime(DateTime.Today);
    }
}
