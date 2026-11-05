# 📖 Översikt: de viktigaste mönstren

Den här texten ger dig en kort introduktion till varje mönster i repot. För varje mönster får du:

- 😖 **Problemet:** vad som gör ont om du inte har mönstret
- 💡 **Idén:** kärnan i lösningen
- 🧪 **Ett litet exempel** i C#
- 🚫 **När du *inte* ska använda det**

> 📚 Ett design pattern är en *namngiven, beprövad lösning på ett återkommande problem*. Det är inte kod du kopierar, utan en idé du anpassar. Det viktigaste med mönster är kanske att de ger dig och dina kollegor ett gemensamt språk: "Lägg en Decorator runt den" säger mycket på få ord.

---

## 💉 1. Dependency Injection (DI)

### 😖 Problemet
```csharp
public class OrderService
{
    private readonly SqlOrderRepository _repo = new SqlOrderRepository("Server=prod;...");
    private readonly SmtpEmailSender _email = new SmtpEmailSender("smtp.company.se");

    public void PlaceOrder(Order order)
    {
        if (DateTime.Now.Hour < 6) throw new InvalidOperationException("Stängt");
        _repo.Save(order);
        _email.Send(order.CustomerEmail, "Tack för din order!");
    }
}
```
`OrderService` skapar själv allt den behöver. Det betyder att du inte kan testa den utan en riktig databas och en riktig mailserver, och att du inte kan byta till en annan databas utan att ändra i klassen. Dessutom är `DateTime.Now` ett dolt beroende: hur testar du att det är stängt klockan fem på morgonen?

### 💡 Idén
En klass ska **be om** sina beroenden i stället för att **skapa** dem. Någon annan, en *composition root* eller en DI-container, bestämmer vilka konkreta implementationer som används.

```csharp
public class OrderService(IOrderRepository repo, IEmailSender email, TimeProvider time)
{
    public void PlaceOrder(Order order)
    {
        if (time.GetLocalNow().Hour < 6) throw new InvalidOperationException("Stängt");
        repo.Save(order);
        email.Send(order.CustomerEmail, "Tack för din order!");
    }
}

// I Program.cs, appens composition root:
builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<OrderService>();
```

### 🚫 När du inte ska använda det
- För **värdeobjekt och data** (`Money`, `Address`, DTO:er). Dem skapar du med `new`.
- För **rena hjälpfunktioner** utan sidoeffekter, som `Math.Round` eller en formattering. De behöver inte bytas ut.
- När du skapar interfaces "för säkerhets skull" som bara någonsin får en implementation och aldrig mockas. Då får du ceremoni utan nytta.

---

## 🗄️ 2. Repository och Unit of Work

### 😖 Problemet
Databaskod sprids ut i hela applikationen. Varje controller skriver egna SQL-frågor eller LINQ-frågor mot `DbContext`, och det finns ingen tydlig plats för "så här hämtar vi en aktiv kund". När flera ändringar ska sparas tillsammans är det oklart vem som ansvarar för att allt eller inget sparas.

### 💡 Idén
- **Repository** (Martin Fowler): ett objekt som beter sig som en *samling* av domänobjekt i minnet. Du säger `customers.GetById(42)` eller `customers.Add(c)` och bryr dig inte om SQL.
- **Unit of Work**: håller koll på alla ändringar under en affärstransaktion och sparar dem *tillsammans*, allt eller inget.

```csharp
public interface ICustomerRepository
{
    Customer? GetById(int id);
    IReadOnlyList<Customer> GetActiveInRegion(string region);
    void Add(Customer customer);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

### 🤔 Men... EF Core gör ju redan detta?
Ja, till stor del! `DbSet<T>` beter sig som ett Repository och `DbContext` *är* en Unit of Work: den spårar ändringar och `SaveChanges()` sparar allt i en transaktion. Därför är det omdebatterat att lägga ett eget generiskt repository ovanpå EF Core. Ett eget repository kan ändå vara värt det när du vill:
- samla **domänspecifika frågor** på ett ställe (`GetOverdueInvoices()`)
- **dölja** EF Core från domänlagret
- göra affärslogiken testbar utan databas

### 🚫 När du inte ska använda det
- Ett **generiskt** `IRepository<T>` med `GetAll`, `Add`, `Update`, `Delete` som bara skickar vidare till `DbSet<T>`. Det tillför inget och gömmer bra EF Core-funktioner som `Include` och projektioner.
- I små CRUD-appar där `DbContext` direkt i en endpoint är fullt läsbart.

---

## 🔀 3. Strategy

### 😖 Problemet
```csharp
public decimal CalculateShipping(Order order, string method) => method switch
{
    "standard" => order.Weight * 10m,
    "express" => 99m + order.Weight * 15m,
    "pickup" => 0m,
    _ => throw new ArgumentException(method)
};
```
Varje ny fraktmetod betyder att du öppnar och ändrar samma metod. Snart finns samma `switch` på tre ställen (pris, leveranstid, visningsnamn) och du glömmer att uppdatera ett av dem.

### 💡 Idén
Kapsla in varje variant av en algoritm i ett eget objekt bakom ett gemensamt interface. Koden som *använder* algoritmen vet inte vilken variant den har.

```csharp
public interface IShippingStrategy
{
    decimal Calculate(Order order);
}

