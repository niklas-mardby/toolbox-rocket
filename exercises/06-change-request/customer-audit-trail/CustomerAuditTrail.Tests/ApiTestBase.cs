using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerAuditTrail.Tests;

public class FixedTime : TimeProvider
{
    public static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 30, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => Now;
}

public abstract class ApiTestBase : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    protected ApiTestBase()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host =>
            host.ConfigureServices(services => services.AddSingleton<TimeProvider>(new FixedTime())));
    }

    public void Dispose() => _factory.Dispose();

    /// <summary>A client that sends X-User (who is making the change), or no user at all.</summary>
    protected HttpClient Client(string? user = "admin-anna")
    {
        var client = _factory.CreateClient();
        if (user is not null) client.DefaultRequestHeaders.Add("X-User", user);
        return client;
    }

    protected IReadOnlyList<AuditEntry> Audit => _factory.Services.GetRequiredService<AuditLog>().Entries;

    protected async Task<Customer> CreateCustomer(string name = "Bo Berg", string email = "bo@example.com")
    {
        var response = await Client().PostAsJsonAsync("/customers", new { name, email });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Customer>())!;
    }

    protected record ErrorBody(List<string> Errors);
}
