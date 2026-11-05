# 🗄️ Repository over EF Core

> 🧩 **Överanvänt mönster:** generiskt Repository och Unit of Work ovanpå EF Core
> 📏 **Omfattning:** ett produktregister med sex operationer mot en SQLite-databas. Koden fungerar och testerna är gröna, men abstraktionen döljer **två verkliga buggar** och ett prestandaproblem. Att hitta dem kräver att du förstår hur EF Core fungerar under huven.
> 🧪 **Tester:** finns och är gröna. Du får ändra `Composition.cs` men inte de befintliga testerna. Du får gärna *lägga till* tester.

## 🎯 Mål

Du ska se varför ett generiskt repository ovanpå EF Core ofta gör saker *sämre*, och skriva om koden så att den använder EF Core som det är tänkt.

## 📖 Bakgrund

Teamet har följt "best practice": all databasåtkomst går via `IRepository<T>` och `IUnitOfWork` i `Repositories.cs`, "så att vi kan byta ORM senare". `ProductService` använder bara de två interfacen.

Kör programmet. Det skriver ut 🛢️ för varje SQL-kommando som skickas till databasen. Titta på hur många som behövs, och fundera på vad varje kommando hämtar.

## 🛠️ Uppgift

1. ▶️ **Kör testerna och programmet:**
   ```bash
   dotnet test exercises/05-overuse-critique/repository-over-ef-core/RepositoryOverEfCore.Tests
   dotnet run --project exercises/05-overuse-critique/repository-over-ef-core/RepositoryOverEfCore
   ```
2. 🔍 **Läs `Repositories.cs` och `ProductService.cs`.** Leta efter:
   - 🐢 ett **prestandaproblem**: vad hämtas från databasen vid varje fråga?
   - 🐛 en **bugg i Unit of Work**: vad gör egentligen `IUnitOfWork.Commit()`?
   - 🐛 en **dold fälla** som bara fungerar på grund av prestandaproblemet.
3. 🧪 **Bevisa buggen.** Skriv ett *nytt* test som visar att `RaisePrices` inte är "allt eller inget". Titta på regeln i `Data.cs`: inget får kosta mer än 20 000 kr. Testet ska bli **rött** med den nuvarande koden.
4. ✂️ **Skriv om** `ProductService` så att den använder `ShopDbContext` direkt, och ta bort `Repositories.cs`. Alla tester, även ditt nya, ska vara gröna.
5. 🛢️ **Kör programmet igen.** Hur många SQL-kommandon blir det nu? Vad innehåller de?

## 💡 Hints

<details>
<summary>💡 Hint 1: GetAll</summary>

`GetAll()` returnerar `db.Set<T>().ToList()`. `ToList()` kör frågan direkt och hämtar *alla rader*. Allt efter det, `Where`, `OrderBy` och `Count`, sker i minnet. Med sex produkter märks det inte. Med 600 000 gör det det.
</details>

<details>
<summary>💡 Hint 2: Var sparas det?</summary>

Sök efter `SaveChanges` i `Repositories.cs`. Hur många gånger anropas det när `RaisePrices` ändrar tre produkter? Vad händer om den andra produkten bryter mot en regel i databasen?
</details>

<details>
<summary>💡 Hint 3: Testet för buggen</summary>

Skapa en egen databas i testet med två produkter i samma kategori: en billig och en som kostar 19 000 kr. Höj priserna med 10 %. Den dyra blir 20 900 kr och bryter mot regeln, så `SaveChanges` kastar `DbUpdateException`. Kontrollera sedan, med en *ny* `ShopDbContext`, vad den billiga produkten kostar.
</details>

<details>
<summary>💡 Hint 4: Den dolda fällan</summary>

`Search` använder `string.Contains(term, StringComparison.OrdinalIgnoreCase)`. Fungerar det om frågan körs i databasen i stället för i minnet? Prova! EF Core ger ett tydligt felmeddelande. Hur kan du göra en sökning som är skiftlägesokänslig i SQL?
</details>

### 📚 Central concepts

- **Repository och dess kritik** – Microsofts eget resonemang. 🔗 [Microsoft Learn: Design the infrastructure persistence layer](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- **Unit of Work** – vad det faktiskt betyder. 🔗 [Martin Fowler: Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- **Transaktioner i EF Core** – `SaveChanges` är redan en transaktion. 🔗 [Microsoft Learn: Using Transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions)
- **IQueryable kontra IEnumerable** – var frågan körs. 🔗 [Microsoft Learn: IQueryable](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable)
- **Change tracking** – varför `Update()` sällan behövs. 🔗 [Microsoft Learn: Change Tracking in EF Core](https://learn.microsoft.com/en-us/ef/core/change-tracking/)
- **Middle Man** – ett lager som bara skickar vidare. 🔗 [Refactoring.Guru: Middle Man](https://refactoring.guru/smells/middle-man)
- **YAGNI** – "så att vi kan byta ORM". 🔗 [Martin Fowler: Yagni](https://martinfowler.com/bliki/Yagni.html)

## 🤔 Reflektionsfrågor

1. Varför syntes inte buggarna i testerna från början? Vad säger det om vad tester kan och inte kan visa?
2. Abstraktionen skulle göra det lätt att "byta ORM". Hade den gjort det? Vad hade behövt ändras om ni bytt till Dapper?
3. Finns det någon situation där du skulle behålla ett repository i den här appen? Hur skulle det se ut?
4. Hur skulle du förklara för en kollega att "best practice" här gjorde koden sämre, utan att kollegan känner sig angripen?

---

🔑 **[Facit](../../../answers/05-overuse-critique/repository-over-ef-core.md)**. Gör ett ärligt försök först!
