using ReportExportFormats;

var report = new SalesReport("Försäljning oktober", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31),
[
    new SalesRow("Hammare", 12, 2388m),
    new SalesRow("Skruv; 100-pack", 40, 2396m),
    new SalesRow("Borrmaskin \"Pro\"", 3, 4497m),
]);

var exporter = new ReportExporter();
foreach (var format in exporter.SupportedFormats)
{
    Console.WriteLine($"📄 {exporter.FileNameFor(report, format)} ({exporter.ContentTypeFor(format)})");
    Console.WriteLine(exporter.Export(report, format));
}
