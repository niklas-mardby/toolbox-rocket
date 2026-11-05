# 🔑 Facit: .NET pattern match

## ✅ Steg 1: Matchningen

| # | Feature | Mönster | Varför |
|---|---------|---------|--------|
| 1 | `foreach` | **Iterator** | `foreach` anropar `GetEnumerator()` och sedan `MoveNext()`/`Current` tills samlingen är slut. Du behöver inte veta om det är en lista, en array eller en databasfråga. |
| 2 | Lazy loading proxies i EF Core | **Proxy** (virtual proxy) | EF Core genererar en subklass av din entitet. Den ser ut som din klass, men när du rör en navigeringsproperty laddar den datan från databasen först. |
| 3 | ASP.NET Core middleware | **Chain of Responsibility** (+ Decorator-likt) | Varje middleware får `next` och väljer att skicka vidare eller kortsluta. Kod före och efter `await next()` lindar in resten av kedjan, vilket är Decorator-likt. |
| 4 | `IHttpClientFactory` | **Factory** | `CreateClient("github")` ger dig en färdigkonfigurerad `HttpClient` och sköter livslängden på de underliggande handlers åt dig. |
| 5 | `IObservable<T>` | **Observer** | Det formella Observer-interfacet: `Subscribe(IObserver<T>)` och sedan `OnNext`, `OnError` och `OnCompleted`. |
| 6 | `BackgroundService.ExecuteAsync` | **Template Method** | `BackgroundService` äger flödet (`StartAsync`, `StopAsync`, avbrottshantering). Du fyller bara i steget `ExecuteAsync`. |
| 7 | `IComparer<T>` | **Strategy** | `Sort` vet *hur* man sorterar. Strategin bestämmer *hur två element jämförs*. |
| 8 | Lambdas i LINQ | **Strategy** | `Where(x => x.Age > 18)`: `Where` är en generell algoritm, och lambdan är den utbytbara strategin. |
| 9 | `IEqualityComparer<T>` | **Strategy** | `Dictionary` och `Distinct` använder den för att avgöra likhet. Du byter ut strategin, till exempel `StringComparer.OrdinalIgnoreCase`. |
| 10 | `BufferedStream` | **Decorator** | *Är* en `Stream`, *har* en `Stream`, och lägger till buffring. |
| 11 | `ILoggerFactory` | **Factory** | `CreateLogger("Kategori")` skapar loggers som skickar vidare till alla registrerade providers. |
| 12 | `DbContext` och `DbSet<T>` | **Unit of Work** + **Repository** | `DbContext` spårar ändringar och sparar dem tillsammans i `SaveChanges()` (Unit of Work). `DbSet<T>` är en samlingslik ingång till entiteterna (Repository-likt). |
| 13 | `StreamReader` | **Adapter** | Översätter en `Stream` av *bytes* till ett text-API (`ReadLine()`, `ReadToEnd()`). |
| 14 | gRPC-klienter | **Proxy** (remote proxy) | Den genererade klienten har samma metoder som tjänsten, men varje anrop skickas över nätverket. |
| 15 | `IEnumerable<T>` | **Iterator** | Själva interfacet för något som går att iterera. |
| 16 | `TextWriter` ovanpå en `Stream` | **Adapter** | Text in, bytes ut. Observera att det är `StreamWriter` som *är* adaptern. `TextWriter` är bara den abstrakta basklassen för text-API:t. |
| 17 | `StringBuilder` | **Builder** (löst) | Bygg en sträng steg för steg (`Append`, `AppendLine`) och få produkten med `ToString()`. |
| 18 | `GZipStream` | **Decorator** | *Är* en `Stream`, *har* en `Stream`, och lägger till komprimering. |
| 19 | `yield return` | **Iterator** | Kompilatorn genererar en hel iteratorklass åt dig, med tillstånd, `MoveNext` och `Current`. |
| 20 | C#-events | **Observer** | Observer inbyggt i språket: `+=` prenumererar, `-=` avslutar och `Invoke` notifierar. |
| 21 | `INotifyPropertyChanged` | **Observer** | UI:t prenumererar på `PropertyChanged` och uppdateras när en property ändras. |
| 22 | `DelegatingHandler` | **Chain of Responsibility** (+ Decorator) | En kedja av handlers runt `HttpClient`. Var och en kan skicka vidare till `InnerHandler` eller svara själv. |
| 23 | `WebApplication.CreateBuilder()` | **Builder** | Ett tydligt exempel: konfigurera `Services`, `Configuration` och `Logging` steg för steg och få sedan en `WebApplication` med `Build()`. |

