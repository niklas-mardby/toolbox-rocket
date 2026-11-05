# 📋 Fusklapp: mönster på en sida

## 🧰 De åtta kärnmönstren

| Pattern | 😖 Problemet | 💡 Idén i en mening | 🔎 Känn igen det på | 🏠 I .NET | 🚫 Undvik när |
|---------|-------------|---------------------|---------------------|-----------|----------------|
| 💉 **Dependency Injection** | Klasser skapar sina beroenden själva, så de blir otestbara och hårt kopplade | Be om beroenden i konstruktorn, låt någon annan skapa dem | Konstruktorer med interfaces, `services.Add...` | `IServiceCollection`, `builder.Services` | Värdeobjekt, DTO:er, rena hjälpfunktioner |
| 🗄️ **Repository** | Databaskod sprids överallt | Ett samlingslikt objekt för domänobjekt | `GetById`, `Add`, domänspecifika frågor | `DbSet<T>` | Ett generiskt repo som bara speglar `DbSet<T>` |
| 📦 **Unit of Work** | Flera ändringar ska sparas tillsammans, allt eller inget | Samla ändringar och spara dem i en transaktion | `SaveChanges()`, `Commit()` | `DbContext` | Du har redan EF Core och behöver inget extra lager |
| 🔀 **Strategy** | `switch`/`if` som växer och dupliceras | Kapsla in varje variant bakom ett interface | Ett interface, flera utbytbara implementationer | `IComparer<T>`, lambdas i LINQ | Bara en variant finns och fler är inte på väg |
| 📣 **Observer** | En klass måste känna till alla som vill veta att något hänt | Publicera händelsen, låt intressenter prenumerera | `event`, `Subscribe`, `+=` | `event`, `IObservable<T>`, `INotifyPropertyChanged` | Ordning och resultat spelar roll, eller events triggar events |
| 🎁 **Decorator** | Loggning, caching och retry trasslar in sig i affärslogiken | Linda in objektet i ett annat med samma interface | Klass som implementerar `IX` och tar en `IX` i konstruktorn | `GZipStream`, `BufferedStream`, `DelegatingHandler` | Stora interfaces med många metoder |
| 🏭 **Factory** | Den som använder objektet ska inte veta hur det skapas | Samla skapandet på ett ställe | `Create...()`, `I...Factory` | `IHttpClientFactory`, `ILoggerFactory` | Objektet skapas med en enkel `new` och varierar aldrig |
| 🧱 **Builder** | Konstruktorer med många valfria parametrar | Bygg steg för steg, avsluta med `Build()` | Fluent-metoder `With...()`, `Build()` | `WebApplication.CreateBuilder()`, `StringBuilder` (löst) | Få och obligatoriska fält: använd konstruktor eller `record` |
| 📨 **Mediator / CQRS** | Gudsklasser och samma tvärgående logik i varje metod | Skicka request-objekt till en mellanhand som hittar rätt handler | `Send(command)`, `IRequestHandler<,>` | (MediatR, egna implementationer) | Små CRUD-appar: indirektion utan vinst |
| 🔌 **Adapter** | Tredjeparts-API passar inte din kod | Ditt interface, översatt till deras API | Klass som implementerar ditt interface och anropar deras SDK | `StreamReader`, `StreamWriter` | API:t är redan bra och används på ett ställe |

## 🧩 Fler mönster du möter i .NET

| Pattern | Idé | I .NET |
|---------|-----|--------|
| 🔁 **Iterator** | Gå igenom en samling utan att känna till dess struktur | `IEnumerable<T>`, `foreach`, `yield return` |
| ⛓️ **Chain of Responsibility** | En kedja av hanterare som var och en kan hantera eller skicka vidare | ASP.NET Core-middleware, `DelegatingHandler` |
| 📐 **Template Method** | Basklassen definierar flödet, subklassen fyller i ett steg | `BackgroundService.ExecuteAsync` |
| 🪞 **Proxy** | Ett ställföreträdande objekt som styr åtkomsten till det riktiga | EF Core lazy loading (virtual proxy), gRPC-klienter (remote proxy) |
| 🗺️ **Identity Map** | Samma rad i databasen ger samma objekt i minnet | EF Core change tracking |
| ⚙️ **Options** | Typad konfiguration via DI | `IOptions<T>` |
| ✅ **Result** | Returnera lyckat eller misslyckat i stället för att kasta exceptions | Egna `Result<T>`-typer |
| 🛡️ **Retry / Circuit Breaker** | Hantera tillfälliga fel i nätverksanrop | Polly, `Microsoft.Extensions.Resilience` |

## 🤔 Snabbguide: vilket mönster?

| Om du vill... | ...titta på |
|---------------|-------------|
| byta *hur* något beräknas eller görs | 🔀 Strategy |
| lägga till beteende *runt* något, utan att ändra det | 🎁 Decorator |
| låta flera reagera på att något *hänt* | 📣 Observer |
| få främmande kod att passa *ditt* interface | 🔌 Adapter |
| dölja *hur* och *vilket* objekt som skapas | 🏭 Factory |
| bygga ett objekt med många valfria delar | 🧱 Builder |
| bryta upp en gudsklass och samla tvärgående logik | 📨 Mediator / CQRS |
| göra kod testbar och löst kopplad | 💉 Dependency Injection |
| samla databasfrågor och spara ändringar tillsammans | 🗄️ Repository + 📦 Unit of Work |
| lösa ett problem som inte finns än | 🛑 Inget mönster! (YAGNI) |

## 🧠 Två principer bakom nästan allt

1. 📚 **Programmera mot interfaces, inte implementationer.**
2. 📚 **Föredra komposition framför arv.**

Och en regel för att inte överanvända mönster: 🛑 **Inför ett mönster när problemet finns, inte innan.**
