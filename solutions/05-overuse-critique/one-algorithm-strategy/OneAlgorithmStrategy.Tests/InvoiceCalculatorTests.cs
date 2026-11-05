namespace OneAlgorithmStrategy.Tests;

// These tests must stay green before AND after your simplification. Don't change them.
public class InvoiceCalculatorTests
{
    private readonly InvoiceCalculator _calculator = new();

    [Fact]
    public void Calculates_net_vat_and_gross()
    {
        var totals = _calculator.CalculateTotals(
        [
            new InvoiceLine("Hammare", 2, 199m),
            new InvoiceLine("Skruv", 3, 59.90m),
        ]);

        Assert.Equal(new InvoiceTotals(577.70m, 144.43m, 722.13m), totals);
    }

    [Fact]
    public void Vat_is_rounded_half_away_from_zero()
    {
        var totals = _calculator.CalculateTotals([new InvoiceLine("Gem", 1, 0.10m)]); // VAT 0.025

        Assert.Equal(0.03m, totals.Vat);
    }

    [Fact]
    public void Net_is_rounded_to_two_decimals()
    {
        var totals = _calculator.CalculateTotals([new InvoiceLine("Bult", 3, 0.335m)]); // 1.005

        Assert.Equal(1.01m, totals.Net);
    }

    [Fact]
    public void Empty_invoice_is_zero()
    {
        Assert.Equal(new InvoiceTotals(0m, 0m, 0m), _calculator.CalculateTotals([]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Non_positive_quantity_is_rejected(int quantity)
    {
        Assert.Throws<ArgumentException>(() => _calculator.CalculateTotals([new InvoiceLine("Fel", quantity, 10m)]));
    }
}
