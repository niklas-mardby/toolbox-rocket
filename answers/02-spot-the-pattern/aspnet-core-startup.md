# 🔑 Facit: ASP.NET Core startup

## 🏷️ 1. Mönster per block

| Block | Mönster | Förklaring |
|-------|---------|------------|
| **A** | **Builder** (+ Factory method) | `CreateBuilder` är en statisk fabriksmetod som ger dig en `WebApplicationBuilder`. Allt mellan A och I konfigurerar buildern. |
| **B** | **Dependency Injection** | Tjänster registreras med livstid. Containern skapar och injicerar dem. `TimeProvider` är ett exempel på att även klockan injiceras. |
| **C** | **Options pattern** (+ DI) | En konfigurationssektion binds till en typad klass som injiceras via `IOptions<SmtpSettings>`. |
| **D** | **Unit of Work** + **Repository** (+ DI, Strategy) | `DbContext` är en Unit of Work och dess `DbSet<T>` är Repository-lika. `UseSqlServer` väljer databasprovider, och det är en sorts Strategy: samma EF Core-kod med olika databaser. |
| **E** | **Factory** + **Decorator** / **Chain of Responsibility** | `AddHttpClient` registrerar klienten hos `IHttpClientFactory` (Factory). `CorrelationIdHandler` och resilience-hanteraren är `DelegatingHandler`s som lindar in varandra runt det egentliga HTTP-anropet (Decorator och CoR). `WeatherClient` är sannolikt en **Adapter** mot det externa API:t. |
| **F** | **Strategy** | `JsonNamingPolicy.SnakeCaseLower` är en utbytbar strategi för hur property-namn översätts. |
| **G** | **Template Method** (+ DI) | `NightlyCleanupService` ärver från `BackgroundService` och fyller bara i `ExecuteAsync`. |
| **H** | **Composite** + **Strategy** (+ Factory) | Varje provider (Console, Debug) är en strategi för vart loggarna tar vägen. `ILoggerFactory` skapar loggers som skickar varje meddelande till *alla* providers, vilket är Composite. |
| **I** | **Builder** | `Build()` skapar den färdiga produkten, `WebApplication`. |
| **J** | **Chain of Responsibility** (+ Decorator) | Middleware-pipelinen. Varje middleware kan göra något före och efter `next()` (Decorator-likt) eller kortsluta (CoR). |
| **K** | **Observer** | `Register` prenumererar på händelsen "appen håller på att stängas". |
| **L** | **DI** (+ Strategy) | `IOrderService` injiceras direkt i endpoint-delegaten. Lambdan själv är en strategi för hur requesten hanteras. |

## 🧩 2. Exempel på deltagare

**Block A + I: Builder**
- *Builder:* `WebApplicationBuilder` med sina delar `Services`, `Configuration`, `Logging` och `Environment`.
- *Produkt:* `WebApplication`.
- *Klient:* `Program.cs`, som styr byggstegen. (Det finns ingen separat *director*. Det gör din kod.)

**Block E: Decorator**
- *Component:* `HttpMessageHandler`.
- *Konkret komponent:* den innersta `SocketsHttpHandler`, som faktiskt skickar över nätverket.
- *Decorators:* `CorrelationIdHandler` och resilience-handlern, som är `DelegatingHandler`s med en `InnerHandler`.

**Block K: Observer**
- *Subject:* `IHostApplicationLifetime.ApplicationStopping` (en `CancellationToken`).
- *Observer:* lambdan som registreras.
- *Notifiering:* när värden stängs avbryts token, och alla registrerade callbacks körs.

## 🔀 3. Ordningen i J

- **`UseAuthorization()` före `UseAuthentication()`:** Behörighetskontrollen körs innan användaren har identifierats. Användaren är då alltid anonym, och endpoints som kräver inloggning ger 401 även för inloggade användare. Kedjan körs *i den ordning den byggs*.
- **Tidsmätningen före `UseExceptionHandler`:** Det blir faktiskt *bättre*. I den ursprungliga ordningen ligger tidsmätningen *innanför* exception handlern. Om en endpoint kastar en exception flyger den rakt igenom tidsmätningen: raden efter `await next(context)` körs aldrig, eftersom det inte finns någon `try/finally`. Misslyckade requests loggas alltså inte. Flyttar du tidsmätningen ytterst fångar exception handlern felet *innanför* den och gör om det till ett vanligt 500-svar, så tidsmätningen loggar även de misslyckade requesten. (Ett alternativ är att lägga `try/finally` i middlewaren.)

> 📚 En middleware ser bara det som händer *innanför* den i kedjan. Det är samma lärdom som i [weather-service-wrappers](../../exercises/01-refactor/weather-service-wrappers/) och [stream-layers](stream-layers.md).

## ⚠️ 4. Det tveksamma mönstret: Service Locator

Det finns två ställen där tjänster hämtas med `GetRequiredService` i stället för att injiceras:

1. **I middleware-lambdan i J:** `context.RequestServices.GetRequiredService<ILogger<Program>>()`. Här hade det varit renare att hämta loggern *en gång* före `app.Use` (`var logger = app.Logger;`) eller att skriva middlewaren som en klass med konstruktorinjektion. Service Locator är **mindre motiverat** här.
2. **I `NightlyCleanupService`:** en `BackgroundService` är en singleton, men `ShopDbContext` är scoped. Du *kan inte* injicera en scoped tjänst i en singleton (det ger en *captive dependency*). Att skapa ett eget scope och hämta `DbContext` från det är det **rekommenderade sättet**. Här är Service Locator **motiverat**, eftersom det är infrastrukturkod som hanterar livstider.

✅ Ett bra svar ser att samma teknik kan vara fel på ett ställe och rätt på ett annat. Det som avgör är *varför* den används.

## 🔎 5. Vad saknas?

**Mediator / CQRS** syns inte alls. **Adapter** syns inte direkt, men `WeatherClient` är med stor sannolikhet en adapter som översätter ett externt HTTP-API till ett `IWeatherClient`-interface i domänens språk. Inuti `OrderService` finns troligen **Repository** (eller direkt användning av `ShopDbContext`) och kanske **Strategy**.

## 🤔 Reflektionsfrågorna

### 1. Varför en Builder?
En webbapplikation har *väldigt* många valfria delar: tjänster, konfigurationskällor, loggning, Kestrel-inställningar och miljö. En konstruktor skulle få dussintals parametrar. En ren konfigurationsfil kan inte uttrycka kod, som lambdas eller egna tjänster. En Builder ger både **läsbarhet** (steg för steg), **utbyggbarhet** (bibliotek kan lägga till extension methods som `AddHttpClient`) och en tydlig gräns mellan **konfiguration** och **körning**.

### 2. Varför allt före `Build()`?
`Build()` skapar `IServiceProvider` från `IServiceCollection`, och efter det är containern **låst**. Den är en färdig, oföränderlig produkt. Det gör att containern kan validera beroenden och optimera hur tjänster skapas. Försöker du registrera tjänster efter `Build()` får du ett `InvalidOperationException`.

### 3. Störst skillnad för testbarheten?
**Dependency Injection**, eftersom den gör att varje tjänst kan bytas ut. I integrationstester med `WebApplicationFactory` kan du till exempel ersätta `ShopDbContext` med SQLite in-memory och `TimeProvider` med en fejkad klocka, utan att ändra någon produktionskod. Ett bra svar kan också nämna **Options pattern** (konfiguration kan ersättas i tester) och **Factory** (`IHttpClientFactory` gör det möjligt att fejka HTTP-anrop).
