# 🥊 Competing patterns

> 🧩 **Mönster:** sex par där två mönster passar: Decorator mot Chain of Responsibility, Strategy mot Factory, Observer mot Mediator, Adapter mot Decorator, Builder mot Factory, och middleware mot pipeline behaviors
> 📏 **Omfattning:** sex scenarier som kräver att du förstår *skillnaden* mellan mönster som liknar varandra. Det finns sällan ett enda rätt svar, men det finns bättre och sämre motiveringar.
> 🧪 **Ingen kod**, men skissa gärna interfaces för att tänka klart.

## 🎯 Mål

Du ska kunna skilja på mönster som ser likadana ut i koden, men som har olika *syften*, och välja det som bäst uttrycker vad du vill åstadkomma.

## 📖 Bakgrund

Många mönster har nästan samma struktur. En Decorator och en Chain of Responsibility är båda "ett objekt som har ett annat objekt av samma typ". En Strategy och en Factory handlar båda om att välja en implementation. När strukturen är likadan avgörs valet av **avsikten**: vad vill du att koden ska *säga* till nästa utvecklare?

## 🛠️ Uppgift

Varje scenario anger två kandidater. Svara för varje scenario:

1. 🥊 **Vilket mönster väljer du**, eller båda, och i så fall hur samverkar de?
2. 🔍 **Vad är den avgörande skillnaden** mellan de två mönstren *i det här scenariot*?
3. 🔀 **När hade du valt det andra?** Beskriv en liten ändring i scenariot som skulle ändra ditt svar.

---

### 🔁 Scenario 1: Retry runt HTTP-anrop
**Decorator eller Chain of Responsibility?**

Appens `HttpClient` ska försöka igen upp till tre gånger vid tillfälliga fel (HTTP 503). Ett anrop ska också få en `Authorization`-header och loggas. Allt implementeras som `DelegatingHandler`s.

### 🧾 Scenario 2: Moms per land
**Strategy eller Factory?**

En e-handelsplattform säljer i fem EU-länder. Momsreglerna skiljer sig: olika satser, olika regler för livsmedel och böcker, och olika avrundning. När kassan räknar ut totalsumman behöver den rätt momsregler för kundens land.

### 📣 Scenario 3: Efter att en order har lagts
**Observer eller Mediator?**

När en order läggs ska ett bekräftelsemejl skickas, lagret uppdateras och statistik registreras. Teamet använder redan en egen mediator för commands och queries. En utvecklare vill använda C#-events, en annan vill publicera en `OrderPlacedNotification` via mediatorn.

### 📜 Scenario 4: Den gamla loggern
**Adapter eller Decorator?**

Företaget har ett eget loggbibliotek från 2015, `CorpLog`, med metoden `CorpLog.Write(int level, string category, string text)`. Nya tjänster använder `ILogger<T>` från .NET. Ni vill att loggar från nya tjänster ska hamna i `CorpLog`, och att varje loggrad ska få med det aktuella `CorrelationId`.

### 📨 Scenario 5: Requests till ett externt API
**Builder eller Factory?**

Appen anropar ett externt API med tolv olika endpoints. Varje request behöver en bas-URL, autentisering, en `Accept`-header och ibland query-parametrar, en JSON-body, en idempotensnyckel eller en timeout. Idag byggs varje `HttpRequestMessage` för hand på tolv ställen.

### 🛂 Scenario 6: Validering av alla requests
**Middleware (Chain of Responsibility) eller pipeline behaviors (Decorator)?**

Ett ASP.NET Core-API använder en mediator. Alla commands ska valideras innan de når sin handler, och vid fel ska API:t svara 400 med en lista över felen. Commands skickas också från en bakgrundstjänst som läser från en meddelandekö.

---

## 💡 Hints

<details>
<summary>💡 Hint 1: Decorator kontra Chain of Responsibility</summary>

En **Decorator** *lägger till* beteende och skickar (nästan) alltid vidare. En **Chain of Responsibility** *väljer* vem som hanterar requesten: varje länk kan hantera den själv och stoppa kedjan. Fråga dig: skickas requesten alltid hela vägen fram, eller kan någon på vägen svara direkt?
</details>

<details>
<summary>💡 Hint 2: Strategy kontra Factory</summary>

**Strategy** handlar om att *använda* en utbytbar algoritm. **Factory** handlar om att *skapa* rätt objekt. De konkurrerar sällan på riktigt. Fundera på om scenariot behöver det ena, det andra eller båda.
</details>

<details>
<summary>💡 Hint 3: Observer kontra Mediator</summary>

Med **Observer** prenumererar lyssnarna direkt på subjectet. Med en **Mediator** går allt via en central mellanhand, och varken avsändare eller mottagare känner till varandra. Vad har teamet redan, och vad vinner du på att vara konsekvent?
</details>

<details>
<summary>💡 Hint 4: Adapter kontra Decorator</summary>

En **Adapter** *ändrar interfacet*. En **Decorator** *behåller interfacet* och lägger till beteende. Scenario 4 innehåller två olika önskemål. Behöver de samma mönster?
</details>

<details>
<summary>💡 Hint 5: Middleware kontra pipeline behaviors</summary>

Middleware ser bara *HTTP-requests*. Pipeline behaviors ser *alla* requests som går genom mediatorn, oavsett varifrån de kommer. Läs scenario 6 noga en gång till.
</details>

### 📚 Central concepts

- **Decorator** – lägg till beteende, behåll interfacet. 🔗 [Refactoring.Guru: Decorator](https://refactoring.guru/design-patterns/decorator)
- **Chain of Responsibility** – låt en kedja avgöra vem som hanterar. 🔗 [Refactoring.Guru: Chain of Responsibility](https://refactoring.guru/design-patterns/chain-of-responsibility)
- **Strategy** – utbytbara algoritmer. 🔗 [Refactoring.Guru: Strategy](https://refactoring.guru/design-patterns/strategy)
- **Factory Method** och **Abstract Factory** – skapa rätt objekt. 🔗 [Refactoring.Guru: Factory Method](https://refactoring.guru/design-patterns/factory-method) · [Abstract Factory](https://refactoring.guru/design-patterns/abstract-factory)
- **Observer** och **Mediator** – två sätt att frikoppla avsändare och mottagare. 🔗 [Refactoring.Guru: Observer](https://refactoring.guru/design-patterns/observer) · [Mediator](https://refactoring.guru/design-patterns/mediator)
- **Adapter** – ändra interfacet. 🔗 [Refactoring.Guru: Adapter](https://refactoring.guru/design-patterns/adapter)
- **Builder** – bygg steg för steg. 🔗 [Refactoring.Guru: Builder](https://refactoring.guru/design-patterns/builder)
- **DelegatingHandler** – HTTP-pipelinen i .NET. 🔗 [Microsoft Learn: DelegatingHandler](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.delegatinghandler)
- **Middleware** och **endpoint filters** i ASP.NET Core. 🔗 [Microsoft Learn: Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/) · [Filters in Minimal API apps](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters)

## 🤔 Reflektionsfrågor

1. I hur många scenarier valde du *båda* mönstren? Vad säger det om hur mönster används i verkligheten?
2. Spelar det roll vad mönstret *heter* om koden ändå ser likadan ut? När är namnet viktigt?
3. Vilket par tycker du är svårast att hålla isär?

---

🔑 **[Facit](../../../answers/03-pick-the-pattern/competing-patterns.md)**. Gör ett ärligt försök först!
