# 🎾 Booking system design

> 🧩 **Mönster:** du väljer själv bland alla åtta kärnmönster, och bestämmer var inget mönster behövs
> 📏 **Omfattning:** en öppen designuppgift för ett helt system med många delar, verkliga avvägningar och tre förändringsscenarier som testar din design. Det finns inget facit med ett enda rätt svar. Planera för flera timmars arbete, gärna i par eller grupp.
> 🧪 **Ingen kod krävs**, men du ska skissa interfaces och visa hur delarna hänger ihop.

## 🎯 Mål

Du ska designa ett realistiskt system och visa att du kan **välja mönster där de gör nytta, avstå där de inte gör det, och motivera varje val**.

## 📖 Bakgrund

**Padelkedjan** har 25 padelhallar i Sverige och Norge. De vill ersätta sitt gamla bokningssystem med ett nytt, byggt i ASP.NET Core med en webbapp och en mobilapp som klienter. Du är ansvarig för designen av backend.

### 📋 Kraven

1. 🗓️ **Bokning:** kunder bokar en bana för 60 eller 90 minuter. En bana kan inte dubbelbokas.
2. 📏 **Bokningsregler per hall:** hallar har olika regler. Vissa tillåter bokning 14 dagar i förväg, andra 7. Vissa kräver medlemskap före klockan 17. En hall i Oslo har särskilda regler för skollov.
3. 💰 **Prissättning:** priset beror på tid (rusningstid eller lågtrafik), helg, medlemskap och kampanjer. Varje hall sätter sina egna priser.
4. 💳 **Betalning:** svenska kunder betalar med Swish eller kort, norska med Vipps eller kort. Varje betalleverantör har sitt eget SDK.
5. 🔔 **Notiser:** bekräftelse, påminnelse 2 timmar innan och information om avbokning. Kunden väljer själv mejl, SMS eller push, eller flera.
6. ❌ **Avbokning:** gratis upp till 24 timmar innan. Efter det betalar kunden 50 %. Medlemmar med guldnivå avbokar alltid gratis.
7. ⏳ **Väntelista:** om en bokad tid avbokas ska den första kunden på väntelistan få ett erbjudande, och har 15 minuter på sig att acceptera.
8. 🔐 **Dörrlås:** hallarna har elektroniska lås från två olika leverantörer. När en bokning bekräftas ska kunden få en dörrkod som gäller under bokningstiden.
9. 📊 **Rapporter:** hallchefer vill se beläggning och intäkter per vecka.
10. 🧾 **Revision:** alla ändringar av bokningar och priser ska loggas med vem som gjorde vad och när.

## 🛠️ Uppgift

### Del 1: Designen

1. 🗺️ **Dela in systemet i områden**, till exempel bokning, prissättning, betalning och notiser. Rita en enkel skiss över hur de hänger ihop.
2. 🧩 **Välj mönster per område.** Skriv för varje område:
   - vilket eller vilka mönster du använder (eller inget!)
   - **varför**, alltså vilket krav eller problem det löser
   - ett skissat interface eller en klassskiss för de viktigaste delarna
3. 🛑 **Visa var du avstår.** Peka ut minst **två** ställen där ett mönster ligger nära till hands men där du väljer enklare kod, och motivera varför.
4. ⚖️ **Beskriv tre avvägningar** du har gjort. Vad vinner och förlorar du på dem?

### Del 2: Testa designen mot förändringar

Gå igenom din design för varje scenario nedan. **Vilka klasser behöver du lägga till eller ändra?** Om svaret är "många", fundera på om designen borde se annorlunda ut.

- 🔄 **Förändring A:** Padelkedjan öppnar hallar i Danmark, där kunderna betalar med MobilePay.
- 🔄 **Förändring B:** En ny hall i Göteborg vill ha "happy hour": 30 % rabatt mellan 13 och 15 på vardagar, men bara för bokningar som görs samma dag.
- 🔄 **Förändring C:** Den ena låsleverantören går i konkurs, och 8 hallar byter till en tredje leverantör.

### Del 3: Börja bygga

5. 🚀 **Vad bygger ni först?** Teamet har två månader på sig för en första version i två pilothallar. Vilka delar bygger ni enkelt först, och vilka mönster väntar ni med?

## 💡 Hints

<details>
<summary>💡 Hint 1: Leta efter variation och förändring</summary>

Mönster gör mest nytta där saker **varierar** (per hall, per land, per kund) och där saker **kommer att förändras**. Gå igenom kraven och markera allt som skiljer sig mellan hallar, länder eller kunder. Där har du dina kandidater.
</details>

<details>
<summary>💡 Hint 2: Tredjeparter</summary>

Räkna hur många externa system Padelkedjan är beroende av. Varje sådant system är en kandidat för samma mönster, och samma förändringsscenario (byt leverantör) gäller för alla.
</details>

<details>
<summary>💡 Hint 3: Vem behöver veta att en bokning har bekräftats?</summary>

Gör en lista. Är det samma sak som ska hända oavsett hur bokningen gjordes? Ska bokningen misslyckas om ett SMS inte går fram?
</details>

### 📚 Central concepts

- **Design patterns-katalogen** – för att jämföra alternativ. 🔗 [Refactoring.Guru: Catalog](https://refactoring.guru/design-patterns/catalog)
- **Architectural principles** – separation of concerns, dependency inversion med mera. 🔗 [Microsoft Learn: Architectural principles](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles)
- **Domain events** – händelser som andra delar av systemet reagerar på. 🔗 [Martin Fowler: Domain Event](https://martinfowler.com/eaaDev/DomainEvent.html)
- **Separation of concerns** – dela upp systemet efter ansvar. 🔗 [Wikipedia: Separation of concerns](https://en.wikipedia.org/wiki/Separation_of_concerns)
- **Multitenancy** – ett system som betjänar flera "kunder" (här: hallar) med egna regler. 🔗 [Wikipedia: Multitenancy](https://en.wikipedia.org/wiki/Multitenancy)
- **CQRS** – om läsning och skrivning har olika behov. 🔗 [Martin Fowler: CQRS](https://martinfowler.com/bliki/CQRS.html)
- **Options pattern** – konfiguration per hall eller land. 🔗 [Microsoft Learn: Options pattern in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
- **Background services** – för påminnelser och väntelistans tidsgränser. 🔗 [Microsoft Learn: Worker services in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/workers)
- **YAGNI** – för att veta vad du ska vänta med. 🔗 [Martin Fowler: Yagni](https://martinfowler.com/bliki/Yagni.html)

## 🤔 Reflektionsfrågor

1. Vilket krav var svårast att hitta ett bra mönster för? Varför?
2. Hur klarade din design de tre förändringsscenarierna? Ändrade du designen efteråt?
3. Om du jämför din design med en kurskamrats, var skiljer de sig mest? Vems lösning är enklast, och vems är mest flexibel?
4. Vad är det första du skulle ändra i din design om Padelkedjan bara hade *en* hall?

---

🔑 **[Facit och exempeldesign](../../../answers/03-pick-the-pattern/booking-system-design.md)**. Gör ett ärligt försök först!
