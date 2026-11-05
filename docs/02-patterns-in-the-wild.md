# 🌍 Patterns in the wild

> 📖 **Självstudier.** Läs i din egen takt. Texten har två delar, och varje del avslutas med en kort uppgift. Ett facit finns i [`answers/docs/02-patterns-in-the-wild.md`](../answers/docs/02-patterns-in-the-wild.md).

Boken *Design Patterns: Elements of Reusable Object-Oriented Software* kom 1994. Den skrevs av fyra författare, därför kallas den ofta **"Gang of Four"** eller **GoF**. Den beskriver 23 mönster och är fortfarande grunden för det mesta som sägs om design patterns.

Men mycket har hänt sedan 1994. Vi har fått webbapplikationer, ORM:er, molntjänster, mikrotjänster och programspråk med lambdas och generics. Det här dokumentet handlar om två saker:

- **Del A:** mönster som har blivit viktiga *efter* GoF, och hur C# som språk har "svalt" flera klassiska mönster.
- **Del B:** mönster som redan finns *inbyggda* i .NET, och som du använder varje dag utan att tänka på det.

---

## 🅰️ Del A: Mönster som kom efter Gang of Four

### 💉 Dependency Injection och Inversion of Control

**Vad?** En klass får sina beroenden utifrån, via konstruktorn, i stället för att skapa dem själv. *Inversion of Control* (IoC) är den bredare principen: det är inte din kod som styr när och hur saker skapas och anropas, det gör ramverket. ("Don't call us, we'll call you.")

