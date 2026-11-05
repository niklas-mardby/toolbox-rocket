# 🔑 Facit: Webshop scenarios

## 🔎 Scenario 1: "Aktiva produkter" → 🗄️ **Repository** (domänspecifikt)

**Varför:** Problemet är att *en domänregel* ("aktiv produkt") har blivit elva olika frågor. Ett `IProductRepository` med metoden `GetActive()` ger regeln *ett namn och ett hem*. Ändras definitionen ändras den på ett ställe.

**Alternativ:** En extension method på `IQueryable<Product>`:
```csharp
public static IQueryable<Product> Active(this IQueryable<Product> products) =>
    products.Where(p => !p.Discontinued && p.Stock > 0);
```
Det är *mycket* lättare och behåller EF Core:s flexibilitet (`db.Products.Active().Where(...).Select(...)`). Det är ett fullt lika bra, kanske bättre, svar. Det som spelar roll är att regeln samlas på ett ställe, inte exakt vilket mönster som används.

**Kostnad:** Ett repository-interface och en implementation, eller bara en statisk metod med alternativet.

## 💳 Scenario 2: Kassan → 📦 **Unit of Work**

**Varför:** Tre ändringar som måste lyckas *eller misslyckas tillsammans*. Med EF Core gör du alla tre ändringarna på samma `DbContext` och anropar `SaveChanges()` **en gång**. EF Core skickar dem i en transaktion. Felet förra veckan berodde troligen på att koden anropade `SaveChanges()` flera gånger, eller använde flera `DbContext`-instanser.

**Alternativ:** En explicit transaktion med `db.Database.BeginTransaction()`. Det behövs om flera `SaveChanges` *måste* ske, eller om annan databaskod ska med i samma transaktion.

**Kostnad:** Ingen, om du redan har EF Core. Mönstret är inbyggt. Ett eget `IUnitOfWork` behövs inte.

> ⚠️ Om kassan också anropar betaltjänsten räcker inte en databastransaktion, eftersom du inte kan rulla tillbaka ett kortköp med SQL. Då behövs andra mönster, som *outbox* eller *saga*, men det ligger utanför den här övningen.

## 🌍 Scenario 3: Betalning per land → 🔌 **Adapter** + 🏭 **Factory**

**Varför:** Två problem:
1. **Olika API:er för samma sak.** Det löser du med ett gemensamt `IPaymentProvider` och en **adapter** per SDK (`SwishAdapter`, `VippsAdapter`, `KlarnaAdapter`, `CardAdapter`).
2. **Välja rätt utifrån land och betalsätt.** Det löser du med en **factory**, `IPaymentProviderFactory.For(country, method)`, som returnerar rätt adapter.

Adaptrarna kan också ses som **Strategies**: utbytbara sätt att ta betalt. Båda namnen är rimliga, men "Adapter" beskriver bäst att det handlar om att översätta tredjeparts-API:er.

**Kostnad:** Ett interface, fyra adaptrar och en fabrik. Det är motiverat, eftersom alternativet är `if (country == "NO")` i kassan.

## 🏷️ Scenario 4: Kampanjerna → 🔀 **Strategy**

**Varför:** Varje kampanjtyp är en variant av "räkna ut rabatt för en varukorg": `IPromotion.Apply(Cart)`. Nya kampanjtyper blir nya klasser.

**Kombinationen:** Eftersom flera kan gälla behövs en regel för hur de kombineras. Det kan vara en klass som tar alla `IPromotion`s och tillämpar dem i tur och ordning, eller väljer den bästa. (Den klassen implementerar ofta *samma* interface, vilket är mönstret Composite, men det behöver du inte kunna namnge.)

**Alternativ:** Om kampanjerna ska konfigureras av marknadsavdelningen *utan utvecklare* behövs en regelmotor eller ett konfigurationsformat i databasen. Strategy löser utbyggbarheten i *koden*, men inte i *konfigurationen*.

**Kostnad:** En klass per kampanjtyp. Det är rimligt, eftersom de verkligen *är* olika algoritmer.

