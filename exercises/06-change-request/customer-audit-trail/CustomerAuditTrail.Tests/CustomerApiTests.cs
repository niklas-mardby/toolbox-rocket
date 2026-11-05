using System.Net;
using System.Net.Http.Json;

namespace CustomerAuditTrail.Tests;

// Existing behaviour. Must stay green throughout. Don't change these tests.
public class CustomerApiTests : ApiTestBase
{
    [Fact]
    public async Task Creating_a_customer_returns_201()
    {
        var response = await Client().PostAsJsonAsync("/customers", new { name = " Bo Berg ", email = "bo@example.com" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<Customer>();
        Assert.Equal(new Customer(customer!.Id, "Bo Berg", "bo@example.com", true), customer);
        Assert.Equal($"/customers/{customer.Id}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Invalid_new_customer_returns_400_with_all_errors()
    {
        var response = await Client().PostAsJsonAsync("/customers", new { name = "", email = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(new[] { "Name is required.", "Email is invalid." }, (await response.Content.ReadFromJsonAsync<ErrorBody>())!.Errors);
    }

    [Fact]
    public async Task Customers_can_be_listed_and_fetched()
    {
        var bo = await CreateCustomer();
        var cilla = await CreateCustomer("Cilla", "cilla@example.com");

        var all = await Client().GetFromJsonAsync<List<Customer>>("/customers");
        var one = await Client().GetFromJsonAsync<Customer>($"/customers/{cilla.Id}");

        Assert.Equal(new[] { bo.Id, cilla.Id }, all!.Select(c => c.Id));
        Assert.Equal("Cilla", one!.Name);
    }

    [Fact]
    public async Task Missing_customer_returns_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await Client().GetAsync("/customers/999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client().PutAsJsonAsync("/customers/999/email", new { email = "x@y.se" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client().PostAsync("/customers/999/deactivate", null)).StatusCode);
    }

    [Fact]
    public async Task Email_can_be_changed()
    {
        var bo = await CreateCustomer();

        var response = await Client().PutAsJsonAsync($"/customers/{bo.Id}/email", new { email = "bo.berg@example.com" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("bo.berg@example.com", (await Client().GetFromJsonAsync<Customer>($"/customers/{bo.Id}"))!.Email);
    }

    [Fact]
    public async Task Customers_can_be_deactivated_and_reactivated()
    {
        var bo = await CreateCustomer();

        Assert.Equal(HttpStatusCode.NoContent, (await Client().PostAsync($"/customers/{bo.Id}/deactivate", null)).StatusCode);
        Assert.False((await Client().GetFromJsonAsync<Customer>($"/customers/{bo.Id}"))!.Active);

        Assert.Equal(HttpStatusCode.NoContent, (await Client().PostAsync($"/customers/{bo.Id}/reactivate", null)).StatusCode);
        Assert.True((await Client().GetFromJsonAsync<Customer>($"/customers/{bo.Id}"))!.Active);
    }

    [Fact]
    public async Task Deactivating_twice_is_a_conflict()
    {
        var bo = await CreateCustomer();
        await Client().PostAsync($"/customers/{bo.Id}/deactivate", null);

        Assert.Equal(HttpStatusCode.Conflict, (await Client().PostAsync($"/customers/{bo.Id}/deactivate", null)).StatusCode);
    }
}
