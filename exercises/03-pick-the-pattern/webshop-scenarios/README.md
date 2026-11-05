# 🛒 Webshop scenarios

> 🧩 **Mönster:** alla åtta kärnmönster kan förekomma, och vissa scenarier har två rimliga svar
> 📏 **Omfattning:** åtta scenarier från samma webbshop. De bygger inte på varandra, men de delar sammanhang, så du behöver tänka på hur dina val passar ihop.
> 🧪 **Ingen kod.** Skriv dina svar i en egen fil.

## 🎯 Mål

Du ska välja mönster i ett realistiskt sammanhang där problemen är mindre renodlade än i en lärobok, och där det ibland finns mer än ett bra svar.

## 📖 Bakgrund

**Verktygsboden** är en webbshop för verktyg. Den är byggd med ASP.NET Core och EF Core och har vuxit i fem år. Teamet är fyra utvecklare. Här är åtta saker de brottas med just nu.

## 🛠️ Uppgift

Skriv för varje scenario **mönster**, **motivering**, **alternativ** och **kostnad** (se [mallen](../README.md#-så-jobbar-du)). Om du tycker att två mönster passar, välj det du tycker bäst om och förklara varför.

---

### 🔎 Scenario 1: "Aktiva produkter"

Frågan "vilka produkter är aktiva?" finns på elva ställen i koden: i produktlistan, i sökningen, i sitemapen, i rekommendationerna och så vidare. Definitionen skiljer sig lite åt mellan ställena. Ibland räknas produkter med lagersaldo 0 som aktiva, ibland inte. Igår visades en utgången produkt på startsidan.

### 💳 Scenario 2: Kassan

När en kund betalar ska tre saker hända: ordern skapas, lagret räknas ner och rabattkoden markeras som använd. Förra veckan kraschade servern mitt i, och en order skapades utan att lagret räknades ner.

### 🌍 Scenario 3: Betalning per land

Butiken ska börja sälja till Norge och Tyskland. Svenska kunder betalar via Swish eller kort, norska via Vipps och tyska via Klarna eller kort. Varje leverantör har sitt eget SDK med helt olika API:er.

### 🏷️ Scenario 4: Kampanjerna

Marknadsavdelningen vill kunna köra kampanjer som "3 för 2 på skruv", "20 % på alla sågar" och "medlemspris på allt". Nya kampanjtyper dyker upp varje kvartal, och ibland gäller flera kampanjer samtidigt.

### 🐢 Scenario 5: Produktsidan

Produktsidan är långsam, eftersom den hämtar produktdata från ett externt PIM-system (Product Information Management) som tar 800 ms att svara. Datan ändras högst några gånger om dagen. `IProductInfoService` används på sju ställen.

### 🏛️ Scenario 6: OrderController

`OrderController` har vuxit till 1 500 rader och 22 actions. Varje action börjar med samma behörighetskontroll och valideringskod och slutar med samma loggning. Två utvecklare har fått merge-konflikter i filen tre gånger den här veckan.

### 🔔 Scenario 7: "Meddela mig när den finns i lager"

Kunder kan klicka "meddela mig" på slutsålda produkter. När lagret fylls på ska de få ett mejl. Dessutom ska sökindexet uppdateras och inköpsavdelningen få statistik. Lagerhanteringen sköts av ett annat team, som inte vill att deras kod ska känna till mejl eller sökindex.

### 📦 Scenario 8: Fraktsedlarna

Butiken skickar paket med PostNord och DHL. Båda har SDK:er för att skapa fraktsedlar, men de fungerar helt olika: PostNord vill ha XML och returnerar en PDF som `byte[]`, DHL vill ha JSON-objekt och returnerar en URL.

---

## 💡 Hints

<details>
<summary>💡 Hint 1: Scenario 1 och 2 handlar om databasen</summary>

Ett av dem handlar om att *frågor* sprids ut och blir inkonsekventa. Det andra handlar om att *ändringar* måste sparas tillsammans. Martin Fowler beskriver ett mönster för vart och ett, och EF Core har redan stöd för det ena.
</details>

<details>
<summary>💡 Hint 2: Scenario 3 och 8 har samma grundproblem</summary>

Olika SDK:er med olika API:er för samma sak. Men i scenario 3 måste koden också *välja* rätt leverantör utifrån landet. Det kan behövas ett mönster till för det.
</details>

<details>
<summary>💡 Hint 3: Scenario 4</summary>

Varje kampanjtyp är en egen regel för att räkna ut rabatt. Men eftersom flera kan gälla samtidigt behöver du också fundera på hur de kombineras. Ska alla tillämpas, eller bara den bästa?
</details>

<details>
<summary>💡 Hint 4: Scenario 6</summary>

Det finns två problem: storleken (merge-konflikter, 22 actions) och dupliceringen (samma kod i varje action). Vilket mönster löser båda?
</details>

### 📚 Central concepts

- **Repository** – samla frågor om domänobjekt. 🔗 [Martin Fowler: Repository](https://martinfowler.com/eaaCatalog/repository.html)
- **Unit of Work** – spara ändringar tillsammans. 🔗 [Martin Fowler: Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- **Transaktioner i EF Core** – vad `SaveChanges` gör åt dig. 🔗 [Microsoft Learn: Using Transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions)
- **Factory Method** – välj vilket objekt som ska skapas. 🔗 [Refactoring.Guru: Factory Method](https://refactoring.guru/design-patterns/factory-method)
- **Adapter** – gör olika API:er lika. 🔗 [Refactoring.Guru: Adapter](https://refactoring.guru/design-patterns/adapter)
- **Mediator** – bryt upp gudsklasser. 🔗 [Refactoring.Guru: Mediator](https://refactoring.guru/design-patterns/mediator)
- **Domain events** – händelser i domänen som andra delar reagerar på. 🔗 [Martin Fowler: Domain Event](https://martinfowler.com/eaaDev/DomainEvent.html)
- **Caching i ASP.NET Core** – `IMemoryCache` och vänner. 🔗 [Microsoft Learn: Cache in-memory in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory)

## 🤔 Reflektionsfrågor

1. Om teamet genomförde alla dina förslag, hur många nya interfaces och klasser skulle det bli? Är det rimligt för ett team på fyra?
2. Vilka tre scenarier skulle du prioritera först, och varför?
3. I vilket scenario är risken störst att mönstret blir överanvänt?

---

🔑 **[Facit](../../../answers/03-pick-the-pattern/webshop-scenarios.md)**. Gör ett ärligt försök först!
