namespace MysteryCodebase;

public record Ticket(Show Show, Visitor Visitor, IReadOnlyList<Seat> Seats, string TariffLabel, decimal Total, bool WithProgramme);

public class TicketDraft
{
    private Show? _show;
    private Visitor? _visitor;
    private readonly List<Seat> _seats = [];
    private bool _withProgramme;

    public TicketDraft For(Show show) { _show = show; return this; }
    public TicketDraft Visitor(Visitor visitor) { _visitor = visitor; return this; }
    public TicketDraft Seat(int row, int number) { _seats.Add(new Seat(row, number)); return this; }
    public TicketDraft WithProgramme() { _withProgramme = true; return this; }

    public Ticket Finish()
    {
        if (_show is null) throw new InvalidOperationException("Ingen föreställning vald.");
        if (_visitor is null) throw new InvalidOperationException("Ingen besökare vald.");
        if (_seats.Count == 0) throw new InvalidOperationException("Inga platser valda.");

        var tariff = TariffDesk.Pick(_visitor);
        var total = tariff.PriceFor(_show) * _seats.Count + (_withProgramme ? 40m : 0m);
        return new Ticket(_show, _visitor, _seats.ToList(), tariff.Label, total, _withProgramme);
    }
}
