namespace MiniMediator.Mediator;

/// <summary>
/// 🛠️ YOUR JOB: the mediator itself.
///
/// Send:
///  1. Find the ONE IRequestHandler&lt;TRequest, TResponse&gt; for the request's runtime type.
///     No handler -> InvalidOperationException whose message contains the request type's name.
///  2. Find all IPipelineBehavior&lt;TRequest, TResponse&gt; (in registration order).
///  3. Chain them so the FIRST registered behavior is the OUTERMOST, and the handler is the innermost.
///
/// Publish:
///  Call every INotificationHandler&lt;TNotification&gt; for the notification's runtime type, in
///  registration order. No handlers is fine: nothing happens.
/// </summary>
public class Mediator(IServiceProvider services) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task Publish(INotification notification, CancellationToken ct = default) =>
        throw new NotImplementedException();
}
