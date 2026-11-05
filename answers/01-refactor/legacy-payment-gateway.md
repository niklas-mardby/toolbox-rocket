# 🔑 Facit: Legacy payment gateway

> 🧩 En fullständig kodlösning, inklusive `NovaPayAdapter`, finns på branchen `solutions` i `solutions/01-refactor/legacy-payment-gateway/`.

## 🏗️ Så här kan strukturen se ut

```csharp
public enum PaymentFailure { InvalidCard, InsufficientFunds, CardExpired, TemporaryError, UnknownTransaction, AmountTooLarge, Other }

public record PaymentResult(bool Success, string? TransactionId, PaymentFailure? Failure);

public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(decimal amount, string currency, Card card);
    Task<PaymentResult> RefundAsync(string transactionId, decimal amount);
}

public class LegacyPayAdapter(LegacyPayClient client) : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(decimal amount, string currency, Card card)
    {
        var code = client.Charge(card.Number, $"{card.ExpiryMonth:00}{card.ExpiryYear % 100:00}",
            ToMinorUnits(amount), currency, out var reference, out _);
        return Task.FromResult(code == 0
            ? new PaymentResult(true, reference, null)
            : new PaymentResult(false, null, MapCode(code)));
    }
    // ... RefundAsync, ToMinorUnits, MapCode
}
```

`CheckoutService` blir:

```csharp
public class CheckoutService(IPaymentGateway gateway)
{
    public async Task<CheckoutResult> CheckoutAsync(decimal amount, string currency, Card card)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));

        var result = await gateway.ChargeAsync(amount, currency, card);
        if (result.Failure == PaymentFailure.TemporaryError)
            result = await gateway.ChargeAsync(amount, currency, card);

        return result.Success
            ? CheckoutResult.Ok(result.TransactionId!)
            : CheckoutResult.Fail(ToCustomerMessage(result.Failure));
    }
}
```

Nu finns **inget** `using LegacyPay.Sdk;` utanför adaptern och `Composition.cs`. Bytet till NovaPay är en ny klass och en rad i `Composition.cs`.

## 🤔 Reflektionsfrågorna

### 1. Vem äger interfacet?

**Din domän.** Interfacet beskriver vad *din applikation* behöver, med *dina* typer (`Card`, `decimal`, `PaymentFailure`). Leverantören anpassas till det via adaptern, och inte tvärtom. Det är *dependency inversion*: affärskoden beror på en abstraktion den själv definierat, och detaljerna (LegacyPay, NovaPay) beror på den abstraktionen.

Det spelar roll eftersom ett interface som ägs av leverantören byts ut när leverantören byts ut, och då måste affärskoden ändras ändå.

### 2. Vad hade hänt om interfacet varit en kopia av LegacyPays API?

Om interfacet hade sett ut som `int Charge(string card, string mmyy, int ore, string cur, out string ref, out string err)` hade du bara fått en **tunn wrapper** utan vinst. Affärskoden hade fortfarande räknat i ören och tolkat magiska siffror, och NovaPay-adaptern hade tvingats *översätta tillbaka* till LegacyPays konstiga format. Det kallas ibland en *leaky abstraction*: abstraktionen läcker detaljer från den underliggande implementationen.

✅ Ett bra interface designas utifrån **anroparens behov**, inte utifrån den första implementationen.

### 3. Var hör återförsöket hemma?

Båda valen är försvarbara:

| Placering | ✅ Fördelar | ⚠️ Nackdelar |
|-----------|-------------|---------------|
| I `CheckoutService` | Synligt i affärsflödet, och affären kan styra policyn | Måste upprepas för varje tjänst som anropar gatewayen |
| I adaptern | Gäller automatiskt för alla anropare | Leverantörsspecifik kod får en policy som egentligen är generell |
| I en **decorator**: `RetryingPaymentGateway : IPaymentGateway` | Återanvändbar för *alla* leverantörer, med en tydlig enskild uppgift | En klass till att hålla reda på |

Decorator-varianten är ofta snyggast. Det är också precis så resilience-biblioteket Polly används med `HttpClient`.

> ⚠️ Försök bara igen på operationer som är säkra att upprepa (*idempotenta*). En återförsökt debitering kan ge dubbeldebitering om det första anropet faktiskt gick igenom. Riktiga betal-API:er har därför ofta en *idempotency key*.

### 4. Hade adaptern varit värd det med bara LegacyPay på ett ställe?

Förmodligen **mindre** värd, men inte värdelös. Även med en enda leverantör ger adaptern:
- ✅ **testbarhet**: du kan fejka `IPaymentGateway` och testa `CheckoutService` utan LegacyPays testkort,
- ✅ **läsbarhet**: affärskoden talar domänens språk.

Men om API:t redan hade varit rimligt (som NovaPays) och bara använts på ett ställe, hade ett extra lager mest varit ceremoni. Ett bra svar väger **hur konstigt** API:t är, **hur många ställen** det används på och **hur troligt** ett byte är.
