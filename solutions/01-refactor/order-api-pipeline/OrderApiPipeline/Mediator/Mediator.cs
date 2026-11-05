using System.Reflection;

namespace OrderApiPipeline.Mediator;

// ===== Contracts =====

public interface IRequest<TResponse>;

/// <summary>Marks a request that CHANGES state. Only commands are audited.</summary>
public interface ICommand<TResponse> : IRequest<TResponse>;

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}

public delegate Task<TResponse> Next<TResponse>();

public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, Next<TResponse> next, CancellationToken ct);
}

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
}

// ===== Results (no HTTP in here) =====

public enum Status { Ok, Invalid, NotFound, Conflict, Forbidden }

public interface IOutcome
{
    Status Status { get; }
    IReadOnlyList<string> Errors { get; }
    bool IsSuccess => Status == Status.Ok;
}

/// <summary>Lets a generic behavior create a failed TResponse without knowing what T is.</summary>
public interface IOutcome<TSelf> : IOutcome where TSelf : IOutcome<TSelf>
{
    static abstract TSelf Failure(Status status, IReadOnlyList<string> errors);
}

public sealed record Result<T>(Status Status, T? Value, IReadOnlyList<string> Errors) : IOutcome<Result<T>>
{
    public static Result<T> Ok(T value) => new(Status.Ok, value, []);
    public static Result<T> Failure(Status status, IReadOnlyList<string> errors) => new(status, default, errors);
    public static Result<T> NotFound() => Failure(Status.NotFound, []);
    public static Result<T> Conflict(string error) => Failure(Status.Conflict, [error]);
}

public sealed record Unit
{
    public static readonly Unit Value = new();
}

// ===== The mediator =====

public class Mediator(IServiceProvider services) : IMediator
{
    private static readonly MethodInfo SendCoreMethod =
        typeof(Mediator).GetMethod(nameof(SendCore), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default) =>
        (Task<TResponse>)SendCoreMethod.MakeGenericMethod(request.GetType(), typeof(TResponse)).Invoke(this, [request, ct])!;

    private async Task<TResponse> SendCore<TRequest, TResponse>(TRequest request, CancellationToken ct)
        where TRequest : IRequest<TResponse>
    {
        var handler = services.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

        Next<TResponse> pipeline = () => handler.Handle(request, ct);
        foreach (var behavior in services.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse())
        {
            var next = pipeline;
            pipeline = () => behavior.Handle(request, next, ct);
        }

        return await pipeline();
    }
}

public static class MediatorRegistration
{
    /// <summary>Registers the mediator and every handler in the assembly.</summary>
    public static IServiceCollection AddMediator(this IServiceCollection services, Assembly assembly)
    {
        services.AddScoped<IMediator, Mediator>();

        foreach (var type in assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false }))
            foreach (var iface in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
                services.AddScoped(iface, type);

        return services;
    }
}
