using ProductCatalogCaching.Catalog;

namespace ProductCatalogCaching.Tests;

/// <summary>
/// A clock you control. Both "wall clock" time (GetUtcNow) and the high-resolution
/// timestamp (GetTimestamp, used for measuring durations) move when you call Advance.
/// </summary>
public class ManualTime : TimeProvider
{
    private DateTimeOffset _now = new(2026, 11, 5, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _now;
    public override long GetTimestamp() => _now.UtcTicks;
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public void Advance(TimeSpan by) => _now += by;
}

/// <summary>A fake catalog. Counts calls, and each call "takes" Latency on the manual clock.</summary>
public class FakeCatalog(ManualTime time) : IProductCatalog
{
    private readonly Product[] _products =
    [
        new("H1", "Hammare", 199m, 12),
        new("S1", "Såg", 349m, 3),
        new("K1", "Kapsåg", 4995m, 8),
        new("B1", "Borrmaskin", 1499m, 0),
    ];

    public TimeSpan Latency { get; set; } = TimeSpan.Zero;
    public List<string> Calls { get; } = [];

    public Task<Product?> GetProductAsync(string sku)
    {
        Calls.Add($"GetProduct({sku})");
        time.Advance(Latency);
        return Task.FromResult(_products.FirstOrDefault(p => p.Sku == sku));
    }

    public Task<IReadOnlyList<Product>> SearchAsync(string term)
    {
        Calls.Add($"Search({term})");
        time.Advance(Latency);
        return Task.FromResult<IReadOnlyList<Product>>(
            _products.Where(p => p.Name.Contains(term.Trim(), StringComparison.OrdinalIgnoreCase)).ToList());
    }
}

public class RecordingLog : IActivityLog
{
    public List<string> Lines { get; } = [];
    public void Write(string line) => Lines.Add(line);
}

public abstract class ShopTestBase
{
    protected readonly ManualTime Time = new();
    protected readonly RecordingLog Log = new();
    protected readonly FakeCatalog Catalog;
    protected readonly Shop Shop;

    protected ShopTestBase()
    {
        Catalog = new FakeCatalog(Time);
        Shop = Composition.CreateShop(Catalog, Time, Log);
    }
}
