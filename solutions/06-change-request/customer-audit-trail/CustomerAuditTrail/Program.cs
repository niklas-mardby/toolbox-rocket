using CustomerAuditTrail;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<CustomerStore>();
builder.Services.AddSingleton<CustomerService>();
builder.Services.AddSingleton<AuditLog>();
builder.Services.AddSingleton<IValidator<CreateCustomerRequest>, CreateCustomerValidator>();
builder.Services.AddSingleton<IValidator<UpdateEmailRequest>, UpdateEmailValidator>();

var app = builder.Build();

// --- Reads: open, not audited ---
app.MapGet("/customers", (CustomerStore store) => store.All());

app.MapGet("/customers/{id:int}", (int id, CustomerStore store) =>
    store.Find(id) is { } customer ? Results.Ok(customer) : Results.NotFound());

// --- Writes: every endpoint in this group requires a user. Each is audited (and validated). ---
// Filter chain, outside in:  RequireUser -> Audit -> Validation -> endpoint
var writes = app.MapGroup("/customers").AddEndpointFilter<RequireUserFilter>();

writes.MapPost("", (CreateCustomerRequest request, CustomerService customers) =>
    {
        var customer = customers.Create(request.Name!, request.Email!);
        return Results.Created($"/customers/{customer.Id}", customer);
    })
    .Audited("CreateCustomer")
    .Validated<CreateCustomerRequest>();

writes.MapPut("/{id:int}/email", (int id, UpdateEmailRequest request, CustomerService customers) =>
        ToHttp(customers.UpdateEmail(id, request.Email!)))
    .Audited("UpdateEmail")
    .Validated<UpdateEmailRequest>();

writes.MapPost("/{id:int}/deactivate", (int id, CustomerService customers) => ToHttp(customers.Deactivate(id)))
    .Audited("DeactivateCustomer");

writes.MapPost("/{id:int}/reactivate", (int id, CustomerService customers) => ToHttp(customers.Reactivate(id)))
    .Audited("ReactivateCustomer");

app.Run();

static IResult ToHttp(OperationResult result) => result switch
{
    OperationResult.Ok => Results.NoContent(),
    OperationResult.NotFound => Results.NotFound(),
    _ => Results.Conflict()
};

// Makes Program visible to WebApplicationFactory<Program> in the test project.
public partial class Program;
