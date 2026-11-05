using LegacyPaymentGateway;

var services = Composition.CreatePaymentServices();
var goodCard = new Card("4111111111111111", 12, 2099);
var poorCard = new Card("4000000000000002", 12, 2099);

var purchase = await services.Checkout.CheckoutAsync(499.90m, "SEK", goodCard);
Console.WriteLine($"💳 Köp 1: {(purchase.Success ? $"OK ({purchase.TransactionId})" : purchase.ErrorMessage)}");

var declined = await services.Checkout.CheckoutAsync(499.90m, "SEK", poorCard);
Console.WriteLine($"💳 Köp 2: {(declined.Success ? "OK" : declined.ErrorMessage)}");

if (purchase.TransactionId is not null)
{
    var refund = await services.Refunds.RefundAsync(purchase.TransactionId, 100m);
    Console.WriteLine($"↩️ Återbetalning: {(refund.Success ? "OK" : refund.ErrorMessage)}");
}
