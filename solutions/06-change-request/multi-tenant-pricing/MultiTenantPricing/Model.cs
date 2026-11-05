namespace MultiTenantPricing;

public record Product(string Sku, string Name, decimal ListPrice, string Category);

public record CartItem(string Sku, int Quantity);

public record Customer(string Id, bool IsMember);

public record QuoteLine(string Sku, int Quantity, decimal ListPrice, decimal LineTotal);

public record QuoteFee(string Name, decimal Amount);

/// <summary>AppliedRules lists the names of the rules that changed the price, in the order they were applied.</summary>
public record Quote(IReadOnlyList<QuoteLine> Lines, IReadOnlyList<QuoteFee> Fees, IReadOnlyList<string> AppliedRules)
{
    public decimal Total => Lines.Sum(l => l.LineTotal) + Fees.Sum(f => f.Amount);
}

public interface IProductCatalog
{
    Product Get(string sku);
}

public class InMemoryProductCatalog : IProductCatalog
{
    private static readonly Dictionary<string, Product> Products = new()
    {
        ["H1"] = new("H1", "Hammare", 199m, "Handverktyg"),
        ["SKRUV"] = new("SKRUV", "Skruv 100-pack", 59m, "Fästelement"),
        ["GK"] = new("GK", "Gräsklippare", 2995m, "Trädgård"),
        ["SP"] = new("SP", "Spade", 249m, "Trädgård"),
    };

    public Product Get(string sku) =>
        Products.TryGetValue(sku, out var product) ? product : throw new KeyNotFoundException($"Unknown product {sku}.");
}
