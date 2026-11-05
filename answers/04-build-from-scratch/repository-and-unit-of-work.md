# 🔑 Facit: Repository and Unit of Work

> 🧩 En fullständig, testad lösning finns på branchen `solutions` i `solutions/04-build-from-scratch/repository-and-unit-of-work/`.

## 🏗️ Kärnan i lösningen

```csharp
public class EfLoanRepository(LibraryDbContext db) : ILoanRepository
{
    public async Task<IReadOnlyList<Loan>> GetOverdueAsync(DateOnly today, CancellationToken ct = default) =>
        await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.ReturnedOn == null && l.DueOn < today)
            .OrderBy(l => l.DueOn)
            .ToListAsync(ct);

    public void Add(Loan loan) => db.Loans.Add(loan);
    // ...
}

public class EfUnitOfWork(LibraryDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
```

## 🤔 Del 2

### 6. Hur många rader blev `EfUnitOfWork`?
**En.** `DbContext` *är* redan en Unit of Work: den registrerar alla ändringar (change tracking) och sparar dem i en transaktion (`SaveChanges`). Ditt interface lägger bara till ett namn och en abstraktion. Det kan ha ett värde, eftersom `LoanService` inte behöver känna till EF Core, men det tillför ingen *funktion*.

### 7. `DirectLoanService` jämfört med `LoanService`

```csharp
public class DirectLoanService(LibraryDbContext db, TimeProvider time)
{
    public async Task<Loan> BorrowAsync(int memberId, string isbn, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([memberId], ct) ?? throw new KeyNotFoundException();
        var book = await db.Books.SingleOrDefaultAsync(b => b.Isbn == isbn, ct) ?? throw new KeyNotFoundException();
        if (!book.IsAvailable) throw new InvalidOperationException();
        if (await db.Loans.CountAsync(l => l.MemberId == memberId && l.ReturnedOn == null, ct) >= 3)
            throw new InvalidOperationException();
        // ...
        await db.SaveChangesAsync(ct);
        return loan;
    }
}
```

| | `LoanService` + repositories | `DirectLoanService` |
|-|------------------------------|---------------------|
| **Antal filer och typer** | 5 interfaces/klasser + tjänsten | Bara tjänsten |
| **Läsbarhet i tjänsten** | ✅ Domänspråk: `CountActiveForMemberAsync` | ⚠️ LINQ-detaljer blandade med affärsregler |
| **Återanvändning av frågor** | ✅ Samma regel överallt | ⚠️ Risk att "aktivt lån" definieras olika på olika ställen |
| **Test mot SQLite** | Lika lätt | Lika lätt |
| **Test med fakes (utan databas)** | ✅ Möjligt | ❌ Kräver en databas |
| **Tillgång till EF Core-funktioner** | ⚠️ Bara det repositoryt exponerar | ✅ Allt: `Include`, projektioner, `AsNoTracking` |

### 8. Ställningstagande (exempelsvar)
> Jag skulle *inte* börja med repositories i ett nytt projekt med EF Core. Jag skulle använda `DbContext` direkt i små tjänster och samla återkommande frågor i extension methods. Jag skulle införa ett **domänspecifikt** repository när en fråga med domänlogik behövs på flera ställen, eller om domänlagret av arkitekturskäl inte får känna till EF Core. Ett **generiskt** `IRepository<T>` skulle jag inte införa.

✅ **Ett bra svar...** skiljer på generiska och domänspecifika repositories, ser att `DbContext` redan är en Unit of Work, och motiverar med konkreta förutsättningar, inte med "best practice".

## 🤔 Reflektionsfrågorna

### 1. "Vi behöver repositories för att kunna testa"
Argumentet blir **mycket svagare**. Testerna här kör snabbt mot en riktig databas (SQLite in-memory), och de testar dessutom att *frågorna* fungerar, vilket fejkade repositories aldrig kan göra. Microsoft själva rekommenderar att testa mot en riktig databas när det går. Repositories *möjliggör* tester utan databas, men det är inte längre det enda sättet.

> ⚠️ SQLite är inte SQL Server. Vissa saker beter sig olika, som datum, case sensitivity och vissa funktioner. För full säkerhet testar man även mot samma databasmotor som i produktion, till exempel med Testcontainers.

### 2. Två olika `DbContext`-instanser?
Då skulle `SaveChangesAsync` på unit of work spara *ingenting*. Ändringarna (bokens `IsAvailable` och det nya lånet) är registrerade i den *andra* contextens change tracker. Testet `Borrowing_creates_a_loan...` skulle bli rött. Det visar att repositories och unit of work **måste dela samma context**. Därför registreras `DbContext` som *scoped* i DI: alla som ber om den under samma request får samma instans.

### 3. Domänregeln i repositoryt
Repositoryt är en **rimlig** plats, eftersom regeln uttrycks som en fråga som ska köras i databasen. Alternativ:
- en **Specification** (en klass som kapslar in villkoret, `OverdueLoansSpecification`), som kan återanvändas av olika repositories,
- en **extension method** på `IQueryable<Loan>`,
- en **metod på entiteten** (`loan.IsOverdue(today)`). Det är bra för *ett* lån i minnet, men den kan inte översättas till SQL om den inte skrivs som ett `Expression`.

### 4. 50 frågor i `ILoanRepository`?
Det blir en **gudsklass**, ett stort interface som alla beror på. Motåtgärder:
- dela upp efter *användningsfall*: `IOverdueLoansQuery`, `IMemberLoanHistoryQuery` och så vidare (det är en tanke från CQRS),
- låt *läsfrågor* gå direkt mot `DbContext` med projektioner, och använd repositories bara för *skrivningar* och aggregat,
- använd Specification pattern för att slippa en metod per variant.
