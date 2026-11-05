namespace ReportExportFormats.Tests;

internal static class TestData
{
    public static SalesReport October() => new("Försäljning oktober", new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31),
    [
        new SalesRow("Hammare", 12, 2388m),
        new SalesRow("Skruv; 100-pack", 40, 2396.5m),
        new SalesRow("Borr \"Pro\" | XL", 3, 4497m),
    ]);
}
