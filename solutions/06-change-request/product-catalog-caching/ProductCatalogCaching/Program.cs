using System.Diagnostics;
using ProductCatalogCaching;
using ProductCatalogCaching.Catalog;

var shop = Composition.CreateShop(new RemoteProductCatalog(), TimeProvider.System, new ConsoleActivityLog());

async Task Timed(string label, Func<Task> action)
{
    var sw = Stopwatch.StartNew();
    await action();
    Console.WriteLine($"⏱️ {label}: {sw.ElapsedMilliseconds} ms");
}

await Timed("Produktsida H1", () => shop.ProductPages.ShowAsync("H1"));
await Timed("Produktsida H1 igen", () => shop.ProductPages.ShowAsync("H1"));
await Timed("Sök 'såg'", () => shop.Search.SearchAsync("såg"));
await Timed("Sök 'såg' igen", () => shop.Search.SearchAsync("såg"));
await Timed("Varukorg (H1 + S1)", () => shop.Cart.TotalAsync([new CartLine("H1", 2), new CartLine("S1", 1)]));
