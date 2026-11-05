using CardGateway.Sdk;
using Swish.Sdk;
using Vipps.Sdk;

namespace SecondPaymentProvider;

/// <summary>OUR interface. Every provider looks like this to the checkout.</summary>
public interface IPaymentProvider
{
    Task<CheckoutResult> PayAsync(CheckoutRequest request);
}

/// <summary>Factory: picks the provider for a country.</summary>
public interface IPaymentProviderSelector
{
    IPaymentProvider For(string country);
}

public class PaymentProviderSelector(IPaymentProvider swish, IPaymentProvider vipps, IPaymentProvider card) : IPaymentProviderSelector
{
    private static readonly HashSet<string> CardCountries = ["DE", "FI", "DK", "NL", "FR", "ES", "IT", "AT", "BE"];

    public IPaymentProvider For(string country) => country switch
    {
        "SE" => swish,
        "NO" => vipps,
        _ when CardCountries.Contains(country) => card,
        _ => throw new NotSupportedException($"We don't sell to {country} yet.")
    };
}

internal static class Phone
{
    /// <summary>"+47 900 00 001" -> "4790000001"</summary>
    public static string ToDigits(string? phone, string providerName) =>
        string.IsNullOrWhiteSpace(phone)
            ? throw new ArgumentException($"{providerName} needs a phone number.", nameof(phone))
            : new string(phone.Where(char.IsDigit).ToArray());
}

internal static class MinorUnits
{
    public static long From(decimal amount) => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
}

// --- Adapters: one per SDK. ALL provider-specific knowledge lives here. ---

public class SwishAdapter(SwishClient swish) : IPaymentProvider
{
    public Task<CheckoutResult> PayAsync(CheckoutRequest request)
    {
        var payment = swish.CreatePaymentRequest(Phone.ToDigits(request.Phone, "Swish"), request.Amount, $"Order {request.OrderId}");

        return Task.FromResult(payment.Status switch
        {
            "PAID" => CheckoutResult.Success(payment.Id),
            "DECLINED" => CheckoutResult.Declined(),
            _ => CheckoutResult.TechnicalError()
        });
    }
}

public class VippsAdapter(VippsClient vipps) : IPaymentProvider
{
    public async Task<CheckoutResult> PayAsync(CheckoutRequest request)
    {
        var result = await vipps.InitiateAsync(new VippsPayment
        {
            MobileNumber = Phone.ToDigits(request.Phone, "Vipps"),
            AmountInOre = MinorUnits.From(request.Amount),
            OrderId = request.OrderId,
            Text = $"Order {request.OrderId}",
        });

        return result.State switch
        {
            VippsState.Captured => CheckoutResult.Success(result.Reference),
            VippsState.Rejected => CheckoutResult.Declined(),
            _ => CheckoutResult.TechnicalError()
        };
    }
}

public class CardAdapter(CardProcessor cards) : IPaymentProvider
{
    public Task<CheckoutResult> PayAsync(CheckoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CardNumber))
            throw new ArgumentException("Card payments need a card number.", nameof(request));

        try
        {
            var authorizationId = cards.Authorize(request.CardNumber, checked((int)MinorUnits.From(request.Amount)), request.Currency);
            return Task.FromResult(CheckoutResult.Success(authorizationId));
        }
        catch (CardDeclinedException)
        {
            return Task.FromResult(CheckoutResult.Declined());
        }
    }
}
