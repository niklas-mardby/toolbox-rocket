namespace ReportExportFormats;

/// <summary>
/// Looks up the right format strategy. It no longer knows anything about CSV, JSON or Markdown.
/// Adding a format = one new class + one line in the list below (or a DI registration).
/// </summary>
public class ReportExporter
{
    private readonly Dictionary<string, IReportFormat> _formats;

    public ReportExporter() : this([new CsvFormat(), new JsonFormat(), new MarkdownFormat()]) { }

    public ReportExporter(IEnumerable<IReportFormat> formats) =>
        _formats = formats.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<string> SupportedFormats => _formats.Keys.ToList();

    public string Export(SalesReport report, string format) => Get(format).Render(report);

    public string FileNameFor(SalesReport report, string format) =>
        $"sales-{report.From:yyyy-MM-dd}-{report.To:yyyy-MM-dd}.{Get(format).FileExtension}";

    public string ContentTypeFor(string format) => Get(format).ContentType;

    private IReportFormat Get(string format) =>
        _formats.TryGetValue(format, out var f) ? f : throw new NotSupportedException($"Format '{format}' is not supported.");
}
