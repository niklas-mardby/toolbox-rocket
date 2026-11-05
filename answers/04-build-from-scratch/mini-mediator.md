# 🔑 Facit: Mini mediator

> 🧩 En fullständig, testad lösning finns på branchen `solutions` i `solutions/04-build-from-scratch/mini-mediator/`.

## 🏗️ Kärnan i lösningen

```csharp
public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
{
    // Reflection EN gång: från "TResponse känd" till "TRequest och TResponse kända".
    var method = SendCoreMethod.MakeGenericMethod(request.GetType(), typeof(TResponse));
    return (Task<TResponse>)method.Invoke(this, [request, ct])!;
}

private async Task<TResponse> SendCore<TRequest, TResponse>(TRequest request, CancellationToken ct)
    where TRequest : IRequest<TResponse>
{
    var handler = services.GetService<IRequestHandler<TRequest, TResponse>>()
        ?? throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}.");

    RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle(request, ct);
    foreach (var behavior in services.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse())
    {
        var next = pipeline;
        pipeline = () => behavior.Handle(request, next, ct);
    }
    return await pipeline();
}
```

**Två fällor:**
1. **`Reverse()`:** för att den *första* registrerade behaviorn ska hamna *ytterst* måste du linda från den sista till den första.
2. **`var next = pipeline;`:** om lambdan använde `pipeline` direkt skulle den peka på *sig själv* när loopen är klar, vilket ger oändlig rekursion. Den lokala variabeln fångar pipelinen *som den var* i just det varvet.

## ➕ Bonusen: `TransactionBehavior` bara för commands

```csharp
public interface ICommand<TResponse> : IRequest<TResponse>;

public class TransactionBehavior<TRequest, TResponse>(AppDbContext db) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>     // ← constraint
{ ... }
```

När DI-containern ska skapa `IPipelineBehavior<GetTodos, ...>` från den öppna typen `TransactionBehavior<,>` kontrollerar den constrainten. `GetTodos` är inte ett `ICommand`, så den behaviorn hoppas över när `GetServices` anropas. .NET:s inbyggda container gör det automatiskt. Ett alternativ är att kontrollera `if (request is not ICommand<TResponse>) return await next();` inne i behaviorn.

## 🤔 Reflektionsfrågorna

### 1. Vad gör MediatR egentligen?
Kärnan är ungefär det du har byggt: hitta handlern via DI, kedja behaviors, publicera notifications och scanna assemblies. Kanske 100–200 rader. MediatR har förstås mer: caching av reflection för prestanda, streaming requests, olika strategier för att publicera notifications, exception handlers och så vidare. Men **det finns ingen magi**. När du vet hur det fungerar blir det lättare att felsöka, och lättare att avgöra om du behöver biblioteket.

### 2. Handlers som använder mediatorn
- ✅ **Fördel:** `CreateTodoHandler` behöver inte känna till vem som reagerar på att en todo skapas. Det är lös koppling och samma idé som Observer.
- ⚠️ **Nackdel:** flödet blir svårare att följa. Ett anrop kan trigga en kedja av notifications och nya requests.
- ⚠️ **Risk:** en handler som *skickar* (`Send`) andra requests via mediatorn kan ge cykler och dolda beroenden. Många team har regeln "handlers får publicera notifications men inte skicka requests".

### 3. Att hitta koden som körs
- **Namnkonvention:** `CreateTodo` → `CreateTodoHandler`.
- **Samma fil** för request, validator och handler (*vertical slices*), som i `Features/Todos.cs`.
- **IDE:** "Find implementations" på `IRequestHandler<CreateTodo, TodoItem>`.
- **Behaviors på ett ställe**, i `Program.cs`, med kommentarer om ordningen.

### 4. Mediator eller tjänster direkt?
- **Utan mediator:** ✅ enklare, F12 fungerar och färre begrepp. ⚠️ Validering och loggning måste läggas in i varje tjänst, eller i endpoint filters.
- **Med mediator:** ✅ en plats för tvärgående logik, små fokuserade handlers och samma pipeline oavsett varifrån requests kommer. ⚠️ Indirektion och fler begrepp.

För *det här* lilla todo-API:t är en mediator ärligt talat **överdrivet**. Se [enterprise-todo-api](../../exercises/05-overuse-critique/enterprise-todo-api/). Övningen handlar om att förstå mönstret, inte om att det alltid ska användas.
