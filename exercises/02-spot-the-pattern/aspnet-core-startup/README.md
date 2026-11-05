# 🚀 ASP.NET Core startup

> 🧩 **Mönster:** du hittar dem själv, och det finns minst åtta
> 📏 **Omfattning:** en `Program.cs` på ungefär 70 rader, uppdelad i tolv block. Varje block är litet, men några innehåller mer än ett mönster.
> 🧪 **Ingen kod att köra.** Koden är skriven för att läsas, och den kräver paket som inte finns i repot.

## 🎯 Mål

Du ska läsa en typisk `Program.cs` för ett ASP.NET Core-API och peka ut vilka design patterns varje del bygger på.

## 📖 Bakgrund

`Program.cs` är den första filen du öppnar i ett ASP.NET Core-projekt. Den är också en av de mest mönstertäta filerna du kommer att se: på några rader använder du Builder, Dependency Injection, Factory, Decorator, Chain of Responsibility och flera till, ofta utan att tänka på det.

## 📄 Koden

```csharp
// ─── A ───────────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);

// ─── B ───────────────────────────────────────────────
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton(TimeProvider.System);

// ─── C ───────────────────────────────────────────────
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

// ─── D ───────────────────────────────────────────────
builder.Services.AddDbContext<ShopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Shop")));

// ─── E ───────────────────────────────────────────────
builder.Services.AddTransient<CorrelationIdHandler>();
builder.Services.AddHttpClient<IWeatherClient, WeatherClient>(client =>
    {
        client.BaseAddress = new Uri("https://weather.example.com");
    })
    .AddHttpMessageHandler<CorrelationIdHandler>()
    .AddStandardResilienceHandler();

// ─── F ───────────────────────────────────────────────
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);

// ─── G ───────────────────────────────────────────────
builder.Services.AddHostedService<NightlyCleanupService>();

// ─── H ───────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ─── I ───────────────────────────────────────────────
var app = builder.Build();

// ─── J ───────────────────────────────────────────────
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    var started = TimeProvider.System.GetTimestamp();
    await next(context);
    var elapsed = TimeProvider.System.GetElapsedTime(started);
    context.RequestServices.GetRequiredService<ILogger<Program>>()
        .LogInformation("{Path} took {Ms} ms", context.Request.Path, elapsed.TotalMilliseconds);
});
app.UseAuthentication();
app.UseAuthorization();

// ─── K ───────────────────────────────────────────────
app.Lifetime.ApplicationStopping.Register(() =>
    Console.WriteLine("Shutting down, finishing in-flight requests..."));

// ─── L ───────────────────────────────────────────────
app.MapGet("/orders/{id:int}", async (int id, IOrderService orders) =>
    await orders.GetAsync(id) is { } order ? Results.Ok(order) : Results.NotFound());

app.Run();

// ─── Används ovan ────────────────────────────────────
public class CorrelationIdHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        request.Headers.Add("X-Correlation-Id", Guid.NewGuid().ToString());
        return base.SendAsync(request, ct);
    }
}

public class NightlyCleanupService(IServiceScopeFactory scopes) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            // ... remove old carts, then db.SaveChangesAsync()
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
```

## 🛠️ Uppgift

1. 🏷️ **Identifiera mönster per block.** Skriv för varje block (A–L) vilket eller vilka mönster det bygger på. Ett block kan ha flera mönster, och några hänger ihop över flera block.
2. 🧩 **Peka ut deltagarna.** Välj tre block. Förklara vilka objekt som spelar vilka roller i mönstret (till exempel: vem är *builder* och vad är *produkten*? Vem är *decorator* och vad lindar den in?).
3. 🔀 **Ordningen i J.** Middleware-ordningen spelar roll. Vad händer om `UseAuthorization()` hamnar före `UseAuthentication()`? Vad händer om tidsmätningen flyttas längst upp, före `UseExceptionHandler`?
4. ⚠️ **Hitta ett tveksamt mönster.** Någonstans i koden används ett mönster som ofta kallas ett anti-pattern. Var, och är det motiverat just här?
5. 🔎 **Hitta något som saknas.** Vilket av repots åtta kärnmönster syns *inte* direkt i koden, men används troligen i klasserna som registreras?

## 💡 Hints

<details>
<summary>💡 Hint 1: Börja med A och I</summary>

Block A ger dig något och block I avslutar det. Vad ger `CreateBuilder` dig, och vad händer när du anropar `Build()`? Allt mellan A och I konfigurerar samma objekt.
</details>

<details>
<summary>💡 Hint 2: Block E innehåller mycket</summary>

`AddHttpClient` registrerar en klient som skapas av en fabrik. `CorrelationIdHandler` ärver från `DelegatingHandler`. Och `AddStandardResilienceHandler` lägger till retry och circuit breaker, på samma sätt som `CorrelationIdHandler`. Det finns tre mönster här, kanske fyra.
</details>

<details>
<summary>💡 Hint 3: Block F och H</summary>

I F väljer du *hur* JSON-properties ska namnges genom att byta ut ett objekt. I H väljer du *vart* loggarna ska skickas genom att lägga till providers. Vilket mönster handlar om att byta ut en algoritm? Och vad är det för mönster när flera providers alla får samma loggmeddelande?
</details>

<details>
<summary>💡 Hint 4: Block K</summary>

`ApplicationStopping` är en `CancellationToken`. Med `Register` säger du "kör den här koden när det händer". Det är samma idé som `+=` på ett event.
</details>

<details>
<summary>💡 Hint 5: Det tveksamma mönstret</summary>

Titta efter ställen där kod *frågar* containern efter en tjänst i stället för att få den i konstruktorn. Det finns två, ett i J och ett i `NightlyCleanupService`. Är båda lika motiverade?
</details>

### 📚 Central concepts

- **Builder** – bygg ett komplext objekt steg för steg. 🔗 [Refactoring.Guru: Builder](https://refactoring.guru/design-patterns/builder)
- **Dependency injection i ASP.NET Core** – hur tjänster registreras och injiceras. 🔗 [Microsoft Learn: Dependency injection in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- **Options pattern** – typad konfiguration via DI. 🔗 [Microsoft Learn: Options pattern in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
- **IHttpClientFactory** – en fabrik för `HttpClient`. 🔗 [Microsoft Learn: Make HTTP requests using IHttpClientFactory](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests)
- **Middleware** – pipelinen av request-hanterare. 🔗 [Microsoft Learn: ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- **Chain of Responsibility** – en kedja där varje länk kan hantera eller skicka vidare. 🔗 [Refactoring.Guru: Chain of Responsibility](https://refactoring.guru/design-patterns/chain-of-responsibility)
- **Hosted services** – bakgrundsjobb i en .NET-app. 🔗 [Microsoft Learn: Worker services in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/workers)
- **Service Locator** – och varför det ofta kallas ett anti-pattern. 🔗 [Mark Seemann: Service Locator is an Anti-Pattern](https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/)
- **Resilience** – retry och circuit breaker för HTTP. 🔗 [Microsoft Learn: Build resilient HTTP apps](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)

## 🤔 Reflektionsfrågor

1. Varför tror du att Microsoft valde en Builder för att konfigurera en webbapplikation, i stället för en stor konstruktor eller en konfigurationsfil?
2. Varför är det viktigt att *alla* registreringar sker *innan* `Build()` anropas?
3. Vilket av mönstren i den här filen tror du gör störst skillnad för hur lätt appen är att testa?

---

🔑 **[Facit](../../../answers/02-spot-the-pattern/aspnet-core-startup.md)**. Gör ett ärligt försök först!
