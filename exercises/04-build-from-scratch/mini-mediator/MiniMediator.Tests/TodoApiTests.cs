using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MiniMediator.Features;

namespace MiniMediator.Tests;

// End-to-end: the todo API works once your mediator, scanning and validation behavior work.
public class TodoApiTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public TodoApiTests() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();

    [Fact(Skip = Todo.Skip)]
    public async Task Creating_a_todo_returns_201_and_shows_up_in_the_list_and_activity_feed()
    {
        var response = await _client.PostAsJsonAsync("/todos", new CreateTodo("  Köp mjölk "));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todos = await _client.GetFromJsonAsync<List<TodoItem>>("/todos");
        Assert.Equal("Köp mjölk", Assert.Single(todos!).Title);
        Assert.Equal(new[] { "Created: Köp mjölk" }, await _client.GetFromJsonAsync<List<string>>("/activity"));
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Invalid_todo_returns_400_with_errors()
    {
        var response = await _client.PostAsJsonAsync("/todos", new CreateTodo(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        Assert.Equal(new[] { "Title is required." }, body!.Errors);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Completing_a_todo_hides_it_from_open_todos()
    {
        var created = await (await _client.PostAsJsonAsync("/todos", new CreateTodo("Diska"))).Content.ReadFromJsonAsync<TodoItem>();
        await _client.PostAsJsonAsync("/todos", new CreateTodo("Damma"));

        var complete = await _client.PostAsync($"/todos/{created!.Id}/complete", null);
        var open = await _client.GetFromJsonAsync<List<TodoItem>>("/todos?onlyOpen=true");

        Assert.Equal(HttpStatusCode.NoContent, complete.StatusCode);
        Assert.Equal(new[] { "Damma" }, open!.Select(t => t.Title));
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Completing_a_missing_todo_returns_404()
    {
        var response = await _client.PostAsync("/todos/999/complete", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record ErrorBody(List<string> Errors);
}
