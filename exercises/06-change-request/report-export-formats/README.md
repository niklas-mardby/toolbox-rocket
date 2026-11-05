# 📊 Report export formats

> 🧩 **Mönstret som hjälper:** Strategy
> 📏 **Omfattning:** en klass med tre metoder, var och en med en `switch` på formatet. Det nya kravet är två nya format med tydliga acceptanstester. Ett mönster, tydliga steg.
> 🧪 **Tester:** 5 befintliga tester är gröna. 5 acceptanstester för det nya kravet är avstängda med `Todo.Skip`.

## 🎯 Mål

Du ska lägga till två exportformat, känna efter hur designen klarar förändringen, och sedan refaktorera till **Strategy** så att nästa format blir en ny klass i stället för en ändring på tre ställen.

## 📖 Bakgrund

Administrationsverktyget kan exportera försäljningsrapporter till CSV. `ReportExporter` har tre metoder:

- `Export(report, format)` skapar själva innehållet
- `FileNameFor(report, format)` ger filnamnet
- `ContentTypeFor(format)` ger MIME-typen för nedladdningen

Dessutom finns `SupportedFormats`, som UI:t använder för att visa en dropdown.

### 📋 Det nya kravet

> **Som** controller **vill jag** kunna exportera rapporten som **JSON** (för vårt BI-verktyg) och som **Markdown** (för att klistra in i vår wiki), **så att** jag slipper konvertera CSV-filen för hand.

De exakta formaten beskrivs av acceptanstesterna i `NewFormatsAcceptanceTests.cs`.

## 🛠️ Uppgift

### Del 1: Lägg till funktionen 🛠️

1. ▶️ Kör testerna. 5 är gröna och 5 är överhoppade.
2. 🧪 Sätt `Todo.Skip` till `null`. 5 tester blir röda.
3. 🛠️ Gör dem gröna **med den nuvarande designen**: lägg till `case` i `switch`-satserna. Gör det enkelt, inte snyggt.
4. 💾 Gör en commit eller spara en kopia.

### Del 2: Reflektera 🤔

5. Svara på frågorna:
   - Hur många ställen i `ReportExporter.cs` ändrade du för varje nytt format?
   - Glömde du något ställe på första försöket? Hur märkte du det?
   - Hur lång är `ReportExporter.cs` nu? Hur lång blir den med fem format?
   - Vad behöver någon som ska lägga till *PDF* nästa månad känna till?

### Del 3: Refaktorera 🔧

6. 🔀 Inför ett interface för ett exportformat. Det ska innehålla *allt* som skiljer sig mellan format.
7. 🏗️ Skapa en klass per format och flytta logiken dit. Kör testerna efter varje format.
8. 🧹 Låt `ReportExporter` hitta rätt format utan `switch`. Även `SupportedFormats` ska komma från samma ställe.
9. ✅ Alla 10 tester ska vara gröna.

### ➕ Nästa förändring

10. Lägg till **HTML**: en `<table>` med samma kolumner som Markdown-versionen, filändelse `.html` och content type `text/html`. Skriv ett eget test. Hur många *befintliga* filer behövde du ändra?

## ▶️ Kom igång

```bash
dotnet run --project exercises/06-change-request/report-export-formats/ReportExportFormats
dotnet test exercises/06-change-request/report-export-formats/ReportExportFormats.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: JSON</summary>

`System.Text.Json` gör det mesta åt dig. Med `new JsonSerializerOptions(JsonSerializerDefaults.Web)` blir property-namnen camelCase. `DateOnly` serialiseras som `"2026-10-01"`. Ett anonymt objekt (`new { report.Title, ... }`) är ett smidigt sätt att styra formen på JSON-dokumentet.
</details>

<details>
<summary>💡 Hint 2: Markdown</summary>

Titta noga på det förväntade resultatet i testet: tomma rader, tankstrecket `–` (inte bindestreck) och att `|` i produktnamn måste skrivas som `\|`.
</details>

<details>
<summary>💡 Hint 3: Vad ska interfacet innehålla?</summary>

Gå igenom de tre metoderna och `SupportedFormats`. Vad skiljer sig mellan CSV, JSON och Markdown? Namnet, filändelsen, content type och själva renderingen. Det är fyra medlemmar. Filnamnets *bas* (`sales-...`) är däremot densamma för alla.
</details>

<details>
<summary>💡 Hint 4: Hur hittar ReportExporter rätt format?</summary>

En `Dictionary<string, IReportFormat>` med `StringComparer.OrdinalIgnoreCase` ger både uppslag och skiftlägesokänslighet. Nycklarna *är* dessutom `SupportedFormats`.
</details>

### 📚 Central concepts

- **Strategy** – utbytbara algoritmer bakom ett interface. 🔗 [Refactoring.Guru: Strategy](https://refactoring.guru/design-patterns/strategy)
- **Replace Conditional with Polymorphism** – från `switch` till klasser. 🔗 [Refactoring.Guru: Replace Conditional with Polymorphism](https://refactoring.guru/replace-conditional-with-polymorphism)
- **Shotgun surgery** – en förändring, många ställen. 🔗 [Refactoring.Guru: Shotgun Surgery](https://refactoring.guru/smells/shotgun-surgery)
- **Open/closed principle** – utöka utan att ändra. 🔗 [Wikipedia: Open–closed principle](https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle)
- **Preparatory refactoring** – "gör förändringen lätt, och gör sedan den lätta förändringen". 🔗 [Martin Fowler: An example of preparatory refactoring](https://martinfowler.com/articles/preparatory-refactoring-example.html)
- **StringBuilder** – för att bygga text effektivt. 🔗 [Microsoft Learn: StringBuilder](https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder)

## 🤔 Reflektionsfrågor

1. Hade det varit bättre att refaktorera *före* du lade till JSON och Markdown? Varför, eller varför inte?
2. Med en `Dictionary` försvann `switch`-satserna. Finns det fortfarande ett ställe som måste ändras för varje nytt format? Hur kunde även det försvinna?
3. Vad händer om två format registreras med samma namn? Vill du upptäcka det när appen startar eller när någon exporterar?

---

🔑 **[Facit](../../../answers/06-change-request/report-export-formats.md)**. Gör ett ärligt försök först!