public class ExpressShipping : IShippingStrategy
{
    public decimal Calculate(Order order) => 99m + order.Weight * 15m;
}
```

> 💡 I modern C# är en Strategy ofta bara en delegate: `Func<Order, decimal>`. `IComparer<T>` och lambdas i LINQ (`OrderBy(x => x.Name)`) är vardagliga strategier.

### 🚫 När du inte ska använda det
- När det bara finns **en** variant och inget tyder på att det kommer fler (se [YAGNI](https://martinfowler.com/bliki/Yagni.html)).
- När en `switch` på två-tre rader är fullt läsbar och bara finns på ett ställe. En `switch expression` är inte ett problem i sig.

---

## 📣 4. Observer

### 😖 Problemet
När en order läggs ska ett mejl skickas, lojalitetspoäng delas ut, statistik uppdateras och lagret meddelas. Om `OrderService` anropar alla dessa direkt måste den känna till alla, och varje ny "intressent" kräver att du ändrar `OrderService`.

### 💡 Idén
Ett objekt (subject) **publicerar** att något har hänt. Andra objekt (observers) **prenumererar** på det. Subjektet vet inte vilka eller hur många som lyssnar.

```csharp
public class OrderService
{
    public event EventHandler<OrderPlacedEventArgs>? OrderPlaced;

    public void PlaceOrder(Order order)
    {
        // ... spara ordern ...
        OrderPlaced?.Invoke(this, new OrderPlacedEventArgs(order));
    }
}

orderService.OrderPlaced += (_, e) => emailSender.SendConfirmation(e.Order);
orderService.OrderPlaced += (_, e) => loyalty.AddPoints(e.Order);
```

I .NET finns Observer inbyggt på flera sätt: C#-`event`, `IObservable<T>`/`IObserver<T>` och `INotifyPropertyChanged`. I front end är det samma idé bakom `addEventListener`, React-state och RxJS.

### 🚫 När du inte ska använda det
- När ordningen och **resultatet** spelar roll. Om lagret *måste* reservera varor innan ordern får bekräftas är det inte en notis, det är ett steg i flödet. Anropa det direkt.
- När du kedjar events som triggar nya events. Flödet blir osynligt och svårt att felsöka.
- Glöm inte att **avregistrera** (`-=`) prenumeranter som lever kortare än subjektet. Annars får du minnesläckor.

---

## 🎁 5. Decorator

### 😖 Problemet
```csharp
public async Task<Forecast> GetForecastAsync(string city)
{
    _logger.Log($"Hämtar {city}");
    if (_cache.TryGetValue(city, out var cached)) return cached;
    var sw = Stopwatch.StartNew();
    var result = await _api.FetchAsync(city);   // <- den egentliga logiken
    _cache[city] = result;
    _logger.Log($"Tog {sw.ElapsedMilliseconds} ms");
    return result;
}
```
En rad affärslogik drunknar i loggning, caching och tidsmätning. Och samma loggning och caching kopieras till alla andra tjänster.

### 💡 Idén
Linda ett objekt i ett annat objekt som har **samma interface**. Wrappern lägger till beteende före eller efter och skickar sedan vidare anropet. Du kan stapla flera wrappers.

```csharp
public class CachingWeatherService(IWeatherService inner) : IWeatherService
{
    private readonly Dictionary<string, Forecast> _cache = new();

    public async Task<Forecast> GetForecastAsync(string city)
    {
        if (_cache.TryGetValue(city, out var cached)) return cached;
        return _cache[city] = await inner.GetForecastAsync(city);
    }
}

IWeatherService service = new LoggingWeatherService(new CachingWeatherService(new WeatherService(api)));
```

I .NET: `Stream`-klasserna (`GZipStream` runt `FileStream`), `DelegatingHandler` för `HttpClient` och, i viss mån, ASP.NET Core-middleware.

### 🚫 När du inte ska använda det
- När interfacet har många metoder. Då måste varje decorator implementera alla, även de den inte bryr sig om.
- När du bara har en enda plats som behöver beteendet. Då är det ofta enklare att skriva det direkt.
- När ordningen mellan decorators blir viktig och svår att förstå. Dokumentera då ordningen tydligt på ett ställe.

---

## 🏭 6. Factory och 🧱 Builder

### 😖 Problemet
- **Factory:** Koden som behöver ett objekt ska inte behöva veta *hur* det skapas eller *vilken* konkret klass det blir. `new SmtpEmailSender(host, port, user, pass, useSsl)` spritt överallt är svårt att ändra.
- **Builder:** Objekt med många valfria delar ger konstruktorer med tio parametrar där halva är `null` ("telescoping constructors").

### 💡 Idén
- **Factory (Method):** samla skapandet på ett ställe. Den som anropar får ett färdigt objekt, ofta via ett interface.
- **Builder:** bygg ett komplext objekt steg för steg och få det färdiga objektet med `Build()`.

```csharp
// Factory: anroparen vet inte vilken klass som skapas eller hur den konfigureras
public interface IPaymentGatewayFactory
{
    IPaymentGateway CreateFor(string countryCode);
}

