using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RepositoryOverEfCore;

using var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();

// LogTo prints every SQL command. Watch how many SELECTs one small operation causes.
var options = new DbContextOptionsBuilder<ShopDbContext>()
    .UseSqlite(connection)
    .LogTo(message => { if (message.Contains("Executed DbCommand")) Console.WriteLine("   🛢️ SQL"); },
        Microsoft.Extensions.Logging.LogLevel.Information)
    .Options;

var quietOptions = new DbContextOptionsBuilder<ShopDbContext>().UseSqlite(connection).Options;
using (var setup = new ShopDbContext(quietOptions))
{
    setup.Database.EnsureCreated();
    setup.Products.AddRange(
        new Product { Sku = "H1", Name = "Hammare", Category = "Handverktyg", Price = 199, Stock = 12 },
        new Product { Sku = "S1", Name = "Såg", Category = "Handverktyg", Price = 349, Stock = 3 },
        new Product { Sku = "B1", Name = "Borrmaskin", Category = "Maskiner", Price = 1499, Stock = 2 });
    setup.SaveChanges();
}

using var db = new ShopDbContext(options);
var service = Composition.CreateProductService(db);

Console.WriteLine("📋 Handverktyg:");
foreach (var p in service.GetActiveInCategory("Handverktyg"))
    Console.WriteLine($"   {p.Name}: {p.Price} kr");

Console.WriteLine("📈 Höjer priserna på handverktyg med 10 %:");
service.RaisePrices("Handverktyg", 10);

Console.WriteLine("🔍 Söker efter 'såg':");
foreach (var p in service.Search("såg"))
    Console.WriteLine($"   {p.Name}");
