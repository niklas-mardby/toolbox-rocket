# 🔑 Facit: Competing patterns

> ⚖️ Här finns sällan ett enda rätt svar. Facit visar en rimlig motivering och **vad ett bra svar tar hänsyn till**.

## 🔁 Scenario 1: Retry runt HTTP-anrop → **Decorator** (med inslag av CoR)

**Val:** Alla tre handlers (retry, auth, loggning) *lägger till beteende och skickar vidare*. Auth-headern och loggningen skickar *alltid* vidare. Retry skickar vidare en eller flera gånger. Ingen av dem svarar själv i stället för servern. Det är Decorator.

**Avgörande skillnad:** Om en handler kan *svara själv utan att skicka vidare*, till exempel en cache som returnerar ett sparat svar eller en "circuit breaker" som avvisar direkt när tjänsten är nere, så beter den sig som Chain of Responsibility.

**Hade valt CoR om:** kedjan skulle innehålla en cache eller en fejk-handler för tester, där vissa länkar avgör att *de* hanterar requesten och resten av kedjan aldrig körs.

✅ Ett bra svar ser att `DelegatingHandler` *stöder båda*, och att det är *användningen* som avgör vilket mönster det är.

## 🧾 Scenario 2: Moms per land → **Båda: Factory skapar, Strategy används**

**Val:** Momsreglerna är **strategier** (`IVatRules` med `SwedishVatRules`, `GermanVatRules` och så vidare). Kassan *använder* strategin utan att veta vilket land det gäller. Något måste *välja* rätt strategi utifrån kundens land. Det är en **factory** (`IVatRulesFactory.For(countryCode)`), eller bara en `Dictionary<string, IVatRules>` registrerad i DI.

**Avgörande skillnad:** Strategy svarar på *"hur räknar vi?"*, och Factory svarar på *"vilken uträkning ska vi använda?"*. De konkurrerar inte, utan kompletterar varandra.

**Hade valt bara Strategy om:** landet var känt när appen startar (en app per land). Då kan rätt strategi registreras direkt i DI, och ingen fabrik behövs.

## 📣 Scenario 3: Efter att en order har lagts → **Mediator-notification** (i det här teamet)

**Val:** Teamet har *redan* en mediator. En `OrderPlacedNotification` med flera `INotificationHandler`s ger samma frikoppling som events, men:
- ✅ handlers registreras och injiceras via DI, så de kan ha egna beroenden (scoped `DbContext` och så vidare),
- ✅ samma pipeline behaviors (loggning, felhantering) kan gälla även för notifications,
- ✅ teamet slipper två sätt att göra samma sak.

**Avgörande skillnad:** Med C#-events måste lyssnarna ha en referens till *subjectet* för att kunna prenumerera (`orderService.OrderPlaced += ...`), och de lever så länge subjectet lever. Med mediatorn känner varken avsändare eller mottagare till varandra. De känner bara till notification-typen.

**Hade valt C#-events om:** det inte fanns någon mediator i projektet, eller om händelsen var lokal inom en klass eller komponent. Events är enkla, snabba och inbyggda.

✅ Ett bra svar väger in **konsekvens i kodbasen**. Det är ofta viktigare än vilket mönster som är "bäst" i teorin.

## 📜 Scenario 4: Den gamla loggern → **Adapter för `CorpLog`, Decorator (eller scope) för `CorrelationId`**

Scenariot innehåller två olika problem:

1. **`ILogger` → `CorpLog`:** Interfacen är olika (`Log<TState>(LogLevel, EventId, ...)` mot `Write(int, string, string)`). Det är en **Adapter**. I .NET skriver du en `ILoggerProvider` som skapar `CorpLogLogger : ILogger`, och den översätter till `CorpLog.Write`.
2. **`CorrelationId` på varje rad:** Samma interface, men med tillagt beteende. Det är en **Decorator**: en `ILogger` som lindar in en annan och lägger till `CorrelationId`.

> 💡 I .NET finns redan ett inbyggt sätt för punkt 2: `logger.BeginScope(new { CorrelationId = id })`. Då behövs ingen egen decorator. Ett bra svar kan nämna det.

**Avgörande skillnad:** Ändrar du *interfacet* är det en Adapter. Behåller du interfacet och lägger till något är det en Decorator.

## 📨 Scenario 5: Requests till ett externt API → **Builder** (kanske med en factory runt)

**Val:** Varje request har en gemensam kärna (bas-URL, auth, `Accept`) och *valfria* delar (query-parametrar, body, idempotensnyckel, timeout). Det är precis vad en **Builder** löser:
```csharp
var request = api.Request(HttpMethod.Post, "orders")
    .WithJsonBody(order)
    .WithIdempotencyKey(orderId)
    .Build();
```

**Avgörande skillnad:** En **Factory** passar när anroparen bara säger *vad* den vill ha (`CreateGetOrderRequest(id)`) och fabriken bestämmer allt. En **Builder** passar när anroparen *själv* vill välja bland många valfria delar.

**Hade valt Factory om:** de tolv endpoints alltid byggdes på exakt samma sätt, med bara en parameter som skiljer. Då är tolv små fabriksmetoder, eller en typad klient med tolv metoder, enklare. En vanlig kombination är en **typad klient** (`ExternalApiClient` via `AddHttpClient`) som internt använder en builder.

## 🛂 Scenario 6: Validering av alla requests → **Pipeline behavior (Decorator)**

**Val:** Commands skickas **inte bara via HTTP** utan också från en bakgrundstjänst som läser från en kö. Middleware och endpoint filters ser bara HTTP-requests. Ett `ValidationBehavior` i mediator-pipelinen ser *alla* commands, oavsett avsändare.

**Hur blir det 400?** Behaviorn returnerar ett felresultat eller kastar en `ValidationException`. För HTTP översätter endpointen, eller en middleware eller exception handler, resultatet till 400. För kön hanterar bakgrundstjänsten felet på sitt sätt, till exempel med en dead letter queue.

**Hade valt middleware eller endpoint filters om:** commands *bara* skickades via HTTP. Då är ASP.NET Core:s inbyggda mekanismer enklare och kräver ingen mediator.

## 🤔 Reflektionsfrågorna

### 1. Hur ofta blev det båda?
I facit blev det "båda" eller "olika mönster för olika delar" i fyra av sex scenarier (2, 4, 5 och delvis 1). I verkligheten **kombineras mönster nästan alltid**: en Factory som skapar en Strategy, en Adapter som dekoreras med caching. Att leta efter *ett* mönster per problem är en lärobokssituation.

### 2. Spelar namnet roll?
**Ja, för kommunikationen.** Två klasser kan vara identiska i struktur, men om den ena heter `CachingProductService` och den andra `ProductServiceAdapter` berättar namnen olika saker om *avsikten*. Nästa utvecklare förstår snabbare vad som är tänkt. Namnet spelar däremot ingen roll för hur koden *fungerar*, och det är inte värt att bråka om i en code review när båda tolkningarna är rimliga.

### 3. Svårast att hålla isär?
Vanligast är **Decorator och Chain of Responsibility**, eftersom de är nästan identiska i struktur och .NET:s egna klasser (middleware, `DelegatingHandler`) använder båda. Även **Adapter och Decorator** förväxlas ofta. Testet är alltid: *ändras interfacet?*
