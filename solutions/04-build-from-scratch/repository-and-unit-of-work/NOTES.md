# 📝 NOTES: Repository and Unit of Work

## 💡 Resonemanget

- **Alla repositories och unit of work delar *samma* `LibraryDbContext`.** Därför kan `LoanService` ändra en bok via ett repository, lägga till ett lån via ett annat, och spara båda med *ett* anrop. I en webbapp registreras `DbContext` som *scoped*, så alla får samma instans under en request.
- **`EfUnitOfWork` är en rad.** `DbContext` *är* redan en Unit of Work: den spårar ändringar och `SaveChanges` skickar dem i en transaktion. Ditt `IUnitOfWork` är bara en dörr till den. Det är en viktig insikt.
- **Repositoryt äger domänreglerna för *frågor*.** "Försenat" betyder `ReturnedOn == null && DueOn < today`. Den regeln finns på *ett* ställe, i `GetOverdueAsync`. Det är ett starkt argument för *domänspecifika* repositories.
- **`Include` i repositoryt.** Kontraktet lovar att `Book` och `Member` är laddade. Anroparen behöver inte veta något om EF Core:s laddningsstrategier.
- **`FindAsync`** för `GetByIdAsync` letar först bland redan spårade entiteter (Identity Map), och går till databasen först om entiteten inte redan spåras.
- **Allt filtreras i databasen.** `Where` och `OrderBy` körs på `IQueryable`, så SQLite gör jobbet. Ett vanligt misstag är att anropa `ToListAsync()` först och filtrera i minnet.

## 🔀 Alternativa lösningar och diskussionen

- **Utan repositories:** `LoanService` hade kunnat ta `LibraryDbContext` direkt. Koden hade blivit *kortare*, och testerna hade fungerat precis likadant, eftersom de ändå kör mot SQLite in-memory. Det är det viktigaste argumentet *mot* egna repositories ovanpå EF Core: de gör inte testningen lättare om du ändå testar mot en riktig databas.
- **Vad repositories tillför här:**
  - ✅ **namngivna frågor** med domänregler (`GetOverdueAsync`, `CountActiveForMemberAsync`),
  - ✅ **en tunn applikationstjänst** som inte känner till EF Core, så att den kan testas med fakes om man vill,
  - ⚠️ men också **fler filer** och risken att interfacet växer med en metod för varje ny fråga.
- **Extension methods** på `IQueryable<Loan>` (`db.Loans.Overdue(today)`) ger samma namngivna frågor utan interfaces. Det är ett populärt mellanläge.
- **Generiskt `IRepository<T>`** hade *inte* tillfört något. Se [repository-over-ef-core](../../../exercises/05-overuse-critique/repository-over-ef-core/) för en övning om just det.