**Varifrån?** Idéerna fanns tidigt, men Martin Fowler gav dem namnet *Dependency Injection* i artikeln [Inversion of Control Containers and the Dependency Injection pattern](https://martinfowler.com/articles/injection.html) (2004). Containrar som Spring (Java) och senare Autofac och Ninject (.NET) gjorde mönstret mainstream.

**Varför viktigt i .NET?** ASP.NET Core är byggt kring en DI-container från grunden. Allt, från `ILogger<T>` till `DbContext` och dina egna tjänster, registreras i `builder.Services` och injiceras i konstruktorer. Du kan inte skriva modern ASP.NET Core utan att förstå DI.

### 🗄️ Repository och Unit of Work

**Vad?** Ett *Repository* beter sig som en samling av domänobjekt i minnet. *Unit of Work* håller ordning på alla ändringar i en affärstransaktion och sparar dem tillsammans.

**Varifrån?** Martin Fowlers bok *Patterns of Enterprise Application Architecture* (2002), ofta kallad PoEAA. Se [Repository](https://martinfowler.com/eaaCatalog/repository.html) och [Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html) i hans katalog.

**Varför viktigt i .NET?** EF Core implementerar båda: `DbSet<T>` är Repository-likt och `DbContext` är en Unit of Work. Därför pågår en ständig diskussion om du ska skriva egna repositories ovanpå EF Core. (Mer om det i Del B.)

### 📨 CQRS och Mediator-pipelines

**Vad?** *CQRS* (Command Query Responsibility Segregation) betyder att du skiljer på operationer som **ändrar** data (commands) och operationer som **läser** data (queries). *Mediator* är ett GoF-mönster, men i .NET-världen används det ofta på ett nytt sätt: varje command eller query är ett objekt som skickas till en mediator som hittar rätt handler. Runt alla handlers kan du lägga *pipeline behaviors* för validering, loggning och transaktioner.

**Varifrån?** Bertrand Meyer formulerade *Command Query Separation* (CQS) på 1980-talet: en metod ska antingen ändra något eller returnera något, inte båda. Greg Young lyfte idén från metoder till hela arkitekturen och kallade det CQRS runt 2010. I .NET gjorde Jimmy Bogards bibliotek [MediatR](https://github.com/jbogard/MediatR) mediator-pipelines populärt.

**Varför viktigt i .NET?** Mönstret används i många "Clean Architecture"-mallar och företagsprojekt. Du kommer att möta det. Men det lägger till indirektion, så det är viktigt att du förstår både vinsterna och kostnaderna. Läs gärna Martin Fowlers varning i [CQRS](https://martinfowler.com/bliki/CQRS.html).

### ⚙️ Options pattern (`IOptions<T>`)

**Vad?** Konfiguration (från `appsettings.json`, miljövariabler och så vidare) binds till en starkt typad klass som injiceras via `IOptions<T>`, `IOptionsSnapshot<T>` eller `IOptionsMonitor<T>`.

```csharp
public class SmtpSettings
{
    public required string Host { get; init; }
    public int Port { get; init; } = 587;
}

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

public class EmailSender(IOptions<SmtpSettings> options) { /* options.Value.Host */ }
```

**Varifrån?** Det är ett .NET-specifikt mönster som kom med ASP.NET Core. Det bygger på DI och på principen att en klass bara ska få den konfiguration den behöver, inte hela `IConfiguration`.

**Varför viktigt?** Du slipper magiska strängar som `config["Smtp:Host"]` spridda i koden. Du får validering vid uppstart och kan ladda om inställningar under körning. Läs mer i [Options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options).

### ✅ Result pattern

**Vad?** I stället för att kasta exceptions för *förväntade* fel ("kunden finns inte", "otillräckligt saldo") returnerar metoden ett objekt som berättar om det gick bra eller dåligt.

```csharp
public record Result<T>(bool IsSuccess, T? Value, string? Error)
{
    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}

Result<Order> result = orderService.PlaceOrder(cmd);
if (!result.IsSuccess) return Results.BadRequest(result.Error);
```

**Varifrån?** Idén kommer från funktionell programmering (`Either` i Haskell, `Result` i Rust och F#). Den har blivit populär i C# de senaste åren.

**Varför viktigt?** Exceptions är dyra och osynliga i metodsignaturen. Med ett `Result` syns det i typen att metoden kan misslyckas, och anroparen tvingas hantera det. Men exceptions är fortfarande rätt för *oväntade* fel, som en databas som inte svarar. Se Microsofts [best practices för exceptions](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions).

### 🛡️ Resilience-mönster: Retry och Circuit Breaker

**Vad?**
- **Retry:** när ett anrop till en annan tjänst misslyckas tillfälligt, försök igen, gärna med växande väntetid mellan försöken (*exponential backoff*).
- **Circuit Breaker:** om en tjänst misslyckas gång på gång, sluta anropa den en stund ("bryt kretsen") så att den får återhämta sig och din app inte hänger på timeouts.

**Varifrån?** Michael Nygards bok *Release It!* (2007) och Martin Fowlers artikel [CircuitBreaker](https://martinfowler.com/bliki/CircuitBreaker.html). I .NET har biblioteket [Polly](https://www.pollydocs.org/) länge varit standard. Numera finns det inbyggt via `Microsoft.Extensions.Resilience` och `Microsoft.Extensions.Http.Resilience`, som bygger på Polly.

```csharp
builder.Services.AddHttpClient<WeatherClient>()
    .AddStandardResilienceHandler(); // retry, circuit breaker, timeout med mera
```

**Varför viktigt?** I molnet och i mikrotjänster *kommer* nätverksanrop att misslyckas ibland. Läs [Build resilient HTTP apps](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience). Notera förresten att resilience-hanterarna läggs in som `DelegatingHandler`s, alltså Decorators runt `HttpClient`!

### 🍽️ Hur C# "åt upp" klassiska mönster

Många GoF-mönster skrevs för C++ och Smalltalk på 90-talet. Sedan dess har C# fått språkfunktioner som gör flera mönster så enkla att de knappt syns:

| Klassiskt mönster | Vad C# gav oss | Exempel |
|-------------------|----------------|---------|
| **Strategy** | Delegates och lambdas | `list.Sort((a, b) => a.Price.CompareTo(b.Price))` i stället för en `PriceComparer`-klass |
| **Command** | Delegates (`Action`, `Func<T>`) | `var undo = () => document.Restore(snapshot);` |
| **Observer** | `event` och `IObservable<T>` | `button.Click += OnClick;` |
| **Iterator** | `IEnumerable<T>` och `yield return` | `foreach` fungerar på allt som är `IEnumerable` |
| **Singleton** | `Lazy<T>` och DI-singletons | `services.AddSingleton<ICache, MemoryCache>()` |
| **Template Method** | Delegates och virtuella metoder i ramverk | `BackgroundService.ExecuteAsync` |

> 💡 **Mönstret försvinner inte, det blir bara billigare.** En lambda som skickas till `OrderBy` är fortfarande en Strategy. Det är bara ingen som behöver skriva ett interface och en klass för den.

Ett exempel på Singleton förr och nu:

```csharp
// 🙁 Klassisk GoF-Singleton: global, svår att testa, svår att byta ut
public sealed class Cache
{
    private static readonly Lazy<Cache> _instance = new(() => new Cache());
    public static Cache Instance => _instance.Value;
    private Cache() { }
}

// 🙂 Modernt: DI-containern ser till att det bara finns en, och klassen är en vanlig klass
builder.Services.AddSingleton<ICache, Cache>();
```

### ✏️ Uppgift A

1. Välj **två** av mönstren i Del A. Beskriv för vart och ett med egna ord: vilket problem löser det, och vad skulle hända i ett ASP.NET Core-projekt om du *inte* använde det?
2. Ta ett klassiskt GoF-mönster ur tabellen ovan och skriv det på två sätt: först "som 1994" med interface och klasser, sedan "som idag" med C#-språkfunktioner. Vad vinner du? Förlorar du något?
3. 🤔 Result pattern och exceptions: hitta ett ställe i en egen tidigare kod där du kastar en exception för ett *förväntat* fel. Skulle ett Result vara bättre där? Varför, eller varför inte?

---

## 🅱️ Del B: Mönster som redan är inbyggda i .NET

Det bästa sättet att förstå mönster är att se dem i kod du redan använder. Här är några av de viktigaste ställena i .NET.

### 🗃️ EF Core

| Del av EF Core | Mönster | Förklaring |
|----------------|---------|------------|
| `DbContext` | **Unit of Work** | Spårar alla ändringar du gör. `SaveChanges()` skickar alla ändringar i en transaktion, allt eller inget. |
| `DbSet<T>` | **Repository**-likt | En samling-liknande ingång till alla entiteter av en typ: `Add`, `Remove`, `Find`, plus LINQ. |
| Change tracking | **Identity Map** | Om du hämtar samma rad två gånger i samma `DbContext` får du *samma objekt* tillbaka. Se [Identity Map](https://martinfowler.com/eaaCatalog/identityMap.html). |
| Lazy loading | **Proxy** (virtual proxy) | Med `UseLazyLoadingProxies()` skapar EF Core en subklass av din entitet. Den laddar navigeringsproperties från databasen först när du rör dem. |

```csharp
using var db = new ShopContext();
var a = db.Customers.First(c => c.Id == 1);
var b = db.Customers.First(c => c.Id == 1);
Console.WriteLine(ReferenceEquals(a, b)); // True – Identity Map!

a.Name = "Nytt namn";          // Change tracking märker ändringen
db.Orders.Add(new Order());     // ...och den här
db.SaveChanges();               // Unit of Work: båda sparas i en transaktion
```

> 🤔 **Debatten:** Om `DbContext` redan är en Unit of Work och `DbSet<T>` redan är ett Repository, vad tillför då ett eget `IRepository<T>`? Ofta inget. Det kan till och med göra saker sämre, eftersom det gömmer `Include`, projektioner och `AsNoTracking`. Men ett *specifikt* repository med domänmetoder (`GetOverdueInvoices()`) kan vara värt det. Du testar den här frågan i övningarna [repository-and-unit-of-work](../exercises/04-build-from-scratch/repository-and-unit-of-work/) och [repository-over-ef-core](../exercises/05-overuse-critique/repository-over-ef-core/).

### 🔁 LINQ

- **Iterator:** Alla LINQ-metoder på `IEnumerable<T>` returnerar nya iteratorer. Inget händer förrän du itererar (*deferred execution*). Internt använder de ofta `yield return`.
- **Expression trees:** När du skriver `db.Customers.Where(c => c.Age > 18)` mot en `IQueryable<T>` blir lambdan inte kompilerad kod utan ett *träd* som beskriver uttrycket. Det är ett exempel på **Interpreter**- och **Composite**-idéer.
- **Query provider:** `IQueryProvider` översätter trädet till något annat, till exempel SQL. Det är samma LINQ-syntax men olika "backend", vilket är en Strategy- och Adapter-liknande idé.

```csharp
IEnumerable<int> numbers = GetNumbers().Where(n => n % 2 == 0).Select(n => n * 10);
// Inget har körts än! Först nu:
foreach (var n in numbers) Console.WriteLine(n);

static IEnumerable<int> GetNumbers()
{
    for (var i = 0; i < 5; i++)
    {
        Console.WriteLine($"Producerar {i}");
        yield return i;
    }
}
```

### 📣 Events och `IObservable<T>`

- C#-`event` är **Observer** inbyggt i språket.
- `IObservable<T>` och `IObserver<T>` är .NET:s formella Observer-interfaces. Biblioteket Reactive Extensions (Rx.NET) bygger på dem. Se [Observer design pattern](https://learn.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern).
- `INotifyPropertyChanged` är Observer för UI-bindning (WPF, MAUI, Blazor).

### 🧅 ASP.NET Core middleware

Varje middleware får `HttpContext` och en referens till *nästa* middleware. Den kan göra något före, anropa nästa, göra något efter, eller **kortsluta** och skicka svar direkt utan att anropa nästa.

```csharp
app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    await next(context);                                   // skicka vidare i kedjan
    Console.WriteLine($"{context.Request.Path} tog {sw.ElapsedMilliseconds} ms");
});

app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Api-Key"))
    {
        context.Response.StatusCode = 401;                 // kortslut kedjan!
        return;
    }
    await next(context);
});
```

- Kedjan av hanterare som var och en kan välja att skicka vidare eller stoppa är **Chain of Responsibility**.
- Att varje middleware "lindar in" resten av pipelinen, med kod före och efter, är **Decorator**-likt beteende.

Läs mer i [ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/).

### 🏭 `IHttpClientFactory` och 🧱 `WebApplication.CreateBuilder`

- **`IHttpClientFactory` är en Factory.** Den skapar `HttpClient`-instanser med rätt konfiguration och återanvänder underliggande anslutningar, så att du slipper problem med uttömda sockets. Se [IHttpClientFactory](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory).
- **`WebApplication.CreateBuilder()` är en Builder.** Du får en builder, konfigurerar tjänster, konfiguration och loggning steg för steg, och anropar sedan `builder.Build()` för att få den färdiga `WebApplication`.

```csharp
var builder = WebApplication.CreateBuilder(args);   // Builder skapas
builder.Services.AddControllers();                    // ...konfigureras steg för steg
builder.Logging.AddConsole();
var app = builder.Build();                           // ...och bygger den färdiga produkten
```

### 💉 Den inbyggda DI-containern: Service Locator eller DI?

`IServiceProvider` kan användas på två sätt:

```csharp
// ✅ Dependency Injection: beroendet syns i konstruktorn
public class OrderService(IOrderRepository repo) { }

// ⚠️ Service Locator: klassen frågar efter beroenden när den behöver dem
public class OrderService(IServiceProvider provider)
{
    public void Place() => provider.GetRequiredService<IOrderRepository>().Save(/*...*/);
}
```

Med *Service Locator* döljs beroendena. Du ser inte i konstruktorn vad klassen behöver, och fel upptäcks först när koden körs. Mark Seemann kallar det ett [anti-pattern](https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/). Men det finns legitima undantag, till exempel i infrastrukturkod som en `BackgroundService` som behöver skapa ett nytt scope per jobb:

```csharp
public class Worker(IServiceScopeFactory scopes) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // ok här
        // ...
    }
}
```

Tumregel: använd vanlig konstruktorinjektion i din applikationskod. Service Locator hör hemma i ramverks- och infrastrukturkod, om ens där.

### ✏️ Uppgift B

1. Kör kodexemplet från LINQ-avsnittet (eller skriv ett eget) och förklara i vilken ordning raderna skrivs ut. Vilket mönster förklarar ordningen?
2. Titta i ett eget tidigare ASP.NET Core-projekt. Hitta minst **tre** ställen där du använt ett mönster från Del B utan att tänka på det.
3. 🤔 Ge ett exempel på när ett eget repository ovanpå EF Core tillför något, och ett exempel på när det inte gör det.

---

## 🎓 Avslutande uppgift

**Välj en .NET-feature som du använder dagligen** (till exempel `ILogger<T>`, `HttpClient`, `DbContext`, `async`/`await`, Razor-komponenter, minimal API-endpoints eller `IConfiguration`).

Skriv en kort förklaring, ungefär en halv sida, som besvarar:

1. 🔍 Vilket eller vilka mönster använder featuren?
2. 🧩 *Hur* syns mönstret? Peka ut deltagarna: vem är "subject", "decorator", "builder" och så vidare?
3. 🤔 **Varför** tror du att designerna valde just det mönstret? Vilket problem hade de, och vad hade alternativet varit?
4. ⚖️ Finns det någon nackdel med valet?

> 💡 **Tips:** Källkoden till .NET är öppen! Sök i [source.dot.net](https://source.dot.net/) efter klassen du valt och se hur den faktiskt är byggd.

🔑 Jämför med [facit och exempelsvar](../answers/docs/02-patterns-in-the-wild.md) när du är klar.
