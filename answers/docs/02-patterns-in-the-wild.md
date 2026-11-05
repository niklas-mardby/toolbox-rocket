# 🔑 Facit: Patterns in the wild

> ⚠️ De flesta uppgifterna här är öppna. Du får därför exempelsvar och en lista över **vad ett bra svar tar hänsyn till**, inte ett enda rätt svar.

---

## 🅰️ Uppgift A

### A1. Två mönster: problemet och vad som händer utan dem

**Exempelsvar: Dependency Injection**
> Problemet DI löser är att klasser skapar sina beroenden själva och därmed blir hårt kopplade till konkreta implementationer. Utan DI i ett ASP.NET Core-projekt skulle varje controller göra `new AppDbContext(...)`, `new SmtpEmailSender(...)` och så vidare. Du skulle inte kunna byta till en testdatabas eller en fejkad mailtjänst i tester. Livstider skulle bli ett problem: vem ser till att `DbContext` kasseras efter varje request? Konfiguration som connection strings skulle spridas ut i koden.

**Exempelsvar: Options pattern**
> Utan Options pattern läser du konfiguration med strängnycklar som `config["Smtp:Port"]` direkt där den behövs. Ett stavfel ger `null` först när koden körs, och du måste parsa `"587"` till `int` på flera ställen. Med `IOptions<SmtpSettings>` får du en typad klass, validering vid uppstart och en klass som bara ser de inställningar den behöver.

✅ **Ett bra svar...**
- beskriver problemet *konkret*, gärna med kod eller ett scenario, inte bara "det blir rörigt".
- nämner **testbarhet** och **koppling** för DI.
- nämner för Repository/UoW att EF Core redan ger mycket av detta.
- nämner för CQRS att vinsten är tydliga, små handlers och en plats för tvärgående logik, men att priset är indirektion.
- nämner för Retry/Circuit Breaker att nätverksfel *är* normalt i distribuerade system och att utan dem kan en trög tjänst dra ner hela appen.

### A2. Ett klassiskt mönster "som 1994" och "som idag"

**Exempelsvar: Strategy för sortering**

```csharp
// 1994-stil
public interface IProductComparer { int Compare(Product a, Product b); }
public class ByPrice : IProductComparer
{
    public int Compare(Product a, Product b) => a.Price.CompareTo(b.Price);
}
public class ByName : IProductComparer
{
    public int Compare(Product a, Product b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal);
}
products.Sort(new ByPrice().Compare);

// Idag
var sorted = products.OrderBy(p => p.Price);
```

**Vinster:** mycket mindre kod, och logiken står precis där den används.
**Förluster:** en lambda har inget namn och kan inte ha eget state på ett tydligt sätt. Om samma jämförelse används på tio ställen, eller är komplex och behöver testas separat, är en namngiven klass (eller en namngiven statisk metod) fortfarande bättre.

✅ **Ett bra svar...**
- visar att det *är samma mönster* i båda fallen: utbytbart beteende bakom ett gemensamt kontrakt (`Func<T, TKey>` är kontraktet).
- ser att den moderna varianten inte alltid är bättre. Komplexa strategier med beroenden eller eget tillstånd mår ofta bra av en klass.

### A3. Result eller exception?

✅ **Ett bra svar...**
- skiljer på **förväntade** fel (validering, "hittades inte", affärsregler) och **oväntade** fel (databasen svarar inte, bugg).
- argumenterar för Result när felet är en del av det normala flödet och anroparen *måste* hantera det.
- argumenterar för exceptions när felet är exceptionellt, när det ska bubbla upp långt, eller när koden blir onödigt pratig av att skicka `Result` genom många lager.
- noterar att ASP.NET Core har bra stöd för båda: `Results.BadRequest(...)` respektive global felhantering med `UseExceptionHandler`.

---

## 🅱️ Uppgift B

### B1. I vilken ordning skrivs raderna ut?

Med koden från texten skrivs följande ut:

```
Producerar 0
0
Producerar 1
Producerar 2
20
Producerar 3
Producerar 4
40
```

