namespace MultiTenantPricing;

/// <summary>
/// Knows HOW to run rules, but not WHICH rules exist or what they do.
/// The rules for a tenant come from its config, turned into objects by PriceRuleFactory.
/// </summary>
public class PricingEngine(IProductCatalog catalog, ITenantConfigStore configs)
{
    public Quote Quote(string tenantId, IReadOnlyList<CartItem> cart, Customer customer, DateTimeOffset at)
    {
        var config = configs.Get(tenantId) ?? throw new KeyNotFoundException($"Unknown tenant {tenantId}.");
        var rules = config.Rules.Select(PriceRuleFactory.Create).ToList();
        var lineRules = rules.OfType<ILinePriceRule>().ToList();
        var orderRules = rules.OfType<IOrderPriceRule>().ToList();

        var applied = new List<string>();
        void MarkApplied(string name) { if (!applied.Contains(name)) applied.Add(name); }

        // 1. Line rules, in config order, each building on the previous result.
        var lines = new List<QuoteLine>();
        foreach (var item in cart)
        {
            var line = new PricedLine(catalog.Get(item.Sku), item.Quantity, customer, at);
            foreach (var rule in lineRules)
                if (rule.Apply(line))
                    MarkApplied(rule.Name);

            lines.Add(new QuoteLine(item.Sku, item.Quantity, line.Product.ListPrice,
                Math.Round(line.Total, 2, MidpointRounding.AwayFromZero)));
        }

        // 2. Order rules, on the rounded line sum.
        var linesTotal = lines.Sum(l => l.LineTotal);
        var fees = new List<QuoteFee>();
        foreach (var rule in orderRules)
        {
            if (rule.Apply(linesTotal) is { } fee)
            {
                fees.Add(fee);
                MarkApplied(rule.Name);
            }
        }

        return new Quote(lines, fees, applied);
    }
}
