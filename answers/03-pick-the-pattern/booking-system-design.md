# 🔑 Facit: Booking system design

> ⚖️ Det finns inget enda rätt svar på den här uppgiften. Här är en **exempeldesign**, följd av **vad en bra design tar hänsyn till**. Din design kan se helt annorlunda ut och ändå vara bra, så länge du kan motivera den.

## 🗺️ Del 1: Exempeldesign

```
            ┌─────────────┐
 Klienter ─▶│  Endpoints  │──▶ Mediator (commands/queries) ──▶ pipeline: audit, validering, loggning
            └─────────────┘                 │
                 ┌──────────────────────────┼─────────────────────────────┐
                 ▼                          ▼                             ▼
          Bokning (kärna)            Prissättning                    Betalning
          regler per hall            regler per hall                 adapter per leverantör
                 │
                 │ BookingConfirmed / BookingCancelled (domain events)
       ┌─────────┼──────────────┬───────────────┐
       ▼         ▼              ▼               ▼
    Notiser   Dörrkoder      Väntelista       Rapporter
    (kanaler) (adapter/lås)  (bakgrundsjobb)  (läsmodell)
```

### 🗓️ Bokning: inget särskilt mönster i kärnan + **Repository** + **Unit of Work**

Själva bokningslogiken ("en bana kan inte dubbelbokas") är vanlig domänkod. Dubbelbokning förhindras säkrast med ett **unikt index** i databasen, inte bara med kod. EF Core:s `DbContext` är Unit of Work. Ett **domänspecifikt** `IBookingRepository` med metoder som `IsSlotFree(courtId, start, end)` och `GetUpcomingFor(customerId)` samlar frågorna.

### 📏 Bokningsregler per hall: **Strategy** (flera regler per hall)

```csharp
public interface IBookingRule
{
    RuleResult Check(BookingRequest request, Club club, DateTimeOffset now);
}
// AdvanceBookingWindowRule(days: 14), MembersOnlyBeforeRule(hour: 17), SchoolHolidayRule(...)
```
Varje hall har en *lista* med regler, konfigurerade per hall i databasen. En bokning är tillåten om alla regler säger ja. Nya regeltyper blir nya klasser, och nya *kombinationer* blir bara konfiguration.

### 💰 Prissättning: **Strategy**, i lager

```csharp
public interface IPriceRule
{
    Money Apply(Money current, PriceContext context);   // rusningstid, helg, medlem, kampanj
}
```
Grundpriset per hall och tidsintervall är **data** (en pristabell). Justeringarna (medlemsrabatt, kampanjer) är strategier som tillämpas i ordning. Förändring B (happy hour) blir en ny `IPriceRule` plus konfiguration för en hall.

### 💳 Betalning: **Adapter** + **Factory**

```csharp
public interface IPaymentProvider
{
    Task<PaymentResult> ChargeAsync(Money amount, PaymentDetails details, CancellationToken ct);
    Task<RefundResult> RefundAsync(string paymentId, Money amount, CancellationToken ct);
}
// SwishAdapter, VippsAdapter, CardAdapter (Stripe), ...
public interface IPaymentProviderFactory
{
    IPaymentProvider For(PaymentMethod method, Country country);
}
```
Förändring A (MobilePay) blir en ny adapter plus en rad i fabrikens konfiguration.

### 🔔 Notiser: **Observer** (domain events) + **Strategy** för kanaler

Bokningen publicerar `BookingConfirmed`, `BookingCancelled` och så vidare. En notishandler reagerar och skickar via kundens valda kanaler:
```csharp
public interface INotificationChannel   // EmailChannel, SmsChannel, PushChannel
{
    NotificationChannelType Type { get; }
    Task SendAsync(Customer customer, Message message, CancellationToken ct);
}
```
Om ett SMS misslyckas ska bokningen **inte** misslyckas. Därför hanteras notiser asynkront (outbox plus en bakgrundstjänst med retry). Påminnelsen 2 timmar innan är ett **schemalagt jobb**, inte ett event.

### ❌ Avbokning: **inget mönster**, en tydlig metod

Reglerna är få och stabila: 24 timmar, 50 % och guldmedlemmar gratis. En metod, `CancellationPolicy.FeeFor(booking, customer, now)`, med bra tester räcker. Om hallarna börjar få *egna* avbokningsregler kan den bli en Strategy senare.

### ⏳ Väntelista: **Observer** + bakgrundstjänst

En handler för `BookingCancelled` skapar ett erbjudande till den första på väntelistan. En `BackgroundService` (Template Method!) kontrollerar varje minut om erbjudanden har gått ut och går i så fall vidare till nästa på listan. Tiden injiceras med `TimeProvider`, så att 15-minutersregeln kan testas.

### 🔐 Dörrlås: **Adapter** + **Factory**

```csharp
public interface IDoorLockSystem
{
    Task<DoorCode> CreateCodeAsync(Club club, DateTimeOffset validFrom, DateTimeOffset validTo, CancellationToken ct);
    Task RevokeCodeAsync(DoorCode code, CancellationToken ct);
}
```
En adapter per låsleverantör, och vilken som används bestäms per hall i konfigurationen. Förändring C blir en ny adapter plus att 8 hallar byter inställning. Ingen annan kod påverkas.

