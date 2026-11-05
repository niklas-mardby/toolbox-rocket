# 🧩 .NET pattern match

> 🧩 **Mönster:** tolv stycken, från Iterator till Unit of Work
> 📏 **Omfattning:** en matchningsuppgift som blir djupare för varje steg. Steg 1 tar en stund. Steg 2–4 kräver att du läser på och resonerar.
> 🧪 **Ingen kod att köra**, men skriv gärna egna små kodexempel.

## 🎯 Mål

Du ska koppla ihop .NET-features du redan använder med mönstren de bygger på, och kunna förklara *hur* de implementerar mönstret.

## 📖 Bakgrund

Du har använt `foreach`, `StringBuilder`, `DbContext` och C#-events många gånger. Men vet du att de är mönster? .NET:s designers har använt klassiska design patterns överallt. När du känner igen dem förstår du ramverket bättre, och du lär dig mönstren genom exempel du redan kan.

## 🛠️ Uppgift

### Steg 1: Matcha

Para ihop varje **feature** med det **mönster** den implementerar. Alla mönster används minst en gång. Vissa features passar på mer än ett mönster. Välj då det du tycker passar bäst och notera det andra.

**Features**

| # | Feature | Mönster |
|---|---------|---------|
| 1 | `foreach` | |
| 2 | Lazy loading proxies i EF Core | |
| 3 | ASP.NET Core middleware pipeline | |
| 4 | `IHttpClientFactory` | |
| 5 | `IObservable<T>` | |
| 6 | `BackgroundService.ExecuteAsync` | |
| 7 | `IComparer<T>` | |
| 8 | Lambdas i LINQ (`Where(x => ...)`) | |
| 9 | `IEqualityComparer<T>` | |
| 10 | `BufferedStream` | |
| 11 | `ILoggerFactory` | |
| 12 | `DbContext` och `DbSet<T>` i EF Core | |
| 13 | `StreamReader` | |
| 14 | gRPC-klienter | |
| 15 | `IEnumerable<T>` | |
| 16 | `TextWriter` ovanpå en `Stream` | |
| 17 | `StringBuilder` | |
| 18 | `GZipStream` | |
| 19 | `yield return` | |
| 20 | C#-events | |
| 21 | `INotifyPropertyChanged` | |
| 22 | `DelegatingHandler` | |
| 23 | `WebApplication.CreateBuilder()` | |

**Mönster:** Iterator · Observer · Decorator · Builder · Factory · Chain of Responsibility · Template Method · Strategy · Unit of Work · Repository · Adapter · Proxy

> 💡 Kopiera tabellen till en egen fil och fyll i den.

### Steg 2: Förklara hur

Välj **tre** features och förklara *hur* de implementerar mönstret. Peka ut mönstrets delar (vem är *subject*, *decorator*, *strategy*, *product* och så vidare?) och skriv ett kort kodexempel för var och en.

### Steg 3: Två tolkningar

Några features kan läsas som mer än ett mönster:

- `DelegatingHandler`
- `StringBuilder`
- `StreamReader`
- `DbSet<T>`

Välj **en** av dem. Argumentera för *båda* tolkningarna och säg sedan vilken du tycker är mest övertygande, och varför.

### Steg 4: Hitta ett eget

Hitta **ett mönster till** i ett .NET-bibliotek eller i ett av dina egna tidigare projekt som inte finns med i listan ovan. Beskriv var det finns och hur det fungerar.

## 💡 Hints

<details>
<summary>💡 Hint 1: Börja med de lätta</summary>

Vissa features har mönstrets namn nästan i sig: `IHttpClientFactory`, `ILoggerFactory` och `WebApplication.CreateBuilder()`. Börja med dem. Sedan finns det tre som handlar om att gå igenom en samling, ett element i taget.
</details>

<details>
<summary>💡 Hint 2: Streams</summary>

