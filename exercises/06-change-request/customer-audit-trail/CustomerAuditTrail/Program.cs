using CustomerAuditTrail;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<CustomerStore>();
builder.Services.AddSingleton<CustomerService>();
builder.Services.AddSingleton<AuditLog>();

var app = builder.Build();

app.MapGet("/customers", (CustomerStore store) => store.All());

app.MapGet("/customers/{id:int}", (int id, CustomerStore store) =>
    store.Find(id) is { } customer ? Results.Ok(customer) : Results.NotFound());

app.MapPost("/customers", (CreateCustomerRequest request, CustomerService customers) =>
{
    var errors = new List<string>();
    if (string.IsNullOrWhiteSpace(request.Name))
        errors.Add("Name is required.");
    if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@') || !request.Email.Split('@')[^1].Contains('.'))
        errors.Add("Email is invalid.");
    if (errors.Count > 0)
        return Results.BadRequest(new ErrorResponse(errors));

    var customer = customers.Create(request.Name!, request.Email!);
    return Results.Created($"/customers/{customer.Id}", customer);
});

app.MapPut("/customers/{id:int}/email", (int id, UpdateEmailRequest request, CustomerService customers) =>
    customers.UpdateEmail(id, request.Email ?? "") switch
    {
        OperationResult.Ok => Results.NoContent(),
        _ => Results.NotFound()
    });

app.MapPost("/customers/{id:int}/deactivate", (int id, CustomerService customers) => ToHttp(customers.Deactivate(id)));

app.MapPost("/customers/{id:int}/reactivate", (int id, CustomerService customers) => ToHttp(customers.Reactivate(id)));

app.Run();

static IResult ToHttp(OperationResult result) => result switch
{
    OperationResult.Ok => Results.NoContent(),
    OperationResult.NotFound => Results.NotFound(),
    _ => Results.Conflict()
};

// Makes Program visible to WebApplicationFactory<Program> in the test project.
public partial class Program;
