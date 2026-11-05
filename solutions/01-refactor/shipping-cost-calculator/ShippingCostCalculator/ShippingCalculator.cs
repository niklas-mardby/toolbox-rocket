namespace ShippingCostCalculator;

/// <summary>
/// Same public API as before, so the tests (and every caller) keep working.
/// Inside, it only does what is COMMON to all methods: validation, lookup and rounding.
/// Everything specific to one shipping method lives in that method's strategy class.
/// </summary>
public class ShippingCalculator
{
    private readonly Dictionary<ShippingMethod, IShippingStrategy> _strategies = new()
    {
        [ShippingMethod.Standard] = new StandardShipping(),
        [ShippingMethod.Express] = new ExpressShipping(),
        [ShippingMethod.Pickup] = new PickupShipping(),
        [ShippingMethod.International] = new InternationalShipping(),
    };

    public decimal CalculateCost(Parcel parcel, ShippingMethod method)
    {
        if (parcel.WeightKg <= 0)
            throw new ArgumentException("Weight must be positive.", nameof(parcel));

        return Math.Round(StrategyFor(method).CalculateCost(parcel), 2);
    }

    public int EstimateDeliveryDays(Parcel parcel, ShippingMethod method) =>
        StrategyFor(method).EstimateDeliveryDays(parcel);

    public string GetDisplayName(ShippingMethod method) => StrategyFor(method).DisplayName;

    private IShippingStrategy StrategyFor(ShippingMethod method) =>
        _strategies.TryGetValue(method, out var strategy)
            ? strategy
            : throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown shipping method.");
}
