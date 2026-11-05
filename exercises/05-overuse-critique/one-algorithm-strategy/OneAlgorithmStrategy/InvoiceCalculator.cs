namespace OneAlgorithmStrategy;

/// <summary>The class the rest of the app (and the tests) use.</summary>
public class InvoiceCalculator
{
    public InvoiceTotals CalculateTotals(IEnumerable<InvoiceLine> lines)
    {
        var rounding = new TwoDecimalsAwayFromZeroRoundingStrategy();
        var rateProvider = new SwedishStandardVatRateProvider();

        var context = new InvoiceCalculationContext();
        context.SetLineTotalStrategy(new QuantityTimesUnitPriceLineTotalCalculationStrategy());
        context.SetRoundingStrategy(rounding);
        context.SetVatStrategy(VatCalculationStrategyFactory.Create(VatCalculationStrategyType.Percentage, rateProvider, rounding));

        return context.Execute(lines);
    }
}
