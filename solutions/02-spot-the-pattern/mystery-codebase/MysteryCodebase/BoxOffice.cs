namespace MysteryCodebase;

public class BoxOffice(ISeatMap seats)
{
    public event Action<Ticket>? Sold;

    public bool Sell(Ticket ticket)
    {
        if (!seats.TryHold(ticket.Show.Code, ticket.Seats))
            return false;

        Sold?.Invoke(ticket);
        return true;
    }
}

public class Scoreboard
{
    private readonly Dictionary<string, decimal> _revenue = [];

    public void Count(Ticket ticket) =>
        _revenue[ticket.Show.Title] = _revenue.GetValueOrDefault(ticket.Show.Title) + ticket.Total;

    public IEnumerable<(string Title, decimal Revenue)> Top() =>
        _revenue.OrderByDescending(kv => kv.Value).Select(kv => (kv.Key, kv.Value));
}
