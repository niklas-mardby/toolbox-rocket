namespace MultiTenantPricing;

/// <summary>The composition root. The tests only use CreateEngine and Quote(...), so you may change the rest.</summary>
public static class Composition
{
    public static PricingEngine CreateEngine(ITenantConfigStore configs) => new(new InMemoryProductCatalog(), configs);
}
