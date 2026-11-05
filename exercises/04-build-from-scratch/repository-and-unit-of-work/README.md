# 📚 Repository and Unit of Work

> 🧩 **Mönster:** Repository och Unit of Work, ovanpå EF Core
> 📏 **Omfattning:** fyra små klasser mot en riktig SQLite-databas i minnet. Själva koden är kort. Det svåra är att förstå *varför* den är så kort, och att ta ställning till om mönstret behövs alls.
> 🧪 **Tester:** 12 tester finns, avstängda med `Todo.Skip`. De kör mot en riktig SQLite-databas.

## 🎯 Mål

Du ska implementera **Repository** och **Unit of Work** med EF Core, och sedan bilda dig en egen uppfattning om när de tillför något och när de inte gör det.

## 📖 Bakgrund

Stadsbiblioteket har ett utlåningssystem. `LoanService` är färdig och innehåller affärsreglerna: högst tre lån per medlem, lånetid 21 dagar, och en bok kan inte lånas om den redan är utlånad. Tjänsten känner *bara* till interfacen i `Contracts.cs`: tre repositories och en unit of work. Den vet inte att EF Core finns.

Din uppgift är att skriva implementationerna med EF Core.

> 📚 **Två regler att komma ihåg:**
> 1. Ett **repository** registrerar ändringar (`Add`) och svarar på frågor. Det **sparar aldrig**.
> 2. En **unit of work** sparar *alla* ändringar tillsammans, allt eller inget.

## 🛠️ Uppgift

### Del 1: Bygg

1. 📖 Läs `Data.cs`, `Contracts.cs` och `LoanService.cs`. De är färdiga.
2. 🧪 Slå på testerna genom att sätta `Todo.Skip` till `null`. Titta på `LibraryDatabase.cs`: varje test får en egen SQLite-databas i minnet.
3. 🛠️ Implementera de fyra klasserna i `EfImplementations.cs`. Börja med `EfBookRepository` och `EfUnitOfWork` och kör `RepositoryTests`.
4. 🛠️ Implementera `EfMemberRepository` och `EfLoanRepository` och kör `LoanServiceTests`.
5. ▶️ Kör programmet:
   ```bash
   dotnet run --project exercises/04-build-from-scratch/repository-and-unit-of-work/RepositoryAndUnitOfWork
   ```

### Del 2: Ifrågasätt

6. 🤔 Hur många rader blev `EfUnitOfWork`? Vad säger det om relationen mellan `DbContext` och mönstret Unit of Work?
7. ✍️ Skriv en kopia av `LoanService` (kalla den `DirectLoanService`) som tar `LibraryDbContext` direkt i stället för repositories och unit of work. Jämför de två versionerna. Vilken är lättast att läsa? Vilken är lättast att testa?
8. ⚖️ Skriv ner ditt ställningstagande: skulle du använda repositories i ett riktigt projekt med EF Core? Under vilka förutsättningar?

## 💡 Hints

<details>
<summary>💡 Hint 1: Repositoryt skickar vidare till DbSet</summary>

Det mesta är en rad: `db.Books.Add(book)` eller `db.Books.SingleOrDefaultAsync(b => b.Isbn == isbn, ct)`. Kom ihåg att `using Microsoft.EntityFrameworkCore;` behövs för de asynkrona LINQ-metoderna.
</details>

<details>
<summary>💡 Hint 2: Unit of Work</summary>

`DbContext` håller redan reda på alla ändringar och sparar dem i en transaktion med `SaveChangesAsync`. Vad behöver din `EfUnitOfWork` göra mer än så?
</details>

<details>
<summary>💡 Hint 3: Hur kan två repositories dela en unit of work?</summary>

Om alla repositories och unit of work får *samma* `LibraryDbContext`-instans, så delar de också samma change tracker. Titta på hur testerna skapar dem.
</details>

<details>
<summary>💡 Hint 4: Relaterad data</summary>

`GetOverdueAsync` lovar att `Book` och `Member` är laddade, och `GetByIdAsync` för lån lovar att `Book` är laddad. Utan `Include` blir de `null` (det finns inga lazy loading-proxies i det här projektet).
</details>

<details>
<summary>💡 Hint 5: Filtrera i databasen</summary>

Skriv `Where` och `OrderBy` *före* `ToListAsync`. Om du hämtar alla lån till minnet och filtrerar där fungerar testerna, men med 100 000 lån blir det långsamt.
</details>

### 📚 Central concepts

- **Repository** – en samlingslik ingång till domänobjekt. 🔗 [Martin Fowler: Repository](https://martinfowler.com/eaaCatalog/repository.html)
- **Unit of Work** – spara ändringar tillsammans. 🔗 [Martin Fowler: Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- **Repositories med EF Core** – Microsofts resonemang om för- och nackdelar. 🔗 [Microsoft Learn: Design the infrastructure persistence layer](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- **Change tracking** – hur `DbContext` håller koll på ändringar. 🔗 [Microsoft Learn: Change Tracking in EF Core](https://learn.microsoft.com/en-us/ef/core/change-tracking/)
- **Saving data** – vad `SaveChanges` gör. 🔗 [Microsoft Learn: Basic Save](https://learn.microsoft.com/en-us/ef/core/saving/basic)
- **DbContext lifetime** – varför `DbContext` är scoped. 🔗 [Microsoft Learn: DbContext Lifetime, Configuration, and Initialization](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- **Testa EF Core-kod** – olika strategier, och var repositories passar in. 🔗 [Microsoft Learn: Choosing a testing strategy](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy)

## 🤔 Reflektionsfrågor

1. Testerna kör mot en riktig SQLite-databas, inte mot fejkade repositories. Vad betyder det för argumentet "vi behöver repositories för att kunna testa"?
2. Vad hade hänt om `LoanService` hade fått *två olika* `LibraryDbContext`-instanser, en för repositoryt och en för unit of work?
3. `ILoanRepository.GetOverdueAsync` innehåller en domänregel. Är ett repository rätt plats för den? Var annars kunde den ligga?
4. Om biblioteket växer till 50 olika frågor om lån, vad händer med `ILoanRepository`? Hur kan du förhindra att det blir en gudsklass?

---

🔑 **[Facit](../../../answers/04-build-from-scratch/repository-and-unit-of-work.md)**. Gör ett ärligt försök först!
