using EnterpriseTodoApi.Domain;
using EnterpriseTodoApi.Infrastructure;

namespace EnterpriseTodoApi.Application;

// ===================== DTOs =====================

public record CreateTodoRequestDto(string? Title);
public record TodoResponseDto(int Id, string Title, bool IsDone, DateTimeOffset CreatedAt);
public record ErrorResponseDto(IReadOnlyList<string> Errors);

// ===================== Mapping =====================

public interface IMapper<in TFrom, out TTo>
{
    TTo Map(TFrom source);
}

public class TodoItemToResponseDtoMapper : IMapper<TodoItem, TodoResponseDto>
{
    public TodoResponseDto Map(TodoItem source) => new(source.Id, source.Title, source.IsDone, source.CreatedAt);
}

public interface IMapperFactory
{
    IMapper<TFrom, TTo> GetMapper<TFrom, TTo>();
}

public class MapperFactory(IServiceProvider services) : IMapperFactory
{
    public IMapper<TFrom, TTo> GetMapper<TFrom, TTo>() => services.GetRequiredService<IMapper<TFrom, TTo>>();
}

// ===================== Validation =====================

public interface IValidator<in T>
{
    IReadOnlyList<string> Validate(T instance);
}

public class CreateTodoCommandValidator : IValidator<CreateTodoCommand>
{
    public const int MaxTitleLength = 100;

    public IReadOnlyList<string> Validate(CreateTodoCommand instance)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(instance.Title))
            errors.Add("Title is required.");
        else if (instance.Title.Trim().Length > MaxTitleLength)
            errors.Add($"Title can be at most {MaxTitleLength} characters.");
        return errors;
    }
}

public interface IValidatorFactory
{
    IValidator<T>? GetValidatorFor<T>();
}

public class ValidatorFactory(IServiceProvider services) : IValidatorFactory
{
    public IValidator<T>? GetValidatorFor<T>() => services.GetService<IValidator<T>>();
}

// ===================== Mediator =====================

public interface IRequest<TResponse>;

public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request);
}

public interface IMediator
{
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request) where TRequest : IRequest<TResponse>;
}

public class Mediator(IServiceProvider services) : IMediator
{
    public Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request) where TRequest : IRequest<TResponse> =>
        services.GetRequiredService<IRequestHandler<TRequest, TResponse>>().HandleAsync(request);
}

/// <summary>Template Method: every handler validates, executes and logs in the same way.</summary>
public abstract class RequestHandlerBase<TRequest, TResponse>(IValidatorFactory validators, ILogger logger)
    : IRequestHandler<TRequest, Result<TResponse>>
    where TRequest : IRequest<Result<TResponse>>
{
    public async Task<Result<TResponse>> HandleAsync(TRequest request)
    {
        logger.LogInformation("Handling {Request}", typeof(TRequest).Name);

        var validator = validators.GetValidatorFor<TRequest>();
        var errors = validator?.Validate(request) ?? [];
        if (errors.Count > 0)
            return Result<TResponse>.ValidationFailed(errors);

        var result = await ExecuteAsync(request);
        logger.LogInformation("Handled {Request}: {Success}", typeof(TRequest).Name, result.IsSuccess);
        return result;
    }

    protected abstract Task<Result<TResponse>> ExecuteAsync(TRequest request);
}

// ===================== Commands and queries =====================

public record CreateTodoCommand(string? Title) : IRequest<Result<TodoResponseDto>>;
public record CompleteTodoCommand(int Id) : IRequest<Result<Unit>>;
public record DeleteTodoCommand(int Id) : IRequest<Result<Unit>>;
public record GetTodosQuery : IRequest<Result<IReadOnlyList<TodoResponseDto>>>;
public record GetTodoByIdQuery(int Id) : IRequest<Result<TodoResponseDto>>;

