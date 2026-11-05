using System.Text.Json;

namespace ReportExportFormats.Tests;

// ✨ The new requirement. Switch these on with Todo.Skip = null. Don't change these tests.
public class NewFormatsAcceptanceTests
{
    private readonly ReportExporter _exporter = new();

    [Fact(Skip = Todo.Skip)]
    public void All_three_formats_are_offered_in_the_dropdown()
    {
        Assert.Equal(new[] { "csv", "json", "markdown" }, _exporter.SupportedFormats.Order());
    }

    // --- JSON ---

    [Fact(Skip = Todo.Skip)]
    public void Exports_json()
    {
        var json = _exporter.Export(TestData.October(), "json");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal("Försäljning oktober", root.GetProperty("title").GetString());
        Assert.Equal("2026-10-01", root.GetProperty("from").GetString());
        Assert.Equal("2026-10-31", root.GetProperty("to").GetString());

        var rows = root.GetProperty("rows").EnumerateArray().ToList();
        Assert.Equal(3, rows.Count);
        Assert.Equal("Borr \"Pro\" | XL", rows[2].GetProperty("product").GetString());
        Assert.Equal(40, rows[1].GetProperty("quantity").GetInt32());
        Assert.Equal(2396.5m, rows[1].GetProperty("revenue").GetDecimal());

        var total = root.GetProperty("total");
        Assert.Equal(55, total.GetProperty("quantity").GetInt32());
        Assert.Equal(9281.5m, total.GetProperty("revenue").GetDecimal());
    }

    [Fact(Skip = Todo.Skip)]
    public void Json_has_a_file_name_and_content_type()
    {
        Assert.Equal("sales-2026-10-01-2026-10-31.json", _exporter.FileNameFor(TestData.October(), "json"));
        Assert.Equal("application/json", _exporter.ContentTypeFor("JSON"));
    }

    // --- Markdown ---

    [Fact(Skip = Todo.Skip)]
    public void Exports_markdown_table_with_escaped_pipes()
    {
        var markdown = _exporter.Export(TestData.October(), "markdown");

        Assert.Equal(
            "# Försäljning oktober\n" +
            "\n" +
            "_2026-10-01 – 2026-10-31_\n" +
            "\n" +
            "| Produkt | Antal | Intäkt |\n" +
            "|---|---:|---:|\n" +
            "| Hammare | 12 | 2388.00 |\n" +
            "| Skruv; 100-pack | 40 | 2396.50 |\n" +
            "| Borr \"Pro\" \\| XL | 3 | 4497.00 |\n" +
            "| **Totalt** | **55** | **9281.50** |\n",
            markdown);
    }

    [Fact(Skip = Todo.Skip)]
    public void Markdown_has_a_file_name_and_content_type()
    {
        Assert.Equal("sales-2026-10-01-2026-10-31.md", _exporter.FileNameFor(TestData.October(), "markdown"));
        Assert.Equal("text/markdown", _exporter.ContentTypeFor("markdown"));
    }
}
