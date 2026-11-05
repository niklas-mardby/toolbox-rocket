using EnterpriseTodoApi.Application;
using EnterpriseTodoApi.Domain;
using EnterpriseTodoApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Infrastructure ---
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<InMemoryTodoRepository>();
builder.Services.AddSingleton<IRepository<TodoItem>>(sp =>
    new CachingRepositoryDecorator<TodoItem>(
        new LoggingRepositoryDecorator<TodoItem>(
            sp.GetRequiredService<InMemoryTodoRepository>(),
            sp.GetRequiredService<ILogger<LoggingRepositoryDecorator<TodoItem>>>())));
builder.Services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();

// --- Domain ---
builder.Services.AddSingleton<ITodoItemBuilderFactory, TodoItemBuilderFactory>();

// --- Application ---
builder.Services.AddSingleton<IMapper<TodoItem, TodoResponseDto>, TodoItemToResponseDtoMapper>();
builder.Services.AddScoped<IMapperFactory, MapperFactory>();
builder.Services.AddSingleton<IValidator<CreateTodoCommand>, CreateTodoCommandValidator>();
builder.Services.AddScoped<IValidatorFactory, ValidatorFactory>();
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddScoped<IRequestHandler<CreateTodoCommand, Result<TodoResponseDto>>, CreateTodoCommandHandler>();
builder.Services.AddScoped<IRequestHandler<CompleteTodoCommand, Result<Unit>>, CompleteTodoCommandHandler>();
builder.Services.AddScoped<IRequestHandler<DeleteTodoCommand, Result<Unit>>, DeleteTodoCommandHandler>();
builder.Services.AddScoped<IRequestHandler<GetTodosQuery, Result<IReadOnlyList<TodoResponseDto>>>, GetTodosQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetTodoByIdQuery, Result<TodoResponseDto>>, GetTodoByIdQueryHandler>();

var app = builder.Build();

app.MapGet("/todos", async (IMediator mediator) =>
    (await mediator.SendAsync<GetTodosQuery, Result<IReadOnlyList<TodoResponseDto>>>(new GetTodosQuery()))
        .ToHttpResult(todos => Results.Ok(todos)));

app.MapGet("/todos/{id:int}", async (int id, IMediator mediator) =>
    (await mediator.SendAsync<GetTodoByIdQuery, Result<TodoResponseDto>>(new GetTodoByIdQuery(id)))
        .ToHttpResult(todo => Results.Ok(todo)));

app.MapPost("/todos", async (CreateTodoRequestDto dto, IMediator mediator) =>
    (await mediator.SendAsync<CreateTodoCommand, Result<TodoResponseDto>>(new CreateTodoCommand(dto.Title)))
        .ToHttpResult(todo => Results.Created($"/todos/{todo.Id}", todo)));

app.MapPost("/todos/{id:int}/complete", async (int id, IMediator mediator) =>
    (await mediator.SendAsync<CompleteTodoCommand, Result<Unit>>(new CompleteTodoCommand(id)))
        .ToHttpResult(_ => Results.NoContent()));

app.MapDelete("/todos/{id:int}", async (int id, IMediator mediator) =>
    (await mediator.SendAsync<DeleteTodoCommand, Result<Unit>>(new DeleteTodoCommand(id)))
        .ToHttpResult(_ => Results.NoContent()));

app.Run();

// Makes Program visible to WebApplicationFactory<Program> in the test project.
public partial class Program;
