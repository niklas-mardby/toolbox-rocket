# 🌍 Second payment provider

> 🧩 **Mönstret som hjälper:** Adapter + Factory
> 📏 **Omfattning:** en kassa som idag bara pratar med Swish. Det nya kravet lägger till två leverantörer med helt olika SDK:er och regler för vilken leverantör som gäller i vilket land. Två samverkande mönster.
> 🧪 **Tester:** 6 befintliga tester är gröna. 7 acceptanstester (9 när testfallen räknas) är avstängda med `Todo.Skip`.

## 🎯 Mål

Du ska utöka en kassa från en till tre betalleverantörer, och upptäcka att problemet består av två delar: att **göra olika SDK:er lika** (Adapter) och att **välja rätt leverantör** (Factory).

## 📖 Bakgrund

Verktygsboden har bara sålt i Sverige, och `CheckoutService` anropar Swish direkt. Nu ska butiken öppna i Norge och resten av Europa. Avtalen är klara:

| Land | Leverantör | SDK |
|------|------------|-----|
| 🇸🇪 SE | Swish | `SwishClient`: synkront, `decimal`, statussträngar |
| 🇳🇴 NO | Vipps | `VippsClient`: `async`, belopp i øre, enum-tillstånd |
| 🇪🇺 DE, FI, DK med flera | Kort | `CardProcessor`: synkront, belopp i cent, exceptions vid nekat kort |

SDK:erna finns i `ThirdParty/Sdks.cs` och får inte ändras. Läs kommentarerna, de är SDK:ernas dokumentation.

### 📋 Det nya kravet

> **Som** kund i Norge eller resten av Europa **vill jag** kunna betala med det som är vanligt där jag bor.

- Alla leverantörer ska ge **samma** felmeddelanden till kunden: `"Betalningen nekades."` och `"Tekniskt fel, försök igen."`.
- Swish och Vipps kräver telefonnummer. Kort kräver kortnummer.
- Länder utanför Europa stöds fortfarande inte.

Detaljerna finns i `NewMarketsAcceptanceTests.cs`.

## 🛠️ Uppgift

### Del 1: Lägg till funktionen 🛠️

1. ▶️ Kör testerna. Sätt `Todo.Skip` till `null`. 7 tester blir röda.
2. 🛠️ Gör dem gröna. Låt `CheckoutService` ta in alla tre SDK:erna och lägg till grenar för varje land.
3. 💾 Gör en commit eller spara en kopia.

### Del 2: Reflektera 🤔

4. Svara på frågorna:
   - Hur stor blev `CheckoutService`? Hur mycket av den handlar om *kassan* och hur mycket om *SDK:erna*?
   - Hur många ställen har texten `"Betalningen nekades."` nu?
   - Nästa kvartal öppnar butiken i Nederländerna med **iDEAL**. Vad behöver ändras?
   - Hur skulle du testa Vipps-logiken utan att samtidigt testa Swish- och kortlogiken?

### Del 3: Refaktorera 🔧

5. 🔌 Inför ett gemensamt interface för betalleverantörer och en **adapter** per SDK.
6. 🏭 Inför något som **väljer** leverantör utifrån land.
7. 🧹 Låt `CheckoutService` använda dem. Den ska inte känna till någon SDK.
8. ✅ Alla 15 tester ska vara gröna.

### ➕ Nästa förändring

9. Svenska kunder ska kunna välja mellan Swish och kort. Hur ändras din väljare? Hur många filer behöver du röra?

## ▶️ Kom igång

```bash
dotnet run --project exercises/06-change-request/second-payment-provider/SecondPaymentProvider
dotnet test exercises/06-change-request/second-payment-provider/SecondPaymentProvider.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Belopp</summary>

Vipps vill ha øre som `long` och kortet vill ha cent som `int`. `499.50m * 100` blir `49950`. Samma omvandling behövs för båda, så skriv den på ett ställe.
</details>

<details>
<summary>💡 Hint 2: Telefonnummer</summary>

`"+47 900 00 001"` ska bli `"4790000001"`. Att behålla bara siffrorna (`char.IsDigit`) fungerar för båda leverantörerna.
</details>

<details>
<summary>💡 Hint 3: Ett gemensamt interface</summary>

Interfacet kan vara väldigt enkelt, eftersom `CheckoutRequest` och `CheckoutResult` redan finns och är *dina* typer:

```csharp
public interface IPaymentProvider
{
    Task<CheckoutResult> PayAsync(CheckoutRequest request);
}
```
</details>

<details>
<summary>💡 Hint 4: Väljaren</summary>

En klass som tar de tre adaptrarna och har en metod, `IPaymentProvider For(string country)`, med en `switch` på landet. En `switch` på *ett* ställe är helt okej. Det som var problemet var `switch`-satser utspridda överallt.
</details>

### 📚 Central concepts

- **Adapter** – gör olika SDK:er lika. 🔗 [Refactoring.Guru: Adapter](https://refactoring.guru/design-patterns/adapter)
- **Factory Method** – välj vilket objekt som ska användas. 🔗 [Refactoring.Guru: Factory Method](https://refactoring.guru/design-patterns/factory-method)
- **Gateway** – ett objekt som kapslar in ett externt system. 🔗 [Martin Fowler: Gateway](https://martinfowler.com/eaaCatalog/gateway.html)
- **Keyed services i DI** – registrera flera implementationer av samma interface med en nyckel. 🔗 [Microsoft Learn: Dependency injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- **Divergent change** – en klass som ändras av många olika skäl. 🔗 [Refactoring.Guru: Divergent Change](https://refactoring.guru/smells/divergent-change)
- **Large class** – det `CheckoutService` blir efter del 1. 🔗 [Refactoring.Guru: Large Class](https://refactoring.guru/smells/large-class)

## 🤔 Reflektionsfrågor

1. Var ska valideringen av telefonnummer och kortnummer ligga: i kassan, i väljaren eller i adaptrarna? Varför?
2. Om väljarens regler (land till leverantör) ska kunna ändras utan en ny release, hur skulle du göra då?
3. Jämför med [legacy-payment-gateway](../../01-refactor/legacy-payment-gateway/) och [payment-provider-adapter](../../04-build-from-scratch/payment-provider-adapter/), om du har gjort dem. Vad är nytt i den här övningen?

---

🔑 **[Facit](../../../answers/06-change-request/second-payment-provider.md)**. Gör ett ärligt försök först!
