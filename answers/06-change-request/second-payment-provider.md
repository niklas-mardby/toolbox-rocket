# 🔑 Facit: Second payment provider

> 🧩 En fullständig lösning finns på branchen `solutions` i `solutions/06-change-request/second-payment-provider/`.

## 🤔 Del 2: Reflektionen efter del 1

- **Storlek:** `CheckoutService` växer från cirka 30 till 80–100 rader. Nästan allt nytt handlar om SDK:erna: omvandling till øre och cent, telefonformat, statusar och exceptions. Kassalogiken (beloppet måste vara positivt) är fortfarande två rader.
- **`"Betalningen nekades."`** finns på **tre** ställen, ett per leverantör.
- **iDEAL:** ny SDK in i konstruktorn, en ny gren i landsvalet och ny översättningskod i samma klass. Varje ny leverantör gör klassen större och riskerar att något befintligt går sönder. Det är *divergent change*: klassen ändras av många olika skäl.
- **Testa Vipps isolerat:** går knappt. Varje test måste skapa alla tre SDK:erna, eftersom kassan beror på alla.

## 🔧 Del 3: Efter refaktoreringen

```csharp
public interface IPaymentProvider { Task<CheckoutResult> PayAsync(CheckoutRequest request); }
public interface IPaymentProviderSelector { IPaymentProvider For(string country); }

public class CheckoutService(IPaymentProviderSelector providers)
{
    public Task<CheckoutResult> CheckoutAsync(CheckoutRequest request)
    {
        if (request.Amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(request));
        return providers.For(request.Country).PayAsync(request);
    }
}
```

- **Adapters** (`SwishAdapter`, `VippsAdapter`, `CardAdapter`): all SDK-kunskap, en klass per SDK.
- **Selector** (Factory): land → leverantör, på ett ställe.
- **Felmeddelandena** definieras en gång (`CheckoutResult.Declined()`).

**iDEAL** blir en ny adapter och en rad i selectorn. **Swish eller kort i Sverige** (nästa förändring) gör att selectorn tar `(country, method)`, och `CheckoutRequest` får ett `PaymentMethod`. Adaptrarna påverkas inte.

## 🤔 Reflektionsfrågorna

### 1. Var ska valideringen ligga?
- **I kassan:** det som gäller *alla* betalningar (beloppet är positivt).
- **I adaptrarna:** det som en viss *leverantör* kräver (telefonnummer för Swish och Vipps, kortnummer för kort). Det är leverantörens krav, så det ska ändras när leverantören byts.
- **Inte i selectorn:** den ska bara välja.

### 2. Regler utan release
Flytta tabellen land → leverantör till **konfiguration** (`appsettings.json`, en databastabell eller en feature flag-tjänst), och registrera adaptrarna med **keyed services** i DI. Selectorn läser konfigurationen (via `IOptionsMonitor<T>` om den ska kunna ändras under körning) och slår upp adaptern med nyckeln. Koden ändras bara när en *ny leverantör* kommer, inte när ett nytt *land* öppnas.

### 3. Vad är nytt jämfört med de andra betalövningarna?
- **I `legacy-payment-gateway`** gömdes *en* leverantör bakom ett interface.
- **I `payment-provider-adapter`** byggdes *två* adapters mot samma kontrakt.
- **Här** kommer **valet** till: vilken adapter som ska användas *medan programmet körs*. Det är där **Factory** kompletterar Adapter. Det är också en *change request*: att uppleva hur designen utan mönster klarar förändringen är en annan lärdom än att bygga rätt från början.
