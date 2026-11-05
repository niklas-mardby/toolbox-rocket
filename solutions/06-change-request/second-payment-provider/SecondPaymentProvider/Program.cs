using CardGateway.Sdk;
using SecondPaymentProvider;
using Swish.Sdk;
using Vipps.Sdk;

var checkout = Composition.CreateCheckout(new SwishClient(), new VippsClient(), new CardProcessor());

CheckoutRequest[] requests =
[
    new("1001", "SE", 499m, "SEK", "+46701234567", null),
    new("1002", "NO", 499m, "NOK", "+4790000001", null),
    new("1003", "DE", 49.90m, "EUR", null, "4111111111111111"),
];

foreach (var request in requests)
{
    try
    {
        var result = await checkout.CheckoutAsync(request);
        Console.WriteLine($"💳 {request.Country}: {(result.Paid ? $"betald ({result.Reference})" : result.Error)}");
    }
    catch (NotSupportedException ex)
    {
        Console.WriteLine($"🚫 {request.Country}: {ex.Message}");
    }
}
