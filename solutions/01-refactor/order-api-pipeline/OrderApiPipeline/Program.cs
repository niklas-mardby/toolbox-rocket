using OrderApiPipeline;
using OrderApiPipeline.Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<OrderStore>();
builder.Services.AddSingleton<ProductCatalog>();
builder.Services.AddSingleton<AuditLog>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<IValidator<PlaceOrder>, PlaceOrderValidator>();

builder.Services.AddMediator(typeof(Program).Assembly);
// Pipeline, outermost first:
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));         // commands only (constraint)
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

// Every endpoint needs to know who's calling. Missing -> 401, before the mediator (so: not audited).
var orders = app.MapGroup("/orders").AddEndpointFilter(async (context, next) =>
    string.IsNullOrWhiteSpace(context.HttpContext.Request.Headers["X-Customer-Id"])
        ? Results.Unauthorized()
        : await next(context));

// Thin endpoints: HTTP -> request object -> mediator -> HTTP.
orders.MapPost("", async (PlaceOrderRequest body, ICurrentUser user, IMediator mediator) =>
    (await mediator.Send(new PlaceOrder(user.Id, body.Lines))).ToHttp(o => Results.Created($"/orders/{o.Id}", o)));

orders.MapGet("", async (ICurrentUser user, IMediator mediator) =>
    (await mediator.Send(new ListOrders(user.Id))).ToHttp(list => Results.Ok(list)));

orders.MapGet("/{id:int}", async (int id, ICurrentUser user, IMediator mediator) =>
    (await mediator.Send(new GetOrder(user.Id, id))).ToHttp(o => Results.Ok(o)));

orders.MapPost("/{id:int}/cancel", async (int id, ICurrentUser user, IMediator mediator) =>
    (await mediator.Send(new CancelOrder(user.Id, id))).ToHttp(_ => Results.NoContent()));

orders.MapPost("/{id:int}/ship", async (int id, IMediator mediator) =>
    (await mediator.Send(new ShipOrder(id))).ToHttp(_ => Results.NoContent()));

app.Run();

// Makes Program visible to WebApplicationFactory<Program> in the test project.
public partial class Program;
