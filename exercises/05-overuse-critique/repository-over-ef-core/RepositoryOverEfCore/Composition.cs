namespace RepositoryOverEfCore;

/// <summary>
/// The composition root. The tests only call CreateProductService(db) and the public
/// methods on what it returns, so you may change everything else.
/// </summary>
public static class Composition
{
    public static ProductService CreateProductService(ShopDbContext db) =>
        new(new EfRepository<Product>(db), new EfUnitOfWork(db));
}
