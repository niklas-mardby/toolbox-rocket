# 🏛️ Order API pipeline

> 🧩 **Mönster:** Mediator och CQRS, med Decorator för pipeline behaviors
> 📏 **Omfattning:** ett minimal API med fem endpoints och en gudsklass på ungefär 250 rader. Du designar själv strukturen, och det finns flera rimliga lösningar. Räkna med att behöva fundera på avvägningar, inte bara följa steg.
> 🧪 **Tester:** 15 integrationstester via HTTP finns och är gröna. Ändra inte testerna. Allt annat får du ändra.

## 🎯 Mål

Du ska bryta upp `OrderManager` i små, fokuserade **command- och query-handlers** som anropas via en **mediator**. Den tvärgående logiken (identifiering av användaren, behörighet, validering, audit-loggning, felhantering och tidsmätning) ska skrivas **en gång** som *pipeline behaviors* i stället för att upprepas i varje metod.

## 📖 Bakgrund

Öppna `OrderManager.cs`. Den har fem metoder, och varje metod gör samma saker:

1. 🪪 läser `X-Customer-Id` från headern och returnerar 401 om det saknas
2. 📝 loggar start
3. 🔐 kontrollerar behörighet (vissa metoder)
4. ✅ validerar indata (vissa metoder)
5. 🧠 **gör det den faktiskt ska göra** (några få rader)
6. 📋 skriver till audit-loggen, men bara för operationer som *ändrar* något
7. ⏱️ loggar tidsåtgången
8. 💥 fångar exceptions

Compliance har just meddelat att alla commands också ska loggas med **vilken data** som skickades. Säkerhetsteamet vill att behörighetskontrollen ska vara omöjlig att glömma. Ingen av förändringarna är rolig att göra i den här klassen.

## 🛠️ Uppgift

Uppgiften är medvetet mer öppen än i de tidigare övningarna. Här är en möjlig väg:

1. ▶️ **Kör testerna.** Läs dem. De är din specifikation.
2. 🔍 **Skilj commands från queries.** Vilka operationer ändrar state och vilka läser bara? Hur hänger det ihop med kravet på audit?
3. 📨 **Inför request-objekt och handlers**, till exempel `PlaceOrderCommand` och `PlaceOrderHandler`. Varje handler innehåller bara affärslogiken för en operation.
4. 🧭 **Bygg en enkel mediator** som tar ett request-objekt och hittar rätt handler via DI-containern. Bygg den själv i stället för att använda MediatR, så att du förstår hur det fungerar.
5. 🧅 **Flytta ut den tvärgående logiken** till pipeline behaviors som lindas runt alla handlers: validering, audit (bara för commands!), loggning och behörighet.
6. ✂️ **Gör endpoints tunna.** Varje endpoint i `Program.cs` ska bara översätta HTTP till ett request-objekt, skicka det och översätta svaret tillbaka.
7. 🗑️ **Ta bort `OrderManager`.**
8. ✅ **Kör testerna.**
9. 🤔 **Skriv ner dina avvägningar.** Vad blev bättre? Vad blev sämre? Hur många filer har du nu, jämfört med före?

## ▶️ Kom igång

```bash
dotnet run --project exercises/01-refactor/order-api-pipeline/OrderApiPipeline
dotnet test exercises/01-refactor/order-api-pipeline/OrderApiPipeline.Tests
```

I `requests.http` finns färdiga anrop du kan köra från VS Code (med tillägget REST Client), Visual Studio eller Rider.

## 💡 Hints

<details>
<summary>💡 Hint 1: Hur kommer handlern åt användaren?</summary>

Handlers ska helst inte känna till `HttpContext`. Ett vanligt mönster är att endpointen läser ut användaren och lägger den i request-objektet, eller att en liten scoped tjänst (`ICurrentUser`) läser den från `IHttpContextAccessor`. Fundera på vilket som är lättast att testa.
</details>

<details>
<summary>💡 Hint 2: Hur ser kontrakten ut?</summary>

En variant som liknar MediatR:

