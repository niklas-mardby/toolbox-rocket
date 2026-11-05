namespace EventPingPong;

public record OrderLine(string Sku, int Quantity, decimal UnitPrice);

public enum OrderStatus { New, Rejected, PaymentFailed, Shipped }

public enum CheckoutResult { Completed, OutOfStock, PaymentDeclined }

public class Order
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string CustomerId { get; init; }
    public required string Email { get; init; }
    public required string CardNumber { get; init; }
    public required IReadOnlyList<OrderLine> Lines { get; init; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public decimal Total => Lines.Sum(l => l.Quantity * l.UnitPrice);
}

public record SentEmail(string To, string Subject);
public record Charge(Guid OrderId, decimal Amount);
public record Shipment(Guid OrderId, string Address);
