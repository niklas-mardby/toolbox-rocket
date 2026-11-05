namespace ReportExportFormats;

public record SalesRow(string Product, int Quantity, decimal Revenue);

public record SalesReport(string Title, DateOnly From, DateOnly To, IReadOnlyList<SalesRow> Rows)
{
    public int TotalQuantity => Rows.Sum(r => r.Quantity);
    public decimal TotalRevenue => Rows.Sum(r => r.Revenue);
}
