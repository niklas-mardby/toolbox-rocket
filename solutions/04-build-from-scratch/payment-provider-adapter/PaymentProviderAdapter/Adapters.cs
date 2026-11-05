using GlobalCard;
using QuickPay.Sdk;

namespace PaymentProviderAdapter;

/// <summary>Shared translation helpers. Both adapters agree on how OUR money becomes minor units.</summary>
internal static class MinorUnits
{
    public static long From(Money money)
    {
        if (money.Amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(money));

        var minor = money.Amount * 100m;
        if (minor != decimal.Truncate(minor))
            throw new ArgumentException("Amount can have at most two decimals.", nameof(money));

        return (long)minor;
    }
}

/// <summary>Adapter: IPaymentProvider (ours) -> QuickPayClient (theirs).</summary>
public class QuickPayAdapter(QuickPayClient client) : IPaymentProvider
{
    public string Name => "QuickPay";

    public Task<PaymentResult> ChargeAsync(Money amount, CardDetails card, CancellationToken ct = default)
    {
        var response = client.Debit(new QuickPayDebit
        {
            Pan = card.Number,
            ExpiryYYMM = $"{card.ExpiryYear % 100:00}{card.ExpiryMonth:00}",
            Cvc = card.Cvc,
            AmountMinor = MinorUnits.From(amount),
            Currency = amount.Currency,
        });

        return Task.FromResult(Translate(response));
    }

    public Task<PaymentResult> RefundAsync(string paymentId, Money amount, CancellationToken ct = default) =>
        Task.FromResult(Translate(client.Credit(paymentId, MinorUnits.From(amount))));

    private static PaymentResult Translate(QuickPayResponse response) =>
        response.Status == "OK"
            ? PaymentResult.Success(response.Reference!)
            : PaymentResult.Failed(response.ReasonCode switch
            {
                "NSF" => FailureReason.InsufficientFunds,
                "EXP" => FailureReason.CardExpired,
                "PAN" => FailureReason.InvalidCard,
                "CUR" => FailureReason.UnsupportedCurrency,
                "REF" => FailureReason.UnknownPayment,
                "AMT" => FailureReason.RefundTooLarge,
                "SYS" => FailureReason.ProviderUnavailable,
                _ => FailureReason.Other
            });
}

/// <summary>Adapter: IPaymentProvider (ours) -> GlobalCardGateway (theirs). Exceptions become results.</summary>
public class GlobalCardAdapter(GlobalCardGateway gateway) : IPaymentProvider
{
    public string Name => "GlobalCard";

    public async Task<PaymentResult> ChargeAsync(Money amount, CardDetails card, CancellationToken ct = default)
    {
        var cents = checked((int)MinorUnits.From(amount));
        if (!TryGetCurrency(amount.Currency, out var currency))
            return PaymentResult.Failed(FailureReason.UnsupportedCurrency);

        try
        {
            var id = await gateway.AuthorizeAndCaptureAsync(
                card.Number, card.ExpiryMonth, card.ExpiryYear, card.Cvc, cents, currency, ct);
            return PaymentResult.Success(id);
        }
        catch (CardDeclinedException ex)
        {
            return PaymentResult.Failed(ex.Code switch
            {
                DeclineCode.InsufficientFunds => FailureReason.InsufficientFunds,
                DeclineCode.Expired => FailureReason.CardExpired,
                DeclineCode.InvalidNumber => FailureReason.InvalidCard,
                _ => FailureReason.Other
            });
        }
        catch (GatewayUnavailableException)
        {
            return PaymentResult.Failed(FailureReason.ProviderUnavailable);
        }
    }

    public async Task<PaymentResult> RefundAsync(string paymentId, Money amount, CancellationToken ct = default)
    {
        var cents = checked((int)MinorUnits.From(amount));
        try
        {
            await gateway.RefundAsync(paymentId, cents, ct);
            return PaymentResult.Success(paymentId);
        }
        catch (UnknownTransactionException)
        {
            return PaymentResult.Failed(FailureReason.UnknownPayment);
        }
        catch (RefundExceedsCaptureException)
        {
            return PaymentResult.Failed(FailureReason.RefundTooLarge);
        }
        catch (GatewayUnavailableException)
        {
            return PaymentResult.Failed(FailureReason.ProviderUnavailable);
        }
    }

    // Enum.TryParse would also accept "752" or "1", so check the defined names explicitly.
    private static bool TryGetCurrency(string code, out Iso4217 currency) =>
        Enum.TryParse(code, ignoreCase: false, out currency) && Enum.GetNames<Iso4217>().Contains(code);
}
