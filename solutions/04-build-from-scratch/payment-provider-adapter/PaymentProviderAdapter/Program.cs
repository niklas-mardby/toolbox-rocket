using GlobalCard;
using PaymentProviderAdapter;
using QuickPay.Sdk;

// A small demo. It throws NotImplementedException until you've built the adapters.
IPaymentProvider[] providers = [new QuickPayAdapter(new QuickPayClient()), new GlobalCardAdapter(new GlobalCardGateway())];
var card = new CardDetails("4111111111111111", 12, 2030, "123");

foreach (var provider in providers)
{
    // This loop doesn't know or care which SDK is behind each provider. That's the point.
    var charge = await provider.ChargeAsync(new Money(249.50m, "SEK"), card);
    Console.WriteLine($"💳 {provider.Name}: {(charge.Succeeded ? $"OK {charge.PaymentId}" : charge.Failure.ToString())}");

    if (charge.PaymentId is not null)
    {
        var refund = await provider.RefundAsync(charge.PaymentId, new Money(49.50m, "SEK"));
        Console.WriteLine($"   ↩️ Återbetalning: {(refund.Succeeded ? "OK" : refund.Failure.ToString())}");
    }
}
