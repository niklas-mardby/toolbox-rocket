using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EnterpriseTodoApi.Tests;

// These tests use only HTTP and their own small records, so they don't care how the code
// inside is structured. They must stay green before AND after your simplification. Don't change them.
public class TodoApiTests : IDisposable
{
    private record TodoDto(int Id, string Title, bool IsDone, DateTimeOffset CreatedAt);
    private record ErrorBody(List<string> Errors);

    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public TodoApiTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    private async Task<TodoDto> Create(string title)
    {
        var response = await _client.PostAsJsonAsync("/todos", new { title });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoDto>())!;
    }

    [Fact]
    public async Task Creating_returns_201_with_location_and_the_todo()
    {
        var response = await _client.PostAsJsonAsync("/todos", new { title = "  Köp mjölk " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.Equal("Köp mjölk", todo!.Title);
        Assert.False(todo.IsDone);
        Assert.Equal($"/todos/{todo.Id}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Listing_returns_all_todos_in_creation_order()
    {
        await Create("Diska");
        await Create("Damma");

        var todos = await _client.GetFromJsonAsync<List<TodoDto>>("/todos");

        Assert.Equal(new[] { "Diska", "Damma" }, todos!.Select(t => t.Title));
    }

    [Fact]
    public async Task A_todo_can_be_fetched_by_id()
    {
        var created = await Create("Diska");

        var fetched = await _client.GetFromJsonAsync<TodoDto>($"/todos/{created.Id}");

        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task Missing_todo_returns_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/todos/999")).StatusCode);
    }

    [Theory]
    [InlineData("", "Title is required.")]
    [InlineData("   ", "Title is required.")]
    public async Task Invalid_title_returns_400_with_errors(string title, string expected)
    {
        var response = await _client.PostAsJsonAsync("/todos", new { title });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(new[] { expected }, (await response.Content.ReadFromJsonAsync<ErrorBody>())!.Errors);
    }

    [Fact]
    public async Task Missing_title_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/todos", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(new[] { "Title is required." }, (await response.Content.ReadFromJsonAsync<ErrorBody>())!.Errors);
    }

    [Fact]
    public async Task Too_long_title_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/todos", new { title = new string('x', 101) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(new[] { "Title can be at most 100 characters." }, (await response.Content.ReadFromJsonAsync<ErrorBody>())!.Errors);
    }

    [Fact]
    public async Task Completing_marks_the_todo_as_done()
    {
        var created = await Create("Diska");

        var response = await _client.PostAsync($"/todos/{created.Id}/complete", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True((await _client.GetFromJsonAsync<TodoDto>($"/todos/{created.Id}"))!.IsDone);
        Assert.True((await _client.GetFromJsonAsync<List<TodoDto>>("/todos"))!.Single().IsDone);
    }

    [Fact]
    public async Task Deleting_removes_the_todo()
    {
        var created = await Create("Diska");
        await Create("Damma");

        var response = await _client.DeleteAsync($"/todos/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(new[] { "Damma" }, (await _client.GetFromJsonAsync<List<TodoDto>>("/todos"))!.Select(t => t.Title));
    }

    [Fact]
    public async Task Completing_or_deleting_a_missing_todo_returns_404()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.PostAsync("/todos/999/complete", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync("/todos/999")).StatusCode);
    }
}
