using MultiTenantPricing;

var store = new InMemoryTenantConfigStore(
    new TenantPricingConfig("verktygsboden",
    [
        new PercentOffForMembers(10),
        new QuantityBreak(10, 5),
        new CategoryCampaign("Trädgård", 15, [DayOfWeek.Saturday, DayOfWeek.Sunday]),
    ]));

var engine = Composition.CreateEngine(store);
var saturday = new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.FromHours(1));

var quote = engine.Quote("verktygsboden",
    [new CartItem("H1", 1), new CartItem("SKRUV", 10), new CartItem("SP", 1)],
    new Customer("anna", IsMember: true),
    saturday);

foreach (var line in quote.Lines)
    Console.WriteLine($"🧾 {line.Quantity} x {line.Sku} à {line.ListPrice} kr = {line.LineTotal} kr");
foreach (var fee in quote.Fees)
    Console.WriteLine($"➕ {fee.Name}: {fee.Amount} kr");
Console.WriteLine($"🏷️ Regler: {string.Join(", ", quote.AppliedRules)}");
Console.WriteLine($"💰 Totalt: {quote.Total} kr");
