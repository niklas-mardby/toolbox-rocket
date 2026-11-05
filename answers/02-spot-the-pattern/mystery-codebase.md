# 🔑 Facit: Mystery codebase

## 🗂️ Mönstren

| # | Var | Mönster | Roller | Bevis |
|---|-----|---------|--------|-------|
| 1 | `IShowCatalog`, `ShowCatalog` | **Repository** | `ShowCatalog` är repositoryt. | Ett samlingslikt objekt med domänspecifika frågor (`ByCode`, `PlayingBetween`) som döljer hur datan lagras. |
| 2 | `OnDemandShowCatalog` | **Proxy** (virtual proxy) | Proxy: `OnDemandShowCatalog`. Riktigt objekt: `ShowCatalog`. | Samma interface som det riktiga objektet. Skapar det riktiga objektet först vid första användning (`(läser in föreställningar...)` skrivs ut först när den första biljetten byggs). |
| 3 | `ITariff` och dess fyra implementationer | **Strategy** | Strategy-interface: `ITariff`. Konkreta strategier: `FullTariff`, `ChildTariff` och så vidare. Context: `TicketDraft.Finish`. | Utbytbara algoritmer för prissättning bakom ett gemensamt interface. |
| 4 | `TariffDesk.Pick` | **Factory** (simple factory) | Fabriken väljer *vilken* strategi som ska användas. | Returnerar ett `ITariff` utan att anroparen vet vilken konkret klass det blir. |
| 5 | `Doorman`, `Diary` | **Decorator** | Component: `ISeatMap`. Konkret komponent: `SeatMap`. Decorators: `Doorman` (begränsning) och `Diary` (loggning). | Implementerar `ISeatMap`, tar en `ISeatMap` i konstruktorn och skickar vidare. |
| 6 | `TicketDraft` | **Builder** | Builder: `TicketDraft`. Produkt: `Ticket`. | Fluent-metoder som returnerar `this`, och `Finish()` validerar och skapar den färdiga produkten. |
| 7 | `BoxOffice.Sold` | **Observer** | Subject: `BoxOffice`. Observers: `Scoreboard.Count` och SMS-lambdan. | Ett `event` som prenumereras på med `+=` i `Program.cs`. `BoxOffice` känner inte till prenumeranterna. |
| 8 | `PagerLink` | **Adapter** | Target: `IMessenger`. Adaptee: `TelePager3000`. Adapter: `PagerLink`. | Översätter ditt enkla API (`Send(phone, text)`) till leverantörens (`msisdn`, `char[]`, flaggor). |
| 9 | `Switchboard`, `IOperator<T>` | **Mediator** (MediatR-stil) | Mediator: `Switchboard`. Requests: `RefundWanted`, `ShowMoved`. Handlers: `RefundOperator`, `RescheduleOperator`. | Anroparen skickar ett request-objekt till mellanhanden, som hittar rätt handler utifrån typen. Anroparen känner inte till handlern. |
| 10 | `Printout` | **Template Method** | Abstrakt klass: `Printout` med template-metoden `Print()`. Konkreta klasser: `TopListPrintout`, `DiaryPrintout`. | `Print()` bestämmer ramen och ordningen. Subklasserna fyller bara i `Title()` och `Lines()`. |

Dessutom är `Program.cs` en **composition root**, och klasserna får sina beroenden via konstruktorn, vilket är **Dependency Injection**, fast utan container.

## 🎭 Lögnen: `PriceDecorator`

`PriceDecorator` är **inte** en Decorator. Det är en statisk hjälpmetod som formaterar ett belopp. En Decorator måste:
1. implementera **samma interface** som det den dekorerar,
2. **linda in** ett objekt av den typen, och
3. **skicka vidare** anrop till det.

`PriceDecorator` gör inget av det. Den "dekorerar" strängen med en emoji i vardaglig mening, men inte i mönstrets mening.

> 📚 **Lärdomen:** lita på strukturen, inte på namnet. Namn ljuger ibland, och ofta oavsiktligt.

## 🧅 Stapeln: varför syns Doris i dagboken?

```
Diary  ──▶  Doorman  ──▶  SeatMap
(ytterst)   (max 4)      (själva bokningen)
```

