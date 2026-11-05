# 🕵️ Mystery codebase

> 🧩 **Mönster:** minst nio stycken, men inget av dem heter något med mönstrets namn. Och minst ett namn ljuger.
> 📏 **Omfattning:** en liten men komplett konsolapp, ett biljettsystem för en teater, på ungefär 350 rader fördelade på åtta filer. Du läser hela kodbasen själv och hittar mönstren utan att få veta var de finns.
> 🧪 **Inga tester.** Kör programmet och läs koden.

## 🎯 Mål

Du ska hitta och namnge design patterns i en kodbas där klasserna har domännamn, precis som i verkliga projekt. Du ska kunna motivera varje fynd utifrån kodens *struktur*, inte utifrån vad klasserna heter.

## 📖 Bakgrund

I verklig kod heter klasser sällan `PriceStrategy` eller `OrderObserver`. De heter `TariffDesk`, `Doorman` och `Switchboard`, för de är uppkallade efter vad de gör i domänen. För att se mönstren måste du titta på *hur* klasserna hänger ihop: vem implementerar vilket interface, vem tar vad i konstruktorn och vem anropar vem.

Den här appen säljer teaterbiljetter. Den har prissättning, platsbokning, SMS-bekräftelser, kundtjänst och rapporter.

## 🛠️ Uppgift

1. ▶️ **Kör programmet** och läs utskriften:
   ```bash
   dotnet run --project exercises/02-spot-the-pattern/mystery-codebase/MysteryCodebase
   ```
2. 📖 **Läs hela kodbasen.** Börja i `Program.cs`, där allt kopplas ihop, och följ trådarna ut i de andra filerna.
3. 🗂️ **Gör en lista** över alla mönster du hittar. För varje fynd, skriv:
   - **Var:** vilka klasser eller interfaces
   - **Vilket mönster:** och vilken roll varje klass spelar
   - **Bevis:** vilken egenskap i koden som avslöjar mönstret
4. 🎭 **Hitta lögnen.** Minst en klass har ett namn som antyder ett mönster den *inte* implementerar. Vilken, och varför är det inte det mönstret?
5. 🧅 **Förklara stapeln.** `ISeatMap` används i tre lager i `Program.cs`. Förklara varför Doris femplatsbokning syns i platsdagboken, trots att den stoppades. Vad hade hänt om lagren legat i omvänd ordning?
6. 🔀 **Kombinationer.** Hitta minst två ställen där två mönster samarbetar.
7. 🛠️ **Bonus:** Lägg till en ny prisklass, `Gruppbiljett` för sällskap om minst 8 personer (20 % rabatt), och en ny kundtjänstfråga, `TicketLost`. Hur många befintliga filer behövde du ändra för var och en?

## 💡 Hints

<details>
<summary>💡 Hint 1: Hur många ska jag hitta?</summary>

Det finns minst nio olika mönster från repot: **Repository, Proxy, Strategy, Factory, Decorator, Builder, Observer, Adapter, Mediator och Template Method**. Det är tio om du räknar Factory separat. Vissa är lätta att hitta, och vissa gömmer sig i en enda fil.
</details>

<details>
<summary>💡 Hint 2: Interfaces med flera implementationer</summary>

Leta efter interfaces som har mer än en implementation. Hur används de? Väljs en implementation någonstans? Eller tar en implementation en *annan* implementation av samma interface i konstruktorn?
</details>

<details>
<summary>💡 Hint 3: Shows.cs</summary>

Det finns två klasser som implementerar `IShowCatalog`. Den ena lagrar föreställningar. Vad gör den andra? När läses föreställningarna in? Titta på utskriften `(läser in föreställningar...)`.
</details>

<details>
<summary>💡 Hint 4: Event och tredje part</summary>

Sök efter `event` och efter kommentaren om en leverantör från 2011. Vad gör `PagerLink` med telefonnummer och text innan den skickar vidare?
</details>

<details>
<summary>💡 Hint 5: Lögnen</summary>

Vad krävs för att något ska vara en Decorator? Jämför med `Doorman` och `Diary`. Vilken klass i `Printouts.cs` uppfyller *inte* de kraven?
</details>

### 📚 Central concepts

- **Design pattern-katalogen** – alla mönster på ett ställe, för att jämföra strukturer. 🔗 [Refactoring.Guru: Catalog](https://refactoring.guru/design-patterns/catalog)
- **Proxy** – ett ställföreträdande objekt. 🔗 [Refactoring.Guru: Proxy](https://refactoring.guru/design-patterns/proxy)
- **Mediator** – kommunikation via en mellanhand. 🔗 [Refactoring.Guru: Mediator](https://refactoring.guru/design-patterns/mediator)
- **Template Method** – basklassen äger flödet. 🔗 [Refactoring.Guru: Template Method](https://refactoring.guru/design-patterns/template-method)
- **Builder** – bygg steg för steg. 🔗 [Refactoring.Guru: Builder](https://refactoring.guru/design-patterns/builder)
- **Repository** – en samlingslik ingång till domänobjekt. 🔗 [Martin Fowler: Repository](https://martinfowler.com/eaaCatalog/repository.html)
- **Pattern matching i C#** – används i en av fabrikerna. 🔗 [Microsoft Learn: Pattern matching](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/pattern-matching)
- **Records och `with`** – används när en föreställning flyttas. 🔗 [Microsoft Learn: Records](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/records)

## 🤔 Reflektionsfrågor

1. Vilket mönster var svårast att hitta? Vad gjorde det svårt?
2. Är det bra eller dåligt att klasserna har domännamn i stället för mönsternamn? När skulle du vilja ha med mönstrets namn i klassnamnet?
3. `Switchboard` mäter också tiden för varje anrop. Vad säger det om var tvärgående logik kan placeras när du har en mediator?
4. Hittade du något i koden som du tycker är *överanvändning* av ett mönster?

---

🔑 **[Facit](../../../answers/02-spot-the-pattern/mystery-codebase.md)**. Gör ett ärligt försök först!
