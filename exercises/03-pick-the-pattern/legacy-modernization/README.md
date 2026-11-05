# 🏚️ Legacy modernization

> 🧩 **Mönster:** Adapter, Dependency Injection, Strategy och Repository, men framför allt *i vilken ordning* och *hur försiktigt* de införs
> 📏 **Omfattning:** ett längre scenario med ett system i produktion, verkliga begränsningar och sju frågor som bygger på varandra. Det finns inget enda rätt svar, och du behöver väga risk, tid och nytta mot varandra.
> 🧪 **Ingen kod**, men du får gärna skissa interfaces och klasser.

## 🎯 Mål

Du ska planera hur design patterns kan införas **gradvis och säkert** i ett befintligt system som inte får gå sönder, och motivera ordningen.

## 📖 Bakgrund

**Föreningskassan** är ett medlemssystem som används av 400 idrottsföreningar. Det skrevs för tio år sedan i .NET Framework och har precis flyttats till .NET 10. Det kompilerar och fungerar, men koden har inte förändrats. Du har just börjat i teamet, som består av tre utvecklare.

### 🔍 Så här ser koden ut

- 🗄️ **Databasen** nås via en statisk klass, `Db.Execute(string sql)` och `Db.Query<T>(string sql)`, från **180 ställen** i koden. SQL-strängarna byggs ofta med `+`.
- 📧 **Mejl** skickas via den statiska `Mailer.Send(to, subject, body)` från 25 ställen.
- 🧮 **`MembershipProcessor`** är 2 000 rader lång. Den räknar ut medlemsavgifter, och har en `switch` på medlemstyp (Junior, Senior, Pensionär, Ständig, Hedersmedlem, Stödmedlem) i **fyra olika metoder**.
- 🏛️ **Riksidrottsförbundets SOAP-tjänst** (`RfSoapClient`) anropas direkt från **30 ställen** för att rapportera medlemsantal och hämta licenser.
- ⏰ **`DateTime.Now`** används överallt, bland annat för att avgöra vilken säsong det är.
- 🧪 **Det finns inga automatiska tester.**

### 📋 Det som händer framöver

1. 👨‍👩‍👧 **Om sex veckor:** föreningarna vill ha en ny medlemstyp, **Familj**, med en avgift som beror på antalet familjemedlemmar.
2. 🔄 **Om sex månader:** Riksidrottsförbundet stänger SOAP-tjänsten och ersätter den med ett **REST-API** med en helt annan datamodell. Under en övergångsperiod på två månader fungerar båda.
3. 🐛 **Hela tiden:** buggrapporter om fel avgifter vid säsongsskiften.

## 🛠️ Uppgift

Svara på frågorna i ordning. De bygger på varandra.

1. 🧪 **Innan du inför något mönster:** Vad gör du först, och varför? Hur gör du det utan att röra koden mer än nödvändigt?
2. 🔌 **SOAP till REST:** Vilket mönster använder du för att förbereda bytet? Hur gör du för att kunna byta *gradvis* under övergångsperioden, och kunna backa om något går fel?
3. 🗄️ **Den statiska databasklassen:** Hur gör du `Db` utbytbar i tester utan att ändra alla 180 ställen på en gång? Vilket eller vilka mönster hjälper?
4. 🧮 **Familj och `MembershipProcessor`:** Hur lägger du till Familj? Ska du refaktorera `MembershipProcessor` till Strategy först, eller lägga till ett `case` i varje `switch` och refaktorera sedan? Motivera med tanke på tidsplanen.
5. ⏰ **Säsongsbuggarna:** Hur angriper du dem? Vilket mönster eller vilken teknik hjälper?
6. 🚫 **Vad gör du *inte*?** Nämn minst två saker som skulle vara frestande men olämpliga.
7. 📅 **Planen:** Skriv en plan för de första fyra veckorna. Vad gör ni vecka 1, 2, 3 och 4?

## 💡 Hints

<details>
<summary>💡 Hint 1: Skyddsnätet först</summary>

Att ändra kod utan tester är som att bygga om ett hus med folk i. Hur kan du skriva tester för kod som är svår att testa? Läs om *characterization tests*: tester som fångar hur koden beter sig *idag*, även om beteendet är fel.
</details>

<details>
<summary>💡 Hint 2: Sömmar</summary>

En *söm* (seam) är ett ställe där du kan byta beteende utan att ändra koden runt omkring. Ett interface som injiceras är en söm. Hur skapar du en söm runt en statisk klass? Tänk på Adapter: ett objekt som implementerar ditt interface och anropar den statiska klassen inuti.
</details>

<details>
<summary>💡 Hint 3: Två implementationer samtidigt</summary>

Under övergångsperioden fungerar både SOAP och REST. Om du har ett interface i domänens språk, med en adapter för varje, hur skulle du kunna välja implementation med en inställning i stället för en ny release?
</details>

<details>
<summary>💡 Hint 4: Gradvis i stället för allt på en gång</summary>

Martin Fowler beskriver *Strangler Fig*: precis som ett fikonträd som växer runt ett annat träd byter du ut ett system bit för bit, tills det gamla inte behövs längre. Hur kan den idén användas för de 180 databasanropen?
</details>

### 📚 Central concepts

- **Characterization tests** – tester som beskriver befintligt beteende. 🔗 [Wikipedia: Characterization test](https://en.wikipedia.org/wiki/Characterization_test)
- **Adapter** – en söm runt tredjeparts- eller legacykod. 🔗 [Refactoring.Guru: Adapter](https://refactoring.guru/design-patterns/adapter)
- **Strangler Fig** – byt ut ett system gradvis. 🔗 [Martin Fowler: StranglerFigApplication](https://martinfowler.com/bliki/StranglerFigApplication.html)
- **Parallel change** – ändra ett interface i flera säkra steg. 🔗 [Martin Fowler: ParallelChange](https://martinfowler.com/bliki/ParallelChange.html)
- **Preparatory refactoring** – refaktorera för att göra nästa förändring lätt. 🔗 [Martin Fowler: An example of preparatory refactoring](https://martinfowler.com/articles/preparatory-refactoring-example.html)
- **Opportunistic refactoring** – städa där du ändå är. 🔗 [Martin Fowler: OpportunisticRefactoring](https://martinfowler.com/bliki/OpportunisticRefactoring.html)
- **Technical debt** – vad det är och hur du pratar om det. 🔗 [Martin Fowler: TechnicalDebt](https://martinfowler.com/bliki/TechnicalDebt.html)
- **TimeProvider** – gör tid testbar. 🔗 [Microsoft Learn: What is TimeProvider?](https://learn.microsoft.com/en-us/dotnet/standard/datetime/timeprovider-overview)

## 🤔 Reflektionsfrågor

1. Hur skulle du förklara för en produktägare varför teamet behöver lägga tid på tester och refaktorering innan Familj kan levereras?
2. Vilket av problemen i kodbasen tror du är *farligast* på sikt? Varför?
3. Är det någonsin rätt att skriva om ett system från grunden i stället för att modernisera det gradvis?

---

🔑 **[Facit](../../../answers/03-pick-the-pattern/legacy-modernization.md)**. Gör ett ärligt försök först!