Doris försöker boka 5 platser. Anropet går först till `Diary`, som skickar vidare till `Doorman`, som säger nej *utan* att anropa `SeatMap`. Svaret `false` går tillbaka till `Diary`, som skriver `-> NEJ` i dagboken. Eftersom `Diary` ligger **ytterst** ser den *alla* försök, även de som stoppas längre in.

**Med omvänd ordning** (`Doorman` ytterst, sedan `Diary`, sedan `SeatMap`) skulle `Doorman` stoppa anropet innan det når `Diary`. Doris försök skulle aldrig synas i dagboken. Vilket som är rätt beror på vad dagboken är till för: alla försök, eller bara de som nådde platskartan?

## 🔀 Kombinationer

- **Factory + Strategy:** `TariffDesk.Pick` (Factory) väljer en `ITariff` (Strategy).
- **Builder + Factory + Strategy:** `TicketDraft.Finish` anropar fabriken och använder strategin för att räkna fram priset.
- **Proxy + Repository:** `OnDemandShowCatalog` står i för repositoryt.
- **Observer + Adapter:** en observer av `Sold` skickar SMS via adaptern.
- **Template Method + Decorator:** `DiaryPrintout` skriver ut sidorna som `Diary`-decoratorn samlat in.
- **Mediator med tvärgående logik:** `Switchboard` mäter tiden för alla anrop, se reflektionsfråga 3.

## 🛠️ Bonusen

- **Gruppbiljett:** en ny klass (`GroupTariff`) plus en ändring i `TariffDesk.Pick`. Men `Pick` tar bara en `Visitor` och vet inte hur stort sällskapet är! Du måste ändra fabrikens signatur, eller låta `TicketDraft` välja tariff utifrån antalet platser. Det visar att Strategy gör det lätt att lägga till *nya algoritmer*, men inte nödvändigtvis att ändra *vilken information* som avgör valet.
- **`TicketLost`:** en ny record, en ny `IOperator<TicketLost>` och en rad i `Program.cs`. **Inga befintliga klasser** behöver ändras. Det är Mediator-mönstrets styrka.

## 🤔 Reflektionsfrågorna

### 1. Svårast att hitta?
Vanliga svar är **Proxy** (det syns bara på *när* saker händer) och **Template Method** (det är "bara arv"). Mönster som syns i *beteendet över tid* är svårare att se än mönster som syns i *strukturen*.

### 2. Domännamn eller mönsternamn?
Domännamn är oftast **bättre**: `Doorman` säger *vad* den gör i systemet, `LimitingSeatMapDecorator` säger *hur* den är byggd. Mönsternamn kan vara bra när mönstret *är* det viktigaste att förstå, till exempel `CachingWeatherService`, `PaymentGatewayFactory` eller `OrderBuilder`, eller i infrastrukturkod där det inte finns någon domän att namnge efter. En vanlig kompromiss är domännamn för domänklasser och mönsternamn för tekniska wrappers.

### 3. Tvärgående logik i mediatorn
Eftersom **alla** requests passerar mediatorn är den en naturlig plats för tvärgående logik som tidsmätning, loggning, validering och felhantering. I `Switchboard` är det hårdkodat. I en mer flexibel lösning är det **pipeline behaviors**, alltså decorators runt varje handler. Se [order-api-pipeline](../../exercises/01-refactor/order-api-pipeline/) och [mini-mediator](../../exercises/04-build-from-scratch/mini-mediator/).

### 4. Överanvändning?
Möjliga, och försvarbara, svar:
- `OnDemandShowCatalog`: att läsa in tre föreställningar är inte dyrt, så Proxy-mönstret löser ett problem som inte finns här.
- `Switchboard` för två sorters kundtjänstfrågor: två direkta metodanrop hade varit enklare. Mediatorn lönar sig först när antalet requests och mängden tvärgående logik växer.
- `Printout` med Template Method för två rapporter som bara skiljer sig i titel och rader: en enda metod `Print(string title, IEnumerable<string> lines)` hade räckt.

✅ Ett bra svar ser att mönstren är **korrekt implementerade** men inte alltid **motiverade** i en så här liten app. Det är skillnaden mellan att *kunna* ett mönster och att *veta när* det ska användas.
