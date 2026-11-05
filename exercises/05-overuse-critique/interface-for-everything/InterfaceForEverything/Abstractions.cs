namespace InterfaceForEverything;

// "Every class gets an interface. Everything goes through DI. That's how you write testable,
//  SOLID code." – the original author

public interface ICustomer
{
    string Name { get; }
    string Email { get; }
    DateOnly BirthDate { get; }
    bool IsVip { get; }
}

public interface ICustomerRepository
{
    IReadOnlyList<ICustomer> GetAll();
}

public interface IDateTimeWrapper
{
    DateTime Now { get; }
}

public interface IStringHelper
{
    string CapitalizeWords(string text);
    string JoinLines(IEnumerable<string> lines);
}

public interface IAgeCalculator
{
    int AgeOnBirthday(DateOnly birthDate, DateOnly birthday);
}

public interface IBirthdayCalculator
{
    DateOnly NextBirthday(DateOnly birthDate, DateOnly today);
}

public interface IReportLineFormatter
{
    string Format(ICustomer customer, int age, int daysUntil);
}

public interface IReportConfiguration
{
    int DaysAhead { get; }
    string Title { get; }
    string EmptyText { get; }
}

public interface IListFactory
{
    List<T> CreateList<T>();
}

public interface IBirthdayReportService
{
    string BuildReport();
}
