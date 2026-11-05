namespace MiniMediator.Mediator;

/// <summary>Runs all validators for TRequest and stops the pipeline if anything is wrong.</summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var errors = validators.SelectMany(v => v.Validate(request)).ToList();
        if (errors.Count > 0)
            throw new ValidationException(errors);   // short-circuit: next() is never called

        return next();
    }
}

/// <summary>An example behavior: logs every request and how long it took.</summary>
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
