namespace OrderApiPipeline;

public enum OrderStatus { Placed, Shipped, Cancelled }

public record OrderLine(string ProductId, int Quantity, decimal UnitPrice);

public class Order
{
    public required int Id { get; init; }
    public required string CustomerId { get; init; }
    public required IReadOnlyList<OrderLine> Lines { get; init; }
    public required DateTimeOffset PlacedAt { get; init; }
    public OrderStatus Status { get; set; } = OrderStatus.Placed;

    public decimal Total => Lines.Sum(l => l.Quantity * l.UnitPrice);
}

// --- HTTP contracts ---

public record PlaceOrderLineRequest(string ProductId, int Quantity);
public record PlaceOrderRequest(List<PlaceOrderLineRequest>? Lines);

public record OrderLineDto(string ProductId, int Quantity, decimal UnitPrice);
public record OrderDto(int Id, string CustomerId, string Status, decimal Total, List<OrderLineDto> Lines)
{
    public static OrderDto From(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Status.ToString(),
        order.Total,
        order.Lines.Select(l => new OrderLineDto(l.ProductId, l.Quantity, l.UnitPrice)).ToList());
}

public record ValidationErrorResponse(List<string> Errors);
