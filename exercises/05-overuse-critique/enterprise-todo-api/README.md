# 🏢 Enterprise todo API

> 🧩 **Överanvända mönster:** Mediator, Template Method, Repository, Unit of Work, Decorator, Builder, Factory, Mapper och Result. Typ allt.
> 📏 **Omfattning:** ett minimal API med fem endpoints för en todo-lista, utspritt över cirka 420 rader och 35 typer. Du bestämmer själv vad som ska bort och vad som får stanna, och motiverar varje beslut. Det finns flera bra lösningar, och färre hints än vanligt.
> 🧪 **Tester:** 11 integrationstester via HTTP finns och är gröna. Ändra inte testerna. Allt annat får du ändra.

## 🎯 Mål

Du ska analysera en överdesignad kodbas, avgöra vilka mönster som gör nytta och vilka som bara kostar, och förenkla så mycket du kan utan att ändra API:ts beteende.

## 📖 Bakgrund

Ett internt todo-API, använt av ett team på sex personer. Det har fem endpoints: lista, hämta, skapa, markera som klar och ta bort. Data lagras i minnet.

Den förra utvecklaren byggde det "enterprise-ready" med Clean Architecture-inspiration:

```
Program.cs ─▶ IMediator ─▶ IRequestHandler (via RequestHandlerBase: validering + loggning)
                              ├─▶ IValidatorFactory ─▶ IValidator<T>
                              ├─▶ ITodoItemBuilderFactory ─▶ ITodoItemBuilder
                              ├─▶ IMapperFactory ─▶ IMapper<TodoItem, TodoResponseDto>
                              ├─▶ IRepository<TodoItem>
                              │     └─ CachingRepositoryDecorator ─▶ LoggingRepositoryDecorator ─▶ InMemoryTodoRepository
                              └─▶ IUnitOfWork
         ◀── Result<T> ◀── ToHttpResult()
```

Nu ska en ny utvecklare lägga till en endpoint för att byta namn på en todo. Hen har frågat hur många filer som behöver ändras.

## 🛠️ Uppgift

1. ▶️ **Kör testerna** och läs koden. Rita gärna upp vad som händer vid `POST /todos`.
2. 📋 **Gör en inventering.** Lista varje mönster och abstraktion. För var och en: vilket problem *skulle* den lösa, finns problemet här, och vad kostar den?
3. 🔢 **Svara den nya utvecklaren:** hur många filer och typer måste röras för att lägga till "byt namn" i den nuvarande koden?
4. ✂️ **Förenkla.** Arbeta i steg och kör testerna ofta. Du får gärna ta bort hela filer.
5. ⚖️ **Bestäm vad som får stanna**, och motivera. Fundera särskilt på valideringen, `Result<T>` och `TimeProvider`.
6. ➕ **Lägg till "byt namn"** (`PUT /todos/{id}` med `{ "title": "..." }`, med samma validering som när en todo skapas) i din förenklade version. Hur många ställen ändrade du?
7. 📝 **Skriv en kort motivering**, ungefär en halv sida, som du skulle kunna lägga i en pull request för att övertyga teamet om förenklingen.

## ▶️ Kom igång

```bash
dotnet test exercises/05-overuse-critique/enterprise-todo-api/EnterpriseTodoApi.Tests
dotnet run --project exercises/05-overuse-critique/enterprise-todo-api/EnterpriseTodoApi
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Börja utifrån</summary>

Testerna pratar bara HTTP. Allt mellan endpointen och listan i minnet är en implementationsdetalj. Ett sätt att börja är att skriva om *en* endpoint, till exempel `GET /todos`, direkt mot `InMemoryTodoRepository`, och se att testerna fortfarande är gröna. Fortsätt sedan med nästa.
</details>

<details>
<summary>💡 Hint 2: Vad gör decorators och unit of work egentligen?</summary>

Läs `CachingRepositoryDecorator` och `InMemoryUnitOfWork` noga. Vad cachas, och var ligger det som cachas? Vad gör `CommitAsync()`? Vilken loggnivå använder `LoggingRepositoryDecorator`, och syns den i konsolen?
</details>

<details>
<summary>💡 Hint 3: Var försiktig med trådar</summary>

API:t hanterar flera requests samtidigt. Lagringen är en singleton. Vilken av de befintliga klasserna är trådsäker, och vilken är det inte? Se till att din förenklade version är trådsäker.
</details>

### 📚 Central concepts

- **Kritik mot design patterns** – när de gör skada. 🔗 [Refactoring.Guru: Criticism of patterns](https://refactoring.guru/design-patterns/criticism)
- **Speculative generality** – abstraktioner "ifall att". 🔗 [Refactoring.Guru: Speculative Generality](https://refactoring.guru/smells/speculative-generality)
- **Middle Man** – lager som bara skickar vidare. 🔗 [Refactoring.Guru: Middle Man](https://refactoring.guru/smells/middle-man)
- **Minimal APIs** – vad ramverket redan ger dig. 🔗 [Microsoft Learn: Minimal APIs overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview)
- **Endpoint filters** – validering utan mediator. 🔗 [Microsoft Learn: Filters in Minimal API apps](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters)
- **CQRS-varningen** – Fowlers råd om när det *inte* passar. 🔗 [Martin Fowler: CQRS](https://martinfowler.com/bliki/CQRS.html)
- **Beck design rules** – passerar testerna, visar avsikt, ingen duplicering, minst antal element. 🔗 [Martin Fowler: BeckDesignRules](https://martinfowler.com/bliki/BeckDesignRules.html)
- **Thread-safe collections** – om du vill använda dem i stället för `lock`. 🔗 [Microsoft Learn: Thread-safe collections](https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)

## 🤔 Reflektionsfrågor

1. Vilket mönster var svårast att bestämma sig om? Varför?
2. Hur stort skulle API:t behöva bli innan du skulle införa en mediator igen? Vad skulle vara den utlösande faktorn?
3. Kan en kodbas vara *för enkel*? Hur märker du det?
4. Vad lärde du dig i den här övningen som du inte hade lärt dig genom att *bygga* mönstren?

---

🔑 **[Facit](../../../answers/05-overuse-critique/enterprise-todo-api.md)**. Gör ett ärligt försök först!
