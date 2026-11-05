using ProductCatalogCaching.Catalog;

namespace ProductCatalogCaching;

public record Shop(ProductPageService ProductPages, SearchPageService Search, CartService Cart);

/// <summary>
/// The composition root. The tests only use CreateShop, so you may change how things are wired.
/// TimeProvider and IActivityLog are passed in because the rest of the app already uses them.
/// </summary>
public static class Composition
{
    public static Shop CreateShop(IProductCatalog catalog, TimeProvider time, IActivityLog log) =>
        new(new ProductPageService(catalog), new SearchPageService(catalog), new CartService(catalog));
}
