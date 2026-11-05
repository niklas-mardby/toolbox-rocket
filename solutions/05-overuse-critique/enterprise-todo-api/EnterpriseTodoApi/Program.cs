using EnterpriseTodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<TodoStore>();

var app = builder.Build();

app.MapGet("/todos", (TodoStore store) => store.All());

app.MapGet("/todos/{id:int}", (int id, TodoStore store) =>
    store.Find(id) is { } todo ? Results.Ok(todo) : Results.NotFound());

app.MapPost("/todos", (CreateTodoRequest request, TodoStore store) =>
{
    var errors = CreateTodoRequest.Validate(request);
    if (errors.Count > 0)
        return Results.BadRequest(new ErrorResponse(errors));

    var todo = store.Add(request.Title!.Trim());
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapPost("/todos/{id:int}/complete", (int id, TodoStore store) =>
    store.Complete(id) ? Results.NoContent() : Results.NotFound());

app.MapDelete("/todos/{id:int}", (int id, TodoStore store) =>
    store.Delete(id) ? Results.NoContent() : Results.NotFound());

app.Run();

// Makes Program visible to WebApplicationFactory<Program> in the test project.
public partial class Program;
