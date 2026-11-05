using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ReportExportFormats;

/// <summary>Strategy: everything that varies between export formats, in one place per format.</summary>
public interface IReportFormat
{
    string Name { get; }
    string FileExtension { get; }
    string ContentType { get; }
    string Render(SalesReport report);
}

internal static class Money
{
    public static string Format(decimal amount) => amount.ToString("0.00", CultureInfo.InvariantCulture);
}

public class CsvFormat : IReportFormat
{
    public string Name => "csv";
    public string FileExtension => "csv";
    public string ContentType => "text/csv";

    public string Render(SalesReport report)
    {
        var sb = new StringBuilder("Product;Quantity;Revenue\n");
        foreach (var row in report.Rows)
            sb.Append($"{Escape(row.Product)};{row.Quantity};{Money.Format(row.Revenue)}\n");
        sb.Append($"TOTAL;{report.TotalQuantity};{Money.Format(report.TotalRevenue)}\n");
        return sb.ToString();
    }

    private static string Escape(string value) =>
        value.Contains(';') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}

public class JsonFormat : IReportFormat
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public string Name => "json";
    public string FileExtension => "json";
    public string ContentType => "application/json";

    public string Render(SalesReport report) => JsonSerializer.Serialize(new
    {
        report.Title,
        report.From,
        report.To,
        Rows = report.Rows.Select(r => new { r.Product, r.Quantity, r.Revenue }),
        Total = new { Quantity = report.TotalQuantity, Revenue = report.TotalRevenue },
    }, Options);
}

public class MarkdownFormat : IReportFormat
{
    public string Name => "markdown";
    public string FileExtension => "md";
    public string ContentType => "text/markdown";

    public string Render(SalesReport report)
    {
        var sb = new StringBuilder();
        sb.Append($"# {report.Title}\n\n");
        sb.Append($"_{report.From:yyyy-MM-dd} – {report.To:yyyy-MM-dd}_\n\n");
        sb.Append("| Produkt | Antal | Intäkt |\n");
        sb.Append("|---|---:|---:|\n");
        foreach (var row in report.Rows)
            sb.Append($"| {row.Product.Replace("|", "\\|")} | {row.Quantity} | {Money.Format(row.Revenue)} |\n");
        sb.Append($"| **Totalt** | **{report.TotalQuantity}** | **{Money.Format(report.TotalRevenue)}** |\n");
        return sb.ToString();
    }
}
