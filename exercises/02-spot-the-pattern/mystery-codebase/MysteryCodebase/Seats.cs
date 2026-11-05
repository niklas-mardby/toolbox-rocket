namespace MysteryCodebase;

public record Seat(int Row, int Number)
{
    public override string ToString() => $"rad {Row} plats {Number}";
}

public interface ISeatMap
{
    bool TryHold(string showCode, IReadOnlyList<Seat> seats);
    int Free(string showCode);
}

public class SeatMap(IShowCatalog catalog) : ISeatMap
{
    private readonly Dictionary<string, HashSet<Seat>> _taken = [];

    public bool TryHold(string showCode, IReadOnlyList<Seat> seats)
    {
        var show = catalog.ByCode(showCode) ?? throw new KeyNotFoundException(showCode);
        var taken = _taken.TryGetValue(showCode, out var set) ? set : _taken[showCode] = [];

        if (seats.Any(s => s.Row < 1 || s.Row > show.Rows || s.Number < 1 || s.Number > show.SeatsPerRow))
            return false;
        if (seats.Any(taken.Contains))
            return false;

        taken.UnionWith(seats);
        return true;
    }

    public int Free(string showCode)
    {
        var show = catalog.ByCode(showCode) ?? throw new KeyNotFoundException(showCode);
        return show.Rows * show.SeatsPerRow - (_taken.TryGetValue(showCode, out var set) ? set.Count : 0);
    }
}

public class Doorman(ISeatMap inner, int maxPerHold) : ISeatMap
{
    public bool TryHold(string showCode, IReadOnlyList<Seat> seats) =>
        seats.Count <= maxPerHold && inner.TryHold(showCode, seats);

    public int Free(string showCode) => inner.Free(showCode);
}

public class Diary(ISeatMap inner, List<string> pages) : ISeatMap
{
    public bool TryHold(string showCode, IReadOnlyList<Seat> seats)
    {
        var ok = inner.TryHold(showCode, seats);
        pages.Add($"{showCode}: {string.Join(", ", seats)} -> {(ok ? "OK" : "NEJ")}");
        return ok;
    }

    public int Free(string showCode) => inner.Free(showCode);
}