```csharp
public interface IRequest<TResponse>;
public interface ICommand<TResponse> : IRequest<TResponse>;   // markerar "ändrar state"

public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}

public interface IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken ct);
}
```

Med ett eget markörinterface för commands kan ett audit-behavior lätt välja att bara agera på commands.
</details>

<details>
<summary>💡 Hint 3: Hur returnerar en handler "404" eller "409"?</summary>

Handlers ska inte returnera `IResult`, för det är HTTP. Låt dem returnera ett eget resultat, till exempel ett `Result<T>` med en felorsak (`NotFound`, `Conflict`, `Invalid`, `Forbidden`). Endpointen, eller en liten hjälpmetod, översätter det till rätt statuskod. Audit-behaviorn kan då se om resultatet lyckades eller inte och skriva `ok` eller `failed`.
</details>

<details>
<summary>💡 Hint 4: Hur hittar mediatorn rätt handler?</summary>

Registrera varje handler som `IRequestHandler<PlaceOrderCommand, Result<OrderDto>>`. Mediatorn kan sedan konstruera den generiska typen med `typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse))` och be `IServiceProvider` om den. Behaviors hämtas på samma sätt, som `IEnumerable<IPipelineBehavior<TRequest, TResponse>>`, och kedjas ihop så att den första lindar in den andra, och så vidare.

Ett alternativ utan reflection är en generisk metod: `Send<TRequest, TResponse>(TRequest request)`. Det blir mindre elegant att anropa men enklare att förstå.
</details>

<details>
<summary>💡 Hint 5: Validering och ordning</summary>

Testet `Every_command_is_audited_but_queries_are_not` visar att ett command som inte klarar valideringen *ska* auditeras som `failed`, medan ett anrop utan `X-Customer-Id` *inte* ska auditeras alls. Det säger något om i vilken ordning dina behaviors måste ligga.
</details>

### 📚 Central concepts

- **Mediator pattern** – objekt kommunicerar via en mellanhand i stället för direkt med varandra. 🔗 [Refactoring.Guru: Mediator](https://refactoring.guru/design-patterns/mediator)
- **CQRS** – skilj på operationer som ändrar och operationer som läser. 🔗 [Martin Fowler: CQRS](https://martinfowler.com/bliki/CQRS.html)
- **Command Query Separation** – principen på metodnivå som CQRS bygger på. 🔗 [Martin Fowler: CommandQuerySeparation](https://martinfowler.com/bliki/CommandQuerySeparation.html)
- **Pipeline behaviors / Decorator** – tvärgående logik som lindas runt alla handlers. 🔗 [Refactoring.Guru: Decorator](https://refactoring.guru/design-patterns/decorator)
- **Open generics i DI** – registrera och hämta generiska typer som `IRequestHandler<,>`. 🔗 [Microsoft Learn: Dependency injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- **Minimal APIs** – endpoints i ASP.NET Core utan controllers. 🔗 [Microsoft Learn: Minimal APIs overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview)
- **Integration tests** – tester som går via HTTP med `WebApplicationFactory`. 🔗 [Microsoft Learn: Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- **Large class (code smell)** – en klass som gör för mycket. 🔗 [Refactoring.Guru: Large Class](https://refactoring.guru/smells/large-class)

## 🤔 Reflektionsfrågor

1. Hur skulle du nu lägga till kravet att alla commands ska loggas med sin indata? Jämför med hur du hade gjort det i den gamla `OrderManager`.
2. Kan en utvecklare fortfarande råka glömma behörighetskontrollen för en ny operation? Hur skulle du göra det omöjligt?
3. Du kan inte längre trycka F12 på `mediator.Send(...)` och hamna i handlern. Hur mycket påverkar det dig? Vad kan du göra åt det?
4. ASP.NET Core har redan *endpoint filters* och middleware. Kunde de ha löst samma problem? Vad är skillnaden mot pipeline behaviors?
5. Är resultatet värt det för just det här API:t med fem endpoints? Var går gränsen för dig?

---

🔑 **[Facit](../../../answers/01-refactor/order-api-pipeline.md)**. Gör ett ärligt försök först!
