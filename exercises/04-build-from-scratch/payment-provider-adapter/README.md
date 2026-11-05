# 💳 Payment provider adapter

> 🧩 **Mönster:** Adapter
> 📏 **Omfattning:** två adapters för två fejkade tredjeparts-SDK:er med *helt* olika API:er: det ena synkront med statuskoder, det andra asynkront med exceptions. Översättningen kräver noggrannhet med belopp och felkoder.
> 🧪 **Tester:** 19 tester (25 när testfallen räknas) finns, avstängda med `Todo.Skip`. Samma *kontraktstester* körs mot båda adaptrarna.

## 🎯 Mål

Du ska skriva två **Adapters** som får två olika betal-SDK:er att se exakt likadana ut för resten av appen, bakom interfacet `IPaymentProvider`.

## 📖 Bakgrund

Webbshoppen ska ta betalt via två leverantörer: **QuickPay** för nordiska valutor och **GlobalCard** för internationella. Interfacet `IPaymentProvider` i `Domain.cs` beskriver hur *vår* app vill prata om betalningar: `decimal`-belopp, vår egen `CardDetails`-typ och ett `PaymentResult` med en tydlig `FailureReason`.

SDK:erna i `ThirdParty/` ser helt annorlunda ut:

| | QuickPay | GlobalCard |
|-|----------|------------|
| Anrop | Synkrona | `async` |
| Belopp | ören som `long` | cent som `int` |
| Valuta | `string` (SEK, NOK, DKK) | `enum Iso4217` (SEK, EUR, USD) |
| Utgångsdatum | `"YYMM"` | månad och år separat |
| Fel | `Status` + `ReasonCode`-strängar | exceptions |

## 🛠️ Uppgift

1. 📖 Läs `Domain.cs` och båda SDK:erna i `ThirdParty/`. Läs kommentarerna högst upp, de är SDK:ernas "dokumentation".
2. 🧪 Slå på testerna genom att sätta `Todo.Skip` till `null` i `PaymentProviderAdapter.Tests/Todo.cs`. Titta på hur `PaymentProviderContractTests` körs mot båda adaptrarna.
3. 🔌 Implementera `QuickPayAdapter` i `Adapters.cs`.
4. 🔌 Implementera `GlobalCardAdapter`. Här blir exceptions till resultat.
5. ✅ Alla tester ska vara gröna. Kör programmet:
   ```bash
   dotnet run --project exercises/04-build-from-scratch/payment-provider-adapter/PaymentProviderAdapter
   ```
6. ➕ **Bonus:** Skriv en `PaymentProviderSelector` som väljer QuickPay för SEK, NOK och DKK och GlobalCard för EUR och USD. Vilket mönster är det?

## 💡 Hints

<details>
<summary>💡 Hint 1: Börja med QuickPay</summary>

QuickPay är enklast: bygg ett `QuickPayDebit`-objekt, anropa `Debit` och översätt svaret. Eftersom QuickPay är synkront kan du returnera `Task.FromResult(...)`.
</details>

<details>
<summary>💡 Hint 2: Belopp och avrundning</summary>

`199.99m * 100` blir `19999.00m`. Det är säkert att konvertera till `long`. Men vad händer med `10.005m`? Testerna kräver att belopp med fler än två decimaler avvisas med `ArgumentException`. Jämför `amount * 100` med `decimal.Truncate(amount * 100)`. Den här logiken är densamma för båda adaptrarna. Var ska den ligga?
</details>

<details>
<summary>💡 Hint 3: Exceptions blir resultat</summary>

Vårt interface lovar att *betalningsfel* är resultat, inte exceptions (se testet `Failures_are_results_not_exceptions`). Fånga GlobalCards specifika exceptions och översätt dem. Fånga *inte* `Exception` i allmänhet, för en bugg ska fortfarande krascha tydligt.
</details>

<details>
<summary>💡 Hint 4: Valutan i GlobalCard</summary>

`Enum.TryParse<Iso4217>("JPY", out _)` returnerar `false`. Men `Enum.TryParse<Iso4217>("1", out _)` returnerar `true`! Kontrollera valutan innan du anropar SDK:t.
</details>

### 📚 Central concepts

- **Adapter pattern** – få ett inkompatibelt interface att passa. 🔗 [Refactoring.Guru: Adapter](https://refactoring.guru/design-patterns/adapter)
- **Gateway** – Fowlers namn på objekt som kapslar in externa system. 🔗 [Martin Fowler: Gateway](https://martinfowler.com/eaaCatalog/gateway.html)
- **Liskov substitution principle** – alla implementationer av ett interface ska kunna bytas mot varandra. Kontraktstesterna kontrollerar just det. 🔗 [Wikipedia: Liskov substitution principle](https://en.wikipedia.org/wiki/Liskov_substitution_principle)
- **Exceptions och best practices** – när du ska kasta och när du ska returnera. 🔗 [Microsoft Learn: Best practices for exceptions](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)
- **Async/await** – ditt interface är asynkront även om SDK:t inte är det. 🔗 [Microsoft Learn: Asynchronous programming](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)
- **Enums** – och fällan med `Enum.TryParse`. 🔗 [Microsoft Learn: Enumeration types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum)

## 🤔 Reflektionsfrågor

1. Varför kastar adaptern `ArgumentException` för `10.005m` men returnerar ett resultat för ett nekat kort? Var går gränsen?
2. Vad är fördelen med kontraktstester jämfört med att skriva separata tester för varje adapter?
3. GlobalCard har en `DeclineCode.SuspectedFraud` som inte finns i vår `FailureReason`. Var det rätt att mappa den till `Other`? Vad skulle du göra om affären ville behandla bedrägeri annorlunda?
4. Om ni lägger till en tredje leverantör, vad behöver du ändra, och vad behöver du *inte* ändra?

---

🔑 **[Facit](../../../answers/04-build-from-scratch/payment-provider-adapter.md)**. Gör ett ärligt försök först!
