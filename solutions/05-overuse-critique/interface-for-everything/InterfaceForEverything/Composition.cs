namespace InterfaceForEverything;

/// <summary>
/// The composition root. With two dependencies there's no need for a container here.
/// (In an ASP.NET Core app you'd register exactly these two in builder.Services.)
/// </summary>
public static class Composition
{
    public static BirthdayReportService CreateReportService(TimeProvider time) =>
        new(new CrmCustomerRepository(), time);
}
