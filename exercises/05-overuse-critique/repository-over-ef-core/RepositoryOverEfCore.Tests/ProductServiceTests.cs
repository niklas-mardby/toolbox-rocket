using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace RepositoryOverEfCore.Tests;

// These tests must stay green before AND after your simplification. Don't change them.
// (You're welcome to ADD tests, though. See the bonus step in the README.)
public sealed class ProductServiceTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<ShopDbContext> _options;

    public ProductServiceTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<ShopDbContext>().UseSqlite(_connection).Options;

        using var db = NewContext();
        db.Database.EnsureCreated();
        db.Products.AddRange(
            new Product { Sku = "H1", Name = "Hammare", Category = "Handverktyg", Price = 199, Stock = 12 },
            new Product { Sku = "S1", Name = "Såg", Category = "Handverktyg", Price = 349, Stock = 3 },
            new Product { Sku = "T1", Name = "Tumstock", Category = "Handverktyg", Price = 89, Stock = 0, Discontinued = true },
            new Product { Sku = "B1", Name = "Borrmaskin", Category = "Maskiner", Price = 1499, Stock = 2 },
            new Product { Sku = "K1", Name = "Kapsåg", Category = "Maskiner", Price = 4995, Stock = 8 },
            new Product { Sku = "SH", Name = "Slägga och hammarskaft", Category = "Handverktyg", Price = 450, Stock = 4 });
        db.SaveChanges();
    }

    public void Dispose() => _connection.Dispose();

    private ShopDbContext NewContext() => new(_options);

    [Fact]
    public void Active_products_in_a_category_cheapest_first()
    {
        using var db = NewContext();

        var products = Composition.CreateProductService(db).GetActiveInCategory("Handverktyg");

        Assert.Equal(new[] { "H1", "S1", "SH" }, products.Select(p => p.Sku));
    }

    [Fact]
    public void Search_is_case_insensitive_and_sorted_by_name()
    {
        using var db = NewContext();

        var products = Composition.CreateProductService(db).Search("HAMMAR");

        Assert.Equal(new[] { "Hammare", "Slägga och hammarskaft" }, products.Select(p => p.Name));
    }

    [Fact]
    public void Counts_active_products_with_low_stock()
    {
        using var db = NewContext();

        Assert.Equal(3, Composition.CreateProductService(db).CountLowStock(threshold: 5)); // S1, B1, SH (T1 is discontinued)
    }

    [Fact]
    public void Adding_a_product_saves_it_and_gives_it_an_id()
    {
        Product added;
        using (var db = NewContext())
        {
            added = Composition.CreateProductService(db).AddProduct("M1", "Mejsel", "Handverktyg", 79);
        }

        Assert.NotEqual(0, added.Id);
        using var check = NewContext();
        Assert.Equal("Mejsel", check.Products.Single(p => p.Sku == "M1").Name);
    }

    [Fact]
    public void Duplicate_sku_is_rejected()
    {
        using var db = NewContext();

        Assert.Throws<InvalidOperationException>(() =>
            Composition.CreateProductService(db).AddProduct("H1", "Hammare 2", "Handverktyg", 249));
    }

    [Fact]
    public void Discontinuing_is_saved()
    {
        using (var db = NewContext())
        {
            Composition.CreateProductService(db).Discontinue("S1");
        }

        using var check = NewContext();
        Assert.True(check.Products.Single(p => p.Sku == "S1").Discontinued);
    }

    [Fact]
    public void Discontinuing_an_unknown_sku_throws()
    {
        using var db = NewContext();

        Assert.Throws<KeyNotFoundException>(() => Composition.CreateProductService(db).Discontinue("NOPE"));
    }

    [Fact]
    public void Raising_prices_rounds_to_whole_kronor_and_is_saved()
    {
        using (var db = NewContext())
        {
            Composition.CreateProductService(db).RaisePrices("Handverktyg", 10);
        }

        using var check = NewContext();
        var prices = check.Products.OrderBy(p => p.Sku).ToDictionary(p => p.Sku, p => p.Price);
        Assert.Equal(219, prices["H1"]);   // 218.9
        Assert.Equal(384, prices["S1"]);   // 383.9
        Assert.Equal(98, prices["T1"]);    // 97.9 (discontinued products are raised too)
        Assert.Equal(495, prices["SH"]);   // 495.0
        Assert.Equal(1499, prices["B1"]);  // other categories untouched
    }
}
