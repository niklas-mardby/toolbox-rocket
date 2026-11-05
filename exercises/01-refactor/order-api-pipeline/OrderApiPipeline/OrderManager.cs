using System.Diagnostics;

namespace OrderApiPipeline;

// The "god class". Five operations, and each one repeats the same steps:
// read the user from headers, log start, check permissions, validate, try/catch,
// write to the audit log, log the elapsed time... The actual business logic
// is a few lines in each method. Can you find them?
public class OrderManager(
    OrderStore store,
    ProductCatalog catalog,
    AuditLog audit,
    ILogger<OrderManager> logger,
    TimeProvider time)
{
    public IResult PlaceOrder(HttpContext http, PlaceOrderRequest request)
    {
        var sw = Stopwatch.StartNew();
        var customerId = http.Request.Headers["X-Customer-Id"].ToString();
        if (string.IsNullOrWhiteSpace(customerId))
        {
            logger.LogWarning("PlaceOrder: missing customer id");
            return Results.Unauthorized();
        }

        logger.LogInformation("PlaceOrder started by {CustomerId}", customerId);
        try
        {
            var errors = new List<string>();
            if (request.Lines is null || request.Lines.Count == 0)
            {
                errors.Add("An order must have at least one line.");
            }
            else
            {
                foreach (var line in request.Lines)
                {
                    if (line.Quantity is < 1 or > 100)
                        errors.Add($"Quantity for {line.ProductId} must be between 1 and 100.");
                    if (!catalog.TryGetPrice(line.ProductId, out _))
                        errors.Add($"Unknown product {line.ProductId}.");
                }
            }

            if (errors.Count > 0)
            {
                logger.LogWarning("PlaceOrder validation failed: {Errors}", string.Join("; ", errors));
                audit.Write($"{customerId} PlaceOrder failed");
                return Results.BadRequest(new ValidationErrorResponse(errors));
            }

            var lines = request.Lines!
                .Select(l =>
                {
                    catalog.TryGetPrice(l.ProductId, out var price);
                    return new OrderLine(l.ProductId, l.Quantity, price);
                })
                .ToList();
            var order = store.Add(customerId, lines, time.GetUtcNow());

            audit.Write($"{customerId} PlaceOrder ok");
            logger.LogInformation("PlaceOrder finished in {Elapsed} ms", sw.ElapsedMilliseconds);
            return Results.Created($"/orders/{order.Id}", OrderDto.From(order));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PlaceOrder crashed");
            audit.Write($"{customerId} PlaceOrder failed");
            return Results.Problem("Something went wrong.");
        }
    }

    public IResult GetOrder(HttpContext http, int id)
    {
        var sw = Stopwatch.StartNew();
        var customerId = http.Request.Headers["X-Customer-Id"].ToString();
        if (string.IsNullOrWhiteSpace(customerId))
        {
            logger.LogWarning("GetOrder: missing customer id");
            return Results.Unauthorized();
        }

        logger.LogInformation("GetOrder started by {CustomerId}", customerId);
        try
        {
            var order = store.Find(id);
            // Customers may only see their own orders. Answer 404 so we don't reveal that it exists.
            if (order is null || order.CustomerId != customerId)
            {
                logger.LogInformation("GetOrder finished in {Elapsed} ms", sw.ElapsedMilliseconds);
                return Results.NotFound();
            }

            logger.LogInformation("GetOrder finished in {Elapsed} ms", sw.ElapsedMilliseconds);
            return Results.Ok(OrderDto.From(order));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetOrder crashed");
            return Results.Problem("Something went wrong.");
        }
    }

    public IResult ListOrders(HttpContext http)
    {
        var sw = Stopwatch.StartNew();
        var customerId = http.Request.Headers["X-Customer-Id"].ToString();
        if (string.IsNullOrWhiteSpace(customerId))
        {
            logger.LogWarning("ListOrders: missing customer id");
            return Results.Unauthorized();
        }

        logger.LogInformation("ListOrders started by {CustomerId}", customerId);
        try
        {
            var orders = store.ForCustomer(customerId).Select(OrderDto.From).ToList();
            logger.LogInformation("ListOrders finished in {Elapsed} ms", sw.ElapsedMilliseconds);
            return Results.Ok(orders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ListOrders crashed");
            return Results.Problem("Something went wrong.");
        }
    }

    public IResult CancelOrder(HttpContext http, int id)
    {
        var sw = Stopwatch.StartNew();
        var customerId = http.Request.Headers["X-Customer-Id"].ToString();
        if (string.IsNullOrWhiteSpace(customerId))
        {
            logger.LogWarning("CancelOrder: missing customer id");
            return Results.Unauthorized();
        }

        logger.LogInformation("CancelOrder started by {CustomerId}", customerId);
        try
        {
            var order = store.Find(id);
            if (order is null || order.CustomerId != customerId)
            {
                audit.Write($"{customerId} CancelOrder failed");
                return Results.NotFound();
            }

            if (order.Status != OrderStatus.Placed)
            {
                logger.LogWarning("CancelOrder: order {Id} is {Status}", id, order.Status);
                audit.Write($"{customerId} CancelOrder failed");
                return Results.Conflict(new ValidationErrorResponse([$"Order {id} is {order.Status} and cannot be cancelled."]));
            }

            order.Status = OrderStatus.Cancelled;

            audit.Write($"{customerId} CancelOrder ok");
            logger.LogInformation("CancelOrder finished in {Elapsed} ms", sw.ElapsedMilliseconds);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CancelOrder crashed");
            audit.Write($"{customerId} CancelOrder failed");
            return Results.Problem("Something went wrong.");
        }
    }

    public IResult ShipOrder(HttpContext http, int id)
    {
        var sw = Stopwatch.StartNew();
        var customerId = http.Request.Headers["X-Customer-Id"].ToString();
        if (string.IsNullOrWhiteSpace(customerId))
        {
            logger.LogWarning("ShipOrder: missing customer id");
            return Results.Unauthorized();
        }

        // Only warehouse staff may ship orders.
        if (http.Request.Headers["X-Role"].ToString() != "staff")
        {
            logger.LogWarning("ShipOrder: {CustomerId} is not staff", customerId);
            audit.Write($"{customerId} ShipOrder failed");
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        logger.LogInformation("ShipOrder started by {CustomerId}", customerId);
        try
        {
            var order = store.Find(id);
            if (order is null)
            {
                audit.Write($"{customerId} ShipOrder failed");
                return Results.NotFound();
            }

            if (order.Status != OrderStatus.Placed)
            {
                audit.Write($"{customerId} ShipOrder failed");
                return Results.Conflict(new ValidationErrorResponse([$"Order {id} is {order.Status} and cannot be shipped."]));
            }

            order.Status = OrderStatus.Shipped;

            audit.Write($"{customerId} ShipOrder ok");
            logger.LogInformation("ShipOrder finished in {Elapsed} ms", sw.ElapsedMilliseconds);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ShipOrder crashed");
            audit.Write($"{customerId} ShipOrder failed");
            return Results.Problem("Something went wrong.");
        }
    }
}