Alla tolv mönster används: Iterator (1, 15, 19), Observer (5, 20, 21), Decorator (10, 18, och delvis 3 och 22), Builder (17, 23), Factory (4, 11), Chain of Responsibility (3, 22), Template Method (6), Strategy (7, 8, 9), Unit of Work (12), Repository (12), Adapter (13, 16) och Proxy (2, 14).

## 🔬 Nyanserna

- **`StringBuilder` är en *lös* Builder.** I GoF-versionen finns en *director* som styr byggandet och ofta flera builders som bygger olika representationer. `StringBuilder` är egentligen en muterbar buffert: du kan läsa och ändra den medan du bygger. `WebApplication.CreateBuilder()` är det tydligare exemplet, med en separat konfigurationsfas, ett `Build()` och en färdig produkt som inte är samma typ som buildern.
- **`StreamReader` och `StreamWriter` är Adapters** från bytes till text. `TextWriter` och `TextReader` är bara de abstrakta basklasserna för text-API:t. `StringWriter` ärver också från `TextWriter` men skriver till en `StringBuilder`, inte till en stream.
- **`DelegatingHandler` är också Decorator-lik.** Den *är* en `HttpMessageHandler` och *har* en `InnerHandler` av samma typ, och lägger till beteende före och efter. Skillnaden mot middleware är liten.
- **Två sorters Proxy.** gRPC-klienter är en *remote proxy*: de står i för ett objekt på en annan maskin. EF Core:s lazy loading är en *virtual proxy*: den står i för data som inte har laddats än.
- **`DbSet<T>` är Repository-likt.** Det är därför det är så omdebatterat att lägga ett eget repository ovanpå EF Core. Du lindar då in ett repository i ett annat repository.

## 🧩 Steg 2: Exempel på "förklara hur"

**`GZipStream` som Decorator**

```csharp
using Stream file = File.Create("data.gz");            // ConcreteComponent
using Stream gzip = new GZipStream(file, CompressionLevel.Optimal); // Decorator
using var writer = new StreamWriter(gzip);              // (Adapter till text)
writer.Write("Hej!");
```
Delarna: `Stream` är *component*-interfacet, `FileStream` är den konkreta komponenten och `GZipStream` är decoratorn. Den som skriver till `gzip` ser bara en `Stream` och vet inte att datan komprimeras.

**`BackgroundService` som Template Method**

```csharp
public class Cleanup : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)   // steget du fyller i
    {
        while (!ct.IsCancellationRequested)
        {
            // städa...
            await Task.Delay(TimeSpan.FromMinutes(5), ct);
        }
    }
}
```
Delarna: `BackgroundService` är *abstract class* med template-metoden (`StartAsync` anropar `ExecuteAsync`, och `StopAsync` avbryter den), och `Cleanup` är *concrete class* som implementerar det abstrakta steget.

**C#-events som Observer**

```csharp
public class Thermometer                                 // Subject
{
    public event EventHandler<double>? TemperatureChanged;
    public void Report(double t) => TemperatureChanged?.Invoke(this, t);
}

var t = new Thermometer();
t.TemperatureChanged += (_, temp) => Console.WriteLine(temp);   // Observer prenumererar
t.Report(21.5);
```

## ⚖️ Steg 3: Två tolkningar (exempelsvar)

**`DelegatingHandler`: Chain of Responsibility eller Decorator?**

