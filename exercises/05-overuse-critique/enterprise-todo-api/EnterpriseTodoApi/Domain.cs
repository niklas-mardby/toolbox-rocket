namespace EnterpriseTodoApi.Domain;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsDone { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public enum ErrorType { None, Validation, NotFound }

public class Result<T>
{
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public ErrorType ErrorType { get; private init; }
    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> ValidationFailed(IReadOnlyList<string> errors) => new() { ErrorType = ErrorType.Validation, Errors = errors };
    public static Result<T> NotFound() => new() { ErrorType = ErrorType.NotFound };
}

public record Unit
{
    public static readonly Unit Value = new();
}

// --- Builder (and a factory for the builder) ---

public interface ITodoItemBuilder
{
    ITodoItemBuilder WithTitle(string title);
    ITodoItemBuilder WithCreatedAt(DateTimeOffset createdAt);
    TodoItem Build();
}

public class TodoItemBuilder : ITodoItemBuilder
{
    private string? _title;
    private DateTimeOffset? _createdAt;

    public ITodoItemBuilder WithTitle(string title) { _title = title; return this; }
    public ITodoItemBuilder WithCreatedAt(DateTimeOffset createdAt) { _createdAt = createdAt; return this; }

    public TodoItem Build() => new()
    {
        Title = _title ?? throw new InvalidOperationException("Title is required."),
        CreatedAt = _createdAt ?? throw new InvalidOperationException("CreatedAt is required."),
    };
}

public interface ITodoItemBuilderFactory
{
    ITodoItemBuilder Create();
}

public class TodoItemBuilderFactory : ITodoItemBuilderFactory
{
    public ITodoItemBuilder Create() => new TodoItemBuilder();
}
