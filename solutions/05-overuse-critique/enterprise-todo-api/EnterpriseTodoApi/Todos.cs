namespace EnterpriseTodoApi;

public record Todo(int Id, string Title, bool IsDone, DateTimeOffset CreatedAt);

public record ErrorResponse(IReadOnlyList<string> Errors);

public record CreateTodoRequest(string? Title)
{
    public const int MaxTitleLength = 100;

    public static IReadOnlyList<string> Validate(CreateTodoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return ["Title is required."];
        if (request.Title.Trim().Length > MaxTitleLength)
            return [$"Title can be at most {MaxTitleLength} characters."];
        return [];
    }
}

/// <summary>
/// The whole "data layer". Thread-safe, because it's a singleton serving concurrent requests.
/// If this moves to a database one day, this is the one class that changes (it becomes a DbContext).
/// </summary>
public class TodoStore(TimeProvider time)
{
    private readonly Lock _lock = new();
    private readonly List<Todo> _todos = [];
    private int _nextId = 1;

    public IReadOnlyList<Todo> All()
    {
        lock (_lock) return _todos.ToList();
    }

    public Todo? Find(int id)
    {
        lock (_lock) return _todos.FirstOrDefault(t => t.Id == id);
    }

    public Todo Add(string title)
    {
        lock (_lock)
        {
            var todo = new Todo(_nextId++, title, false, time.GetUtcNow());
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

    public bool Delete(int id)
    {
        lock (_lock) return _todos.RemoveAll(t => t.Id == id) > 0;
    }
}
