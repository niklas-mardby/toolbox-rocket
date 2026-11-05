using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace OrderApiPipeline.Tests;

// These tests talk to the API over HTTP, so they don't care how the code inside is structured.
// They must stay green before AND after your refactoring. Don't change them.
public class OrderApiTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();

    public void Dispose() => _factory.Dispose();

    private HttpClient ClientFor(string? customerId, string? role = null)
    {
        var client = _factory.CreateClient();
        if (customerId is not null) client.DefaultRequestHeaders.Add("X-Customer-Id", customerId);
        if (role is not null) client.DefaultRequestHeaders.Add("X-Role", role);
        return client;
    }

    private IReadOnlyList<string> AuditEntries => _factory.Services.GetRequiredService<AuditLog>().Entries;

    private static PlaceOrderRequest TwoHammers() => new([new PlaceOrderLineRequest("HAMMER", 2)]);

    private async Task<OrderDto> PlaceOrderAs(string customerId)
    {
        var response = await ClientFor(customerId).PostAsJsonAsync("/orders", TwoHammers());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    // --- Place order ---

    [Fact]
    public async Task Placing_an_order_returns_201_with_the_order()
    {
        var response = await ClientFor("anna").PostAsJsonAsync("/orders",
            new PlaceOrderRequest([new("HAMMER", 2), new("SAW", 1)]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.Equal("anna", order.CustomerId);
        Assert.Equal("Placed", order.Status);
        Assert.Equal(2 * 199m + 349m, order.Total);
        Assert.Equal($"/orders/{order.Id}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Placing_an_order_without_customer_id_returns_401()
    {
        var response = await ClientFor(null).PostAsJsonAsync("/orders", TwoHammers());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Empty(AuditEntries);
    }

    [Fact]
    public async Task Invalid_order_returns_400_with_all_errors()
    {
        var response = await ClientFor("anna").PostAsJsonAsync("/orders",
            new PlaceOrderRequest([new("HAMMER", 0), new("UNICORN", 1)]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        Assert.Equal(
            new[] { "Quantity for HAMMER must be between 1 and 100.", "Unknown product UNICORN." },
            body!.Errors);
    }

    [Fact]
    public async Task Empty_order_returns_400()
    {
        var response = await ClientFor("anna").PostAsJsonAsync("/orders", new PlaceOrderRequest([]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        Assert.Equal(new[] { "An order must have at least one line." }, body!.Errors);
    }

    // --- Queries ---

    [Fact]
    public async Task Customers_can_get_their_own_order()
    {
        var placed = await PlaceOrderAs("anna");

        var order = await ClientFor("anna").GetFromJsonAsync<OrderDto>($"/orders/{placed.Id}");

        Assert.Equal(placed.Id, order!.Id);
    }

    [Fact]
    public async Task Customers_cannot_see_other_customers_orders()
    {
        var placed = await PlaceOrderAs("anna");

        var response = await ClientFor("bo").GetAsync($"/orders/{placed.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Listing_orders_returns_only_my_orders()
    {
        var first = await PlaceOrderAs("anna");
        await PlaceOrderAs("bo");
        var second = await PlaceOrderAs("anna");

        var orders = await ClientFor("anna").GetFromJsonAsync<List<OrderDto>>("/orders");

        Assert.Equal(new[] { first.Id, second.Id }, orders!.Select(o => o.Id));
    }

    [Fact]
    public async Task Queries_without_customer_id_return_401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await ClientFor(null).GetAsync("/orders")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await ClientFor(null).GetAsync("/orders/1")).StatusCode);
    }

    // --- Cancel ---

    [Fact]
    public async Task Customers_can_cancel_their_own_order()
    {
        var placed = await PlaceOrderAs("anna");

        var response = await ClientFor("anna").PostAsync($"/orders/{placed.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var order = await ClientFor("anna").GetFromJsonAsync<OrderDto>($"/orders/{placed.Id}");
        Assert.Equal("Cancelled", order!.Status);
    }

    [Fact]
    public async Task Cancelling_someone_elses_order_returns_404()
    {
        var placed = await PlaceOrderAs("anna");

        var response = await ClientFor("bo").PostAsync($"/orders/{placed.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Cancelling_a_shipped_order_returns_409()
    {
        var placed = await PlaceOrderAs("anna");
        await ClientFor("lisa", "staff").PostAsync($"/orders/{placed.Id}/ship", null);

        var response = await ClientFor("anna").PostAsync($"/orders/{placed.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // --- Ship ---

    [Fact]
    public async Task Staff_can_ship_an_order()
    {
        var placed = await PlaceOrderAs("anna");

        var response = await ClientFor("lisa", "staff").PostAsync($"/orders/{placed.Id}/ship", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var order = await ClientFor("anna").GetFromJsonAsync<OrderDto>($"/orders/{placed.Id}");
        Assert.Equal("Shipped", order!.Status);
    }

    [Fact]
    public async Task Customers_cannot_ship_orders()
    {
        var placed = await PlaceOrderAs("anna");

        var response = await ClientFor("anna").PostAsync($"/orders/{placed.Id}/ship", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Shipping_a_missing_order_returns_404()
    {
        var response = await ClientFor("lisa", "staff").PostAsync("/orders/999/ship", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- Audit ---

    [Fact]
    public async Task Every_command_is_audited_but_queries_are_not()
    {
        var placed = await PlaceOrderAs("anna");                                            // ok
        await ClientFor("anna").PostAsJsonAsync("/orders", new PlaceOrderRequest([]));      // failed
        await ClientFor("anna").GetAsync($"/orders/{placed.Id}");                          // query: not audited
        await ClientFor("anna").GetAsync("/orders");                                        // query: not audited
        await ClientFor("anna").PostAsync($"/orders/{placed.Id}/ship", null);              // failed (not staff)
        await ClientFor("lisa", "staff").PostAsync($"/orders/{placed.Id}/ship", null);     // ok
        await ClientFor("anna").PostAsync($"/orders/{placed.Id}/cancel", null);            // failed (shipped)

        Assert.Equal(
            new[]
            {
                "anna PlaceOrder ok",
                "anna PlaceOrder failed",
                "anna ShipOrder failed",
                "lisa ShipOrder ok",
                "anna CancelOrder failed",
            },
            AuditEntries);
    }
}
