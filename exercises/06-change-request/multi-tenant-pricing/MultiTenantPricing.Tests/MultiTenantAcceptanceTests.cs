using static MultiTenantPricing.Tests.TestTenants;

namespace MultiTenantPricing.Tests;

// ✨ The new requirement: every tenant's rules come from its configuration.
// Switch these on with Todo.Skip = null. Don't change these tests.
public class MultiTenantAcceptanceTests
{
    private readonly PricingEngine _engine = Composition.CreateEngine(Store());

    // --- byggmax-light: QuantityBreak(5, 10), BuyXPayForY(SKRUV, 3, 2), MinimumOrderFee(500, 49) ---

    [Fact(Skip = Todo.Skip)]
    public void Quantity_break_threshold_and_percent_come_from_config()
    {
        var quote = _engine.Quote("byggmax-light", [new CartItem("H1", 5)], Guest, Wednesday);

        Assert.Equal(895.50m, quote.Total);   // 995 * 0.90, over 500 so no fee
        Assert.Equal(new[] { "Mängdrabatt 10 % från 5 st" }, quote.AppliedRules);
        Assert.Empty(quote.Fees);
    }

    [Fact(Skip = Todo.Skip)]
    public void Buy_three_pay_for_two_and_small_order_fee()
    {
        var quote = _engine.Quote("byggmax-light", [new CartItem("SKRUV", 3)], Guest, Wednesday);

        Assert.Equal(118m, quote.Lines.Single().LineTotal);                       // 3 x 59, one free
        Assert.Equal(new[] { new QuoteFee("Småordertillägg", 49m) }, quote.Fees);
        Assert.Equal(167m, quote.Total);
        Assert.Equal(new[] { "Köp 3 betala för 2 (SKRUV)", "Småordertillägg 49 kr" }, quote.AppliedRules);
    }

    [Fact(Skip = Todo.Skip)]
    public void Small_order_fee_alone()
    {
        var quote = _engine.Quote("byggmax-light", [new CartItem("H1", 1), new CartItem("SP", 1)], Guest, Wednesday);

        Assert.Equal(497m, quote.Total);   // 448 + 49
        Assert.Equal(new[] { "Småordertillägg 49 kr" }, quote.AppliedRules);
    }

    [Fact(Skip = Todo.Skip)]
    public void Line_rules_build_on_each_other_in_config_order()
    {
        // 7 x 59 = 413 -> quantity break 10 %: 371.70 -> two free units at 53.10: 265.50 -> fee 49
        var quote = _engine.Quote("byggmax-light", [new CartItem("SKRUV", 7)], Guest, Wednesday);

        Assert.Equal(265.50m, quote.Lines.Single().LineTotal);
        Assert.Equal(314.50m, quote.Total);
        Assert.Equal(
            new[] { "Mängdrabatt 10 % från 5 st", "Köp 3 betala för 2 (SKRUV)", "Småordertillägg 49 kr" },
            quote.AppliedRules);
    }

    // --- tradgardsbutiken: CategoryCampaign(Trädgård, 20, weekdays), PercentOffForMembers(5) ---

    [Fact(Skip = Todo.Skip)]
    public void Weekday_garden_campaign_and_member_discount()
    {
        var quote = _engine.Quote("tradgardsbutiken", [new CartItem("SP", 2), new CartItem("H1", 1)], Member, Tuesday);

        Assert.Equal(378.48m, quote.Lines[0].LineTotal);   // 498 * 0.80 * 0.95
        Assert.Equal(189.05m, quote.Lines[1].LineTotal);   // 199 * 0.95
        Assert.Equal(567.53m, quote.Total);
        Assert.Equal(new[] { "Kampanj Trädgård 20 %", "Medlemsrabatt 5 %" }, quote.AppliedRules);
    }

    [Fact(Skip = Todo.Skip)]
    public void The_garden_campaign_does_not_apply_at_weekends_for_this_tenant()
    {
        var quote = _engine.Quote("tradgardsbutiken", [new CartItem("SP", 2)], Member, Saturday);

        Assert.Equal(473.10m, quote.Total);   // 498 * 0.95
        Assert.Equal(new[] { "Medlemsrabatt 5 %" }, quote.AppliedRules);
    }

    // --- general ---

    [Fact(Skip = Todo.Skip)]
    public void A_tenant_without_rules_pays_list_price()
    {
        var quote = _engine.Quote("no-rules", [new CartItem("H1", 2)], Member, Saturday);

        Assert.Equal(398m, quote.Total);
        Assert.Empty(quote.AppliedRules);
    }

    [Fact(Skip = Todo.Skip)]
    public void The_same_cart_is_priced_differently_per_tenant()
    {
        CartItem[] cart = [new CartItem("H1", 5)];

        Assert.Equal(995m, _engine.Quote("verktygsboden", cart, Guest, Wednesday).Total);
        Assert.Equal(895.50m, _engine.Quote("byggmax-light", cart, Guest, Wednesday).Total);
    }

    [Fact(Skip = Todo.Skip)]
    public void Unknown_tenants_are_rejected()
    {
        Assert.Throws<KeyNotFoundException>(() => _engine.Quote("nobody", [new CartItem("H1", 1)], Guest, Wednesday));
    }
}
