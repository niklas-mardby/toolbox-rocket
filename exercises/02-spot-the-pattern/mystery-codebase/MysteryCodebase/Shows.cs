namespace MysteryCodebase;

public record Show(string Code, string Title, DateOnly Date, decimal BasePrice, int Rows, int SeatsPerRow);

public interface IShowCatalog
{
    Show? ByCode(string code);
    IReadOnlyList<Show> PlayingBetween(DateOnly from, DateOnly to);
    void Register(Show show);
}

public class ShowCatalog : IShowCatalog
{
    private readonly Dictionary<string, Show> _shows = [];

    public Show? ByCode(string code) => _shows.GetValueOrDefault(code);

    public IReadOnlyList<Show> PlayingBetween(DateOnly from, DateOnly to) =>
        _shows.Values.Where(s => s.Date >= from && s.Date <= to).OrderBy(s => s.Date).ToList();

    public void Register(Show show) => _shows[show.Code] = show;
}

public class OnDemandShowCatalog(Func<ShowCatalog> load) : IShowCatalog
{
    private ShowCatalog? _real;

    private ShowCatalog Real
    {
        get
        {
            if (_real is null)
            {
                Console.WriteLine("   (läser in föreställningar...)");
                _real = load();
            }
            return _real;
        }
    }

    public Show? ByCode(string code) => Real.ByCode(code);
    public IReadOnlyList<Show> PlayingBetween(DateOnly from, DateOnly to) => Real.PlayingBetween(from, to);
    public void Register(Show show) => Real.Register(show);
}
