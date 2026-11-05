using EventPingPong;

var shop = Composition.CreateShop(
    new Dictionary<string, int> { ["HAMMER"] = 5 },
    new Dictionary<string, string> { ["anna"] = "Storgatan 1, Örebro" });

var order = new Order
{
    CustomerId = "anna",
    Email = "anna@example.com",
    CardNumber = "4111111111111111",
    Lines = [new OrderLine("HAMMER", 2, 199m)],
};

Console.WriteLine($"🛒 Resultat: {shop.Checkout.Checkout(order)}");
Console.WriteLine($"📦 Lager kvar: {shop.Inventory.StockOf("HAMMER")}");
Console.WriteLine($"🚚 Leveranser: {shop.Shipping.Shipments.Count}");
Console.WriteLine($"⭐ Poäng: {shop.Loyalty.PointsFor("anna")}");
foreach (var mail in shop.Emails.Sent)
    Console.WriteLine($"📧 {mail.To}: {mail.Subject}");