### 📊 Rapporter: **CQRS** (den enkla varianten)

Rapporter är *läsning* med andra behov än bokning: aggregeringar och många rader. Separata query-handlers som läser med `AsNoTracking()` och projektioner, eller en databasvy. **Inga** separata databaser än.

### 🧾 Revision: **Mediator** med pipeline behavior (eller EF Core-interceptor)

Om ni använder en mediator kan ett `AuditBehavior` logga alla commands med användare och tidpunkt. Ett alternativ utan mediator är en EF Core `SaveChangesInterceptor` som loggar alla ändrade entiteter. Det fångar även ändringar som inte går via commands.

## 🛑 Var exempeldesignen avstår

| Ställe | Frestande mönster | Varför avstå |
|--------|-------------------|--------------|
| Avbokningsregler | Strategy | Få, stabila regler som är lika för alla hallar. En metod räcker. |
| Grundpriser | Strategy per hall | Priserna skiljer sig i *värden*, inte i *logik*. Det är data i en tabell, inte kod. |
| Repository | Generiskt `IRepository<T>` | `DbContext` räcker för de flesta frågor. Domänspecifika repositories bara för bokningar. |
| CQRS | Separata läs- och skrivdatabaser | Volymerna är små. Separata query-handlers räcker. |
| Notiskanaler | En `Builder` för meddelanden | Meddelandena har få fält. En `record` räcker. |

## ⚖️ Avvägningar (exempel)

1. **Mediator eller inte?** ✅ Ger en plats för audit och validering, och små handlers. ⚠️ Mer indirektion och fler filer. *Val:* ja, eftersom revisionskravet gäller *alla* ändringar.
2. **Events i processen eller meddelandekö?** ✅ En kö (Service Bus) ger retry och skalbarhet. ⚠️ Mer infrastruktur. *Val:* börja med en outbox-tabell plus en bakgrundstjänst i samma app, och flytta till en kö om och när det behövs.
3. **Regler som kod eller som konfiguration?** ✅ Konfiguration gör att hallar kan ändra själva. ⚠️ Mer komplext att bygga och testa. *Val:* regel*typer* som kod (Strategy), regel*parametrar* som konfiguration i databasen.

## 🔄 Del 2: Förändringarna

| Förändring | Nya klasser | Ändrade klasser |
|------------|-------------|-----------------|
| **A: MobilePay** | `MobilePayAdapter` | Konfigurationen i fabriken (eller en DI-registrering). Kanske `Country` och `PaymentMethod`. |
| **B: Happy hour** | `HappyHourRule : IPriceRule` | Ingen kod. Konfiguration för Göteborgshallen. (`PriceContext` måste innehålla *när bokningen görs*, inte bara när den gäller. Det är bra att upptäcka i designfasen!) |
| **C: Ny låsleverantör** | `NewVendorLockAdapter` | Konfigurationen för 8 hallar. |

✅ Om din design gav "en ny klass och lite konfiguration" för alla tre har den klarat testet. Om någon förändring krävde ändringar på många ställen, fundera på vilket mönster som saknades där.

## 🚀 Del 3: Vad byggs först?

**Exempelsvar för två pilothallar på två månader:**
- ✅ **Bygg:** bokning med ett unikt index mot dubbelbokning, en enkel pristabell, *en* betalleverantör (kort) via `IPaymentProvider` (interfacet är billigt, och adapters för fler leverantörer kommer snart), bekräftelse via mejl och dörrkoder för pilothallarnas låsleverantör via `IDoorLockSystem`.
- ⏸️ **Vänta med:** väntelistan, kampanjer, Vipps och Swish, push-notiser, rapporter (en enkel SQL-fråga räcker för pilotperioden) och mediatorn. Audit kan börja som en EF Core-interceptor.
- 💡 **Principen:** inför **interfaces vid gränserna mot omvärlden** direkt (betalning, lås, notiser), eftersom de är billiga och vi *vet* att det kommer fler implementationer. Vänta med de interna mönstren tills problemen syns.

## 🤔 Reflektionsfrågorna

### 1. Svårast?
Ofta **väntelistan** (tidsgränser, samtidighet, vad händer om två avbokar samtidigt?) eller **revisionen** (hur fångar man *alla* ändringar?). De passar inte rakt in i ett enda mönster. De kräver en kombination av mönster *och* infrastruktur, som bakgrundsjobb och transaktioner.

### 2. Klarade designen förändringarna?
Ett bra svar är **ärligt**: "Förändring B krävde att `PriceContext` fick mer information, vilket jag inte hade tänkt på." Att upptäcka sådant är hela poängen med att testa designen mot förändringsscenarier.

### 3. Jämför med en kurskamrat
Öppen fråga. Ofta har en person en mer flexibel men mer komplex design och en annan en enklare design. Diskutera: **vilka förändringar är verkligen troliga?** Flexibilitet för förändringar som aldrig kommer är bara komplexitet.

### 4. Med bara en hall?
Det mesta av variationen försvinner: regler per hall, priser per hall och flera låsleverantörer. Strategy för bokningsregler och adapters för lås blir onödiga. Betalnings-adaptern finns troligen kvar (för testbarhet och för att kunna byta), och domain events för notiser är fortfarande bra. Ett bra svar ser att **antalet mönster ska följa mängden variation**.