public class CreateTodoCommandHandler(
    IRepository<TodoItem> repository,
    IUnitOfWork unitOfWork,
    ITodoItemBuilderFactory builderFactory,
    IMapperFactory mappers,
    TimeProvider time,
    IValidatorFactory validators,
    ILogger<CreateTodoCommandHandler> logger)
    : RequestHandlerBase<CreateTodoCommand, TodoResponseDto>(validators, logger)
{
    protected override async Task<Result<TodoResponseDto>> ExecuteAsync(CreateTodoCommand request)
    {
        var item = builderFactory.Create()
            .WithTitle(request.Title!.Trim())
            .WithCreatedAt(time.GetUtcNow())
            .Build();

        await repository.AddAsync(item);
        await unitOfWork.CommitAsync();

        return Result<TodoResponseDto>.Success(mappers.GetMapper<TodoItem, TodoResponseDto>().Map(item));
    }
}

public class CompleteTodoCommandHandler(
    IRepository<TodoItem> repository,
    IUnitOfWork unitOfWork,
    IValidatorFactory validators,
    ILogger<CompleteTodoCommandHandler> logger)
    : RequestHandlerBase<CompleteTodoCommand, Unit>(validators, logger)
{
    protected override async Task<Result<Unit>> ExecuteAsync(CompleteTodoCommand request)
    {
        var item = await repository.GetByIdAsync(request.Id);
        if (item is null) return Result<Unit>.NotFound();

        item.IsDone = true;
        await repository.UpdateAsync(item);
        await unitOfWork.CommitAsync();
        return Result<Unit>.Success(Unit.Value);
    }
}

public class DeleteTodoCommandHandler(
    IRepository<TodoItem> repository,
    IUnitOfWork unitOfWork,
    IValidatorFactory validators,
    ILogger<DeleteTodoCommandHandler> logger)
    : RequestHandlerBase<DeleteTodoCommand, Unit>(validators, logger)
{
    protected override async Task<Result<Unit>> ExecuteAsync(DeleteTodoCommand request)
    {
        var item = await repository.GetByIdAsync(request.Id);
        if (item is null) return Result<Unit>.NotFound();

        await repository.DeleteAsync(item);
        await unitOfWork.CommitAsync();
        return Result<Unit>.Success(Unit.Value);
    }
}

public class GetTodosQueryHandler(
    IRepository<TodoItem> repository,
    IMapperFactory mappers,
    IValidatorFactory validators,
    ILogger<GetTodosQueryHandler> logger)
    : RequestHandlerBase<GetTodosQuery, IReadOnlyList<TodoResponseDto>>(validators, logger)
{
    protected override async Task<Result<IReadOnlyList<TodoResponseDto>>> ExecuteAsync(GetTodosQuery request)
    {
        var mapper = mappers.GetMapper<TodoItem, TodoResponseDto>();
        var items = await repository.GetAllAsync();
        return Result<IReadOnlyList<TodoResponseDto>>.Success(items.Select(mapper.Map).ToList());
    }
}

public class GetTodoByIdQueryHandler(
    IRepository<TodoItem> repository,
    IMapperFactory mappers,
    IValidatorFactory validators,
    ILogger<GetTodoByIdQueryHandler> logger)
    : RequestHandlerBase<GetTodoByIdQuery, TodoResponseDto>(validators, logger)
{
    protected override async Task<Result<TodoResponseDto>> ExecuteAsync(GetTodoByIdQuery request)
    {
        var item = await repository.GetByIdAsync(request.Id);
        return item is null
            ? Result<TodoResponseDto>.NotFound()
            : Result<TodoResponseDto>.Success(mappers.GetMapper<TodoItem, TodoResponseDto>().Map(item));
    }
}

// ===================== Result -> HTTP =====================

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess) => result.ErrorType switch
    {
        ErrorType.None => onSuccess(result.Value!),
        ErrorType.Validation => Results.BadRequest(new ErrorResponseDto(result.Errors)),
        ErrorType.NotFound => Results.NotFound(),
        _ => Results.StatusCode(500)
    };
}
