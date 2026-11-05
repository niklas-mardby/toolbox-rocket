# 📝 NOTES: Repository over EF Core

## 💡 Resonemanget

Det generiska repositoryt var inte bara onödigt. Det gjorde koden **långsammare** och **felaktig**.

| Problem i originalet | Konsekvens | I lösningen |
|----------------------|------------|-------------|
| `GetAll()` returnerar `IEnumerable<T>` via `ToList()` | **Hela tabellen** hämtas till minnet vid *varje* fråga. `CountLowStock` hämtar alla produkter för att räkna några. | Frågorna körs på `IQueryable` i databasen: `WHERE`, `ORDER BY`, `COUNT(*)`. |
| `Add`, `Update` och `Delete` anropar `SaveChanges()` själva | **Unit of Work är trasig.** `RaisePrices` sparar en produkt i taget. Om den andra misslyckas har den första redan sparats. | *En* `SaveChanges()` per operation. Allt eller inget. Se `AtomicityTests.cs`. |
| `EfUnitOfWork.Commit()` | Gör ingenting, eftersom allt redan är sparat. Den *ser* ut som en transaktion men är inte det. | Borttagen. `DbContext` *är* en Unit of Work. |
| `Update(entity)` | Onödigt för spårade entiteter. EF Core markerar dessutom *alla* kolumner som ändrade. | Ändra propertyn, och change tracking sköter resten. |
| Generiskt `IRepository<T>` | Döljer `Include`, `AsNoTracking`, projektioner och `ExecuteUpdate`. | Full tillgång till EF Core. |
| "Så att vi kan byta ORM" | Ingen planerar det, och abstraktionen hade inte gjort bytet lätt. | YAGNI. |

- **`Search` var en fälla.** `string.Contains(term, StringComparison.OrdinalIgnoreCase)` kan inte översättas till SQL. Den fungerade bara för att `GetAll()` redan hade hämtat allt till minnet. I lösningen används `ToLower().Contains(...)`, som översätts till `lower()` och `instr()` i SQLite. Observera att SQLite:s `lower()` bara hanterar ASCII. För svenska tecken krävs en collation, eller att sökningen görs på en normaliserad kolumn.
- **`AsNoTracking()`** på läsfrågor: datan ska inte ändras, så change tracking behövs inte.

## 🔀 Alternativ

- **`ExecuteUpdate`** för `RaisePrices`: `db.Products.Where(...).ExecuteUpdate(s => s.SetProperty(p => p.Price, p => ...))` ger en enda `UPDATE`-sats utan att entiteterna laddas. Avrundningsregeln är dock svårare att uttrycka exakt likadant i SQL, så loopen är tydligare här.
- **Ett *domänspecifikt* repository** (`IProductCatalog` med `GetActiveInCategory`, `Search`) kan vara motiverat om frågorna används på många ställen. Men det ska då exponera *domänfrågor*, inte `GetAll()`, och aldrig spara på egen hand.
- **Extension methods** på `IQueryable<Product>` (`db.Products.Active().InCategory(c)`) ger återanvändbara frågor utan ett extra lager.
