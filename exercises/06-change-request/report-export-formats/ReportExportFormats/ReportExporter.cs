using System.Globalization;
using System.Text;

namespace ReportExportFormats;

public class ReportExporter
{
    /// <summary>The formats the UI shows in its dropdown.</summary>
    public IReadOnlyList<string> SupportedFormats => ["csv"];

    public string Export(SalesReport report, string format)
    {
        switch (format.ToLowerInvariant())
        {
            case "csv":
            {
                var sb = new StringBuilder();
                sb.Append("Product;Quantity;Revenue\n");
                foreach (var row in report.Rows)
                {
                    var product = row.Product.Contains(';') || row.Product.Contains('"')
                        ? "\"" + row.Product.Replace("\"", "\"\"") + "\""
                        : row.Product;
                    sb.Append(product).Append(';')
                      .Append(row.Quantity).Append(';')
                      .Append(row.Revenue.ToString("0.00", CultureInfo.InvariantCulture)).Append('\n');
                }
                sb.Append("TOTAL;")
                  .Append(report.TotalQuantity).Append(';')
                  .Append(report.TotalRevenue.ToString("0.00", CultureInfo.InvariantCulture)).Append('\n');
                return sb.ToString();
            }

            default:
                throw new NotSupportedException($"Format '{format}' is not supported.");
        }
    }

    public string FileNameFor(SalesReport report, string format)
    {
        var baseName = $"sales-{report.From:yyyy-MM-dd}-{report.To:yyyy-MM-dd}";
        switch (format.ToLowerInvariant())
        {
            case "csv":
                return baseName + ".csv";
            default:
                throw new NotSupportedException($"Format '{format}' is not supported.");
        }
    }

    public string ContentTypeFor(string format)
    {
        switch (format.ToLowerInvariant())
        {
            case "csv":
                return "text/csv";
            default:
                throw new NotSupportedException($"Format '{format}' is not supported.");
        }
    }
}
