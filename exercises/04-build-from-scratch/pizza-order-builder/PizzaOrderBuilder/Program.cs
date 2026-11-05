using PizzaOrderBuilder;

// A small demo. It throws NotImplementedException until you've built the builders.
var order = new OrderBuilder()
    .AddPizza(PizzaMenu.Capricciosa().WithSize(PizzaSize.Large).Build(), quantity: 2)
    .AddPizza(p => p.WithSize(PizzaSize.Medium).WithCrust(Crust.Thin).AddTopping("Tonfisk").AddTopping("Lök").WithExtraCheese())
    .DeliverTo("Storgatan 1, Örebro")
    .Build();

Console.WriteLine("🍕 Din beställning:");
foreach (var line in order.Lines)
    Console.WriteLine($"   {line.Quantity} x {line.Pizza.Size} {line.Pizza.Crust} ({string.Join(", ", line.Pizza.Toppings)}): {line.LineTotal} kr");
Console.WriteLine($"🚗 Leverans: {order.DeliveryFee} kr");
Console.WriteLine($"💰 Totalt: {order.Total} kr");
