namespace MiniMediator.Mediator;

// ✅ The contracts. Finished. They look a lot like MediatR's, on purpose.

/// <summary>A request (command or query) that produces a TResponse.</summary>
public interface IRequest<TResponse>;

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}

/// <summary>Calls the next step in the pipeline: another behavior, or finally the handler.</summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>Wraps every request of a matching type. Can act before and after, or short-circuit.</summary>
public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct);
}

/// <summary>Something that happened. Any number of handlers (zero, one or many) may react.</summary>
public interface INotification;

public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken ct);
}

public interface IMediator
{
    /// <summary>Sends a request to its ONE handler, through all pipeline behaviors.</summary>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);

    /// <summary>Publishes a notification to ALL its handlers (no behaviors).</summary>
    Task Publish(INotification notification, CancellationToken ct = default);
}

// --- Validation, used by ValidationBehavior ---

public interface IValidator<in T>
{
    /// <summary>Returns all problems. Empty means valid.</summary>
    IEnumerable<string> Validate(T instance);
}

public class ValidationException(IReadOnlyList<string> errors) : Exception("Validation failed: " + string.Join("; ", errors))
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
