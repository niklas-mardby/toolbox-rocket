using ProductCatalogCaching.Catalog;

namespace ProductCatalogCaching;

public record Shop(ProductPageService ProductPages, SearchPageService Search, CartService Cart);

public static class Composition
{
    public static Shop CreateShop(IProductCatalog catalog, TimeProvider time, IActivityLog log)
    {
        // Outside in: cache -> timing -> the real (remote) catalog.
        // Timing sits INSIDE the cache, so it only measures and logs real remote calls.
        IProductCatalog decorated = new CachingProductCatalog(new TimingProductCatalog(catalog, time, log), time);

        // ONE decorated instance shared by all three services, so they share the cache.
        return new Shop(new ProductPageService(decorated), new SearchPageService(decorated), new CartService(decorated));
    }
}
