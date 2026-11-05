# 🗃️ EF Core under the hood

> 🧩 **Mönster:** Unit of Work, Repository, Identity Map, Proxy och Iterator
> 📏 **Omfattning:** ett körbart program med fem små experiment mot en SQLite-databas i minnet. Du kör, observerar, förklarar och ändrar experimenten själv.
> 🧪 **Inga tester.** Programmets utskrift, inklusive den SQL som EF Core skickar, är det du analyserar.

## 🎯 Mål

Du ska se med egna ögon hur EF Core implementerar flera klassiska mönster, och kunna förklara vilket mönster som förklarar vad du ser.

## 📖 Bakgrund

Du har använt `DbContext`, `DbSet<T>` och `SaveChanges()` många gånger. Men vad händer egentligen? Programmet i `EfCoreUnderTheHood/` kör fem experiment och skriver ut både resultatet och **den SQL som EF Core skickar** till databasen (raderna med 🛢️).

## 🛠️ Uppgift

1. ▶️ **Kör programmet:**
   ```bash
   dotnet run --project exercises/02-spot-the-pattern/ef-core-under-the-hood/EfCoreUnderTheHood
   ```
2. 🔬 **Gå igenom experimenten ett i taget.** Läs koden för experimentet i `Program.cs` och titta på utskriften. Svara för varje experiment:
   - Vad hände?
   - Vilket mönster förklarar det?
   - Vilken klass eller metod i EF Core spelar vilken roll i mönstret?
3. 🤨 **Leta efter överraskningar.** Här är några saker att titta extra noga på:
   - I experiment 1: skickas det en SQL-fråga för *den andra* frågan också? Vad betyder det för vad mönstret är, och vad det *inte* är?
   - I experiment 2: varför står det `CustomerProxy` på vissa rader och `Customer` på andra?
   - I experiment 3: vad hände med David, som inte hade något fel?
   - I experiment 4: när skickas SQL-frågan för `Orders`, och hur många gånger?
4. 🧪 **Ändra experimenten:**
   - Experiment 1: hämta Anna med `db.Customers.Find(id)` två gånger. Hur många SQL-frågor skickas nu?
   - Experiment 4: ta bort `UseLazyLoadingProxies()` i `ShopContext.cs`. Vad händer med `anna.Orders.Count`? Hur får du ordrarna att laddas ändå?
   - Experiment 5: lägg till `.ToList()` efter `Select`. Vad ändras i utskriften?
5. 🤔 **Ta ställning:** Om du skulle skriva ett eget `IUnitOfWork` och ett eget `IRepository<Customer>` ovanpå det här, vad skulle de tillföra?

## 💡 Hints

<details>
<summary>💡 Hint 1: Experiment 1</summary>

Läs om **Identity Map** hos Martin Fowler. Mönstret garanterar att samma rad bara finns som *ett* objekt i minnet per `DbContext`. Det är inte samma sak som en cache. Jämför med hur många SQL-frågor du ser.
</details>

<details>
<summary>💡 Hint 2: Experiment 2 och 3</summary>

Titta på listan över `ChangeTracker.Entries()` och på raden `Committed transaction.`. Vem håller reda på alla ändringar, och när skickas de? Vad betyder det att alla ändringar skickas i *en* transaktion när en av dem misslyckas?
</details>

<details>
<summary>💡 Hint 3: Experiment 4</summary>

`GetType()` visar att objektet inte är en vanlig `Customer` utan en subklass. Den har EF Core genererat medan programmet körs. Den ser ut som en `Customer` och kan användas som en `Customer`, men den gör något extra när du rör `Orders`. Vilket mönster beskriver ett ställföreträdande objekt?
</details>

<details>
<summary>💡 Hint 4: Experiment 5</summary>

Titta på *när* SQL:en skickas. Det är inte på raden där frågan byggs, utan när `foreach` börjar. `query.Expression` visar att din lambda inte har kompilerats till kod utan sparats som ett uttrycksträd som EF Core sedan översätter till SQL.
</details>

### 📚 Central concepts

- **Unit of Work** – samla ändringar och spara dem tillsammans. 🔗 [Martin Fowler: Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- **Identity Map** – samma rad ger samma objekt. 🔗 [Martin Fowler: Identity Map](https://martinfowler.com/eaaCatalog/identityMap.html)
- **Lazy Load** – ladda data först när den behövs. 🔗 [Martin Fowler: Lazy Load](https://martinfowler.com/eaaCatalog/lazyLoad.html)
- **Proxy** – ett objekt som står i för ett annat. 🔗 [Refactoring.Guru: Proxy](https://refactoring.guru/design-patterns/proxy)
- **Change tracking i EF Core** – hur `DbContext` håller koll på dina entiteter. 🔗 [Microsoft Learn: Change Tracking in EF Core](https://learn.microsoft.com/en-us/ef/core/change-tracking/)
- **Identity resolution** – EF Core:s egen förklaring av Identity Map. 🔗 [Microsoft Learn: Identity Resolution in EF Core](https://learn.microsoft.com/en-us/ef/core/change-tracking/identity-resolution)
- **Lazy loading** – hur och när proxies laddar relaterad data. 🔗 [Microsoft Learn: Lazy Loading of Related Data](https://learn.microsoft.com/en-us/ef/core/querying/related-data/lazy)
- **Transactions** – vad `SaveChanges` gör i en transaktion. 🔗 [Microsoft Learn: Using Transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions)
- **Expression trees** – kod som data. 🔗 [Microsoft Learn: Expression Trees](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/expression-trees/)

## 🤔 Reflektionsfrågor

1. Varför vill du *inte* att `DbContext` ska leva länge, till exempel som singleton i en webbapp? Tänk på Identity Map och Unit of Work.
2. Lazy loading verkar bekvämt. Vilket prestandaproblem kan det orsaka om du loopar över 100 kunder och rör `customer.Orders` för var och en?
3. Experiment 5 visar att `IQueryable<T>` översätts till SQL. Vad händer om du skriver en metod i C# som EF Core inte kan översätta, och anropar den i en `Where`?

---

🔑 **[Facit](../../../answers/02-spot-the-pattern/ef-core-under-the-hood.md)**. Gör ett ärligt försök först!
