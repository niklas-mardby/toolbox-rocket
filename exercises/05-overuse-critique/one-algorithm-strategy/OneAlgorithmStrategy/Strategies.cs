namespace OneAlgorithmStrategy;

// "We follow the open/closed principle and the Strategy pattern so the
//  invoice calculation is future-proof and fully extensible." – the original author
//
// The company only sells in Sweden. The VAT rate has been 25 % since 1990.

public interface IVatRateProvider
{
    decimal GetVatRate();
}

public class SwedishStandardVatRateProvider : IVatRateProvider
{
    public decimal GetVatRate() => 0.25m;
}

public interface IRoundingStrategy
{
    decimal Round(decimal value);
}

public class TwoDecimalsAwayFromZeroRoundingStrategy : IRoundingStrategy
{
    public decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

public interface ILineTotalCalculationStrategy
{
    decimal Calculate(InvoiceLine line);
}

public class QuantityTimesUnitPriceLineTotalCalculationStrategy : ILineTotalCalculationStrategy
{
    public decimal Calculate(InvoiceLine line)
    {
        if (line.Quantity <= 0)
            throw new ArgumentException($"Quantity must be positive for '{line.Description}'.", nameof(line));
        return line.Quantity * line.UnitPrice;
    }
}

public interface IVatCalculationStrategy
{
    decimal CalculateVat(decimal net);
}

public class PercentageVatCalculationStrategy(IVatRateProvider rateProvider, IRoundingStrategy rounding) : IVatCalculationStrategy
{
    public decimal CalculateVat(decimal net) => rounding.Round(net * rateProvider.GetVatRate());
}

public enum VatCalculationStrategyType
{
    Percentage
}

public static class VatCalculationStrategyFactory
{
    public static IVatCalculationStrategy Create(
        VatCalculationStrategyType type, IVatRateProvider rateProvider, IRoundingStrategy rounding) => type switch
    {
        VatCalculationStrategyType.Percentage => new PercentageVatCalculationStrategy(rateProvider, rounding),
        _ => throw new NotSupportedException($"Unknown VAT strategy {type}.")
    };
}

public class InvoiceCalculationContext
{
    private ILineTotalCalculationStrategy? _lineStrategy;
    private IVatCalculationStrategy? _vatStrategy;
    private IRoundingStrategy? _roundingStrategy;

    public void SetLineTotalStrategy(ILineTotalCalculationStrategy strategy) => _lineStrategy = strategy;
    public void SetVatStrategy(IVatCalculationStrategy strategy) => _vatStrategy = strategy;
    public void SetRoundingStrategy(IRoundingStrategy strategy) => _roundingStrategy = strategy;

    public InvoiceTotals Execute(IEnumerable<InvoiceLine> lines)
    {
        if (_lineStrategy is null || _vatStrategy is null || _roundingStrategy is null)
            throw new InvalidOperationException("All strategies must be set before Execute is called.");

        var net = _roundingStrategy.Round(lines.Sum(_lineStrategy.Calculate));
        var vat = _vatStrategy.CalculateVat(net);
        return new InvoiceTotals(net, vat, net + vat);
    }
}
