using System.Diagnostics;

namespace OrderApiPipeline.Mediator;

// Cross-cutting concerns, written ONCE. Registered outermost first:
//   Logging -> Audit (commands only) -> Authorization -> Validation -> handler

public interface ICurrentUser
{
    string Id { get; }
    string? Role { get; }
}

public class HttpCurrentUser(IHttpContextAccessor http) : ICurrentUser
{
    public string Id => http.HttpContext!.Request.Headers["X-Customer-Id"].ToString();
    public string? Role => http.HttpContext!.Request.Headers["X-Role"].FirstOrDefault();
}

/// <summary>Requests that implement this need a role. Everything else is open to any signed-in customer.</summary>
public interface IRequireRole
{
    string RequiredRole { get; }
}

public interface IValidator<in T>
{
    IEnumerable<string> Validate(T request);
}

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUser user)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Next<TResponse> next, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("{Request} started by {User}", typeof(TRequest).Name, user.Id);
        try
        {
            var response = await next();
            logger.LogInformation("{Request} finished in {Elapsed} ms", typeof(TRequest).Name, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Request} crashed", typeof(TRequest).Name);
            throw;   // a global exception handler turns this into a 500
        }
    }
}

/// <summary>The constraint TRequest : ICommand means the container simply skips this behavior for queries.</summary>
public class AuditBehavior<TRequest, TResponse>(AuditLog audit, ICurrentUser user)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
    where TResponse : IOutcome
{
    public async Task<TResponse> Handle(TRequest request, Next<TResponse> next, CancellationToken ct)
    {
        try
        {
            var response = await next();
            audit.Write($"{user.Id} {typeof(TRequest).Name} {(response.IsSuccess ? "ok" : "failed")}");
            return response;
        }
        catch
        {
            audit.Write($"{user.Id} {typeof(TRequest).Name} failed");
            throw;
        }
    }
}

public class AuthorizationBehavior<TRequest, TResponse>(ICurrentUser user)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IOutcome<TResponse>
{
    public Task<TResponse> Handle(TRequest request, Next<TResponse> next, CancellationToken ct) =>
        request is IRequireRole r && user.Role != r.RequiredRole
            ? Task.FromResult(TResponse.Failure(Status.Forbidden, []))
            : next();
}

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IOutcome<TResponse>
{
    public Task<TResponse> Handle(TRequest request, Next<TResponse> next, CancellationToken ct)
    {
        var errors = validators.SelectMany(v => v.Validate(request)).ToList();
        return errors.Count > 0 ? Task.FromResult(TResponse.Failure(Status.Invalid, errors)) : next();
    }
}
