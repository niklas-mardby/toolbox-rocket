namespace MultiTenantPricing;

// Written when Verktygsboden was our only customer. Their rules are hardcoded below.
// (The config store is injected because the platform team asked us to "prepare for it".)
public class PricingEngine(IProductCatalog catalog, ITenantConfigStore configs)
{
    public Quote Quote(string tenantId, IReadOnlyList<CartItem> cart, Customer customer, DateTimeOffset at)
    {
        if (tenantId != "verktygsboden")
            throw new NotSupportedException($"Tenant {tenantId} is not supported yet.");

        var lines = new List<QuoteLine>();
        var applied = new List<string>();

        foreach (var item in cart)
        {
            var product = catalog.Get(item.Sku);
            var total = product.ListPrice * item.Quantity;

            // Members get 10 % off
            if (customer.IsMember)
            {
                total *= 0.90m;
                if (!applied.Contains("Medlemsrabatt 10 %")) applied.Add("Medlemsrabatt 10 %");
            }

            // 10 or more of the same product: another 5 % off
            if (item.Quantity >= 10)
            {
                total *= 0.95m;
                if (!applied.Contains("Mängdrabatt 5 % från 10 st")) applied.Add("Mängdrabatt 5 % från 10 st");
            }

            // Weekend garden campaign: 15 % off
            if (product.Category == "Trädgård" && at.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                total *= 0.85m;
                if (!applied.Contains("Kampanj Trädgård 15 %")) applied.Add("Kampanj Trädgård 15 %");
            }

            lines.Add(new QuoteLine(item.Sku, item.Quantity, product.ListPrice, Math.Round(total, 2, MidpointRounding.AwayFromZero)));
        }

        return new Quote(lines, [], applied);
    }
}
