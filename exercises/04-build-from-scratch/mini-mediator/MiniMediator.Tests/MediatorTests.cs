using Microsoft.Extensions.DependencyInjection;
using MiniMediator.Mediator;

namespace MiniMediator.Tests;

public class MediatorTests
{
    private readonly CallLog _log = new();

    private IMediator BuildMediator(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_log);
        services.AddMiniMediator(typeof(MediatorTests).Assembly);
        configure?.Invoke(services);
        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    // --- Send ---

    [Fact(Skip = Todo.Skip)]
    public async Task Send_calls_the_handler_and_returns_its_response()
    {
        var mediator = BuildMediator();

        Assert.Equal("Pong: hej", await mediator.Send(new Ping("hej")));
        Assert.Equal(5, await mediator.Send(new Add(2, 3)));
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Send_without_a_handler_throws_and_names_the_request()
    {
        var mediator = BuildMediator();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(new Orphan()));

        Assert.Contains(nameof(Orphan), ex.Message);
    }

    // --- Scanning ---

    [Fact(Skip = Todo.Skip)]
    public void Scanning_registers_handlers_as_transient_and_the_mediator()
    {
        var services = new ServiceCollection();

        services.AddMiniMediator(typeof(MediatorTests).Assembly);

        Assert.Contains(services, d => d.ServiceType == typeof(IMediator));
        var ping = Assert.Single(services, d => d.ServiceType == typeof(IRequestHandler<Ping, string>));
        Assert.Equal(typeof(PingHandler), ping.ImplementationType);
        Assert.Equal(ServiceLifetime.Transient, ping.Lifetime);
        Assert.Equal(2, services.Count(d => d.ServiceType == typeof(INotificationHandler<SomethingHappened>)));
    }

    // --- Pipeline behaviors ---

    [Fact(Skip = Todo.Skip)]
    public async Task Behaviors_wrap_the_handler_with_the_first_registered_outermost()
    {
        var mediator = BuildMediator(s => s
            .AddPipelineBehavior(typeof(OuterBehavior<,>))
            .AddPipelineBehavior(typeof(InnerBehavior<,>)));

        await mediator.Send(new Ping("hej"));

        Assert.Equal(
            new[] { "outer before", "inner before", "handler", "inner after", "outer after" },
            _log.Entries);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Open_behaviors_apply_to_every_request_type()
    {
        var mediator = BuildMediator(s => s.AddPipelineBehavior(typeof(OuterBehavior<,>)));

        await mediator.Send(new Add(1, 1));

        Assert.Equal(new[] { "outer before", "outer after" }, _log.Entries);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task A_behavior_can_short_circuit_the_handler()
    {
        var mediator = BuildMediator(s => s.AddTransient<IPipelineBehavior<Ping, string>, CachedPingBehavior>());

        var response = await mediator.Send(new Ping("hej"));

        Assert.Equal("Pong from cache", response);
        Assert.Equal(new[] { "cache hit" }, _log.Entries); // the handler never ran
    }

    [Fact(Skip = Todo.Skip)]
    public async Task A_closed_behavior_only_applies_to_its_own_request_type()
    {
        var mediator = BuildMediator(s => s.AddTransient<IPipelineBehavior<Ping, string>, CachedPingBehavior>());

        Assert.Equal(5, await mediator.Send(new Add(2, 3)));
        Assert.Empty(_log.Entries);
    }

    // --- Validation behavior ---

    [Fact(Skip = Todo.Skip)]
    public async Task Validation_collects_all_errors_and_stops_the_handler()
    {
        var mediator = BuildMediator(s => s
            .AddTransient<IValidator<Signup>, EmailValidator>()
            .AddTransient<IValidator<Signup>, AgeValidator>()
            .AddPipelineBehavior(typeof(ValidationBehavior<,>)));

        var ex = await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(new Signup("no-at-sign", 9)));

        Assert.Equal(new[] { "Email is invalid.", "You must be at least 13." }, ex.Errors);
        Assert.Empty(_log.Entries);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Valid_requests_pass_through_validation()
    {
        var mediator = BuildMediator(s => s
            .AddTransient<IValidator<Signup>, EmailValidator>()
            .AddPipelineBehavior(typeof(ValidationBehavior<,>)));

        Assert.Equal("ok", await mediator.Send(new Signup("anna@example.com", 30)));
        Assert.Equal(new[] { "handler" }, _log.Entries);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Requests_without_validators_pass_through_validation()
    {
        var mediator = BuildMediator(s => s.AddPipelineBehavior(typeof(ValidationBehavior<,>)));

        Assert.Equal("Pong: hej", await mediator.Send(new Ping("hej")));
    }

    // --- Publish ---

    [Fact(Skip = Todo.Skip)]
    public async Task Publish_calls_every_handler_in_registration_order()
    {
        var mediator = BuildMediator();

        await mediator.Publish(new SomethingHappened("boom"));

        Assert.Equal(2, _log.Entries.Count);
        Assert.Contains("first: boom", _log.Entries);
        Assert.Contains("second: boom", _log.Entries);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Publish_without_handlers_does_nothing()
    {
        var mediator = BuildMediator();

        var exception = await Record.ExceptionAsync(() => mediator.Publish(new NobodyCares()));

        Assert.Null(exception);
    }
}
