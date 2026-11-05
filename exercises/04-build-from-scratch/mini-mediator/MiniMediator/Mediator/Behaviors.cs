namespace MiniMediator.Mediator;

/// <summary>
/// 🛠️ YOUR JOB: run ALL validators for TRequest. If any of them returns errors,
/// throw ValidationException with ALL errors (from all validators, in order) and do NOT call next.
/// No validators, or no errors -> just call next.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct) =>
        throw new NotImplementedException();
}

/// <summary>✅ Finished. An example behavior: logs every request and how long it took.</summary>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var started = TimeProvider.System.GetTimestamp();
        logger.LogInformation("Handling {Request}", typeof(TRequest).Name);
        var response = await next();
        logger.LogInformation("Handled {Request} in {Ms} ms", typeof(TRequest).Name,
            TimeProvider.System.GetElapsedTime(started).TotalMilliseconds);
        return response;
    }
}
