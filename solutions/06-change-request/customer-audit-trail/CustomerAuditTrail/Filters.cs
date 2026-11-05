namespace CustomerAuditTrail;

// Cross-cutting concerns as ASP.NET Core ENDPOINT FILTERS: a chain of responsibility around
// each endpoint. Each filter does one thing, and either calls next or short-circuits.

/// <summary>Writes require an X-User header. Short-circuits with 401 otherwise.</summary>
public class RequireUserFilter : IEndpointFilter
{
    public const string Header = "X-User";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var user = context.HttpContext.Request.Headers[Header].ToString();
        if (string.IsNullOrWhiteSpace(user))
            return Results.Unauthorized();

        return await next(context);
    }
}

/// <summary>Lets the endpoint run, then writes ONE audit entry based on the result's status code.</summary>
public class AuditFilter(string action, AuditLog audit, TimeProvider time) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);

        var http = context.HttpContext;
        var status = (result as IStatusCodeHttpResult)?.StatusCode ?? StatusCodes.Status200OK;
        var outcome = status switch
        {
            >= 200 and < 300 => "ok",
            StatusCodes.Status400BadRequest => "invalid",
            StatusCodes.Status404NotFound => "not-found",
            StatusCodes.Status409Conflict => "conflict",
            _ => $"status-{status}"
        };

        audit.Write(new AuditEntry(
            http.Request.Headers[RequireUserFilter.Header].ToString(),
            action,
            http.Request.Path,
            outcome,
            time.GetUtcNow()));

        return result;
    }
}

/// <summary>Validates the request body of type T with every registered IValidator&lt;T&gt;.</summary>
public class ValidationFilter<T>(IEnumerable<IValidator<T>> validators) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<T>().FirstOrDefault();
        if (request is not null)
        {
            var errors = validators.SelectMany(v => v.Validate(request)).ToList();
            if (errors.Count > 0)
                return Results.BadRequest(new ErrorResponse(errors));
        }

        return await next(context);
    }
}

public static class EndpointFilterExtensions
{
    /// <summary>Adds audit + (optional) validation for a write endpoint, in that order (audit outermost).</summary>
    public static RouteHandlerBuilder Audited(this RouteHandlerBuilder endpoint, string action) =>
        endpoint.AddEndpointFilterFactory((factory, next) =>
        {
            var services = factory.ApplicationServices;
            var filter = new AuditFilter(action, services.GetRequiredService<AuditLog>(), services.GetRequiredService<TimeProvider>());
            return context => filter.InvokeAsync(context, next);
        });

    public static RouteHandlerBuilder Validated<T>(this RouteHandlerBuilder endpoint) =>
        endpoint.AddEndpointFilter<ValidationFilter<T>>();
}
