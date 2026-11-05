using System.Net;
using System.Net.Http.Json;

namespace CustomerAuditTrail.Tests;

// ✨ The new requirement. Switch these on with Todo.Skip = null. Don't change these tests.
public class AuditAcceptanceTests : ApiTestBase
{
    private static AuditEntry Entry(string action, string target, string outcome, string user = "admin-anna") =>
        new(user, action, target, outcome, FixedTime.Now);

    // --- Who made the change? ---

    [Fact(Skip = Todo.Skip)]
    public async Task Changes_without_an_X_User_header_are_rejected_with_401_and_change_nothing()
    {
        var bo = await CreateCustomer();
        var anonymous = Client(user: null);

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsJsonAsync("/customers", new { name = "Eve", email = "eve@example.com" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PutAsJsonAsync($"/customers/{bo.Id}/email", new { email = "eve@example.com" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync($"/customers/{bo.Id}/deactivate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync($"/customers/{bo.Id}/reactivate", null)).StatusCode);

        var unchanged = await Client().GetFromJsonAsync<Customer>($"/customers/{bo.Id}");
        Assert.Equal("bo@example.com", unchanged!.Email);
        Assert.True(unchanged.Active);
        Assert.Single(await Client().GetFromJsonAsync<List<Customer>>("/customers") ?? []);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Reads_do_not_need_a_user()
    {
        Assert.Equal(HttpStatusCode.OK, (await Client(user: null).GetAsync("/customers")).StatusCode);
    }

    // --- The audit trail ---

    [Fact(Skip = Todo.Skip)]
    public async Task Every_successful_change_is_audited()
    {
        var bo = await CreateCustomer();
        await Client("admin-cecilia").PutAsJsonAsync($"/customers/{bo.Id}/email", new { email = "bo.berg@example.com" });
        await Client().PostAsync($"/customers/{bo.Id}/deactivate", null);
        await Client().PostAsync($"/customers/{bo.Id}/reactivate", null);

        Assert.Equal(
            new[]
            {
                Entry("CreateCustomer", "/customers", "ok"),
                Entry("UpdateEmail", $"/customers/{bo.Id}/email", "ok", user: "admin-cecilia"),
                Entry("DeactivateCustomer", $"/customers/{bo.Id}/deactivate", "ok"),
                Entry("ReactivateCustomer", $"/customers/{bo.Id}/reactivate", "ok"),
            },
            Audit);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Failed_changes_are_audited_with_their_outcome()
    {
        var bo = await CreateCustomer();
        await Client().PostAsJsonAsync("/customers", new { name = "", email = "x" });   // invalid
        await Client().PostAsync("/customers/999/deactivate", null);                    // not found
        await Client().PostAsync($"/customers/{bo.Id}/reactivate", null);               // already active: conflict

        Assert.Equal(
            new[]
            {
                Entry("CreateCustomer", "/customers", "ok"),
                Entry("CreateCustomer", "/customers", "invalid"),
                Entry("DeactivateCustomer", "/customers/999/deactivate", "not-found"),
                Entry("ReactivateCustomer", $"/customers/{bo.Id}/reactivate", "conflict"),
            },
            Audit);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Reads_and_rejected_anonymous_requests_are_not_audited()
    {
        await Client().GetAsync("/customers");
        await Client().GetAsync("/customers/999");
        await Client(user: null).PostAsJsonAsync("/customers", new { name = "Eve", email = "eve@example.com" });

        Assert.Empty(Audit);
    }

    // --- Validation everywhere ---

    [Theory(Skip = Todo.Skip)]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("bo@localhost")]
    public async Task Changing_to_an_invalid_email_returns_400_and_changes_nothing(string email)
    {
        var bo = await CreateCustomer();

        var response = await Client().PutAsJsonAsync($"/customers/{bo.Id}/email", new { email });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(new[] { "Email is invalid." }, (await response.Content.ReadFromJsonAsync<ErrorBody>())!.Errors);
        Assert.Equal("bo@example.com", (await Client().GetFromJsonAsync<Customer>($"/customers/{bo.Id}"))!.Email);
        Assert.Equal("invalid", Audit[^1].Outcome);
    }
}
