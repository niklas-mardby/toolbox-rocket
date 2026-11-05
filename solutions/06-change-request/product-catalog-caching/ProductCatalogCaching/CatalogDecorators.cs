using ProductCatalogCaching.Catalog;

namespace ProductCatalogCaching;

/// <summary>Decorator: caches products for 5 minutes and searches for 1 minute.</summary>
public class CachingProductCatalog(IProductCatalog inner, TimeProvider time) : IProductCatalog
{
    private static readonly TimeSpan ProductTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SearchTtl = TimeSpan.FromMinutes(1);

    private readonly Dictionary<string, (Product Value, DateTimeOffset Expires)> _products = [];
    private readonly Dictionary<string, (IReadOnlyList<Product> Value, DateTimeOffset Expires)> _searches = [];
    private readonly Lock _lock = new();

    public async Task<Product?> GetProductAsync(string sku)
    {
        lock (_lock)
        {
            if (_products.TryGetValue(sku, out var hit) && time.GetUtcNow() < hit.Expires)
                return hit.Value;
        }

        var product = await inner.GetProductAsync(sku);
        if (product is not null)   // don't cache "not found": the product may be created any minute
        {
            lock (_lock) _products[sku] = (product, time.GetUtcNow() + ProductTtl);
        }
        return product;
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string term)
    {
        var key = term.Trim().ToLowerInvariant();
        lock (_lock)
        {
            if (_searches.TryGetValue(key, out var hit) && time.GetUtcNow() < hit.Expires)
                return hit.Value;
        }

        var result = await inner.SearchAsync(term);
        lock (_lock) _searches[key] = (result, time.GetUtcNow() + SearchTtl);
        return result;
    }
}

/// <summary>Decorator: logs every call that passes through it, with its duration.</summary>
public class TimingProductCatalog(IProductCatalog inner, TimeProvider time, IActivityLog log) : IProductCatalog
{
    private static readonly TimeSpan SlowThreshold = TimeSpan.FromMilliseconds(500);

    public Task<Product?> GetProductAsync(string sku) => Timed($"GetProduct({sku})", () => inner.GetProductAsync(sku));

    public Task<IReadOnlyList<Product>> SearchAsync(string term) => Timed($"Search({term})", () => inner.SearchAsync(term));

    private async Task<T> Timed<T>(string operation, Func<Task<T>> call)
    {
        var started = time.GetTimestamp();
        try
        {
            return await call();
        }
        finally
        {
            var elapsed = time.GetElapsedTime(started);
            var prefix = elapsed > SlowThreshold ? "SLOW " : "";
            log.Write($"{prefix}Remote {operation} {elapsed.TotalMilliseconds:0} ms");
        }
    }
}
