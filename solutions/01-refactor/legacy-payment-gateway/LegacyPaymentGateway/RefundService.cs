namespace LegacyPaymentGateway;

public class RefundService(IPaymentGateway gateway)
{
    public async Task<RefundResult> RefundAsync(string transactionId, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        var result = await gateway.RefundAsync(transactionId, amount);

        return result.Success
            ? RefundResult.Ok()
            : RefundResult.Fail(result.Failure switch
            {
                PaymentFailure.UnknownTransaction => "Köpet hittades inte.",
                PaymentFailure.AmountTooLarge => "Beloppet är större än köpet.",
                _ => "Återbetalningen kunde inte genomföras."
            });
    }
}
