using LegacyPay.Sdk;

namespace LegacyPaymentGateway;

// More business code that knows far too much about LegacyPay.
public class RefundService(LegacyPayClient legacyPay)
{
    public Task<RefundResult> RefundAsync(string transactionId, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        var amountInOre = (int)Math.Round(amount * 100m);
        var code = legacyPay.Refund(transactionId, amountInOre, out var error);

        if (code == 0)
            return Task.FromResult(RefundResult.Ok());

        Console.WriteLine($"LegacyPay error {code}: {error}");

        if (code == 12)
            return Task.FromResult(RefundResult.Fail("Köpet hittades inte."));
        if (code == 13)
            return Task.FromResult(RefundResult.Fail("Beloppet är större än köpet."));
        return Task.FromResult(RefundResult.Fail("Återbetalningen kunde inte genomföras."));
    }
}
