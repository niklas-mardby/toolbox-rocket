# 📝 NOTES: Report export formats

## 💡 Resonemanget

- **Allt som varierar per format** samlades i `IReportFormat`: namn, filändelse, content type och rendering. Det som *inte* varierar, som filnamnets bas och felet för okända format, ligger kvar i `ReportExporter`.
- **`ReportExporter` vet ingenting om formaten.** Den slår upp i en dictionary. `SupportedFormats` kommer från samma dictionary, så dropdownen och exporten kan aldrig hamna ur synk.
- **Två konstruktorer:** en parameterlös som testerna och `Program.cs` använder, och en som tar `IEnumerable<IReportFormat>`, redo för DI.
- **`Money.Format`** delas av CSV och Markdown, eftersom samma decimalformat behövs i båda. JSON använder siffror, inte text.
- **JSON via anonymt objekt** med `JsonSerializerDefaults.Web` ger camelCase utan attribut på domänklasserna.

## 🔀 Alternativ

- **`Func<SalesReport, string>`** som strategi räcker inte här, eftersom fyra saker varierar och inte bara renderingen. En record (`ReportFormat(string Name, string Extension, string ContentType, Func<SalesReport, string> Render)`) är ett lättare alternativ till ett interface med klasser.
- **Abstrakt basklass** med gemensam logik (Template Method) om formaten börjar dela mycket kod, till exempel för textbaserade tabellformat.
- **Registrering via DI** när `ReportExporter` används i en webbapp: `services.AddSingleton<IReportFormat, CsvFormat>()` och så vidare.
