# 🧾 Customer audit trail

> 🧩 **Mönstret som hjälper:** Chain of Responsibility (endpoint filters), Decorator eller Mediator. Du väljer.
> 📏 **Omfattning:** ett minimal API med sex endpoints för kundregistret. Kravet gäller *tvärgående* logik: audit, behörighet och validering på alla skrivoperationer. Det finns flera bra lösningar, och avvägningen mellan dem är en del av uppgiften.
> 🧪 **Tester:** 7 befintliga integrationstester är gröna. 6 acceptanstester (8 när testfallen räknas) är avstängda med `Todo.Skip`.

## 🎯 Mål

Du ska lägga till audit-loggning, behörighetskontroll och enhetlig validering på alla skrivande endpoints, och hitta en design där **en ny endpoint inte kan glömma** något av det.

## 📖 Bakgrund

Kundtjänst använder ett internt API för att administrera kunder: skapa, byta e-post, inaktivera och återaktivera. Revisorerna har nu ställt krav. Klassen `AuditLog` finns redan, men inget skriver till den.

### 📋 Det nya kravet

> **Som** revisor **vill jag** se vem som har ändrat vad, när, och om det lyckades, **så att** vi uppfyller kraven för spårbarhet.

1. 🪪 Alla **skrivande** anrop kräver headern `X-User`. Saknas den blir svaret **401**, och ingenting ändras.
2. 📋 Varje skrivande anrop med en användare skrivs till `AuditLog` som en `AuditEntry(User, Action, Target, Outcome, At)`:
   - `Action`: `CreateCustomer`, `UpdateEmail`, `DeactivateCustomer` eller `ReactivateCustomer`
   - `Target`: request-pathen, till exempel `/customers/1/deactivate`
   - `Outcome`: `ok`, `invalid`, `not-found` eller `conflict`
   - `At`: tidpunkt från `TimeProvider`
3. 🔍 Läsningar auditeras **inte** och kräver ingen användare.
4. ✅ **All** indata valideras på samma sätt. Även byte av e-post ska nu ge 400 med `"Email is invalid."`.

## 🛠️ Uppgift

### Del 1: Lägg till funktionen 🛠️

1. ▶️ Kör testerna. Sätt `Todo.Skip` till `null`. Flera tester blir röda.
2. 🛠️ Gör dem gröna direkt i varje endpoint i `Program.cs`.
3. 💾 Gör en commit eller spara en kopia.

### Del 2: Reflektera 🤔

4. Svara på frågorna:
   - Hur många gånger skrev du kod för att läsa `X-User`? För att skriva till `AuditLog`?
   - Hittade du en **bugg** i originalkoden när du gick igenom valideringen?
   - Nästa vecka kommer endpointen `PUT /customers/{id}/name`. Vad måste den som skriver den komma ihåg?
   - Vad händer med kraven om någon glömmer dem?

### Del 3: Refaktorera 🔧

5. 🧭 **Välj en mekanism** för den tvärgående logiken. Tre alternativ att överväga:
   - ⛓️ **Endpoint filters** i ASP.NET Core (Chain of Responsibility), gärna på en route group
   - 🎁 en **Decorator** runt `CustomerService`
   - 📨 en **Mediator** med pipeline behaviors
6. 🧅 Implementera ditt val. Tänk på **ordningen**: anonyma anrop ska inte auditeras, men ogiltiga ska det.
7. ✅ Alla 15 tester ska vara gröna.
8. 📝 Skriv några meningar om varför du valde som du gjorde.

### ➕ Nästa förändring

9. Lägg till `PUT /customers/{id}/name` med validering (`"Name is required."`) och audit (`RenameCustomer`). Hur mycket kod skrev du för audit, behörighet och validering?

## ▶️ Kom igång

```bash
dotnet test exercises/06-change-request/customer-audit-trail/CustomerAuditTrail.Tests
dotnet run --project exercises/06-change-request/customer-audit-trail/CustomerAuditTrail
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Tiden i testerna</summary>

Testerna ersätter `TimeProvider` med en fast klocka via `WithWebHostBuilder`. Hämta `TimeProvider` från DI, och använd inte `DateTimeOffset.UtcNow`, så får audit-posterna rätt tid.
</details>

<details>
<summary>💡 Hint 2: Endpoint filters</summary>

Ett endpoint filter implementerar `IEndpointFilter` och får `context` och `next`. Det kan göra något före, anropa `await next(context)`, göra något efter, eller returnera ett resultat direkt utan att anropa `next`. Med `app.MapGroup("/customers").AddEndpointFilter<...>()` gäller filtret för alla endpoints i gruppen.
</details>

<details>
<summary>💡 Hint 3: Hur vet audit-filtret om det gick bra?</summary>

Resultatet från `next(context)` är ett `IResult`. De flesta inbyggda resultat implementerar `IStatusCodeHttpResult`, som har en `StatusCode`. 2xx betyder `ok`, 400 betyder `invalid` och så vidare. Då behöver audit-filtret inte veta något om kunder.
</details>

<details>
<summary>💡 Hint 4: Ordningen</summary>

Filter som läggs till först ligger ytterst. Gruppens filter ligger utanför endpointens egna. Rita kedjan och kontrollera mot testerna: vilka anrop ska *inte* nå audit-filtret?
</details>

### 📚 Central concepts

- **Chain of Responsibility** – en kedja där varje länk kan hantera eller skicka vidare. 🔗 [Refactoring.Guru: Chain of Responsibility](https://refactoring.guru/design-patterns/chain-of-responsibility)
- **Endpoint filters** – CoR i minimal APIs. 🔗 [Microsoft Learn: Filters in Minimal API apps](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters)
- **Decorator** – ett alternativ som inte är bundet till HTTP. 🔗 [Refactoring.Guru: Decorator](https://refactoring.guru/design-patterns/decorator)
- **Mediator** – ett tredje alternativ. 🔗 [Refactoring.Guru: Mediator](https://refactoring.guru/design-patterns/mediator)
- **Cross-cutting concerns** – audit, behörighet och validering. 🔗 [Wikipedia: Cross-cutting concern](https://en.wikipedia.org/wiki/Cross-cutting_concern)
- **Middleware** – nästa nivå ut, för alla requests. 🔗 [Microsoft Learn: ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- **Integration tests med WebApplicationFactory** – och hur du byter tjänster i tester. 🔗 [Microsoft Learn: Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

## 🤔 Reflektionsfrågor

1. Vilken mekanism valde du, och vad skulle få dig att välja en annan?
2. Ett nattligt jobb ska börja inaktivera kunder som inte loggat in på två år. Auditeras det i din lösning? Ska det?
3. Varför är det bättre att audit-filtret räknar ut utfallet från statuskoden än att varje endpoint själv rapporterar sitt utfall?

---

🔑 **[Facit](../../../answers/06-change-request/customer-audit-trail.md)**. Gör ett ärligt försök först!
