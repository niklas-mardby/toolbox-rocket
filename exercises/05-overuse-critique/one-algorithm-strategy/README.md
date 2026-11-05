# 🧮 One algorithm strategy

> 🧩 **Överanvänt mönster:** Strategy (plus en factory och ett "context")
> 📏 **Omfattning:** ungefär 100 rader för en beräkning som ryms på 15. En bra första övning i att våga ta bort kod.
> 🧪 **Tester:** finns och är gröna. Låt dem förbli gröna.

## 🎯 Mål

Du ska känna igen när Strategy används utan att det finns något att variera, och förenkla koden till det den faktiskt behöver vara.

## 📖 Bakgrund

Verktygsboden säljer bara i Sverige. Momsen har varit 25 % sedan 1990. Fakturan räknas ut med `InvoiceCalculator.CalculateTotals(lines)`.

Den förra utvecklaren ville göra beräkningen "framtidssäker". Öppna `Strategies.cs`: där finns fyra interfaces, fyra implementationer, en enum, en factory och ett *context* med setters. Varje interface har exakt **en** implementation.

## 🛠️ Uppgift

1. ▶️ **Kör testerna** och se att de är gröna.
2. ✍️ **Skriv ner din kritik** innan du ändrar något:
   - Hur många typer finns det? Hur många av dem gör något *på riktigt*?
   - Vilket problem skulle Strategy lösa här? Finns det problemet?
   - Hittar du något som är *farligt*, inte bara onödigt? (Titta på `InvoiceCalculationContext`.)
3. ✂️ **Förenkla.** Målet är att hela beräkningen ska rymmas i `InvoiceCalculator`. Kör testerna efter varje steg.
4. 🔍 **Jämför.** Hur många rader och typer har du nu? Vad syns *tydligare* i den nya koden?
5. 🤔 **Tänk framåt:** företaget planerar att börja sälja i Norge om två år. Ändrar det ditt svar? Vad skulle du göra *då*?

## ▶️ Kom igång

```bash
dotnet test exercises/05-overuse-critique/one-algorithm-strategy/OneAlgorithmStrategy.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Följ ett anrop</summary>

Börja i `InvoiceCalculator.CalculateTotals` och följ vad som faktiskt händer, steg för steg. Skriv ner varje *beräkning* som görs. Hur många är de?
</details>

<details>
<summary>💡 Hint 2: Inline, en i taget</summary>

Ta en strategi i taget: ersätt anropet till den med koden den innehåller ("Inline Class"), kör testerna och ta bort interfacet och klassen. Börja med den enklaste, `SwedishStandardVatRateProvider`.
</details>

<details>
<summary>💡 Hint 3: Det farliga</summary>

Vad händer om någon använder `InvoiceCalculationContext` och glömmer att anropa `SetRoundingStrategy`? När upptäcks felet: när koden kompileras eller när den körs? Vad hade en vanlig konstruktor eller en vanlig metod gett?
</details>

### 📚 Central concepts

- **Strategy** – och när det *inte* passar. 🔗 [Refactoring.Guru: Strategy](https://refactoring.guru/design-patterns/strategy)
- **Speculative generality** – abstraktioner "ifall att". 🔗 [Refactoring.Guru: Speculative Generality](https://refactoring.guru/smells/speculative-generality)
- **Lazy class** – klasser som inte gör nog för att motivera sig själva. 🔗 [Refactoring.Guru: Lazy Class](https://refactoring.guru/smells/lazy-class)
- **Inline class** – refaktoreringen som slår ihop en onödig klass med sin användare. 🔗 [Refactoring.Guru: Inline Class](https://refactoring.guru/inline-class)
- **YAGNI** – bygg inte det du inte behöver än. 🔗 [Martin Fowler: Yagni](https://martinfowler.com/bliki/Yagni.html)
- **Avrundning i .NET** – `MidpointRounding`. 🔗 [Microsoft Learn: Math.Round](https://learn.microsoft.com/en-us/dotnet/api/system.math.round)

## 🤔 Reflektionsfrågor

1. Den förra utvecklaren hänvisade till open/closed principle. Vad är fel med det resonemanget här?
2. Om Norge kommer om två år, är det bättre att ha abstraktionerna redan nu? Vad kostar det att ha dem i två år, och vad kostar det att lägga till dem *då*?
3. Vilken del av den gamla koden tycker du är *mest* skadlig? Varför?

---

🔑 **[Facit](../../../answers/05-overuse-critique/one-algorithm-strategy.md)**. Gör ett ärligt försök först!
