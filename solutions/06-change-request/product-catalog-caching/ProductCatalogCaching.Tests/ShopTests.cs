namespace ProductCatalogCaching.Tests;

// Existing behaviour. Must stay green throughout. Don't change these tests.
public class ShopTests : ShopTestBase
{
    [Fact]
    public async Task Product_page_shows_name_price_and_stock()
    {
        Assert.Equal(new ProductPage("Hammare", "199 kr", true), await Shop.ProductPages.ShowAsync("H1"));
        Assert.Equal(new ProductPage("Borrmaskin", "1499 kr", false), await Shop.ProductPages.ShowAsync("B1"));
    }

    [Fact]
    public async Task Unknown_product_page_is_null()
    {
        Assert.Null(await Shop.ProductPages.ShowAsync("NOPE"));
    }

    [Fact]
    public async Task Search_lists_matching_products_by_name()
    {
        Assert.Equal(new[] { "Kapsåg (4995 kr)", "Såg (349 kr)" }, await Shop.Search.SearchAsync("såg"));
    }

    [Fact]
    public async Task Blank_search_returns_nothing_without_asking_the_catalog()
    {
        Assert.Empty(await Shop.Search.SearchAsync("  "));
        Assert.Empty(Catalog.Calls);
    }

    [Fact]
    public async Task Cart_total_uses_catalog_prices()
    {
        Assert.Equal(2 * 199m + 349m, await Shop.Cart.TotalAsync([new CartLine("H1", 2), new CartLine("S1", 1)]));
    }

    [Fact]
    public async Task Cart_with_unknown_product_throws()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Shop.Cart.TotalAsync([new CartLine("NOPE", 1)]));
    }
}
