using Microsoft.EntityFrameworkCore;

namespace RepositoryOverEfCore;

/// <summary>
/// Uses ShopDbContext directly. DbSet&lt;Product&gt; is already a repository and the DbContext is
/// already a unit of work. Every query runs IN THE DATABASE, and every change is saved ONCE.
/// </summary>
public class ProductService(ShopDbContext db)
{
    public IReadOnlyList<Product> GetActiveInCategory(string category) =>
        db.Products
            .AsNoTracking()
            .Where(p => p.Category == category && !p.Discontinued)
            .OrderBy(p => p.Price)
            .ToList();

    public IReadOnlyList<Product> Search(string term)
    {
        var lowered = term.ToLower();
        return db.Products
            .AsNoTracking()
            .Where(p => p.Name.ToLower().Contains(lowered))   // translated to SQL: lower(...) / instr(...)
            .OrderBy(p => p.Name)
            .ToList();
    }

    public int CountLowStock(int threshold) =>
        db.Products.Count(p => !p.Discontinued && p.Stock < threshold);   // SELECT COUNT(*), not the whole table

    public Product AddProduct(string sku, string name, string category, int price)
    {
        if (db.Products.Any(p => p.Sku == sku))
            throw new InvalidOperationException($"SKU {sku} already exists.");

        var product = new Product { Sku = sku, Name = name, Category = category, Price = price };
        db.Products.Add(product);
        db.SaveChanges();   // EF Core sets product.Id here
        return product;
    }

    public void Discontinue(string sku)
    {
        var product = db.Products.SingleOrDefault(p => p.Sku == sku)
            ?? throw new KeyNotFoundException(sku);

        product.Discontinued = true;   // change tracking notices; no Update() call needed
        db.SaveChanges();
    }

    /// <summary>All prices in the category change together, or none of them do.</summary>
    public void RaisePrices(string category, int percent)
    {
        foreach (var product in db.Products.Where(p => p.Category == category))
            product.Price = (int)Math.Round(product.Price * (100 + percent) / 100m, MidpointRounding.AwayFromZero);

        db.SaveChanges();   // ONE save, ONE transaction
    }
}
