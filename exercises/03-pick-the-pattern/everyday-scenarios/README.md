# ☕ Everyday scenarios

> 🧩 **Mönster:** ett per scenario: Strategy, Decorator, Observer, Adapter, Builder och Dependency Injection, i blandad ordning
> 📏 **Omfattning:** sex korta scenarier med ett tydligt bästa svar vardera. En bra start om du vill träna på att koppla problem till mönster.
> 🧪 **Ingen kod.** Skriv dina svar i en egen fil.

## 🎯 Mål

Du ska känna igen *problemet* bakom sex vanliga mönster, och välja rätt mönster utifrån en kort beskrivning.

## 🛠️ Uppgift

Läs varje scenario. Skriv för varje scenario **vilket mönster** du väljer, **varför**, ett **alternativ** och mönstrets **kostnad** (se [mallen](../README.md#-så-jobbar-du)).

---

### 📄 Scenario 1: Rapportexporten

I ett internt administrationsverktyg kan användaren exportera en försäljningsrapport. Idag finns bara CSV, men användarna har bett om Excel och PDF. Varje format har sin egen logik för att skriva ut rader och summor. Användaren väljer format i en dropdown.

### ⏱️ Scenario 2: Anropen till externa API:er

Systemet har tolv tjänsteklasser som anropar olika externa API:er. Driftteamet vill att *varje* anrop ska loggas med hur lång tid det tog och om det lyckades. Ingen vill öppna och ändra alla tolv klasser, och nästa månad kommer tre nya.

### 📬 Scenario 3: Nya användare

När en ny användare registrerar sig ska tre saker hända: ett välkomstmejl skickas, användaren läggs till i nyhetsbrevslistan och en händelse skickas till analysverktyget. Marknadsteamet har redan sagt att de vill lägga till fler saker framöver. Registreringskoden ska inte behöva ändras varje gång.

### 🌦️ Scenario 4: Väderdatan

Appen visar väder från ett tredjeparts-SDK. SDK:t returnerar temperatur i Kelvin som `double`, datum som Unix-tid i en `long` och väderbeskrivning som en numerisk kod. Ingen annan del av appen vill behöva veta om det. Dessutom finns det en risk att ni byter väderleverantör nästa år.

### ✉️ Scenario 5: E-postmeddelandet

Ett `EmailMessage`-objekt kan ha mottagare, kopia, dold kopia, ämne, text, HTML-version, bilagor, prioritet, svarsadress och egna headers. De flesta mejl använder bara tre eller fyra av dem. Just nu har klassen en konstruktor med elva parametrar, och anropen ser ut så här: `new EmailMessage("a@b.se", null, null, "Hej", "Text", null, null, 0, null, null, null)`.

### 🧪 Scenario 6: Den otestbara klassen

`InvoiceService` skapar själv en `SqlConnection` med en hårdkodad connection string, en `SmtpClient` och använder `DateTime.Now` för att avgöra om en faktura är förfallen. En kollega försöker skriva ett enhetstest för regeln "en faktura är förfallen 30 dagar efter fakturadatum", men lyckas inte.

---

## 💡 Hints

<details>
<summary>💡 Hint 1: Leta efter nyckelorden</summary>

Vissa ord i scenarierna pekar mot ett visst mönster:
- "*väljer format*", "*olika logik*" → olika varianter av samma sak
- "*varje anrop*", "*utan att ändra*" → något runt befintlig kod
- "*fler saker framöver*", "*ska hända när*" → reaktioner på en händelse
- "*tredjeparts-SDK*", "*byter leverantör*" → översättning
- "*många valfria*" → steg för steg
- "*skapar själv*", "*testa*" → beroenden
</details>

<details>
<summary>💡 Hint 2: Scenario 2 och 3 liknar varandra</summary>

Båda handlar om att lägga till beteende utan att ändra befintlig kod. Men i scenario 2 ska något ske *runt* varje anrop, före och efter. I scenario 3 ska något ske *efter* att något har hänt, och den som registrerar användaren behöver inte veta vad.
</details>

<details>
<summary>💡 Hint 3: Scenario 4</summary>

Fundera på vem som ska bestämma hur väderdata ser ut i din app: du eller leverantören?
</details>

### 📚 Central concepts

- **Design patterns-katalogen** – jämför mönstrens syften. 🔗 [Refactoring.Guru: Catalog](https://refactoring.guru/design-patterns/catalog)
- **Strategy** – utbytbara algoritmer. 🔗 [Refactoring.Guru: Strategy](https://refactoring.guru/design-patterns/strategy)
- **Decorator** – lägg till beteende runt ett objekt. 🔗 [Refactoring.Guru: Decorator](https://refactoring.guru/design-patterns/decorator)
- **Observer** – reagera på händelser. 🔗 [Refactoring.Guru: Observer](https://refactoring.guru/design-patterns/observer)
- **Adapter** – översätt mellan interfaces. 🔗 [Refactoring.Guru: Adapter](https://refactoring.guru/design-patterns/adapter)
- **Builder** – bygg objekt steg för steg. 🔗 [Refactoring.Guru: Builder](https://refactoring.guru/design-patterns/builder)
- **Dependency Injection** – få beroenden utifrån. 🔗 [Microsoft Learn: Dependency injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- **Long parameter list** – kodlukten bakom scenario 5. 🔗 [Refactoring.Guru: Long Parameter List](https://refactoring.guru/smells/long-parameter-list)

## 🤔 Reflektionsfrågor

1. Vilket scenario var svårast? Vad gjorde det svårt?
2. I vilket scenario skulle du kunna kombinera två mönster för en ännu bättre lösning?
3. Tänk på ett projekt du har gjort. Känner du igen något av problemen? Löste du det med ett mönster, eller på något annat sätt?

---

🔑 **[Facit](../../../answers/03-pick-the-pattern/everyday-scenarios.md)**. Gör ett ärligt försök först!
