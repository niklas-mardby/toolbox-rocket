# 📝 NOTES: Enterprise todo API

## 💡 Resonemanget

**Från cirka 420 rader i fyra filer och cirka 35 typer, till cirka 100 rader i två filer och 5 typer.** Alla 11 tester är gröna.

| Mönster i originalet | Beslut | Varför |
|----------------------|--------|--------|
| Mediator + 5 handlers | ❌ | Fem endpoints som gör en sak var. Ingen tvärgående logik behöver en pipeline. Indirektionen gjorde att `SendAsync<GetTodosQuery, Result<IReadOnlyList<TodoResponseDto>>>` behövdes för att lista todos. |
| Template Method (`RequestHandlerBase`) | ❌ | Loggning och validering "för alla", men bara *ett* command har en validator. ASP.NET Core loggar redan alla requests. |
| `IRepository<T>` + decorators (loggning, caching) | ❌ | Caching av en lista *i minnet* är meningslös. Loggningen var på `Debug`-nivå och syntes aldrig. Och cachen var inte trådsäker. |
| `IUnitOfWork` | ❌ | `CommitAsync()` gjorde ingenting. |
| Builder + builder factory | ❌ | En `TodoItem` har två fält att sätta. |
| Mapper + mapper factory | ❌ | En rad: `new TodoResponseDto(item.Id, ...)`. Med en `record` som både entitet och DTO behövs ingen mappning alls. |
| Validator + validator factory | 🔄 **Statisk metod** | Valideringen *är* affärslogik och ska finnas kvar, på ett ställe. Men den behöver ingen fabrik eller DI. |
| `Result<T>` + `ToHttpResult` | ❌ | Endpoints returnerar `IResult` direkt. Med fem endpoints är `Results.NotFound()` tydligare än en översättning. |

**Det som är kvar:**
- `TodoStore`: *en* klass med lagringen. Den är trådsäker, eftersom den är en singleton i en webbapp. Det var originalets `InMemoryTodoRepository` också, men decoratorerna runt den var det inte.
- `CreateTodoRequest.Validate`: reglerna på ett ställe, lätta att testa.
- `TimeProvider`: klockan är fortfarande injicerad.
- Minimal API-endpoints som är läsbara uppifrån och ned.

## ⚖️ När skulle (en del av) originalet vara motiverat?

- **Riktig databas:** byt `TodoStore` mot en `DbContext`. Fortfarande inget repository.
- **Många endpoints med gemensam validering och behörighet:** endpoint filters, eller *då* en mediator med pipeline behaviors.
- **Separata API-kontrakt och domänmodeller som verkligen skiljer sig:** separata DTO:er och kanske en mapper. Men som vanlig kod, inte som en fabrik.
- **Result pattern:** om domänlogiken växer och anropas från annat än HTTP.

## 🔀 Alternativ

- **Endpoint filter för validering** (`AddEndpointFilter`) i stället för anropet i endpointen. Det är snyggt om fler endpoints får validering.
- **`TypedResults`** i stället för `Results`, vilket ger bättre OpenAPI-dokumentation och typkontroll.
- **Behåll `Result<T>`** om du gillar mönstret. Det är försvarbart, men ingen nödvändighet här.
