# 🔑 Facit: Customer audit trail

> 🧩 En fullständig lösning, med endpoint filters, finns på branchen `solutions` i `solutions/06-change-request/customer-audit-trail/`. Lösningar med Decorator eller Mediator är lika giltiga.

## 🤔 Del 2: Reflektionen efter del 1

- **Upprepning:** att läsa `X-User` och skriva till `AuditLog` blir **fyra gånger**, en per skrivande endpoint. Och för varje endpoint måste flera utfall (ok, invalid, not-found, conflict) auditeras, så det blir ofta 10–15 anrop till `audit.Write`.
- **Buggen:** `PUT /customers/{id}/email` hade **ingen validering**. En kund kunde få e-postadressen `""` eller `"hej"`. Valideringen fanns bara i create-endpointen.
- **Nästa endpoint:** utvecklaren måste komma ihåg *tre* saker: läsa användaren, validera och auditera alla utfall. Om något glöms märks det inte förrän revisorerna frågar.

## 🔧 Del 3: Exempel med endpoint filters

```csharp
var writes = app.MapGroup("/customers").AddEndpointFilter<RequireUserFilter>();   // 401

writes.MapPut("/{id:int}/email", (int id, UpdateEmailRequest request, CustomerService customers) =>
        ToHttp(customers.UpdateEmail(id, request.Email!)))
    .Audited("UpdateEmail")                 // audit efter next(), utfall från statuskoden
    .Validated<UpdateEmailRequest>();       // 400 med alla fel
```

**Kedjan:** `RequireUser → Audit → Validation → endpoint`.
- Anonyma anrop stoppas *före* audit, så de auditeras inte. ✅
- Ogiltiga anrop stoppas *innanför* audit, så de auditeras som `invalid`. ✅

**Nästa förändring** (`PUT /customers/{id}/name`): en endpoint i gruppen, `.Audited("RenameCustomer")`, `.Validated<RenameRequest>()` och en validator. Behörigheten följer automatiskt med gruppen.

## 🤔 Reflektionsfrågorna

### 1. Val av mekanism
- **Endpoint filters** passar när operationerna **bara nås via HTTP**. De är inbyggda och har tillgång till headers, path och statuskod.
- **Decorator** runt `CustomerService` passar om samma operationer anropas **från flera håll** (HTTP, jobb, kö) och *alla* ska auditeras.
- **Mediator** passar när det finns **många** operationer med gemensam tvärgående logik, och teamet redan använder mönstret.

✅ Ett bra svar motiverar med **var anropen kommer ifrån** och **hur många operationer** det rör sig om.

### 2. Det nattliga jobbet
Med endpoint filters: **nej**, jobbet anropar `CustomerService` direkt och går aldrig genom HTTP. Men revisorerna vill sannolikt veta *att* kunder inaktiverades automatiskt (`User = "system:inactivity-job"`). Det talar för att flytta audit närmare domänen: en decorator runt `CustomerService`, eller en mediator som både endpoints och jobbet använder. Det här är en verklig avvägning, inte en lärobokssituation.

### 3. Utfallet från statuskoden
- ✅ **En plats** för översättningen av utfall, så att alla endpoints auditeras konsekvent.
- ✅ **Kan inte glömmas:** en endpoint kan inte "glömma" att rapportera sitt utfall.
- ✅ **Audit-filtret är generiskt.** Det fungerar för vilken endpoint som helst, även framtida.
- ⚠️ Det bygger på att statuskoderna används konsekvent. Om någon returnerar 200 med `{ "error": ... }` blir audit fel. Det är ytterligare ett argument för konsekventa HTTP-svar.
