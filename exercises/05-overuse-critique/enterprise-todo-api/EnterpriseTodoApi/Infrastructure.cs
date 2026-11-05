using EnterpriseTodoApi.Domain;

namespace EnterpriseTodoApi.Infrastructure;

public interface IRepository<T>
{
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

public interface IUnitOfWork
{
    Task CommitAsync();
}

/// <summary>The actual storage. Everything else in this folder wraps it.</summary>
public class InMemoryTodoRepository : IRepository<TodoItem>
{
    private readonly Lock _lock = new();
    private readonly List<TodoItem> _items = [];
    private int _nextId = 1;

    public Task<IReadOnlyList<TodoItem>> GetAllAsync()
    {
        lock (_lock) return Task.FromResult<IReadOnlyList<TodoItem>>(_items.OrderBy(i => i.Id).ToList());
    }

    public Task<TodoItem?> GetByIdAsync(int id)
    {
        lock (_lock) return Task.FromResult(_items.FirstOrDefault(i => i.Id == id));
    }

    public Task AddAsync(TodoItem entity)
    {
        lock (_lock)
        {
            entity.Id = _nextId++;
            _items.Add(entity);
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TodoItem entity) => Task.CompletedTask; // in memory: already updated

    public Task DeleteAsync(TodoItem entity)
    {
        lock (_lock) _items.Remove(entity);
        return Task.CompletedTask;
    }
}

/// <summary>Decorator: logs every repository call.</summary>
public class LoggingRepositoryDecorator<T>(IRepository<T> inner, ILogger<LoggingRepositoryDecorator<T>> logger) : IRepository<T>
{
    public Task<IReadOnlyList<T>> GetAllAsync() { logger.LogDebug("GetAll {Type}", typeof(T).Name); return inner.GetAllAsync(); }
    public Task<T?> GetByIdAsync(int id) { logger.LogDebug("GetById {Type} {Id}", typeof(T).Name, id); return inner.GetByIdAsync(id); }
    public Task AddAsync(T entity) { logger.LogDebug("Add {Type}", typeof(T).Name); return inner.AddAsync(entity); }
    public Task UpdateAsync(T entity) { logger.LogDebug("Update {Type}", typeof(T).Name); return inner.UpdateAsync(entity); }
    public Task DeleteAsync(T entity) { logger.LogDebug("Delete {Type}", typeof(T).Name); return inner.DeleteAsync(entity); }
}

/// <summary>Decorator: caches GetAll (of an in-memory list...) and clears the cache on every write.</summary>
public class CachingRepositoryDecorator<T>(IRepository<T> inner) : IRepository<T>
{
    private IReadOnlyList<T>? _cache;

    public async Task<IReadOnlyList<T>> GetAllAsync() => _cache ??= await inner.GetAllAsync();
    public Task<T?> GetByIdAsync(int id) => inner.GetByIdAsync(id);
    public async Task AddAsync(T entity) { _cache = null; await inner.AddAsync(entity); }
    public async Task UpdateAsync(T entity) { _cache = null; await inner.UpdateAsync(entity); }
    public async Task DeleteAsync(T entity) { _cache = null; await inner.DeleteAsync(entity); }
}

public class InMemoryUnitOfWork : IUnitOfWork
{
    public Task CommitAsync() => Task.CompletedTask;
}
