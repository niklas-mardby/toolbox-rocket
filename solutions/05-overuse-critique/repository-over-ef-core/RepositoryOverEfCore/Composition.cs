namespace RepositoryOverEfCore;

public static class Composition
{
    public static ProductService CreateProductService(ShopDbContext db) => new(db);
}
