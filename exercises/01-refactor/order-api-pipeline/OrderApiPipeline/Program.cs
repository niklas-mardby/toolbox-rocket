using OrderApiPipeline;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<OrderStore>();
builder.Services.AddSingleton<ProductCatalog>();
builder.Services.AddSingleton<AuditLog>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<OrderManager>();

var app = builder.Build();

app.MapPost("/orders", (HttpContext http, PlaceOrderRequest request, OrderManager orders) => orders.PlaceOrder(http, request));
app.MapGet("/orders", (HttpContext http, OrderManager orders) => orders.ListOrders(http));
app.MapGet("/orders/{id:int}", (HttpContext http, int id, OrderManager orders) => orders.GetOrder(http, id));
app.MapPost("/orders/{id:int}/cancel", (HttpContext http, int id, OrderManager orders) => orders.CancelOrder(http, id));
app.MapPost("/orders/{id:int}/ship", (HttpContext http, int id, OrderManager orders) => orders.ShipOrder(http, id));

app.Run();

// Makes Program visible to WebApplicationFactory<Program> in the test project.
public partial class Program;
