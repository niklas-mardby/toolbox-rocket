namespace ReportExportFormats.Tests;

// Existing behaviour. Must stay green throughout. Don't change these tests.
public class CsvExportTests
{
    private readonly ReportExporter _exporter = new();

    [Fact]
    public void Exports_csv_with_header_rows_and_total()
    {
        var csv = _exporter.Export(TestData.October(), "csv");

        Assert.Equal(
            "Product;Quantity;Revenue\n" +
            "Hammare;12;2388.00\n" +
            "\"Skruv; 100-pack\";40;2396.50\n" +
            "\"Borr \"\"Pro\"\" | XL\";3;4497.00\n" +
            "TOTAL;55;9281.50\n",
            csv);
    }

    [Fact]
    public void Format_name_is_case_insensitive()
    {
        Assert.Equal(_exporter.Export(TestData.October(), "csv"), _exporter.Export(TestData.October(), "CSV"));
    }

    [Fact]
    public void Csv_has_a_file_name_and_content_type()
    {
        Assert.Equal("sales-2026-10-01-2026-10-31.csv", _exporter.FileNameFor(TestData.October(), "csv"));
        Assert.Equal("text/csv", _exporter.ContentTypeFor("csv"));
    }

    [Fact]
    public void Unknown_formats_are_rejected()
    {
        Assert.Throws<NotSupportedException>(() => _exporter.Export(TestData.October(), "pdf"));
        Assert.Throws<NotSupportedException>(() => _exporter.FileNameFor(TestData.October(), "pdf"));
        Assert.Throws<NotSupportedException>(() => _exporter.ContentTypeFor("pdf"));
    }

    [Fact]
    public void Csv_is_supported()
    {
        Assert.Contains("csv", _exporter.SupportedFormats);
    }
}
