namespace MysteryCodebase;

public record RefundWanted(string ShowCode, string VisitorName);
public record ShowMoved(string ShowCode, DateOnly NewDate);

public interface IOperator<in TCall>
{
    string Handle(TCall call);
}

public class Switchboard
{
    private readonly Dictionary<Type, Func<object, string>> _lines = [];

    public void Connect<TCall>(IOperator<TCall> op) => _lines[typeof(TCall)] = call => op.Handle((TCall)call);

    public string Dial(object call)
    {
        var started = DateTime.UtcNow;
        try
        {
            return _lines.TryGetValue(call.GetType(), out var line)
                ? line(call)
                : $"Ingen svarar på {call.GetType().Name}.";
        }
        finally
        {
            Console.WriteLine($"   ☎️ {call.GetType().Name} tog {(DateTime.UtcNow - started).TotalMilliseconds:0} ms");
        }
    }
}

public class RefundOperator : IOperator<RefundWanted>
{
    public string Handle(RefundWanted call) => $"Återbetalning registrerad för {call.VisitorName} ({call.ShowCode}).";
}

public class RescheduleOperator(IShowCatalog catalog) : IOperator<ShowMoved>
{
    public string Handle(ShowMoved call)
    {
        var show = catalog.ByCode(call.ShowCode);
        if (show is null) return $"Okänd föreställning {call.ShowCode}.";
        catalog.Register(show with { Date = call.NewDate });
        return $"{show.Title} flyttad till {call.NewDate:yyyy-MM-dd}.";
    }
}
