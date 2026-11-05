using Microsoft.Extensions.DependencyInjection;

namespace SubscriptionReminders;

/// <summary>
/// The composition root. All configuration (connection string, SMTP host) lives HERE now,
/// and the built-in DI container builds the object graph (the bonus step).
/// </summary>
public static class Composition
{
    public static ReminderService CreateReminderService()
    {
        var services = new ServiceCollection();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IEmailSender>(_ => new SmtpEmailSender("smtp.toolbox-shop.se", 587));
        services.AddScoped<ICustomerRepository>(_ =>
            new SqlCustomerRepository("Server=prod-db;Database=Shop;Trusted_Connection=True"));
        services.AddScoped<ReminderService>();

        // In a real app the container lives as long as the app. In this small console demo
        // nothing is disposable, so building a provider just to resolve one service is fine.
        var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = false });
        return provider.GetRequiredService<ReminderService>();
    }
}
