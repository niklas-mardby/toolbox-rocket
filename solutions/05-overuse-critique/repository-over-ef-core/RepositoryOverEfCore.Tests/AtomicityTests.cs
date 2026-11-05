using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace RepositoryOverEfCore.Tests;

// The bonus test. It FAILS with the original generic repository (each Update saved on its own,
// so the first product kept its new price) and PASSES once there is one SaveChanges.
public sealed class AtomicityTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<ShopDbContext> _options;

    public AtomicityTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<ShopDbContext>().UseSqlite(_connection).Options;

        using var db = new ShopDbContext(_options);
        db.Database.EnsureCreated();
        db.Products.AddRange(
            new Product { Sku = "B1", Name = "Borrmaskin", Category = "Maskiner", Price = 1499, Stock = 2 },
            new Product { Sku = "R1", Name = "Industrirobot", Category = "Maskiner", Price = 19000, Stock = 1 });
        db.SaveChanges();
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public void Raising_prices_is_all_or_nothing()
    {
        using (var db = new ShopDbContext(_options))
        {
            // 19 000 + 10 % = 20 900, which breaks the max-price rule in the database.
            Assert.Throws<DbUpdateException>(() => Composition.CreateProductService(db).RaisePrices("Maskiner", 10));
        }

        using var check = new ShopDbContext(_options);
        Assert.Equal(1499, check.Products.Single(p => p.Sku == "B1").Price);   // NOT 1649
    }
}