// Builder: steg för steg, läsbart och validerat i Build()
var pizza = new PizzaBuilder()
    .WithSize(PizzaSize.Large)
    .WithTopping("Mozzarella")
    .WithTopping("Basilika")
    .Build();
```

I .NET: `IHttpClientFactory`, `ILoggerFactory`, `WebApplication.CreateBuilder()` och `Host.CreateApplicationBuilder()`.

### 🚫 När du inte ska använda det
- En Factory för ett objekt som skapas med en rad `new` och aldrig varierar. Fabriker av fabriker är ett klassiskt varningstecken.
- En Builder för ett objekt med tre obligatoriska fält. Använd en konstruktor eller en `record` med `required`-properties och object initializers.

---

## 📨 7. Mediator och CQRS

### 😖 Problemet
En controller eller tjänst växer till en "gudsklass": `OrderManager` med 30 metoder, där varje metod upprepar samma validering, loggning, behörighetskontroll och felhantering.

### 💡 Idén
- **Mediator:** objekt kommunicerar inte direkt med varandra utan via en mellanhand. I .NET-världen betyder det oftast: skicka ett *request-objekt* (`PlaceOrderCommand`) till en mediator som hittar rätt *handler*.
- **CQRS** (Command Query Responsibility Segregation): skilj på operationer som **ändrar** (commands) och som **läser** (queries). De kan ha olika modeller, och ibland även olika databaser.
- **Pipeline behaviors:** eftersom alla requests går genom mediatorn kan du lägga validering, loggning och transaktioner på *ett* ställe, som decorators runt alla handlers.

```csharp
public record PlaceOrderCommand(int CustomerId, List<OrderLine> Lines) : IRequest<int>;

public class PlaceOrderHandler(AppDbContext db) : IRequestHandler<PlaceOrderCommand, int>
{
    public async Task<int> Handle(PlaceOrderCommand cmd, CancellationToken ct)
    {
        var order = Order.Create(cmd.CustomerId, cmd.Lines);
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        return order.Id;
    }
}

// I endpointen:
app.MapPost("/orders", (PlaceOrderCommand cmd, IMediator mediator) => mediator.Send(cmd));
```

> ⚠️ Biblioteket MediatR är populärt men har sedan 2025 en kommersiell licens för större företag. I det här repot bygger du hellre en egen enkel mediator, så att du förstår hur det fungerar.

### 🚫 När du inte ska använda det
- I små CRUD-appar. En mediator lägger till ett indirekt steg: du kan inte längre trycka F12 på ett anrop och hamna i koden som körs.
- CQRS med separata läs- och skrivdatabaser är en stor arkitekturell investering. Börja med den enkla varianten, separata klasser för commands och queries, och gå vidare bara om du har ett verkligt behov.

---

## 🔌 8. Adapter

### 😖 Problemet
Du använder ett tredjeparts-SDK för betalningar. Dess API är konstigt: belopp i ören som `int`, felkoder som magiska tal, inga `async`-metoder. Anropen sprids ut i hela din kod, och den dag ni byter leverantör måste du ändra överallt.

### 💡 Idén
Definiera **ditt eget** interface, i din domäns språk, och skriv en adapter som översätter till tredjepartens API. Din kod känner bara till ditt interface.

```csharp
public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(Money amount, CardDetails card);
}

public class LegacyPayAdapter(LegacyPayClient client) : IPaymentGateway
{
    public Task<PaymentResult> ChargeAsync(Money amount, CardDetails card)
    {
        var code = client.Charge(card.Number, (int)(amount.Amount * 100), amount.Currency, out var error);
        return Task.FromResult(code == 0 ? PaymentResult.Success() : PaymentResult.Failed(error));
    }
}
```

I .NET: `StreamReader` och `StreamWriter` adapterar en `Stream` av bytes till text.

### 🚫 När du inte ska använda det
- När tredjepartens API redan är rimligt och används på *ett* ställe. Då är adaptern bara ett extra lager.
- Låt inte adaptern bli ett ställe för affärslogik. Den ska *översätta*, inte bestämma.

---

## 🧭 Hur hänger de ihop?

```
                  Dependency Injection
         (limmet som kopplar ihop allt nedan)
                          │
     ┌────────────────────┼─────────────────────┐
     ▼                    ▼                     ▼
 Strategy, Adapter    Decorator, Mediator   Factory, Builder
 (byt ut beteende)    (lägg till beteende)  (skapa objekt)
     │                    │                     │
     └──── alla bygger på: programmera mot interfaces ────┘
```

De flesta mönster i den här listan bygger på samma två idéer:
1. 📚 **Programmera mot ett interface, inte mot en implementation.**
2. 📚 **Föredra komposition framför arv** (composition over inheritance).

När du förstår de två idéerna känns mönstren mindre som saker att memorera och mer som naturliga följder.

➡️ Gå vidare till [övningarna](../exercises/) eller läs [Patterns in the wild](02-patterns-in-the-wild.md).
