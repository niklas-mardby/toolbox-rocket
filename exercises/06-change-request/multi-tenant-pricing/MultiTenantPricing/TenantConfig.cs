namespace MultiTenantPricing;

// ✅ The platform team built this configuration model last quarter, so each tenant (shop)
// can have its own price rules. It's stored in a database. The PricingEngine doesn't use it yet.
//
// RULE SEMANTICS (agreed with the product owner):
//  - Line rules are applied to each line in the order they appear in the config.
//    Each one works on the line total as it is AFTER the previous rules.
//      PercentOffForMembers(p)        members get p % off every line.                       Name: "Medlemsrabatt {p} %"
//      QuantityBreak(min, p)          p % off a line whose quantity is at least min.        Name: "Mängdrabatt {p} % från {min} st"
//      CategoryCampaign(cat, p, days) p % off lines in the category, on the given weekdays. Name: "Kampanj {cat} {p} %"
//      BuyXPayForY(sku, x, y)         for every full x of the sku, (x - y) units are free,
//                                     valued at the line's current price per unit.          Name: "Köp {x} betala för {y} ({sku})"
//  - Line totals are rounded to 2 decimals (half away from zero) after all line rules.
//  - Order rules come after the line rules:
//      MinimumOrderFee(threshold, fee) if the lines sum to less than threshold, add a fee. Name: "Småordertillägg {fee} kr"
//  - A rule's name is added to Quote.AppliedRules only if it changed something (once, even if it hit several lines).
//  - Numbers in names are written without trailing zeros: 10 %, 49 kr, 7.5 %.

public abstract record PriceRuleConfig;
public record PercentOffForMembers(decimal Percent) : PriceRuleConfig;
public record QuantityBreak(int MinQuantity, decimal Percent) : PriceRuleConfig;
public record CategoryCampaign(string Category, decimal Percent, IReadOnlyList<DayOfWeek> Days) : PriceRuleConfig;
public record BuyXPayForY(string Sku, int X, int Y) : PriceRuleConfig;
public record MinimumOrderFee(decimal Threshold, decimal Fee) : PriceRuleConfig;

public record TenantPricingConfig(string TenantId, IReadOnlyList<PriceRuleConfig> Rules);

public interface ITenantConfigStore
{
    TenantPricingConfig? Get(string tenantId);
}

public class InMemoryTenantConfigStore(params TenantPricingConfig[] configs) : ITenantConfigStore
{
    public TenantPricingConfig? Get(string tenantId) => configs.FirstOrDefault(c => c.TenantId == tenantId);
}