**Varför?** Det är mönstret **Iterator** med *deferred execution* (uppskjuten körning). `Where` och `Select` kör ingenting när de anropas. De returnerar en ny iterator som "drar" värden från den föregående först när någon itererar. `foreach` ber om ett element i taget, så `GetNumbers` producerar ett värde, `Where` testar det, och om det passerar transformerar `Select` det och `foreach` skriver ut det. Sedan börjar det om. Udda tal (1 och 3) produceras men filtreras bort, därför syns "Producerar 1" utan att något tal skrivs ut efteråt.

### B2. Tre mönster i ett eget projekt

✅ Vanliga och korrekta exempel:
- `builder.Services.AddScoped<...>()` → **Dependency Injection**
- `WebApplication.CreateBuilder(args)` → **Builder**
- `app.UseAuthentication()`, `app.UseAuthorization()` → **Chain of Responsibility** (middleware)
- `DbContext` och `SaveChanges()` → **Unit of Work**
- `db.Products.Where(...)` → **Repository**-likt plus **Iterator** och query provider
- `IHttpClientFactory` eller `AddHttpClient` → **Factory**
- `ILogger<T>` som skapas av `ILoggerFactory` → **Factory**
- `IOptions<T>` → **Options pattern**

### B3. När tillför ett eget repository ovanpå EF Core något?

**Tillför något:**
> En faktureringsdomän har frågan "vilka fakturor är förfallna?". Den har en regel som inkluderar betalningsvillkor, helgdagar och delbetalningar och används på fem ställen. `IInvoiceRepository.GetOverdue(DateOnly today)` ger frågan ett namn, gör den testbar och ser till att alla fem ställen använder samma definition.

**Tillför inget:**
> `IRepository<T>` med `GetAll()`, `GetById()`, `Add()`, `Update()`, `Delete()` som bara anropar motsvarande på `DbSet<T>`. Det ger mer kod och gömmer `Include`, `AsNoTracking` och projektioner med `Select`. Dessutom leder `GetAll()` ofta till att hela tabellen hämtas till minnet innan den filtreras.

✅ **Ett bra svar...**
- skiljer på *generiska* och *domänspecifika* repositories.
- nämner att `DbContext` redan är en Unit of Work och att `DbSet<T>` redan är Repository-likt.
- tar upp testbarhet: EF Core går att testa mot SQLite in-memory, så "vi behöver repository för att kunna testa" är ett svagare argument än det låter.

---

## 🎓 Avslutande uppgift: exempelsvar

**Feature: `ILogger<T>`**

> 🔍 **Mönster:** `ILogger<T>` bygger på **Factory**, **Dependency Injection**, **Composite** och **Strategy** (eller Adapter).
>
> 🧩 **Hur:** När jag ber om `ILogger<OrderService>` i en konstruktor skapar DI-containern loggern via `ILoggerFactory` (Factory). Fabriken känner till alla registrerade `ILoggerProvider`s: Console, Debug, Application Insights och så vidare. Loggern jag får är egentligen en samling loggers, en per provider, och när jag loggar skickas meddelandet till alla (Composite). Varje provider är ett utbytbart sätt att skriva ut loggar (Strategy), och providers för tredjepartssystem som Serilog fungerar som Adapters mot respektive bibliotek.
>
> 🤔 **Varför:** Designerna ville att *min* kod bara ska bero på ett litet interface. Var loggarna hamnar ska vara ett konfigurationsbeslut i `Program.cs`, inte något som är hårdkodat i varje klass. Alternativet, att varje klass skapar sin egen `ConsoleLogger`, hade gjort det omöjligt att byta loggmål utan att ändra överallt.
>
> ⚖️ **Nackdel:** Indirektionen gör det svårt att förstå *var* en loggrad tar vägen. Du måste titta på konfigurationen. Och med många providers kostar varje loggning lite mer.

✅ **Ett bra svar...**
- pekar ut **deltagarna** i mönstret konkret (vilken klass är fabriken, vilken är produkten?).
- förklarar **problemet** designerna hade, inte bara vilket mönster de valde.
- nämner ett **alternativ** och varför det hade varit sämre.
- tar upp minst en **nackdel**. Inget mönster är gratis.