> **Chain of Responsibility:** Handlers kopplas i en kedja (`InnerHandler`). Varje handler kan välja att hantera requesten själv, till exempel genom att returnera ett cachat svar eller ett fejkat svar i tester, utan att skicka vidare. Det är kärnan i CoR.
>
> **Decorator:** Varje handler *är* en `HttpMessageHandler` och *har* en `HttpMessageHandler`. Den vanligaste användningen är att lägga till något *runt* anropet, som en header, loggning eller retry, och sedan alltid skicka vidare. Det är kärnan i Decorator.
>
> **Mest övertygande:** Decorator. I praktiken skickar nästan alla handlers vidare, och strukturen (samma typ, en inner) är exakt Decorator. CoR-läsningen är också korrekt, men stämmer bäst för specialfall som kortsluter.

✅ **Ett bra svar...** pekar på den *strukturella* skillnaden: Decorator *lägger till* beteende och skickar alltid vidare, medan CoR *väljer* vem som hanterar requesten. Ett bra svar argumenterar också utifrån hur featuren *faktiskt används*.

Andra rimliga par:
- `StringBuilder`: Builder (bygger steg för steg och `ToString()` ger produkten) eller bara en muterbar buffert som inte är något mönster alls?
- `StreamReader`: Adapter (bytes till text) eller Decorator (lindar in en stream och lägger till buffring och avkodning)? Adapter är starkare eftersom gränssnittet utåt är *ett annat*.
- `DbSet<T>`: Repository (samlingslikt, med `Add`, `Remove` och `Find`) eller bara en query-ingång (`IQueryable<T>`) som inte döljer persistensen alls?

## 🔎 Steg 4: Exempel på egna fynd

- **Options pattern:** `IOptions<T>`
- **Composite:** `IConfiguration`, som består av flera konfigurationskällor i lager, och `CompositeFileProvider`
- **Command:** `ICommand` i WPF/MAUI
- **Facade:** `File.ReadAllText`, som döljer `FileStream`, `StreamReader` och kodning bakom ett enda anrop
- **Null Object:** `NullLogger<T>.Instance`, en logger som inte gör något
- **Prototype:** `record`-typer med `with`-uttryck, som skapar en kopia med vissa ändringar
- **Flyweight:** stränginternering (`string.Intern`)

## 🤔 Reflektionsfrågorna

### 1. Vad överraskade?
Öppen fråga. Vanliga svar är `BackgroundService` som Template Method och `foreach` som Iterator, eftersom de är så vardagliga att man inte tänker på dem som mönster.

### 2. Varför så många Decorators bland streams?
Därför att funktionerna **kan kombineras fritt**: buffring, komprimering, kryptering och nätverk. Med arv skulle du behöva en klass för varje kombination: `BufferedGZipFileStream`, `EncryptedGZipNetworkStream` och så vidare. Antalet klasser exploderar. Med Decorator har du en klass per funktion och kombinerar dem när programmet körs. Det är **composition over inheritance** i sin renaste form.

### 3. Vad betyder `DbSet<T>` för egna repositories?
Ett eget *generiskt* repository ovanpå EF Core lindar in ett repository i ett annat. Det tillför sällan något och gömmer användbara funktioner som `Include`, projektioner och `AsNoTracking`. Ett *domänspecifikt* repository med metoder som `GetOverdueInvoices()` kan däremot ge namn och struktur åt frågor. Se [repository-over-ef-core](../../exercises/05-overuse-critique/repository-over-ef-core/).

### 4. Språk och mönster
Många mönster är **lösningar på brister i språket**. När språket får en funktion som löser problemet direkt "försvinner" mönstret in i språket: delegates och lambdas ersätter många Strategy- och Command-klasser, `event` ersätter handskrivna Observer-listor och `yield return` ersätter handskrivna iteratorer. Mönstret finns kvar som *idé*, men koden blir mycket kortare. Peter Norvig visade redan 1996 att 16 av de 23 GoF-mönstren blir enklare eller osynliga i dynamiska språk som Lisp.
