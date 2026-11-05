# 🐢 Product catalog caching

> 🧩 **Mönstret som hjälper:** Decorator
> 📏 **Omfattning:** tre tjänster som använder en långsam katalog. Kravet är caching och tidsloggning, och du får **inte** ändra katalogens kod. Frestelsen är att lägga in cachen på tre ställen.
> 🧪 **Tester:** 6 befintliga tester är gröna. 6 acceptanstester för det nya kravet är avstängda med `Todo.Skip`.

## 🎯 Mål

Du ska lägga till caching och tidsmätning för en extern tjänst, och upptäcka att det går att göra utan att röra varken tjänsten eller koden som använder den, med **Decorator**.

## 📖 Bakgrund

Produktinformationen kommer från katalogteamets PIM-system via `RemoteProductCatalog`. Den ligger i mappen `Catalog/`, som är ett delat paket som **vi inte får ändra**. Varje anrop tar 300–900 ms.

Tre delar av butiken använder katalogen: **produktsidan**, **sökningen** och **varukorgen**. Kör programmet och se hur långsamt det är.

### 📋 Det nya kravet

> **Som** kund **vill jag** att sidorna laddar snabbt. **Som** driftansvarig **vill jag** se hur lång tid varje anrop till katalogen tar.

Reglerna:
- ⏱️ **Produkter** cachas i 5 minuter, och **sökningar** i 1 minut.
- 🔤 Sökningar som bara skiljer sig i versaler eller mellanslag räknas som samma sökning.
- 🚫 Produkter som **inte finns** cachas inte.
- 🤝 Produktsidan och varukorgen ska **dela** cache.
- 📝 Varje anrop som faktiskt går till katalogen loggas: `Remote GetProduct(H1) 120 ms`. Anrop över 500 ms märks med `SLOW `.
- 🚫 Koden i `Catalog/` får inte ändras.

## 🛠️ Uppgift

### Del 1: Lägg till funktionen 🛠️

1. ▶️ Kör programmet och testerna. Sätt `Todo.Skip` till `null`. 6 tester blir röda.
2. 🛠️ Gör testerna gröna på det sätt som känns naturligt *först*. Kanske genom att lägga till caching i de tre tjänsterna?
3. 💾 Gör en commit eller spara en kopia.

### Del 2: Reflektera 🤔

4. Svara på frågorna:
   - Hur många ställen lade du till caching och loggning på? Var det samma kod på flera ställen?
   - Hur fick du produktsidan och varukorgen att dela cache?
   - Imorgon kommer en fjärde tjänst, *rekommendationer*, som också använder katalogen. Vad behöver den som skriver den komma ihåg?

### Del 3: Refaktorera 🔧

5. 🎁 Flytta caching och loggning till **decorators** som implementerar `IProductCatalog`.
6. 🧅 Stapla dem i `Composition.cs`. Fundera noga på **ordningen** och på hur många **instanser** du skapar.
7. 🧹 Ta bort all caching- och loggningskod ur de tre tjänsterna.
8. ✅ Alla 12 tester ska vara gröna.

### ➕ Nästa förändring

9. Katalogen kraschar ibland. Lägg till en decorator som försöker en gång till om anropet kastar `HttpRequestException`. Var i stapeln ska den ligga? Ska ett nytt försök loggas som ett eget anrop?

## ▶️ Kom igång

```bash
dotnet run --project exercises/06-change-request/product-catalog-caching/ProductCatalogCaching
dotnet test exercises/06-change-request/product-catalog-caching/ProductCatalogCaching.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Tid i testerna</summary>

Testerna använder en `ManualTime` som styr både klockan (`GetUtcNow`) och tidsmätningen (`GetTimestamp`). Använd `TimeProvider` för båda: `time.GetUtcNow()` för utgångstider, och `time.GetTimestamp()` plus `time.GetElapsedTime(start)` för att mäta hur lång tid ett anrop tar. `Stopwatch` fungerar inte med en fejkad klocka.
</details>

<details>
<summary>💡 Hint 2: Ordningen</summary>

Kravet säger att bara anrop som *faktiskt går till katalogen* ska loggas. Cache-träffar ska alltså inte loggas. Vilken av de två decoratorerna måste då ligga ytterst?
</details>

<details>
<summary>💡 Hint 3: Delad cache</summary>

Om du skapar en decorator-stapel för varje tjänst får varje tjänst en egen cache. Skapa stapeln *en* gång och ge samma instans till alla tre.
</details>

<details>
<summary>💡 Hint 4: Sökningens cachenyckel</summary>

Använd en *normaliserad* nyckel (`term.Trim().ToLowerInvariant()`) för cachen, men skicka den ursprungliga termen vidare till katalogen. Titta på vad loggen förväntas innehålla.
</details>

### 📚 Central concepts

- **Decorator** – lägg till beteende utan att ändra det som lindas in. 🔗 [Refactoring.Guru: Decorator](https://refactoring.guru/design-patterns/decorator)
- **Caching i .NET** – `IMemoryCache` och `HybridCache`. 🔗 [Microsoft Learn: Caching in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/caching)
- **TimeProvider** – testbar tid och tidsmätning. 🔗 [Microsoft Learn: What is TimeProvider?](https://learn.microsoft.com/en-us/dotnet/standard/datetime/timeprovider-overview)
- **Open/closed principle** – utöka utan att ändra. 🔗 [Wikipedia: Open–closed principle](https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle)
- **Cross-cutting concerns** – caching och loggning är klassiska exempel. 🔗 [Wikipedia: Cross-cutting concern](https://en.wikipedia.org/wiki/Cross-cutting_concern)
- **Duplicate code** – kodlukten efter del 1. 🔗 [Refactoring.Guru: Duplicate Code](https://refactoring.guru/smells/duplicate-code)

## 🤔 Reflektionsfrågor

1. Varför ska "hittades inte" inte cachas? Finns det situationer där det *borde* cachas?
2. Vad händer om 100 användare samtidigt öppnar en produkt som inte finns i cachen än? Hur många anrop går till katalogen? Hur skulle du lösa det?
3. Cachen gör att en prisändring syns först efter upp till 5 minuter. Är det ett problem i varukorgen? Hur skulle du hantera det?

---

🔑 **[Facit](../../../answers/06-change-request/product-catalog-caching.md)**. Gör ett ärligt försök först!
