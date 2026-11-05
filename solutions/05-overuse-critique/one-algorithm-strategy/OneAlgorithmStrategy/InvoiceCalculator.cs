namespace OneAlgorithmStrategy;

/// <summary>Swedish invoice totals: 25 % VAT, rounded to whole öre (half away from zero).</summary>
public class InvoiceCalculator
{
    private const decimal VatRate = 0.25m;

    public InvoiceTotals CalculateTotals(IEnumerable<InvoiceLine> lines)
    {
        var net = Round(lines.Sum(LineTotal));
        var vat = Round(net * VatRate);
        return new InvoiceTotals(net, vat, net + vat);
    }

    private static decimal LineTotal(InvoiceLine line)
    {
        if (line.Quantity <= 0)
            throw new ArgumentException($"Quantity must be positive for '{line.Description}'.", nameof(line));
        return line.Quantity * line.UnitPrice;
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
