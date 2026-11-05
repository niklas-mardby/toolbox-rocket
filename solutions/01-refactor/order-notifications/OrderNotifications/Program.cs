using OrderNotifications;

var shop = Composition.CreateShop(new Dictionary<string, int>
{
    ["HAMMER"] = 10,
    ["DRILL"] = 2,
});

var order = new Order(
    Guid.NewGuid(),
    CustomerId: "anna",
    CustomerEmail: "anna@example.com",
    Lines: [new OrderLine("HAMMER", 2, 199m), new OrderLine("DRILL", 1, 1499m)]);

shop.Orders.PlaceOrder(order);

Console.WriteLine($"🛒 Order lagd: {order.Total:0.00} kr");
Console.WriteLine($"📧 Mejl skickade: {shop.Outbox.Sent.Count}");
foreach (var mail in shop.Outbox.Sent)
    Console.WriteLine($"   till {mail.To}: {mail.Subject}");
Console.WriteLine($"⭐ Poäng för anna: {shop.Loyalty.PointsFor("anna")}");
Console.WriteLine($"📊 Ordrar idag: {shop.Statistics.OrderCount}, omsättning {shop.Statistics.Revenue:0.00} kr");
Console.WriteLine($"📦 Att plocka: {shop.Picking.Pending.Count}");
Console.WriteLine($"💬 Slack-meddelanden: {shop.Slack.Messages.Count}");
