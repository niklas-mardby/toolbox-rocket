# 🔑 Facit: Report export formats

> 🧩 En fullständig lösning finns på branchen `solutions` i `solutions/06-change-request/report-export-formats/`.

## 🤔 Del 2: Reflektionen efter del 1

- **Ställen per nytt format:** **fyra**: `SupportedFormats`, `Export`, `FileNameFor` och `ContentTypeFor`. Allt i samma fil, men utspritt.
- **Glömde du något?** Det är vanligt att glömma `SupportedFormats` eller `ContentTypeFor`. Testerna `All_three_formats_are_offered...` och `..._has_a_file_name_and_content_type` fångar det. *Utan* de testerna hade felet upptäckts av en användare.
- **Längd:** från cirka 60 till cirka 130 rader med tre format, och troligen 200+ med fem. Formaten blandas i varandra.
- **Vad behöver PDF-utvecklaren veta?** Att det finns fyra ställen att ändra, och att de inte får glömmas. Kunskapen finns i huvudet, inte i koden. Det är kodlukten *shotgun surgery*.

## 🔧 Del 3: Efter refaktoreringen

```csharp
public interface IReportFormat
{
    string Name { get; }
    string FileExtension { get; }
    string ContentType { get; }
    string Render(SalesReport report);
}

public class ReportExporter
{
    private readonly Dictionary<string, IReportFormat> _formats;

    public ReportExporter() : this([new CsvFormat(), new JsonFormat(), new MarkdownFormat()]) { }
    public ReportExporter(IEnumerable<IReportFormat> formats) =>
        _formats = formats.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<string> SupportedFormats => _formats.Keys.ToList();
    public string Export(SalesReport report, string format) => Get(format).Render(report);
    // ...
}
```

**HTML (nästa förändring):** en ny klass, `HtmlFormat`, och en rad i listan av format. Inga andra ändringar. Interfacet *tvingar* fram alla fyra delarna, så kompilatorn påminner om något saknas.

## 🤔 Reflektionsfrågorna

### 1. Refaktorera före eller efter?
Båda är försvarbara:
- **Före** (*preparatory refactoring*: "make the change easy, then make the easy change"): du vet att två nya format kommer, så du kan lika gärna göra det enkelt först. Med tester är det säkert.
- **Efter:** du har sett *exakt* vad som varierar mellan tre konkreta format, så abstraktionen blir rätt. Med bara CSV hade du kanske missat att content type skiljer sig.

I praktiken är en bra tumregel att refaktorera när du har **minst två konkreta varianter** framför dig, antingen i koden eller i kravet.

### 2. Finns det fortfarande ett ställe att ändra?
Ja: **listan i konstruktorn** (`[new CsvFormat(), ...]`). Den kan försvinna med:
- **DI:** `services.AddSingleton<IReportFormat, HtmlFormat>()` och `ReportExporter(IEnumerable<IReportFormat> formats)`. Konstruktorn finns redan.
- **Assembly scanning:** hitta alla klasser som implementerar `IReportFormat` med reflection.

Även med DI måste *något* registrera formatet. Det är ofta bättre att ha **en uttrycklig rad** än magisk scanning: det syns vad som finns.

### 3. Två format med samma namn
`ToDictionary` kastar `ArgumentException` direkt när `ReportExporter` skapas. Det är bra: du vill upptäcka konfigurationsfel **när appen startar** ("fail fast"), inte när en användare försöker exportera. Om `ReportExporter` registreras som singleton i DI sker det först vid första användningen. Vill du vara säker på att felet syns vid uppstart kan du hämta tjänsten en gång direkt efter `builder.Build()`, eller skriva ett test som skapar den.
