using ProductCatalogCaching.Catalog;

namespace ProductCatalogCaching;

// Three parts of our shop that use the catalog.

public record ProductPage(string Title, string PriceText, bool InStock);

public class ProductPageService(IProductCatalog catalog)
{
    public async Task<ProductPage?> ShowAsync(string sku)
    {
        var product = await catalog.GetProductAsync(sku);
        return product is null
            ? null
            : new ProductPage(product.Name, $"{product.Price:0} kr", product.Stock > 0);
    }
}

public class SearchPageService(IProductCatalog catalog)
{
    public async Task<IReadOnlyList<string>> SearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return [];
        var products = await catalog.SearchAsync(term);
        return products.OrderBy(p => p.Name).Select(p => $"{p.Name} ({p.Price:0} kr)").ToList();
    }
}

public record CartLine(string Sku, int Quantity);

public class CartService(IProductCatalog catalog)
{
    public async Task<decimal> TotalAsync(IEnumerable<CartLine> lines)
    {
        var total = 0m;
        foreach (var line in lines)
        {
            var product = await catalog.GetProductAsync(line.Sku)
                ?? throw new KeyNotFoundException($"Unknown product {line.Sku}.");
            total += product.Price * line.Quantity;
        }
        return total;
    }
}

// --- Things the rest of the app already has ---

public interface IActivityLog
{
    void Write(string line);
}

public class ConsoleActivityLog : IActivityLog
{
    public void Write(string line) => Console.WriteLine($"   📝 {line}");
}
