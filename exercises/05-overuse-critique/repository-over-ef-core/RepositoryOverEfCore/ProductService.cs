namespace RepositoryOverEfCore;

public class ProductService(IRepository<Product> products, IUnitOfWork unitOfWork)
{
    public IReadOnlyList<Product> GetActiveInCategory(string category) =>
        products.GetAll()
            .Where(p => p.Category == category && !p.Discontinued)
            .OrderBy(p => p.Price)
            .ToList();

    public IReadOnlyList<Product> Search(string term) =>
        products.GetAll()
            .Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Name)
            .ToList();

    public int CountLowStock(int threshold) =>
        products.GetAll().Count(p => !p.Discontinued && p.Stock < threshold);

    public Product AddProduct(string sku, string name, string category, int price)
    {
        if (products.GetAll().Any(p => p.Sku == sku))
            throw new InvalidOperationException($"SKU {sku} already exists.");

        var product = new Product { Sku = sku, Name = name, Category = category, Price = price };
        products.Add(product);
        unitOfWork.Commit();
        return product;
    }

    public void Discontinue(string sku)
    {
        var product = products.GetAll().SingleOrDefault(p => p.Sku == sku)
            ?? throw new KeyNotFoundException(sku);

        product.Discontinued = true;
        products.Update(product);
        unitOfWork.Commit();
    }

    /// <summary>Raises all prices in a category by a percentage, rounded to whole kronor.</summary>
    public void RaisePrices(string category, int percent)
    {
        foreach (var product in products.GetAll().Where(p => p.Category == category))
        {
            product.Price = (int)Math.Round(product.Price * (100 + percent) / 100m, MidpointRounding.AwayFromZero);
            products.Update(product);
        }

        unitOfWork.Commit();
    }
}
