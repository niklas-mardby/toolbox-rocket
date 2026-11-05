using Microsoft.Extensions.DependencyInjection;

namespace InterfaceForEverything;

/// <summary>
/// The composition root. The tests only call CreateReportService(time) and BuildReport(),
/// so you may change everything else, including the return type, as long as it has BuildReport().
/// </summary>
public static class Composition
{
    public static IBirthdayReportService CreateReportService(TimeProvider time)
    {
        var services = new ServiceCollection();

        services.AddSingleton(time);
        services.AddSingleton<IDateTimeWrapper, TimeProviderDateTimeWrapper>();
        services.AddSingleton<ICustomerRepository, CrmCustomerRepository>();
        services.AddSingleton<IStringHelper, StringHelper>();
        services.AddSingleton<IAgeCalculator, AgeCalculator>();
        services.AddSingleton<IBirthdayCalculator, BirthdayCalculator>();
        services.AddSingleton<IReportLineFormatter, ReportLineFormatter>();
        services.AddSingleton<IReportConfiguration, DefaultReportConfiguration>();
        services.AddSingleton<IListFactory, ListFactory>();
        services.AddSingleton<IBirthdayReportService, BirthdayReportService>();

        return services.BuildServiceProvider().GetRequiredService<IBirthdayReportService>();
    }
}
