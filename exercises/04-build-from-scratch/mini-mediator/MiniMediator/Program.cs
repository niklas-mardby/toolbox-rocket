using MiniMediator.Features;
using MiniMediator.Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TodoStore>();
builder.Services.AddSingleton<TodoActivityFeed>();
builder.Services.AddTransient<IValidator<CreateTodo>, CreateTodoValidator>();

builder.Services.AddMiniMediator(typeof(Program).Assembly);
builder.Services.AddPipelineBehavior(typeof(LoggingBehavior<,>));     // outermost
builder.Services.AddPipelineBehavior(typeof(ValidationBehavior<,>));  // then validation, then the handler

var app = builder.Build();

// Turn validation failures into 400 Bad Request, wherever they come from.
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { errors = ex.Errors });
    }
});

// Thin endpoints: HTTP in, request object to the mediator, HTTP out.
app.MapPost("/todos", async (CreateTodo command, IMediator mediator) =>
{
    var todo = await mediator.Send(command);
    return Results.Created($"/todos/{todo.Id}", todo);
});

app.MapGet("/todos", (bool? onlyOpen, IMediator mediator) => mediator.Send(new GetTodos(onlyOpen ?? false)));

app.MapPost("/todos/{id:int}/complete", async (int id, IMediator mediator) =>
    await mediator.Send(new CompleteTodo(id)) ? Results.NoContent() : Results.NotFound());

app.MapGet("/activity", (TodoActivityFeed feed) => feed.Entries);

app.Run();

// Makes Program visible to WebApplicationFactory<Program> in the test project.
public partial class Program;
