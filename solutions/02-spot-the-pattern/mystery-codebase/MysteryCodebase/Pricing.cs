namespace MysteryCodebase;

public record Visitor(string Name, int Age, bool HasStudentCard, string Phone);

public interface ITariff
{
    string Label { get; }
    decimal PriceFor(Show show);
}

public class FullTariff : ITariff
{
    public string Label => "Vuxen";
    public decimal PriceFor(Show show) => show.BasePrice;
}

public class ChildTariff : ITariff
{
    public string Label => "Barn";
    public decimal PriceFor(Show show) => Math.Round(show.BasePrice * 0.5m, 0);
}

public class StudentTariff : ITariff
{
    public string Label => "Student";
    public decimal PriceFor(Show show) => Math.Max(show.BasePrice - 50m, 0m);
}

public class SeniorTariff : ITariff
{
    public string Label => "Pensionär";
    public decimal PriceFor(Show show) =>
        show.Date.DayOfWeek is DayOfWeek.Tuesday or DayOfWeek.Wednesday
            ? Math.Round(show.BasePrice * 0.6m, 0)
            : Math.Round(show.BasePrice * 0.8m, 0);
}

public static class TariffDesk
{
    public static ITariff Pick(Visitor visitor) => visitor switch
    {
        { Age: < 12 } => new ChildTariff(),
        { Age: >= 65 } => new SeniorTariff(),
        { HasStudentCard: true } => new StudentTariff(),
        _ => new FullTariff()
    };
}
