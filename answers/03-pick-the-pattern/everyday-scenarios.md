# 🔑 Facit: Everyday scenarios

## 📄 Scenario 1: Rapportexporten → 🔀 **Strategy**

**Varför:** Det finns flera *varianter av samma algoritm* ("skriv ut rapporten") och användaren väljer variant när programmet körs. Med ett `IReportExporter`-interface och en klass per format (`CsvExporter`, `ExcelExporter`, `PdfExporter`) kan nya format läggas till utan att den befintliga exportkoden ändras.

**Alternativ:** En `switch` på formatet. Det fungerar med två format men växer illa, och varje format har *mycket* egen logik som inte hör hemma i samma metod.

**Kostnad:** En klass per format, och något som väljer rätt strategi utifrån dropdownen. Det är ofta en dictionary eller en liten **Factory**.

## ⏱️ Scenario 2: Anropen till externa API:er → 🎁 **Decorator**

**Varför:** Beteendet ska läggas till *runt* befintliga anrop, före (starta timer) och efter (logga tid och resultat), utan att klasserna ändras. Om alla tolv tjänster går via `HttpClient` är en `DelegatingHandler` (en Decorator runt HTTP-anropet) det enklaste: en klass, registrerad med `AddHttpMessageHandler`, som gäller för alla.

**Alternativ:** Lägg loggningen i varje klass. Det betyder tolv kopior och femton om en månad. Eller en gemensam basklass, men då tvingas alla tjänster in i en arvshierarki.

**Kostnad:** Ett lager till att förstå, och om decoratorn implementeras per tjänsteinterface behövs en decorator per interface. Därför är decoratorn på `HttpClient`-nivå extra elegant.

## 📬 Scenario 3: Nya användare → 📣 **Observer**

**Varför:** Registreringen är *subject*, och mejl, nyhetsbrev och analys är *observers*. Registreringskoden publicerar "användare registrerad" och vet inte vem som lyssnar. Nya reaktioner läggs till utan att registreringen ändras. I .NET kan det vara ett C#-event, en lista med `IUserRegisteredHandler` som injiceras eller en mediator som publicerar en notification.

**Alternativ:** Anropa de tre tjänsterna direkt i registreringskoden. Det är tydligt och enkelt med tre stycken, men registreringen måste ändras för varje ny önskan.

**Kostnad:** Flödet syns inte längre på ett ställe. Felhantering måste genomtänkas: ska registreringen misslyckas om nyhetsbrevet inte svarar? (Troligen inte.)

## 🌦️ Scenario 4: Väderdatan → 🔌 **Adapter**

**Varför:** Ett tredjeparts-API med format som inte passar din app. Definiera ett eget interface (`IWeatherProvider`) som returnerar *dina* typer (`Forecast` med °C, `DateTimeOffset` och en enum), och skriv en adapter som översätter från SDK:t. Vid ett byte av leverantör skriver du en ny adapter, och resten av appen påverkas inte.

**Alternativ:** Konvertera där datan används. Då sprids kunskap om Kelvin och Unix-tid över hela appen.

**Kostnad:** Ett interface och en klass. Det är väldigt billigt i förhållande till vinsten här.

## ✉️ Scenario 5: E-postmeddelandet → 🧱 **Builder**

**Varför:** Ett objekt med *många valfria delar* och en konstruktor med elva parametrar, där de flesta är `null`. En builder gör anropet läsbart och kan validera helheten i `Build()`:

```csharp
var mail = new EmailBuilder()
    .To("a@b.se")
    .Subject("Hej")
    .Body("Text")
    .Build();
```

**Alternativ:** I modern C# räcker det ofta med **object initializers** och `required`-properties:
```csharp
var mail = new EmailMessage { To = ["a@b.se"], Subject = "Hej", Body = "Text" };
```
Det är ett fullt rimligt svar! En builder vinner när byggandet har *logik*, som validering av kombinationer, `AddAttachment` som läser filer, eller standardvärden som beror på andra val.

**Kostnad:** En extra klass att underhålla parallellt med `EmailMessage`.

## 🧪 Scenario 6: Den otestbara klassen → 💉 **Dependency Injection**

**Varför:** Klassen skapar sina beroenden själv: databas, mejl och *klocka*. Med konstruktorinjektion av `IInvoiceRepository`, `IEmailSender` och `TimeProvider` kan testet använda en fejkad klocka och skapa en faktura som är exakt 30 dagar gammal.

**Alternativ:** Testa mot en riktig databas och vänta 30 dagar. 😉 Mer seriöst: bryt ut regeln till en ren funktion, `IsOverdue(invoiceDate, today)`, som inte behöver några beroenden alls. Det är också ett bra svar, och fungerar fint *tillsammans* med DI.

**Kostnad:** Några interfaces och en composition root. I ASP.NET Core finns containern redan, så kostnaden är låg.

## 🤔 Reflektionsfrågorna

### 1. Svårast?
Vanligast är att scenario 2 och 3 blandas ihop. **Tumregel:** Decorator lägger till något *runt* ett anrop, och anroparen märker inget. Observer reagerar *efter* att något har hänt, och subjectet vet inte vem som lyssnar.

### 2. Kombinationer
- **Scenario 1:** Strategy + **Factory**, där fabriken väljer exportör utifrån dropdownens värde.
- **Scenario 4:** Adapter + **Decorator** för caching, eftersom väderdata inte ändras varje sekund.
- **Scenario 6:** DI + Adapter, om `SmtpClient` ska bytas mot en mejltjänst i molnet.
- **Scenario 3:** Observer + **DI**, där handlers registreras i containern och injiceras som `IEnumerable<IUserRegisteredHandler>`.

### 3. Egna projekt
Öppen fråga. Ett bra svar beskriver *problemet* du hade, hur du löste det och om du i efterhand skulle välja ett mönster eller inte.
