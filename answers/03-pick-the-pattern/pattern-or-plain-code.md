# 🔑 Facit: Pattern or plain code?

| # | Scenario | Svar |
|---|----------|------|
| 1 | Två rabatter | ❌ Nej |
| 2 | Det interna verktyget | ❌ Nej |
| 3 | Datumformatet | ❌ Nej |
| 4 | Mejltjänsten | ✅ Ja, interfacet är motiverat |
| 5 | Kundposten | ❌ Nej |
| 6 | Loggningen | ❌ Nej |
| 7 | Databasbytet | ❌ Nej |
| 8 | Fakturaexporten | ✅ Ja, nu är det dags |

---

## 💸 Scenario 1: Två rabatter → ❌ **Nej**

**Varför:** Fyra rader `if/else` som inte har ändrats på fem år är *lättare* att läsa än ett interface med två klasser och något som väljer bland dem. Open/closed principle handlar om att skydda kod som *ändras ofta*. Kod som aldrig ändras behöver inget skydd.

**Ändrar mig om:** verksamheten börjar lägga till rabattyper regelbundet, eller om rabattlogiken för varje typ växer till många rader.

## 🧰 Scenario 2: Det interna verktyget → ❌ **Nej**

**Varför:** Tre endpoints och fem användare. `DbContext` direkt i endpoints är fullt läsbart och testbart, till exempel med SQLite in-memory. Repository, UoW, CQRS och mediator skulle kanske tredubbla kodmängden utan att lösa något problem. "Rätt från början" betyder här **enkelt från början**.

**Ändrar mig om:** verktyget växer till många endpoints med gemensam affärslogik, eller om flera team börjar bygga i det.

## 📅 Scenario 3: Datumformatet → ❌ **Nej**

**Varför:** Ett ställe, ett format. `date.ToString("d MMMM yyyy", new CultureInfo("sv-SE"))` eller en liten statisk hjälpmetod räcker. Ett interface i DI för en ren funktion utan sidoeffekter tillför inget, och det finns inget att fejka i ett test.

**Ändrar mig om:** formatet ska bero på användarens språk eller en inställning. Då är `IFormatProvider` och `CultureInfo`, som redan *är* .NET:s Strategy för formatering, rätt verktyg, fortfarande utan ett eget interface.

## 📧 Scenario 4: Mejltjänsten → ✅ **Ja**

**Varför:** Det *finns* två implementationer: den riktiga och den i testerna. `SmtpEmailSender` har en **sidoeffekt mot omvärlden** (den skickar mejl). Utan interfacet kan du inte testa `OrderService` utan att skicka riktiga mejl eller sätta upp en SMTP-server. Interfacet är en **söm** för testning.

> 📚 **Tumregel:** Interfaces är motiverade för saker som *pratar med omvärlden* (databas, nätverk, filsystem, klocka, mejl), även med en enda produktionsimplementation. Rena beräkningar behöver dem sällan.

**Ändrar mig om:** testerna inte behöver isolera `OrderService`, till exempel om ni bara har integrationstester mot en testmiljö med en fejkad SMTP-server. Det är dock ovanligt.

## 📇 Scenario 5: Kundposten → ❌ **Nej**

**Varför:** Fyra *obligatoriska* fält är just vad en konstruktor är bra på: kompilatorn tvingar fram alla fyra. Med buildern kan någon glömma `WithPhone()`, och felet upptäcks först när `Build()` körs. En builder gör det alltså *sämre* här. I modern C#:
```csharp
public record Customer(string Name, string Email, string Phone, string CustomerNumber);
```

**Ändrar mig om:** objektet får många *valfria* delar, eller om byggandet kräver logik som validering av kombinationer.

> 💡 Builders är ofta bra i **tester** (som *test data builders*) för att skapa objekt med rimliga standardvärden. Men det är ett annat syfte än produktionskod.

## 📝 Scenario 6: Loggningen → ❌ **Nej**

**Varför:** Två loggrader i en metod. Skriv dem i metoden med `ILogger`. En Decorator kostar ett interface (om det inte redan finns), en klass och en ändrad DI-registrering, och den kan inte logga något som händer *inne i* metoden.

**Ändrar mig om:** samma loggning behövs runt många tjänster, eller om loggningen börjar trängas med affärslogiken.

## 🔄 Scenario 7: Databasbytet → ❌ **Nej**

**Varför:** Det är klassisk *speculative generality*. Ingen har efterfrågat MongoDB. Ett generiskt `IRepository<T>` gör inte heller bytet lätt i praktiken: en dokumentdatabas har helt andra styrkor, frågor och transaktionsmodeller, och en abstraktion som täcker både SQL och dokumentdatabaser blir den minsta gemensamma nämnaren. Du förlorar EF Core:s styrkor (`Include`, projektioner, LINQ) för att skydda dig mot något som troligen aldrig händer. Om bytet *någon gång* sker blir det ett stort projekt ändå.

**Ändrar mig om:** det finns ett konkret, finansierat beslut om att byta databas. Även då är *domänspecifika* repositories bättre än ett generiskt.

## 🧾 Scenario 8: Fakturaexporten → ✅ **Ja, Strategy**

**Varför:** Alla varningssignaler finns:
- 🔁 **Tredje gången** ett format läggs till, och Rule of Three är uppfylld.
- 📈 **Förändringstrycket är verkligt:** två nya format på en månad.
- 🐛 **Buggar från duplicering:** avrundningen är rätt i ett format men fel i två. Med en klass per format, `IInvoiceExporter`, och gemensam logik som avrundning brutet ut på *ett* ställe, försvinner den sortens bugg.
- 📏 **Stora case-block**, 40 rader vardera, är svåra att läsa i en `switch`.

**Kostnad:** Ett interface och tre klasser, plus något som väljer rätt (en dictionary eller DI med nycklar). Den kostnaden betalar sig.

## 🤔 Reflektionsfrågorna

### 1. Hur många "nej"?
Facit har sex "nej" av åtta. Många är förvånade över hur ofta svaret är nej. Det är poängen: **mönster är verktyg för specifika problem**, och de flesta kodrader har inte de problemen.

### 2. Att säga nej till en kollega
Bra strategier:
- 🤝 **Fråga efter problemet:** "Vilket problem har vi idag som det här löser?" Om det finns ett konkret svar kanske mönstret *är* rätt.
- 📅 **Skjut upp i stället för att avfärda:** "Vi gör det när vi får ett tredje format." Det är lättare att säga ja till.
- 🧪 **Gör ett experiment:** skriv båda versionerna och jämför. Ofta blir det uppenbart.
- 📏 **Hänvisa till gemensamma principer**, som YAGNI och KISS, i stället för till personliga preferenser.

### 3. För tidigt eller för sent?
**Båda kostar, men olika.**
- **För tidigt:** du betalar direkt och hela tiden med mer kod att läsa och underhålla, och abstraktionen blir ofta *fel*, eftersom du gissade hur framtiden skulle se ut. Fel abstraktioner är svåra att ta bort.
- **För sent:** du betalar med duplicering och buggar, men refaktoreringen kan göras *när du vet vad som behövs*, och med tester som skyddsnät är den sällan dramatisk.

Många erfarna utvecklare menar att det oftast är billigare att vara lite för sen än lite för tidig, förutsatt att koden har bra tester. Det är därför övningarna i `01-refactor` har tester.
