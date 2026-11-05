namespace MysteryCodebase;

public abstract class Printout
{
    public void Print()
    {
        var title = Title();
        Console.WriteLine($"   ┌─ {title} {new string('─', Math.Max(0, 36 - title.Length))}");
        foreach (var line in Lines())
            Console.WriteLine($"   │ {line}");
        Console.WriteLine($"   └{new string('─', 40)}");
    }

    protected abstract string Title();
    protected abstract IEnumerable<string> Lines();
}

public class TopListPrintout(Scoreboard scoreboard) : Printout
{
    protected override string Title() => "Topplista";

    protected override IEnumerable<string> Lines() =>
        scoreboard.Top().Select((t, i) => $"{i + 1}. {t.Title}: {PriceDecorator.Pretty(t.Revenue)}");
}

public class DiaryPrintout(List<string> pages) : Printout
{
    protected override string Title() => "Platsdagbok";
    protected override IEnumerable<string> Lines() => pages;
}

// Despite the name...
public static class PriceDecorator
{
    public static string Pretty(decimal amount) => $"💰 {amount:0} kr";
}
