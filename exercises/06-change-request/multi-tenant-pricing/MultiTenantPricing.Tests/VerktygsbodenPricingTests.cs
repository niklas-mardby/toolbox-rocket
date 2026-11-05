using static MultiTenantPricing.Tests.TestTenants;

namespace MultiTenantPricing.Tests;

// Existing behaviour for our first tenant. Must stay green throughout. Don't change these tests.
public class VerktygsbodenPricingTests
{
    private readonly PricingEngine _engine = Composition.CreateEngine(Store());

    private Quote QuoteFor(Customer customer, DateTimeOffset at, params CartItem[] cart) =>
        _engine.Quote("verktygsboden", cart, customer, at);

    [Fact]
    public void List_prices_when_no_rule_applies()
    {
        var quote = QuoteFor(Guest, Wednesday, new CartItem("H1", 2));

        Assert.Equal(new[] { new QuoteLine("H1", 2, 199m, 398m) }, quote.Lines);
        Assert.Empty(quote.Fees);
        Assert.Empty(quote.AppliedRules);
        Assert.Equal(398m, quote.Total);
    }

    [Fact]
    public void Members_get_ten_percent_off()
    {
        var quote = QuoteFor(Member, Wednesday, new CartItem("H1", 2));

        Assert.Equal(358.20m, quote.Total);
        Assert.Equal(new[] { "Medlemsrabatt 10 %" }, quote.AppliedRules);
    }

    [Fact]
    public void Ten_or_more_of_a_product_gives_five_percent_off_that_line()
    {
        var quote = QuoteFor(Guest, Wednesday, new CartItem("SKRUV", 10), new CartItem("H1", 1));

        Assert.Equal(560.50m, quote.Lines[0].LineTotal);
        Assert.Equal(199m, quote.Lines[1].LineTotal);
        Assert.Equal(new[] { "Mängdrabatt 5 % från 10 st" }, quote.AppliedRules);
    }

    [Fact]
    public void Rules_stack_in_order()
    {
        var quote = QuoteFor(Member, Wednesday, new CartItem("SKRUV", 10));

        Assert.Equal(504.45m, quote.Total);   // 590 * 0.90 * 0.95
        Assert.Equal(new[] { "Medlemsrabatt 10 %", "Mängdrabatt 5 % från 10 st" }, quote.AppliedRules);
    }

    [Fact]
    public void Garden_products_are_15_percent_off_at_weekends_only()
    {
        Assert.Equal(2545.75m, QuoteFor(Guest, Saturday, new CartItem("GK", 1)).Total);
        Assert.Equal(new[] { "Kampanj Trädgård 15 %" }, QuoteFor(Guest, Saturday, new CartItem("GK", 1)).AppliedRules);
        Assert.Equal(2995m, QuoteFor(Guest, Wednesday, new CartItem("GK", 1)).Total);
    }

    [Fact]
    public void Line_totals_are_rounded_half_away_from_zero()
    {
        var quote = QuoteFor(Member, Saturday, new CartItem("SP", 1));   // 249 * 0.90 * 0.85 = 190.485

        Assert.Equal(190.49m, quote.Total);
        Assert.Equal(new[] { "Medlemsrabatt 10 %", "Kampanj Trädgård 15 %" }, quote.AppliedRules);
    }

    [Fact]
    public void A_rule_is_listed_once_even_if_it_hits_several_lines()
    {
        var quote = QuoteFor(Member, Wednesday, new CartItem("H1", 1), new CartItem("SP", 1));

        Assert.Equal(new[] { "Medlemsrabatt 10 %" }, quote.AppliedRules);
    }

    [Fact]
    public void Unknown_products_are_rejected()
    {
        Assert.Throws<KeyNotFoundException>(() => QuoteFor(Guest, Wednesday, new CartItem("NOPE", 1)));
    }
}
