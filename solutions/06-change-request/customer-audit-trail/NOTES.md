# 📝 NOTES: Customer audit trail

## 💡 Resonemanget

Lösningen använder **ASP.NET Core endpoint filters**, alltså **Chain of Responsibility** inbyggt i ramverket. Varje filter gör en sak och anropar `next` eller kortsluter.

```
RequireUserFilter ─▶ AuditFilter ─▶ ValidationFilter<T> ─▶ endpoint
 (401, kortsluter)    (skriver efter)   (400, kortsluter)
```

- **Ordningen följer kraven:**
  - Anonyma anrop ska **inte** auditeras, så `RequireUser` ligger *utanför* `Audit`.
  - Ogiltiga anrop **ska** auditeras som `invalid`, så `Validation` ligger *innanför* `Audit`.
- **En route group** (`app.MapGroup("/customers").AddEndpointFilter<RequireUserFilter>()`) samlar alla skrivande endpoints. En ny skrivande endpoint i gruppen *kan inte* glömma behörighetskontrollen.
- **Utfallet räknas ut från HTTP-resultatets statuskod** (`IStatusCodeHttpResult`). Audit-filtret behöver alltså inte veta något om kunder. Det fungerar för alla endpoints.
- **`Target` är request-pathen.** Den är enkel, unik och kräver inget per endpoint.
- **Valideringen** ligger i `IValidator<T>`-klasser, och e-postregeln i `EmailRule`. Det **avslöjade en bugg**: `PUT /customers/{id}/email` hade ingen validering alls i originalet. Kunder kunde få e-postadressen `""`. Med en gemensam valideringsmekanism syns det direkt vilka requests som saknar validator.
- **`CustomerService` är orörd.** Allt nytt ligger runt den.

## 🔀 Alternativ

| Alternativ | ✅ Fördelar | ⚠️ Nackdelar |
|------------|-------------|---------------|
| **Endpoint filters** (vald) | Inbyggt i ASP.NET Core, och har tillgång till HTTP (header, path, statuskod) | Gäller bara HTTP. Ett bakgrundsjobb som ändrar kunder auditeras inte. |
| **Decorator runt `CustomerService`** (`AuditingCustomerService`) | Gäller *alla* anropare, inte bara HTTP | Varje metod måste dekoreras för sig, och användaren måste föras in på något sätt (till exempel med `ICurrentUser`) |
| **Mediator med pipeline behaviors** | En plats för audit och validering för *alla* commands, oavsett ingång | En mediator och command-klasser måste införas. Det är stort för fyra operationer. |
| **EF Core `SaveChangesInterceptor`** (om databas fanns) | Fångar *alla* ändringar i databasen | Vet inte vilket *affärskommando* som orsakade ändringen eller om det misslyckades före sparning |

För fyra skrivoperationer som bara nås via HTTP är endpoint filters det lättaste som fungerar. Om samma operationer börjar anropas från en kö eller ett schemalagt jobb blir decorator eller mediator mer motiverat.
