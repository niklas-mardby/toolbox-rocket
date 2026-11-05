# 🔑 Facit: Order API pipeline

> 🧩 En fullständig kodlösning finns på branchen `solutions` i `solutions/01-refactor/order-api-pipeline/`. Det finns många rimliga lösningar. Här beskrivs en av dem och varför den ser ut som den gör.

## 🗂️ Commands och queries

| Operation | Typ | Auditeras? |
|-----------|-----|------------|
| `PlaceOrder` | ✏️ Command | Ja |
| `CancelOrder` | ✏️ Command | Ja |
| `ShipOrder` | ✏️ Command | Ja |
| `GetOrder` | 🔍 Query | Nej |
| `ListOrders` | 🔍 Query | Nej |

Kravet "audit för allt som ändrar något" *är* CQRS-tanken: commands och queries har olika behov. Med ett markörinterface `ICommand<T>` kan audit-behaviorn agera bara på commands.

## 🏗️ En möjlig struktur

```
Features/
  PlaceOrder.cs     -> PlaceOrderCommand, PlaceOrderValidator, PlaceOrderHandler
  CancelOrder.cs
  ShipOrder.cs
  GetOrder.cs       -> GetOrderQuery, GetOrderHandler
  ListOrders.cs
Mediator/
  Contracts.cs      -> IRequest, ICommand, IRequestHandler, IPipelineBehavior, IValidator
  Mediator.cs
  Result.cs
Behaviors/
  LoggingBehavior.cs
  AuditBehavior.cs
  ValidationBehavior.cs
  AuthorizationBehavior.cs
Program.cs          -> registreringar och tunna endpoints
```

En handler blir kort och fokuserad:

```csharp
public record CancelOrderCommand(string CustomerId, int OrderId) : ICommand<Result>;

public class CancelOrderHandler(OrderStore store) : IRequestHandler<CancelOrderCommand, Result>
{
    public Task<Result> Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        var order = store.Find(cmd.OrderId);
        if (order is null || order.CustomerId != cmd.CustomerId) return Task.FromResult(Result.NotFound());
        if (order.Status != OrderStatus.Placed)
            return Task.FromResult(Result.Conflict($"Order {cmd.OrderId} is {order.Status} and cannot be cancelled."));

        order.Status = OrderStatus.Cancelled;
        return Task.FromResult(Result.Ok());
    }
}
```

Och en endpoint blir en rad:

```csharp
app.MapPost("/orders/{id:int}/cancel", (int id, ICurrentUser user, IMediator m) =>
    m.Send(new CancelOrderCommand(user.Id, id)).ToHttp());
```

### Ordningen på behaviors

Utifrån och in: **logging → audit → authorization → validation → handler**.

- **401 (ingen `X-Customer-Id`)** hanteras redan *före* mediatorn, i endpointen eller i ett endpoint filter. Därför auditeras det inte.
- **Audit ligger utanför authorization och validation.** Annars skulle avvisade commands (403, 400) aldrig nå audit-behaviorn, men testet kräver `"anna ShipOrder failed"`.
- **Logging ytterst** mäter tiden för hela pipelinen och fångar exceptions från alla lager.

## 🤔 Reflektionsfrågorna

### 1. Logga alla commands med indata

**Nu:** en rad i `AuditBehavior`, till exempel `audit.Write($"{user} {name} {result} {JsonSerializer.Serialize(request)}")`. Det gäller automatiskt för alla commands, även framtida.
**Förut:** ändra i tre metoder och komma ihåg det för varje ny metod.

> ⚠️ Fundera på känslig data. Kortnummer och lösenord ska inte hamna i en audit-logg. Ett behavior som loggar allt behöver ett sätt att maskera fält.

### 2. Kan behörighetskontrollen glömmas?

Det beror på designen. Om behörigheten ligger i ett behavior som kräver att varje request *deklarerar* sin behörighet, till exempel `IRequireRole { string Role { get; } }`, kan den fortfarande glömmas genom att man inte implementerar interfacet. Sätt att göra det svårare:
- ✅ **Deny by default:** behaviorn avvisar alla commands som *inte* uttryckligen anger en behörighetsregel.
- ✅ Ett **test** som med reflection hittar alla `ICommand`-typer och kontrollerar att alla har en behörighetsregel.
- ✅ ASP.NET Core:s egen `RequireAuthorization()` med policies på endpoint-nivå.

### 3. Förlorad navigerbarhet (F12)

Det är den största kostnaden med mediator-mönstret: anropet och koden som körs är inte längre direkt kopplade. Sätt att mildra det:
- lägg command, validator och handler **i samma fil** (*vertical slices*), så att du hittar allt om en operation på ett ställe,
- använd en tydlig **namnkonvention** (`XCommand` → `XHandler`),
- använd "Go to implementation" på `IRequestHandler<XCommand, ...>` i din IDE.

### 4. Endpoint filters och middleware i stället?

Ja, delvis:

| Mekanism | Ser | Bra för |
|----------|-----|---------|
| **Middleware** | Hela HTTP-requesten, före routing | Autentisering, felhantering, CORS, loggning av alla requests |
| **Endpoint filters** | Endpointens argument | Validering och behörighet per endpoint |
| **Pipeline behaviors** | Request-objektet, oberoende av HTTP | Logik som ska gälla även när handlers anropas från annat än HTTP, som bakgrundsjobb, meddelandeköer eller tester |

Om handlers *bara* anropas via HTTP räcker endpoint filters ofta, och då slipper du en egen mediator. Pipeline behaviors lönar sig när samma use cases anropas från flera håll.

### 5. Värt det för fem endpoints?

Ärligt svar: **tveksamt**. Det beror på framtiden. Ett bra svar tar hänsyn till:
- ✅ att den tvärgående logiken var **mycket duplicerad** redan med fem endpoints, och att nya krav (audit med data, behörighet) skulle multiplicera den,
- ✅ att handlers blir **lätta att testa** utan HTTP,
- ⚠️ att antalet filer och begrepp ökade kraftigt, och att en ny utvecklare måste förstå mediatorn först,
- ⚠️ att ett mellanting också fungerar: små tjänsteklasser per operation plus endpoint filters, utan en mediator.

Det viktiga är att du kan **motivera** var gränsen går. Svaret "alltid" och svaret "aldrig" är båda varningssignaler.
