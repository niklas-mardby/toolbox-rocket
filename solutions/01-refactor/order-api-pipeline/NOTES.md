# 📝 NOTES: Order API pipeline

## 💡 Resonemanget

**Struktur:**
```
Mediator/Mediator.cs    -> kontrakt, Result<T>, mediatorn och registrering (cirka 100 rader)
Mediator/Behaviors.cs   -> Logging, Audit, Authorization och Validation (en gång, för alla)
Features.cs             -> 5 requests, 1 validator och 5 handlers: bara affärslogik
Program.cs              -> registreringar och 5 tunna endpoints
```

- **Commands och queries:** `PlaceOrder`, `CancelOrder` och `ShipOrder` är `ICommand<T>`. `GetOrder` och `ListOrders` är vanliga `IRequest<T>`.
- **Audit bara för commands, utan `if`:** `AuditBehavior` har constrainten `where TRequest : ICommand<TResponse>`, och DI-containern hoppar då över den för queries.
- **Pipelinens ordning:** Logging → Audit → Authorization → Validation → handler.
  - 401 (ingen `X-Customer-Id`) stoppas i ett endpoint filter på route-gruppen, *före* mediatorn, och auditeras därför inte.
  - 403 och 400 stoppas *innanför* audit och auditeras som `failed`, vilket testet kräver.
- **Handlers vet inget om HTTP.** De returnerar `Result<T>` med en `Status`. `ToHttp` översätter till statuskoder på *ett* ställe.
- **`IOutcome<TSelf>` med en `static abstract`-metod** låter generiska behaviors skapa ett misslyckat `TResponse` (`TResponse.Failure(...)`) utan reflection. Det är en modern C#-funktion som passar just här.
- **Behörighet deklareras på requesten** (`ShipOrder : IRequireRole`). Behaviorn behöver inte känna till enskilda commands.
- **`OrderManager.cs` är borttagen.** Den duplicerade loggningen, headerläsningen och try/catch finns nu på ett ställe vardera.
- **Kravet "logga commands med indata"** blir en rad i `AuditBehavior`, men tänk på känslig data.

## ⚖️ Kostnaden

Fler filer och begrepp (mediator, behaviors, `Result<T>`). För fem endpoints är det en kvalitetsmässig vinst men en tveksam *ekonomisk* vinst. Se facit, reflektionsfråga 5.

## 🔀 Alternativ

- **Endpoint filters i stället för en mediator:** route-gruppens filter kan göra audit och validering (se [customer-audit-trail](../../../exercises/06-change-request/customer-audit-trail/)). Det är enklare, men gäller bara HTTP.
- **Små tjänsteklasser per use case** (`PlaceOrderService`) som injiceras direkt i endpoints, plus filters. Ett bra mellanläge.
- **Exceptions för 400 och 403** (`ValidationException`, `ForbiddenException`) och en exception handler, i stället för `Result<T>`. Det är vanligt med MediatR, men gör felvägarna mindre synliga i typerna.
- **MediatR eller ett annat bibliotek** i stället för en egen mediator. Tänk på licensen och på att du ändå behöver förstå hur det fungerar.
