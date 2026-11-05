# 🔑 Facit: Payment provider adapter

> 🧩 En fullständig, testad lösning finns på branchen `solutions` i `solutions/04-build-from-scratch/payment-provider-adapter/`.

## 🏗️ Kärnan i lösningen

```csharp
// Delad översättning: vår Money → minor units
internal static class MinorUnits
{
    public static long From(Money money)
    {
        if (money.Amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(money));
        var minor = money.Amount * 100m;
        if (minor != decimal.Truncate(minor)) throw new ArgumentException("At most two decimals.", nameof(money));
        return (long)minor;
    }
}

// GlobalCard: exceptions → resultat
try
{
    var id = await gateway.AuthorizeAndCaptureAsync(card.Number, card.ExpiryMonth, card.ExpiryYear, card.Cvc, cents, currency, ct);
    return PaymentResult.Success(id);
}
catch (CardDeclinedException ex) { return PaymentResult.Failed(Map(ex.Code)); }
catch (GatewayUnavailableException) { return PaymentResult.Failed(FailureReason.ProviderUnavailable); }
```

**Bonus:** `PaymentProviderSelector` är en **Factory**. Den väljer *vilken* adapter som ska användas utifrån valutan.

## 🤔 Reflektionsfrågorna

### 1. Exception eller resultat?
- **`10.005m` är ett programmeringsfel.** Ingen kund kan betala en halv öre, så den som anropar med ett sådant belopp har en bugg. Buggar ska synas tydligt och tidigt, alltså med en exception.
- **Ett nekat kort är ett förväntat affärsutfall.** Det händer hela tiden och ska hanteras i flödet ("Kortet saknar täckning, prova ett annat"). Därför är det ett resultat.

> 📚 Tumregel: **exceptions för det som inte borde hända, resultat för det som förväntas hända ibland.**

### 2. Varför kontraktstester?
- ✅ **Samma regler för alla.** Det garanterar att adaptrarna verkligen är utbytbara (Liskov). Om QuickPay-adaptern returnerar `InvalidCard` men GlobalCard-adaptern kastar en exception för samma situation, märks det direkt.
- ✅ **Gratis tester för nya adapters**: en subklass med en rad.
- ✅ **Kontraktet blir dokumenterat** i kod, inte bara i en kommentar.
- ⚠️ Leverantörsspecifika beteenden (QuickPay saknar EUR, GlobalCard har fraud) behöver fortfarande egna tester i subklasserna.

### 3. `SuspectedFraud` som `Other`
För den här appen är det **rimligt**: för kunden är resultatet detsamma ("betalningen gick inte igenom"). Men om affären vill hantera bedrägeri annorlunda, till exempel blockera kontot eller larma, ska `FailureReason` utökas med `SuspectedFraud`. Då behöver QuickPay-adaptern inte ändras, eftersom den aldrig returnerar det värdet.

✅ Ett bra svar ser att **domänens interface ska spegla vad domänen behöver veta**. Det ska inte vara en union av alla leverantörers alla felkoder.

### 4. En tredje leverantör
- **Lägg till:** en ny adapterklass, en ny subklass av kontraktstesterna och en rad i selectorn eller DI-registreringen.
- **Ändra *inte*:** `IPaymentProvider`, `PaymentResult`, `FailureReason` (om inte leverantören har ett genuint nytt affärsutfall), kassakoden eller de befintliga adaptrarna.

Det är Adapter-mönstrets löfte: **förändringen isoleras till en ny klass.**
