namespace ProductCatalogCaching.Catalog;

// ⚠️ OWNED BY THE CATALOG TEAM. This folder is a shared package. You may NOT change it.

public record Product(string Sku, string Name, decimal Price, int Stock);

public interface IProductCatalog
{
    /// <summary>The product, or null if the SKU doesn't exist.</summary>
    Task<Product?> GetProductAsync(string sku);

    Task<IReadOnlyList<Product>> SearchAsync(string term);
}

/// <summary>Talks to the catalog team's PIM system over HTTP. Slow: typically 300-900 ms per call.</summary>
public class RemoteProductCatalog : IProductCatalog
{
    private static readonly Product[] Products =
    [
        new("H1", "Hammare", 199m, 12),
        new("S1", "Såg", 349m, 3),
        new("K1", "Kapsåg", 4995m, 8),
        new("B1", "Borrmaskin", 1499m, 0),
    ];

    public async Task<Product?> GetProductAsync(string sku)
    {
        await Task.Delay(Random.Shared.Next(300, 900));
        return Products.FirstOrDefault(p => p.Sku == sku);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string term)
    {
        await Task.Delay(Random.Shared.Next(300, 900));
        return Products.Where(p => p.Name.Contains(term.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
