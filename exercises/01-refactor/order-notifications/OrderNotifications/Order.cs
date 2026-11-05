namespace OrderNotifications;

public record OrderLine(string Sku, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => Quantity * UnitPrice;
}

public record Order(Guid Id, string CustomerId, string CustomerEmail, IReadOnlyList<OrderLine> Lines)
{
    public decimal Total => Lines.Sum(l => l.LineTotal);
}
