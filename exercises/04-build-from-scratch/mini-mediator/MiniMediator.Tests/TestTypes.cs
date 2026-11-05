using MiniMediator.Mediator;

namespace MiniMediator.Tests;

// Small requests, handlers and behaviors used only by the tests.

public class CallLog
{
    public List<string> Entries { get; } = [];
}

public record Ping(string Text) : IRequest<string>;

public class PingHandler(CallLog log) : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken ct)
    {
        log.Entries.Add("handler");
        return Task.FromResult($"Pong: {request.Text}");
    }
}

public record Add(int A, int B) : IRequest<int>;

public class AddHandler : IRequestHandler<Add, int>
{
    public Task<int> Handle(Add request, CancellationToken ct) => Task.FromResult(request.A + request.B);
}

/// <summary>Nobody handles this one.</summary>
public record Orphan : IRequest<string>;

public class OuterBehavior<TRequest, TResponse>(CallLog log) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        log.Entries.Add("outer before");
        var response = await next();
        log.Entries.Add("outer after");
        return response;
    }
}

public class InnerBehavior<TRequest, TResponse>(CallLog log) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        log.Entries.Add("inner before");
        var response = await next();
        log.Entries.Add("inner after");
        return response;
    }
}

/// <summary>Answers Ping requests itself, without calling the handler.</summary>
public class CachedPingBehavior(CallLog log) : IPipelineBehavior<Ping, string>
{
    public Task<string> Handle(Ping request, RequestHandlerDelegate<string> next, CancellationToken ct)
    {
        log.Entries.Add("cache hit");
        return Task.FromResult("Pong from cache");
    }
}

public record SomethingHappened(string What) : INotification;

public class FirstListener(CallLog log) : INotificationHandler<SomethingHappened>
{
    public Task Handle(SomethingHappened n, CancellationToken ct) { log.Entries.Add($"first: {n.What}"); return Task.CompletedTask; }
}

public class SecondListener(CallLog log) : INotificationHandler<SomethingHappened>
{
    public Task Handle(SomethingHappened n, CancellationToken ct) { log.Entries.Add($"second: {n.What}"); return Task.CompletedTask; }
}

public record NobodyCares : INotification;

public record Signup(string Email, int Age) : IRequest<string>;

public class SignupHandler(CallLog log) : IRequestHandler<Signup, string>
{
    public Task<string> Handle(Signup request, CancellationToken ct) { log.Entries.Add("handler"); return Task.FromResult("ok"); }
}

public class EmailValidator : IValidator<Signup>
{
    public IEnumerable<string> Validate(Signup s)
    {
        if (!s.Email.Contains('@')) yield return "Email is invalid.";
    }
}

public class AgeValidator : IValidator<Signup>
{
    public IEnumerable<string> Validate(Signup s)
    {
        if (s.Age < 13) yield return "You must be at least 13.";
    }
}
