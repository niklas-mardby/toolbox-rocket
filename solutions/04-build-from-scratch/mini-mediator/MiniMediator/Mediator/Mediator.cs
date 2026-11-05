namespace MiniMediator.Mediator;

/// <summary>
/// The mediator. Send() only knows TResponse at compile time, but the handler and behaviors are
/// registered for the request's RUNTIME type. So we bridge once with reflection into a fully
/// generic method, SendCore&lt;TRequest, TResponse&gt;, and everything after that is ordinary typed code.
/// </summary>
public class Mediator(IServiceProvider services) : IMediator
{
    private static readonly System.Reflection.MethodInfo SendCoreMethod =
        typeof(Mediator).GetMethod(nameof(SendCore), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

    private static readonly System.Reflection.MethodInfo PublishCoreMethod =
        typeof(Mediator).GetMethod(nameof(PublishCore), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var method = SendCoreMethod.MakeGenericMethod(request.GetType(), typeof(TResponse));
        return (Task<TResponse>)method.Invoke(this, [request, ct])!;
    }

    public Task Publish(INotification notification, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        var method = PublishCoreMethod.MakeGenericMethod(notification.GetType());
        return (Task)method.Invoke(this, [notification, ct])!;
    }

    // async on purpose: any exception (no handler, validation...) ends up INSIDE the returned Task.
    // Without async, a synchronous throw would escape through MethodInfo.Invoke wrapped in a
    // TargetInvocationException, and callers would no longer see the real exception type.
    private async Task<TResponse> SendCore<TRequest, TResponse>(TRequest request, CancellationToken ct)
        where TRequest : IRequest<TResponse>
    {
        var handler = services.GetService<IRequestHandler<TRequest, TResponse>>()
            ?? throw new InvalidOperationException(
                $"No handler registered for {typeof(TRequest).Name}. Did you forget AddMiniMediator(assembly)?");

        // The innermost step is the handler itself.
        RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle(request, ct);

        // Wrap from the LAST behavior to the FIRST, so the first registered ends up outermost.
        foreach (var behavior in services.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse())
        {
            var next = pipeline;                       // capture the current pipeline in a local
            pipeline = () => behavior.Handle(request, next, ct);
        }

        return await pipeline();
    }

    private async Task PublishCore<TNotification>(TNotification notification, CancellationToken ct)
        where TNotification : INotification
    {
        foreach (var handler in services.GetServices<INotificationHandler<TNotification>>())
            await handler.Handle(notification, ct);
    }
}