## 🐢 Scenario 5: Produktsidan → 🎁 **Decorator**

**Varför:** En `CachingProductInfoService : IProductInfoService` som lindar in den riktiga tjänsten och cachar i till exempel 10 minuter. Alla sju ställen som använder `IProductInfoService` får cachen automatiskt via DI, och ingen kod behöver ändras utom registreringen.

**Alternativ:** `IMemoryCache` direkt i den riktiga tjänsten. Det fungerar, men blandar caching med integration. Eller HTTP-caching (`ResponseCache`) på hela sidan, vilket kan vara ännu effektivare om hela sidan är cachebar.

**Kostnad:** En klass. Tänk på invalidering: hur får du cachen att släppa data när en produkt ändras?

## 🏛️ Scenario 6: OrderController → 📨 **Mediator / CQRS**

**Varför:** Varje action blir ett command eller en query med en egen handler (*en fil per use case*), vilket löser storleksproblemet och merge-konflikterna. Den duplicerade behörighetskontrollen, valideringen och loggningen flyttar ut till *pipeline behaviors* som gäller för alla.

**Alternativ:** Dela upp controllern i flera mindre controllers, och använd ASP.NET Core:s **filters** (action filters eller endpoint filters) för den tvärgående logiken. Det är enklare och kräver ingen mediator. För många team är det rätt första steg.

**Kostnad:** Fler filer, en mediator att förstå och indirektion (F12 tar dig inte längre till handlern).

## 🔔 Scenario 7: "Meddela mig" → 📣 **Observer** (domain events)

**Varför:** Lagerteamets kod ska kunna publicera "lagret har fyllts på för produkt X" *utan att känna till* vem som lyssnar. Mejlet, sökindexet och statistiken är observers. Det är just det Observer löser: subjectet är frikopplat från observers.

**Implementation:** C#-events, en mediator med notifications, eller, eftersom det är olika team och kanske olika tjänster, ett **meddelandesystem** som Azure Service Bus eller RabbitMQ. Samma mönster i större skala (publish/subscribe).

**Kostnad:** Flödet blir osynligare. Vad händer om mejlet misslyckas? Lagerpåfyllningen ska ju inte misslyckas för det.

## 📦 Scenario 8: Fraktsedlarna → 🔌 **Adapter**

**Varför:** Exakt samma grundproblem som scenario 3: två SDK:er med olika format för samma sak. `IShippingLabelService.CreateLabel(Shipment) → ShippingLabel` (där `ShippingLabel` till exempel alltid innehåller en PDF), med en `PostNordAdapter` och en `DhlAdapter`. DHL-adaptern hämtar PDF:en från URL:en, så att resten av koden alltid får samma sak.

**Kostnad:** Ett interface och två klasser. Billigt och motiverat.

## 🤔 Reflektionsfrågorna

### 1. Hur många nya klasser?
Ungefär 20–25 interfaces och klasser. För ett team på fyra är det **rimligt om det sker gradvis**, när varje problem faktiskt gör ont, men inte som en stor omskrivning. Ett bra svar ser att alla scenarier inte behöver lösas samtidigt.

### 2. Prioritering
Ett bra svar prioriterar efter **affärsrisk och smärta**:
- 🥇 **Scenario 2 (kassan):** fel data i databasen just nu, och billigt att fixa.
- 🥈 **Scenario 1 (aktiva produkter):** kunder ser fel produkter, och det är också billigt.
- 🥉 **Scenario 3 (betalning per land):** blockerar en affärssatsning.
- Scenario 6 är jobbigt men inte akut. Det kan tas gradvis, en action i taget.

### 3. Störst risk för överanvändning
**Scenario 6.** Att införa mediator och CQRS i hela kodbasen är en stor förändring. För en controller med problem kan filters och uppdelning räcka. **Scenario 1** är en annan kandidat: risken är att det blir ett *generiskt* `IRepository<T>` för alla entiteter, i stället för ett ställe för den specifika regeln.
