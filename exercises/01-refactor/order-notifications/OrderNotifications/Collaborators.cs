namespace OrderNotifications;

// Simple in-memory stand-ins for real systems. You don't need to change these.

public record SentEmail(string To, string Subject, string Body);

public class EmailOutbox
{
    private readonly List<SentEmail> _sent = [];
    public IReadOnlyList<SentEmail> Sent => _sent;

    public void Send(string to, string subject, string body) => _sent.Add(new SentEmail(to, subject, body));
}

public class InsufficientStockException(string sku) : Exception($"Not enough stock for {sku}.")
{
    public string Sku { get; } = sku;
}

public class Inventory(Dictionary<string, int> initialStock)
{
    private readonly Dictionary<string, int> _stock = new(initialStock);

    public int StockOf(string sku) => _stock.GetValueOrDefault(sku);

    /// <summary>Reserves all lines, or none of them if any line lacks stock.</summary>
    public void ReserveAll(IEnumerable<OrderLine> lines)
    {
        var list = lines.ToList();
        foreach (var line in list)
        {
            if (StockOf(line.Sku) < line.Quantity)
                throw new InsufficientStockException(line.Sku);
        }

        foreach (var line in list)
            _stock[line.Sku] -= line.Quantity;
    }
}

public class LoyaltyProgram
{
    private readonly Dictionary<string, int> _points = [];

    public int PointsFor(string customerId) => _points.GetValueOrDefault(customerId);

    public void AddPoints(string customerId, int points) =>
        _points[customerId] = PointsFor(customerId) + points;
}

public class SalesStatistics
{
    public int OrderCount { get; private set; }
    public decimal Revenue { get; private set; }

    public void Record(Order order)
    {
        OrderCount++;
        Revenue += order.Total;
    }
}

public class PickingQueue
{
    private readonly Queue<Guid> _queue = new();
    public IReadOnlyCollection<Guid> Pending => _queue;

    public void Enqueue(Order order) => _queue.Enqueue(order.Id);
}

/// <summary>Used in the bonus task. Nothing uses it yet.</summary>
public class SlackChannel
{
    private readonly List<string> _messages = [];
    public IReadOnlyList<string> Messages => _messages;

    public void Post(string message) => _messages.Add(message);
}
