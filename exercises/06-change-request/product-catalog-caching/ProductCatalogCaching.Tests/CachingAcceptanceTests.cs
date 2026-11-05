namespace ProductCatalogCaching.Tests;

// ✨ The new requirement. Switch these on with Todo.Skip = null. Don't change these tests.
public class CachingAcceptanceTests : ShopTestBase
{
    // --- Caching ---

    [Fact(Skip = Todo.Skip)]
    public async Task Products_are_cached_for_five_minutes()
    {
        await Shop.ProductPages.ShowAsync("H1");
        Time.Advance(TimeSpan.FromMinutes(4));
        await Shop.ProductPages.ShowAsync("H1");

        Assert.Equal(new[] { "GetProduct(H1)" }, Catalog.Calls);

        Time.Advance(TimeSpan.FromMinutes(1));   // 5 minutes after the first call
        await Shop.ProductPages.ShowAsync("H1");

        Assert.Equal(2, Catalog.Calls.Count);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Product_page_and_cart_share_the_same_cache()
    {
        await Shop.ProductPages.ShowAsync("H1");
        await Shop.Cart.TotalAsync([new CartLine("H1", 1), new CartLine("S1", 1)]);
        await Shop.ProductPages.ShowAsync("S1");

        Assert.Equal(new[] { "GetProduct(H1)", "GetProduct(S1)" }, Catalog.Calls);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Unknown_products_are_not_cached()
    {
        await Shop.ProductPages.ShowAsync("NEW");
        await Shop.ProductPages.ShowAsync("NEW");

        Assert.Equal(2, Catalog.Calls.Count);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Searches_are_cached_for_one_minute_ignoring_case_and_spaces()
    {
        await Shop.Search.SearchAsync("såg");
        Time.Advance(TimeSpan.FromSeconds(59));
        await Shop.Search.SearchAsync("  SÅG ");

        Assert.Single(Catalog.Calls);

        Time.Advance(TimeSpan.FromSeconds(1));
        await Shop.Search.SearchAsync("såg");

        Assert.Equal(2, Catalog.Calls.Count);
    }

    // --- Timing log ---

    [Fact(Skip = Todo.Skip)]
    public async Task Every_call_to_the_remote_catalog_is_logged_with_its_duration()
    {
        Catalog.Latency = TimeSpan.FromMilliseconds(120);

        await Shop.ProductPages.ShowAsync("H1");
        await Shop.ProductPages.ShowAsync("H1");   // cache hit: not a remote call, not logged
        await Shop.Search.SearchAsync("såg");

        Assert.Equal(new[] { "Remote GetProduct(H1) 120 ms", "Remote Search(såg) 120 ms" }, Log.Lines);
    }

    [Fact(Skip = Todo.Skip)]
    public async Task Calls_slower_than_500_ms_are_marked_as_slow()
    {
        Catalog.Latency = TimeSpan.FromMilliseconds(800);

        await Shop.Search.SearchAsync("såg");

        Assert.Equal(new[] { "SLOW Remote Search(såg) 800 ms" }, Log.Lines);
    }
}
