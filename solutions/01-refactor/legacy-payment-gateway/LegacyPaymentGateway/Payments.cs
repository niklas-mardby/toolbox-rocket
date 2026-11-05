using LegacyPay.Sdk;
using NovaPay;

namespace LegacyPaymentGateway;

/// <summary>OUR reasons a payment can fail, independent of any provider.</summary>
public enum PaymentFailure { InvalidCard, InsufficientFunds, CardExpired, TemporaryError, UnknownTransaction, AmountTooLarge, Other }

public record PaymentResult(bool Success, string? TransactionId, PaymentFailure? Failure)
{
    public static PaymentResult Ok(string transactionId) => new(true, transactionId, null);
    public static PaymentResult Failed(PaymentFailure failure) => new(false, null, failure);
}

/// <summary>The interface OUR business code depends on. Designed from what checkout and refunds need.</summary>
public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(decimal amount, string currency, Card card);
    Task<PaymentResult> RefundAsync(string transactionId, decimal amount);
}

/// <summary>Adapter: IPaymentGateway -> LegacyPayClient. All öre, MMYY and magic numbers live here.</summary>
public class LegacyPayAdapter(LegacyPayClient client) : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(decimal amount, string currency, Card card)
    {
        var expiry = $"{card.ExpiryMonth:00}{card.ExpiryYear % 100:00}";
        var code = client.Charge(card.Number, expiry, ToOre(amount), currency, out var reference, out _);
        return Task.FromResult(code == 0 ? PaymentResult.Ok(reference) : PaymentResult.Failed(Map(code)));
    }

    public Task<PaymentResult> RefundAsync(string transactionId, decimal amount)
    {
        var code = client.Refund(transactionId, ToOre(amount), out _);
        return Task.FromResult(code == 0 ? PaymentResult.Ok(transactionId) : PaymentResult.Failed(Map(code)));
    }

    private static int ToOre(decimal amount) => (int)Math.Round(amount * 100m);

    private static PaymentFailure Map(int code) => code switch
    {
        14 => PaymentFailure.InvalidCard,
        51 => PaymentFailure.InsufficientFunds,
        54 => PaymentFailure.CardExpired,
        96 => PaymentFailure.TemporaryError,
        12 => PaymentFailure.UnknownTransaction,
        13 => PaymentFailure.AmountTooLarge,
        _ => PaymentFailure.Other
    };
}

/// <summary>Adapter: IPaymentGateway -> NovaPayApi (the bonus). Exceptions become results.</summary>
public class NovaPayAdapter(NovaPayApi api) : IPaymentGateway
{
    public async Task<PaymentResult> ChargeAsync(decimal amount, string currency, Card card)
    {
        try
        {
            var charge = await api.CreateChargeAsync(new NovaChargeRequest(card.Number, card.ExpiryMonth, card.ExpiryYear, amount, currency));
            return PaymentResult.Ok(charge.Id);
        }
        catch (NovaPayException ex)
        {
            return PaymentResult.Failed(Map(ex.Code));
        }
    }

    public async Task<PaymentResult> RefundAsync(string transactionId, decimal amount)
    {
        try
        {
            await api.RefundAsync(transactionId, amount);
            return PaymentResult.Ok(transactionId);
        }
        catch (NovaPayException ex)
        {
            return PaymentResult.Failed(Map(ex.Code));
        }
    }

    private static PaymentFailure Map(string code) => code switch
    {
        "invalid_card" => PaymentFailure.InvalidCard,
        "card_declined" => PaymentFailure.InsufficientFunds,
        "expired_card" => PaymentFailure.CardExpired,
        "rate_limited" => PaymentFailure.TemporaryError,
        "charge_not_found" => PaymentFailure.UnknownTransaction,
        "amount_too_large" => PaymentFailure.AmountTooLarge,
        _ => PaymentFailure.Other
    };
}
