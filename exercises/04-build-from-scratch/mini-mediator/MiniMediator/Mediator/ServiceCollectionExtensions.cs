using System.Reflection;

namespace MiniMediator.Mediator;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 🛠️ YOUR JOB: register the mediator and scan the given assemblies.
    ///  - Register IMediator -> Mediator (scoped).
    ///  - For every concrete (non-abstract) class that implements IRequestHandler&lt;,&gt;
    ///    or INotificationHandler&lt;&gt;, register it as TRANSIENT under each such interface it implements.
    ///    Example: PlaceOrderHandler : IRequestHandler&lt;PlaceOrder, int&gt;
    ///      -> services.AddTransient(typeof(IRequestHandler&lt;PlaceOrder, int&gt;), typeof(PlaceOrderHandler))
    /// </summary>
    public static IServiceCollection AddMiniMediator(this IServiceCollection services, params Assembly[] assemblies) =>
        throw new NotImplementedException();

    /// <summary>✅ Finished. Registers an OPEN generic behavior, e.g. typeof(LoggingBehavior&lt;,&gt;).</summary>
    public static IServiceCollection AddPipelineBehavior(this IServiceCollection services, Type openBehaviorType)
    {
        if (!openBehaviorType.IsGenericTypeDefinition)
            throw new ArgumentException("Pass an open generic type, like typeof(LoggingBehavior<,>).", nameof(openBehaviorType));

        return services.AddTransient(typeof(IPipelineBehavior<,>), openBehaviorType);
    }
}
