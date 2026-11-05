namespace InterfaceForEverything;

public record Customer(string Name, string Email, DateOnly BirthDate, bool IsVip) : ICustomer;

/// <summary>Pretend this reads from the CRM database.</summary>
public class CrmCustomerRepository : ICustomerRepository
{
    public IReadOnlyList<ICustomer> GetAll() =>
    [
        new Customer("anna andersson", "anna@example.com", new DateOnly(1991, 11, 5), IsVip: true),
        new Customer("bo berg", "bo@example.com", new DateOnly(1985, 11, 8), IsVip: false),
        new Customer("cecilia carlsson", "cecilia@example.com", new DateOnly(2000, 11, 20), IsVip: false),
        new Customer("david dahl", "david@example.com", new DateOnly(1996, 2, 29), IsVip: true),
        new Customer("erik ek", "erik@example.com", new DateOnly(1970, 11, 6), IsVip: false),
    ];
}

public class TimeProviderDateTimeWrapper(TimeProvider time) : IDateTimeWrapper
{
    public DateTime Now => time.GetLocalNow().DateTime;
}

public class StringHelper : IStringHelper
{
    public string CapitalizeWords(string text) =>
        string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpperInvariant(w[0]) + w[1..]));

    public string JoinLines(IEnumerable<string> lines) => string.Join('\n', lines);
}

public class AgeCalculator : IAgeCalculator
{
    public int AgeOnBirthday(DateOnly birthDate, DateOnly birthday) => birthday.Year - birthDate.Year;
}

public class BirthdayCalculator : IBirthdayCalculator
{
    public DateOnly NextBirthday(DateOnly birthDate, DateOnly today)
    {
        var thisYear = BirthdayIn(birthDate, today.Year);
        return thisYear >= today ? thisYear : BirthdayIn(birthDate, today.Year + 1);
    }

    // People born on 29 February celebrate on 28 February in non-leap years.
    private static DateOnly BirthdayIn(DateOnly birthDate, int year) =>
        birthDate is { Month: 2, Day: 29 } && !DateTime.IsLeapYear(year)
            ? new DateOnly(year, 2, 28)
            : new DateOnly(year, birthDate.Month, birthDate.Day);
}

public class ReportLineFormatter(IStringHelper strings) : IReportLineFormatter
{
    public string Format(ICustomer customer, int age, int daysUntil)
    {
        var when = daysUntil switch
        {
            0 => "idag",
            1 => "imorgon",
            _ => $"om {daysUntil} dagar"
        };
        return $"- {strings.CapitalizeWords(customer.Name)} fyller {age} {when}{(customer.IsVip ? " ⭐" : "")}";
    }
}

public class DefaultReportConfiguration : IReportConfiguration
{
    public int DaysAhead => 14;
    public string Title => "🎂 Födelsedagar de närmaste 14 dagarna";
    public string EmptyText => "Inga födelsedagar.";
}

public class ListFactory : IListFactory
{
    public List<T> CreateList<T>() => new();
}

public class BirthdayReportService(
    ICustomerRepository customers,
    IDateTimeWrapper dateTime,
    IStringHelper strings,
    IAgeCalculator ages,
    IBirthdayCalculator birthdays,
    IReportLineFormatter formatter,
    IReportConfiguration configuration,
    IListFactory lists) : IBirthdayReportService
{
    public string BuildReport()
    {
        var today = DateOnly.FromDateTime(dateTime.Now);
        var upcoming = lists.CreateList<(ICustomer Customer, DateOnly Birthday)>();

        foreach (var customer in customers.GetAll())
        {
            var next = birthdays.NextBirthday(customer.BirthDate, today);
            if (next.DayNumber - today.DayNumber <= configuration.DaysAhead)
                upcoming.Add((customer, next));
        }

        var lines = lists.CreateList<string>();
        lines.Add(configuration.Title);

        if (upcoming.Count == 0)
            lines.Add(configuration.EmptyText);

        foreach (var (customer, birthday) in upcoming.OrderBy(u => u.Birthday).ThenBy(u => u.Customer.Name))
            lines.Add(formatter.Format(customer, ages.AgeOnBirthday(customer.BirthDate, birthday), birthday.DayNumber - today.DayNumber));

        return strings.JoinLines(lines);
    }
}