Fråga dig för varje stream-relaterad klass: **har den samma typ utåt som det den lindar in?** `GZipStream` *är* en `Stream` och *tar* en `Stream`. `StreamReader` tar en `Stream`, men den *är* ingen `Stream` utan ger dig `string` och `ReadLine()`. Det är skillnaden mellan två mönster.
</details>

<details>
<summary>💡 Hint 3: Algoritmer du skickar in</summary>

`IComparer<T>`, `IEqualityComparer<T>` och lambdas i LINQ har något gemensamt: du skickar in ett *beteende* till en metod som annars är generell. `List<T>.Sort` vet hur man sorterar men inte hur två element ska jämföras.
</details>

<details>
<summary>💡 Hint 4: Basklassen bestämmer flödet</summary>

`BackgroundService` har redan `StartAsync` och `StopAsync` färdiga. Du överskuggar bara en metod, `ExecuteAsync`. Vilket mönster går ut på att basklassen äger flödet och subklassen fyller i ett steg?
</details>

<details>
<summary>💡 Hint 5: Ställföreträdare</summary>

Två features ger dig ett objekt som *ser ut* som det riktiga, men som i själva verket gör något annat bakom kulisserna. Den ena laddar data från databasen först när du rör en property. Den andra skickar dina metodanrop över nätverket till en annan server.
</details>

<details>
<summary>💡 Hint 6: Kedjor</summary>

Middleware och `DelegatingHandler` består båda av en serie hanterare som var och en kan göra något och sedan skicka vidare, eller stoppa. Vilket mönster beskriver en *kedja* av hanterare?
</details>

### 📚 Central concepts

- **Iterator** – gå igenom en samling utan att känna till hur den är uppbyggd. 🔗 [Refactoring.Guru: Iterator](https://refactoring.guru/design-patterns/iterator)
- **Decorator** – linda in ett objekt med samma interface och lägg till beteende. 🔗 [Refactoring.Guru: Decorator](https://refactoring.guru/design-patterns/decorator)
- **Adapter** – översätt ett interface till ett annat. 🔗 [Refactoring.Guru: Adapter](https://refactoring.guru/design-patterns/adapter)
- **Proxy** – ett ställföreträdande objekt som styr åtkomsten till det riktiga. 🔗 [Refactoring.Guru: Proxy](https://refactoring.guru/design-patterns/proxy)
- **Chain of Responsibility** – en kedja av hanterare som kan hantera eller skicka vidare. 🔗 [Refactoring.Guru: Chain of Responsibility](https://refactoring.guru/design-patterns/chain-of-responsibility)
- **Template Method** – basklassen definierar algoritmens skelett och subklasser fyller i steg. 🔗 [Refactoring.Guru: Template Method](https://refactoring.guru/design-patterns/template-method)
- **Repository** och **Unit of Work** – Fowlers mönster för dataåtkomst. 🔗 [Martin Fowler: Repository](https://martinfowler.com/eaaCatalog/repository.html) · [Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- **Iterators i C#** – hur `yield return` och `IEnumerable<T>` fungerar. 🔗 [Microsoft Learn: Iterators](https://learn.microsoft.com/en-us/dotnet/csharp/iterators)
- **Källkoden till .NET** – sök upp klasser och se hur de faktiskt är byggda. 🔗 [source.dot.net](https://source.dot.net/)

## 🤔 Reflektionsfrågor

1. Vilken matchning överraskade dig mest? Varför?
2. Varför tror du att .NET har så många Decorators bland sina streams? Vad hade alternativet varit?
3. Om `DbSet<T>` redan är Repository-likt, vad betyder det för frågan om du ska skriva egna repositories ovanpå EF Core?
4. Flera features (lambdas, `yield return`, events) är *språkfunktioner* snarare än klasser. Vad säger det om relationen mellan programspråk och design patterns?

---

🔑 **[Facit](../../../answers/02-spot-the-pattern/dotnet-pattern-match.md)**. Gör ett ärligt försök först!
