// ⚠️ THIRD-PARTY CODE. Pretend these are three NuGet packages. Don't change them.
// They are in-memory fakes that record what they receive, so tests can inspect them.

namespace Swish.Sdk
{
    public record SwishPaymentRequest(string Id, string PayerAlias, decimal Amount, string Message, string Status, string? ErrorCode);

    /// <summary>
    /// Swish (Sweden). Synchronous. PayerAlias = phone digits without "+" (e.g. "46701234567").
    /// Amounts in SEK as decimal, max 150 000. Status: "PAID", "DECLINED" or "ERROR".
    /// Test number 46700000000 always declines.
    /// </summary>
    public class SwishClient
    {
        private int _next = 1;
        public List<SwishPaymentRequest> Requests { get; } = [];

        public SwishPaymentRequest CreatePaymentRequest(string payerAlias, decimal amount, string message)
        {
            var (status, error) =
                amount > 150_000m ? ("ERROR", "AMOUNT_LIMIT") :
                payerAlias == "46700000000" ? ("DECLINED", "RF07") :
                ("PAID", (string?)null);

            var request = new SwishPaymentRequest($"SW-{_next++:D5}", payerAlias, amount, message, status, error);
            Requests.Add(request);
            return request;
        }
    }
}

namespace Vipps.Sdk
{
    public enum VippsState { Captured, Rejected, Failed }

    public class VippsPayment
    {
        public required string MobileNumber { get; init; }   // digits only, e.g. "4790000001"
        public required long AmountInOre { get; init; }      // NOK in øre
        public required string OrderId { get; init; }
        public required string Text { get; init; }
    }

    public record VippsResult(string Reference, VippsState State);

    /// <summary>
    /// Vipps (Norway). Async. Amounts in øre. Only NOK.
    /// Test number 4790000000 is always rejected.
    /// </summary>
    public class VippsClient
    {
        private int _next = 1;
        public List<VippsPayment> Payments { get; } = [];

        public async Task<VippsResult> InitiateAsync(VippsPayment payment)
        {
            await Task.Yield();
            Payments.Add(payment);
            var state = payment.MobileNumber == "4790000000" ? VippsState.Rejected : VippsState.Captured;
            return new VippsResult($"VP-{_next++:D5}", state);
        }
    }
}

namespace CardGateway.Sdk
{
    public class CardDeclinedException(string reason) : Exception($"Card declined: {reason}");

    public record Authorization(string Id, string CardNumber, int AmountInCents, string Currency);

    /// <summary>
    /// Card payments (all countries). Synchronous. Amounts in cents (int). Currency: EUR, SEK, NOK, DKK.
    /// Declines are EXCEPTIONS. Test card 4000000000000002 is always declined.
    /// </summary>
    public class CardProcessor
    {
        private int _next = 1;
        public List<Authorization> Authorizations { get; } = [];

        public string Authorize(string cardNumber, int amountInCents, string currency)
        {
            if (currency is not ("EUR" or "SEK" or "NOK" or "DKK"))
                throw new ArgumentException($"Unsupported currency {currency}.", nameof(currency));
            if (cardNumber == "4000000000000002")
                throw new CardDeclinedException("insufficient_funds");

            var auth = new Authorization($"CD-{_next++:D5}", cardNumber, amountInCents, currency);
            Authorizations.Add(auth);
            return auth.Id;
        }
    }
}
