using OrderApiPipeline.Mediator;

namespace OrderApiPipeline;

// One use case per section: request + (validator) + handler. Only business logic is left.

// ---------- PlaceOrder ----------

public record PlaceOrder(string CustomerId, List<PlaceOrderLineRequest>? Lines) : ICommand<Result<OrderDto>>;

public class PlaceOrderValidator(ProductCatalog catalog) : IValidator<PlaceOrder>
{
    public IEnumerable<string> Validate(PlaceOrder request)
    {
        if (request.Lines is null || request.Lines.Count == 0)
        {
            yield return "An order must have at least one line.";
            yield break;
        }

        foreach (var line in request.Lines)
        {
            if (line.Quantity is < 1 or > 100)
                yield return $"Quantity for {line.ProductId} must be between 1 and 100.";
            if (!catalog.TryGetPrice(line.ProductId, out _))
                yield return $"Unknown product {line.ProductId}.";
        }
    }
}

public class PlaceOrderHandler(OrderStore store, ProductCatalog catalog, TimeProvider time)
    : IRequestHandler<PlaceOrder, Result<OrderDto>>
{
    public Task<Result<OrderDto>> Handle(PlaceOrder request, CancellationToken ct)
    {
        var lines = request.Lines!
            .Select(l =>
            {
                catalog.TryGetPrice(l.ProductId, out var price);
                return new OrderLine(l.ProductId, l.Quantity, price);
            })
            .ToList();

        var order = store.Add(request.CustomerId, lines, time.GetUtcNow());
        return Task.FromResult(Result<OrderDto>.Ok(OrderDto.From(order)));
    }
}

// ---------- GetOrder / ListOrders (queries: never audited) ----------

public record GetOrder(string CustomerId, int OrderId) : IRequest<Result<OrderDto>>;

public class GetOrderHandler(OrderStore store) : IRequestHandler<GetOrder, Result<OrderDto>>
{
    public Task<Result<OrderDto>> Handle(GetOrder request, CancellationToken ct)
    {
        var order = store.Find(request.OrderId);
        // Customers may only see their own orders. 404 so we don't reveal that it exists.
        return Task.FromResult(order is null || order.CustomerId != request.CustomerId
            ? Result<OrderDto>.NotFound()
            : Result<OrderDto>.Ok(OrderDto.From(order)));
    }
}

public record ListOrders(string CustomerId) : IRequest<Result<List<OrderDto>>>;

public class ListOrdersHandler(OrderStore store) : IRequestHandler<ListOrders, Result<List<OrderDto>>>
{
    public Task<Result<List<OrderDto>>> Handle(ListOrders request, CancellationToken ct) =>
        Task.FromResult(Result<List<OrderDto>>.Ok(store.ForCustomer(request.CustomerId).Select(OrderDto.From).ToList()));
}

// ---------- CancelOrder ----------

public record CancelOrder(string CustomerId, int OrderId) : ICommand<Result<Unit>>;

public class CancelOrderHandler(OrderStore store) : IRequestHandler<CancelOrder, Result<Unit>>
{
    public Task<Result<Unit>> Handle(CancelOrder request, CancellationToken ct)
    {
        var order = store.Find(request.OrderId);
        if (order is null || order.CustomerId != request.CustomerId)
            return Task.FromResult(Result<Unit>.NotFound());
        if (order.Status != OrderStatus.Placed)
            return Task.FromResult(Result<Unit>.Conflict($"Order {order.Id} is {order.Status} and cannot be cancelled."));

        order.Status = OrderStatus.Cancelled;
        return Task.FromResult(Result<Unit>.Ok(Unit.Value));
    }
}

// ---------- ShipOrder (staff only) ----------

public record ShipOrder(int OrderId) : ICommand<Result<Unit>>, IRequireRole
{
    public string RequiredRole => "staff";
}

public class ShipOrderHandler(OrderStore store) : IRequestHandler<ShipOrder, Result<Unit>>
{
    public Task<Result<Unit>> Handle(ShipOrder request, CancellationToken ct)
    {
        var order = store.Find(request.OrderId);
        if (order is null)
            return Task.FromResult(Result<Unit>.NotFound());
        if (order.Status != OrderStatus.Placed)
            return Task.FromResult(Result<Unit>.Conflict($"Order {order.Id} is {order.Status} and cannot be shipped."));

        order.Status = OrderStatus.Shipped;
        return Task.FromResult(Result<Unit>.Ok(Unit.Value));
    }
}

// ---------- Result -> HTTP, in ONE place ----------

public static class ResultHttp
{
    public static IResult ToHttp<T>(this Result<T> result, Func<T, IResult> onSuccess) => result.Status switch
    {
        Status.Ok => onSuccess(result.Value!),
        Status.Invalid => Results.BadRequest(new ValidationErrorResponse(result.Errors.ToList())),
        Status.NotFound => Results.NotFound(),
        Status.Conflict => Results.Conflict(new ValidationErrorResponse(result.Errors.ToList())),
        Status.Forbidden => Results.StatusCode(StatusCodes.Status403Forbidden),
        _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
    };
}
