using LegacyPay.Sdk;

namespace LegacyPaymentGateway;

// Our business code. Notice how much of it is about LegacyPay's strange API
// rather than about checking out: öre conversion, MMYY strings, magic numbers...
public class CheckoutService(LegacyPayClient legacyPay)
{
    public Task<CheckoutResult> CheckoutAsync(decimal amount, string currency, Card card)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        var amountInOre = (int)Math.Round(amount * 100m);
        var expiry = $"{card.ExpiryMonth:00}{card.ExpiryYear % 100:00}";

        var code = legacyPay.Charge(card.Number, expiry, amountInOre, currency, out var reference, out var error);

        // 96 = temporary error. Try once more.
        if (code == 96)
        {
            code = legacyPay.Charge(card.Number, expiry, amountInOre, currency, out reference, out error);
        }

        if (code == 0)
            return Task.FromResult(CheckoutResult.Ok(reference));

        Console.WriteLine($"LegacyPay error {code}: {error}");

        var message = code switch
        {
            14 => "Ogiltigt kortnummer.",
            51 => "Kortet saknar täckning.",
            54 => "Kortet har gått ut.",
            _ => "Betalningen kunde inte genomföras. Försök igen senare."
        };
        return Task.FromResult(CheckoutResult.Fail(message));
    }
}
