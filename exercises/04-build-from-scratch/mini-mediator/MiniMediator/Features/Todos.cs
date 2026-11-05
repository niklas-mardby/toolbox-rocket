using MiniMediator.Mediator;

namespace MiniMediator.Features;

// ✅ A tiny todo feature that USES the mediator. Finished. Once your mediator works, so does this.

public record TodoItem(int Id, string Title, bool IsDone);

public class TodoStore
{
    private readonly Lock _lock = new();
    private readonly List<TodoItem> _todos = [];
    private int _nextId = 1;

    public TodoItem Add(string title)
    {
        lock (_lock)
        {
            var todo = new TodoItem(_nextId++, title, false);
            _todos.Add(todo);
            return todo;
        }
    }

    public bool Complete(int id)
    {
        lock (_lock)
        {
            var index = _todos.FindIndex(t => t.Id == id);
            if (index < 0) return false;
            _todos[index] = _todos[index] with { IsDone = true };
            return true;
        }
    }

    public IReadOnlyList<TodoItem> All()
    {
        lock (_lock) return _todos.ToList();
    }
}

// --- Create ---

public record CreateTodo(string Title) : IRequest<TodoItem>;

public class CreateTodoValidator : IValidator<CreateTodo>
{
    public IEnumerable<string> Validate(CreateTodo request)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) yield return "Title is required.";
        else if (request.Title.Length > 100) yield return "Title can be at most 100 characters.";
    }
}

public class CreateTodoHandler(TodoStore store, IMediator mediator) : IRequestHandler<CreateTodo, TodoItem>
{
    public async Task<TodoItem> Handle(CreateTodo request, CancellationToken ct)
    {
        var todo = store.Add(request.Title.Trim());
        await mediator.Publish(new TodoCreated(todo), ct);
        return todo;
    }
}

// --- Complete ---

public record CompleteTodo(int Id) : IRequest<bool>;

public class CompleteTodoHandler(TodoStore store) : IRequestHandler<CompleteTodo, bool>
{
    public Task<bool> Handle(CompleteTodo request, CancellationToken ct) => Task.FromResult(store.Complete(request.Id));
}

// --- List ---

public record GetTodos(bool OnlyOpen) : IRequest<IReadOnlyList<TodoItem>>;

public class GetTodosHandler(TodoStore store) : IRequestHandler<GetTodos, IReadOnlyList<TodoItem>>
{
    public Task<IReadOnlyList<TodoItem>> Handle(GetTodos request, CancellationToken ct)
    {
        IReadOnlyList<TodoItem> todos = request.OnlyOpen ? store.All().Where(t => !t.IsDone).ToList() : store.All();
        return Task.FromResult(todos);
    }
}

// --- Notification ---

public record TodoCreated(TodoItem Todo) : INotification;

public class TodoActivityFeed
{
    private readonly List<string> _entries = [];
    public IReadOnlyList<string> Entries => _entries;
    public void Add(string entry) => _entries.Add(entry);
}

public class AddToActivityFeed(TodoActivityFeed feed) : INotificationHandler<TodoCreated>
{
    public Task Handle(TodoCreated notification, CancellationToken ct)
    {
        feed.Add($"Created: {notification.Todo.Title}");
        return Task.CompletedTask;
    }
}
