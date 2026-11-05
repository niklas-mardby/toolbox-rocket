using Microsoft.EntityFrameworkCore;

namespace RepositoryOverEfCore;

public class Product
{
    public int Id { get; set; }
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public int Price { get; set; }          // whole kronor
    public int Stock { get; set; }
    public bool Discontinued { get; set; }
}

public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Product>().HasIndex(p => p.Sku).IsUnique();
        // Business rule from the finance team: nothing may cost more than 20 000 kr.
        model.Entity<Product>().ToTable(t => t.HasCheckConstraint("CK_Product_MaxPrice", "Price <= 20000"));
    }
}
