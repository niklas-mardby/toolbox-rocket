namespace MultiTenantPricing.Tests;

internal static class TestTenants
{
    public static readonly TenantPricingConfig Verktygsboden = new("verktygsboden",
    [
        new PercentOffForMembers(10),
        new QuantityBreak(10, 5),
        new CategoryCampaign("Trädgård", 15, [DayOfWeek.Saturday, DayOfWeek.Sunday]),
    ]);

    public static readonly TenantPricingConfig ByggmaxLight = new("byggmax-light",
    [
        new QuantityBreak(5, 10),
        new BuyXPayForY("SKRUV", 3, 2),
        new MinimumOrderFee(500, 49),
    ]);

    public static readonly TenantPricingConfig Tradgardsbutiken = new("tradgardsbutiken",
    [
        new CategoryCampaign("Trädgård", 20, [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday]),
        new PercentOffForMembers(5),
    ]);

    public static readonly TenantPricingConfig NoRules = new("no-rules", []);

    public static ITenantConfigStore Store() =>
        new InMemoryTenantConfigStore(Verktygsboden, ByggmaxLight, Tradgardsbutiken, NoRules);

    public static readonly DateTimeOffset Tuesday = new(2026, 11, 3, 12, 0, 0, TimeSpan.FromHours(1));
    public static readonly DateTimeOffset Wednesday = new(2026, 11, 4, 12, 0, 0, TimeSpan.FromHours(1));
    public static readonly DateTimeOffset Saturday = new(2026, 11, 7, 12, 0, 0, TimeSpan.FromHours(1));

    public static readonly Customer Member = new("anna", IsMember: true);
    public static readonly Customer Guest = new("guest", IsMember: false);
}
