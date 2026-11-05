namespace OrderApiPipeline;

// In-memory stand-ins for a database and an audit system. You don't need to change these.

public class OrderStore
{
    private readonly Lock _lock = new();
    private readonly List<Order> _orders = [];
    private int _nextId = 1;

    public Order Add(string customerId, IReadOnlyList<OrderLine> lines, DateTimeOffset placedAt)
    {
        lock (_lock)
        {
            var order = new Order { Id = _nextId++, CustomerId = customerId, Lines = lines, PlacedAt = placedAt };
            _orders.Add(order);
            return order;
        }
    }

    public Order? Find(int id)
    {
        lock (_lock) return _orders.FirstOrDefault(o => o.Id == id);
    }

    public IReadOnlyList<Order> ForCustomer(string customerId)
    {
        lock (_lock) return _orders.Where(o => o.CustomerId == customerId).OrderBy(o => o.Id).ToList();
    }
}

public class ProductCatalog
{
    private readonly Dictionary<string, decimal> _prices = new()
    {
        ["HAMMER"] = 199m,
        ["DRILL"] = 1499m,
        ["SAW"] = 349m,
        ["SCREWS-100"] = 59m,
    };

    public bool TryGetPrice(string productId, out decimal price) => _prices.TryGetValue(productId, out price);
}

/// <summary>Compliance requires an audit entry for every operation that CHANGES something.</summary>
public class AuditLog
{
    private readonly Lock _lock = new();
    private readonly List<string> _entries = [];

    public IReadOnlyList<string> Entries
    {
        get { lock (_lock) return _entries.ToList(); }
    }

    public void Write(string entry)
    {
        lock (_lock) _entries.Add(entry);
    }
}
