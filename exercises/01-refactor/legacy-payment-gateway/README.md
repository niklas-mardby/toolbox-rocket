# 💳 Legacy payment gateway

> 🧩 **Mönster:** Adapter
> 📏 **Omfattning:** två tjänster som båda använder ett gammalt tredjeparts-SDK, plus ett modernt SDK i en bonusuppgift. Du designar själv ett interface i domänens språk.
> 🧪 **Tester:** finns och är gröna. Du får ändra `Composition.cs` men inte testerna. Koden i `ThirdParty/` får du inte ändra alls.

## 🎯 Mål

Du ska gömma det gamla betal-SDK:t bakom **ditt eget interface** och en **Adapter**, så att affärskoden inte längre vet något om LegacyPay, och så att ett byte av leverantör bara påverkar en klass.

## 📖 Bakgrund

Webbshoppen tar betalt via **LegacyPay**, ett SDK från 2009. Dess API är... speciellt:

- belopp anges i **ören** som `int`
- utgångsdatum är en sträng i formatet `"MMYY"`
- resultatet är en **magisk siffra** (`0`, `14`, `51`, `54`, `96`...) plus `out`-parametrar

Öppna `CheckoutService.cs` och `RefundService.cs`. Båda omvandlar belopp till ören, tolkar felkoder och loggar fel, var och en på sitt sätt. Affärskoden är full av LegacyPay-detaljer.

Nu har ledningen bestämt att företaget ska byta till **NovaPay** nästa år. Det är ett modernt SDK med `async`, `decimal` och exceptions (det finns i `ThirdParty/NovaPay/`). Med dagens kod skulle bytet kräva ändringar överallt.

## 🛠️ Uppgift

1. ▶️ **Kör testerna** och se att de är gröna.
2. 📝 **Lista allt LegacyPay-specifikt** i `CheckoutService` och `RefundService`. Vad är *affärslogik* och vad är *översättning* till LegacyPay?
3. 🧩 **Designa ett interface**, till exempel `IPaymentGateway`, som beskriver betalningar på *ditt* sätt: `decimal`-belopp, din `Card`-typ, `async` och ett tydligt resultat i stället för magiska siffror.
4. 🔌 **Skriv en `LegacyPayAdapter`** som implementerar interfacet och översätter till och från `LegacyPayClient`. All kunskap om ören, `"MMYY"` och felkoder ska hamna här.
5. 🧹 **Låt `CheckoutService` och `RefundService` bero på ditt interface**, inte på `LegacyPayClient`.
6. 🔧 **Uppdatera `Composition.cs`** och kör testerna.
7. ➕ **Bonus:** Skriv en `NovaPayAdapter` för samma interface och byt till den i `Composition.cs`. Är testerna fortfarande gröna? Hur många filer utanför adaptern behövde du ändra?

## ▶️ Kom igång

```bash
dotnet run --project exercises/01-refactor/legacy-payment-gateway/LegacyPaymentGateway
dotnet test exercises/01-refactor/legacy-payment-gateway/LegacyPaymentGateway.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Vad är översättning?</summary>

Översättning är allt som skulle se annorlunda ut med en annan leverantör: omvandlingen till ören, formatet `"MMYY"`, `out`-parametrarna, de numeriska felkoderna och deras textsträngar. Affärslogik är sådant som gäller oavsett leverantör, som att beloppet måste vara positivt och vilket felmeddelande *kunden* ska se.
</details>

<details>
<summary>💡 Hint 2: Hur ser resultatet ut?</summary>

Låt adaptern översätta leverantörens felkoder till *dina* egna felorsaker, till exempel en enum:

```csharp
public enum PaymentFailure { InvalidCard, InsufficientFunds, CardExpired, TemporaryError, UnknownTransaction, AmountTooLarge, Other }
public record PaymentResult(bool Success, string? TransactionId, PaymentFailure? Failure);
```

Då kan `CheckoutService` bestämma vilken svensk text kunden får se, och den texten är densamma oavsett leverantör.
</details>

<details>
<summary>💡 Hint 3: Var hör återförsöket hemma?</summary>

`CheckoutService` försöker igen vid felkod 96. Är "försök igen vid tillfälliga fel" affärslogik eller leverantörslogik? Det finns flera rimliga svar. Om det ligger kvar i `CheckoutService` måste adaptern kunna berätta att felet var tillfälligt. Om det flyttas till adaptern gäller det automatiskt för alla som använder den.
</details>

<details>
<summary>💡 Hint 4: Refunds delar state</summary>

LegacyPay kommer ihåg köp i den *instans* av `LegacyPayClient` som skapade dem. Därför måste `CheckoutService` och `RefundService` dela samma adapter, eller åtminstone samma klient. Titta på hur `Composition.cs` gör det idag.
</details>

<details>
<summary>💡 Hint 5: NovaPay-adaptern</summary>

NovaPay kastar `NovaPayException` med en `Code`-sträng. Adaptern fångar exceptionen och översätter koden (`"card_declined"`, `"expired_card"`, `"rate_limited"` och så vidare) till samma `PaymentFailure` som LegacyPay-adaptern använder. Det är hela poängen: två helt olika API:er ser likadana ut för affärskoden.
</details>

### 📚 Central concepts

- **Adapter pattern** – få ett objekt med ett inkompatibelt interface att passa det interface din kod förväntar sig. 🔗 [Refactoring.Guru: Adapter](https://refactoring.guru/design-patterns/adapter)
- **Gateway** – ett objekt som kapslar in åtkomsten till ett externt system. 🔗 [Martin Fowler: Gateway](https://martinfowler.com/eaaCatalog/gateway.html)
- **Dependency inversion principle** – din affärskod äger interfacet, och tredjepartskoden anpassas till det. 🔗 [Wikipedia: Dependency inversion principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
- **Primitive obsession** – att representera domänbegrepp med `int` och `string`, som ören och `"MMYY"`. 🔗 [Refactoring.Guru: Primitive Obsession](https://refactoring.guru/smells/primitive-obsession)
- **Shotgun surgery** – när en förändring kräver små ändringar på många ställen. 🔗 [Refactoring.Guru: Shotgun Surgery](https://refactoring.guru/smells/shotgun-surgery)
- **Async/await** – ditt interface kan vara asynkront även om det gamla SDK:t inte är det. 🔗 [Microsoft Learn: Asynchronous programming](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)

## 🤔 Reflektionsfrågor

1. Vem "äger" interfacet `IPaymentGateway`, din domän eller leverantören? Varför spelar det roll?
2. Hur avgjorde du vad som skulle finnas i interfacet? Vad hade hänt om du hade gjort det till en kopia av LegacyPays API?
3. Var placerade du återförsöket? Vilka för- och nackdelar har ditt val?
4. Om ni bara någonsin skulle använda LegacyPay, och bara på ett ställe, hade adaptern varit värd det?

---

🔑 **[Facit](../../../answers/01-refactor/legacy-payment-gateway.md)**. Gör ett ärligt försök först!
