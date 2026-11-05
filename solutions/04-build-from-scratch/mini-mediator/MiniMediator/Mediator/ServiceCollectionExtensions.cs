using System.Reflection;

namespace MiniMediator.Mediator;

public static class ServiceCollectionExtensions
{
    private static readonly Type[] HandlerInterfaces = [typeof(IRequestHandler<,>), typeof(INotificationHandler<>)];

    /// <summary>Registers the mediator and every handler found in the given assemblies.</summary>
    public static IServiceCollection AddMiniMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IMediator, Mediator>();

        var registrations =
            from type in assemblies.SelectMany(a => a.GetTypes())
            where type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
            from iface in type.GetInterfaces()
            where iface.IsGenericType && HandlerInterfaces.Contains(iface.GetGenericTypeDefinition())
            select (Service: iface, Implementation: type);

        foreach (var (service, implementation) in registrations)
            services.AddTransient(service, implementation);

        return services;
    }

    /// <summary>Registers an OPEN generic behavior, e.g. typeof(LoggingBehavior&lt;,&gt;).</summary>
    public static IServiceCollection AddPipelineBehavior(this IServiceCollection services, Type openBehaviorType)
    {
        if (!openBehaviorType.IsGenericTypeDefinition)
            throw new ArgumentException("Pass an open generic type, like typeof(LoggingBehavior<,>).", nameof(openBehaviorType));

        return services.AddTransient(typeof(IPipelineBehavior<,>), openBehaviorType);
    }
}
