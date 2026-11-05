using System.Globalization;

namespace MultiTenantPricing;

/// <summary>One cart line while it's being priced. Line rules change Total.</summary>
public class PricedLine(Product product, int quantity, Customer customer, DateTimeOffset at)
{
    public Product Product { get; } = product;
    public int Quantity { get; } = quantity;
    public Customer Customer { get; } = customer;
    public DateTimeOffset At { get; } = at;
    public decimal Total { get; set; } = product.ListPrice * quantity;
}

/// <summary>Strategy for a rule that works on one line. Returns true if it changed the line.</summary>
public interface ILinePriceRule
{
    string Name { get; }
    bool Apply(PricedLine line);
}

/// <summary>Strategy for a rule that works on the whole order. Returns a fee, or null.</summary>
public interface IOrderPriceRule
{
    string Name { get; }
    QuoteFee? Apply(decimal linesTotal);
}

internal static class Num
{
    /// <summary>10 -> "10", 7.5 -> "7.5" (no trailing zeros, always a dot).</summary>
    public static string Format(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    public static decimal Off(decimal amount, decimal percent) => amount * (100m - percent) / 100m;
}

// --- Line rules ---

public class MemberDiscountRule(decimal percent) : ILinePriceRule
{
    public string Name => $"Medlemsrabatt {Num.Format(percent)} %";

    public bool Apply(PricedLine line)
    {
        if (!line.Customer.IsMember) return false;
        line.Total = Num.Off(line.Total, percent);
        return true;
    }
}

public class QuantityBreakRule(int minQuantity, decimal percent) : ILinePriceRule
{
    public string Name => $"Mängdrabatt {Num.Format(percent)} % från {minQuantity} st";

    public bool Apply(PricedLine line)
    {
        if (line.Quantity < minQuantity) return false;
        line.Total = Num.Off(line.Total, percent);
        return true;
    }
}

public class CategoryCampaignRule(string category, decimal percent, IReadOnlyList<DayOfWeek> days) : ILinePriceRule
{
    public string Name => $"Kampanj {category} {Num.Format(percent)} %";

    public bool Apply(PricedLine line)
    {
        if (line.Product.Category != category || !days.Contains(line.At.DayOfWeek)) return false;
        line.Total = Num.Off(line.Total, percent);
        return true;
    }
}

public class BuyXPayForYRule(string sku, int x, int y) : ILinePriceRule
{
    public string Name => $"Köp {x} betala för {y} ({sku})";

    public bool Apply(PricedLine line)
    {
        var freeUnits = line.Product.Sku == sku ? line.Quantity / x * (x - y) : 0;
        if (freeUnits == 0) return false;

        var pricePerUnit = line.Total / line.Quantity;   // the CURRENT price, after earlier rules
        line.Total -= freeUnits * pricePerUnit;
        return true;
    }
}

// --- Order rules ---

public class MinimumOrderFeeRule(decimal threshold, decimal fee) : IOrderPriceRule
{
    public string Name => $"Småordertillägg {Num.Format(fee)} kr";

    public QuoteFee? Apply(decimal linesTotal) => linesTotal < threshold ? new QuoteFee("Småordertillägg", fee) : null;
}

/// <summary>Factory: config (data) -> rule (behaviour). The ONLY place that knows every rule type.</summary>
public static class PriceRuleFactory
{
    public static object Create(PriceRuleConfig config) => config switch
    {
        PercentOffForMembers c => new MemberDiscountRule(c.Percent),
        QuantityBreak c => new QuantityBreakRule(c.MinQuantity, c.Percent),
        CategoryCampaign c => new CategoryCampaignRule(c.Category, c.Percent, c.Days),
        BuyXPayForY c => new BuyXPayForYRule(c.Sku, c.X, c.Y),
        MinimumOrderFee c => new MinimumOrderFeeRule(c.Threshold, c.Fee),
        _ => throw new NotSupportedException($"Unknown price rule {config.GetType().Name}.")
    };
}
