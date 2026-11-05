namespace InterfaceForEverything;

// Data: a record. No interface.
public record Customer(string Name, string Email, DateOnly BirthDate, bool IsVip);

// ✅ KEPT: this talks to the outside world (the CRM database), so it is a real seam.
public interface ICustomerRepository
{
    IReadOnlyList<Customer> GetAll();
}

/// <summary>Pretend this reads from the CRM database.</summary>
public class CrmCustomerRepository : ICustomerRepository
{
    public IReadOnlyList<Customer> GetAll() =>
    [
        new Customer("anna andersson", "anna@example.com", new DateOnly(1991, 11, 5), IsVip: true),
        new Customer("bo berg", "bo@example.com", new DateOnly(1985, 11, 8), IsVip: false),
        new Customer("cecilia carlsson", "cecilia@example.com", new DateOnly(2000, 11, 20), IsVip: false),
        new Customer("david dahl", "david@example.com", new DateOnly(1996, 2, 29), IsVip: true),
        new Customer("erik ek", "erik@example.com", new DateOnly(1970, 11, 6), IsVip: false),
    ];
}

/// <summary>
/// Two real dependencies: the customer data (outside world) and the clock (outside world).
/// Everything else is plain, pure code: private methods, a static helper and constants.
/// </summary>
public class BirthdayReportService(ICustomerRepository customers, TimeProvider time)
{
    private const int DaysAhead = 14;
    private const string Title = "🎂 Födelsedagar de närmaste 14 dagarna";

    public string BuildReport()
    {
        var today = DateOnly.FromDateTime(time.GetLocalNow().DateTime);

        var upcoming = customers.GetAll()
            .Select(c => (Customer: c, Birthday: Birthdays.Next(c.BirthDate, today)))
            .Where(u => u.Birthday.DayNumber - today.DayNumber <= DaysAhead)
            .OrderBy(u => u.Birthday)
            .ThenBy(u => u.Customer.Name)
            .Select(u => FormatLine(u.Customer, u.Birthday, today))
            .ToList();

        return string.Join('\n', [Title, .. upcoming.Count == 0 ? ["Inga födelsedagar."] : upcoming]);
    }

    private static string FormatLine(Customer customer, DateOnly birthday, DateOnly today)
    {
        var daysUntil = birthday.DayNumber - today.DayNumber;
        var when = daysUntil switch
        {
            0 => "idag",
            1 => "imorgon",
            _ => $"om {daysUntil} dagar"
        };
        var age = birthday.Year - customer.BirthDate.Year;
        return $"- {CapitalizeWords(customer.Name)} fyller {age} {when}{(customer.IsVip ? " ⭐" : "")}";
    }

    private static string CapitalizeWords(string text) =>
        string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpperInvariant(w[0]) + w[1..]));
}

/// <summary>A pure function with a tricky rule. Static, and easy to unit test directly if you want to.</summary>
public static class Birthdays
{
    public static DateOnly Next(DateOnly birthDate, DateOnly today)
    {
        var thisYear = In(birthDate, today.Year);
        return thisYear >= today ? thisYear : In(birthDate, today.Year + 1);
    }

    // People born on 29 February celebrate on 28 February in non-leap years.
    private static DateOnly In(DateOnly birthDate, int year) =>
        birthDate is { Month: 2, Day: 29 } && !DateTime.IsLeapYear(year)
            ? new DateOnly(year, 2, 28)
            : new DateOnly(year, birthDate.Month, birthDate.Day);
}
