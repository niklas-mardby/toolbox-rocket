namespace ShippingCostCalculator;

/// <summary>Everything that varies between shipping methods, in one place per method.</summary>
public interface IShippingStrategy
{
    string DisplayName { get; }
    decimal CalculateCost(Parcel parcel);
    int EstimateDeliveryDays(Parcel parcel);
}

public class StandardShipping : IShippingStrategy
{
    public string DisplayName => "Standardfrakt";

    public decimal CalculateCost(Parcel parcel) => parcel.WeightKg switch
    {
        <= 1 => 49m,
        <= 5 => 79m,
        _ => 79m + (parcel.WeightKg - 5) * 10m
    };

    public int EstimateDeliveryDays(Parcel parcel) => parcel.WeightKg > 5 ? 4 : 3;
}

public class ExpressShipping : IShippingStrategy
{
    private const decimal MaxWeightKg = 20;

    public string DisplayName => "Expressfrakt";

    public decimal CalculateCost(Parcel parcel)
    {
        if (parcel.WeightKg > MaxWeightKg)
            throw new InvalidOperationException("Express only accepts parcels up to 20 kg.");
        return 149m + parcel.WeightKg * 15m;
    }

    public int EstimateDeliveryDays(Parcel parcel) => 1;
}

public class PickupShipping : IShippingStrategy
{
    public string DisplayName => "Hämtas i butik";
    public decimal CalculateCost(Parcel parcel) => 0m;
    public int EstimateDeliveryDays(Parcel parcel) => 0;
}

public class InternationalShipping : IShippingStrategy
{
    // Previously duplicated in two switch statements. Now in ONE place.
    private static readonly HashSet<string> Nordic = ["NO", "DK", "FI"];

    public string DisplayName => "Internationell frakt";

    public decimal CalculateCost(Parcel parcel)
    {
        if (parcel.DestinationCountry == "SE")
            throw new InvalidOperationException("Use a domestic method for Swedish addresses.");

        return Nordic.Contains(parcel.DestinationCountry)
            ? 150m + parcel.WeightKg * 20m
            : 250m + parcel.WeightKg * 35m;
    }

    public int EstimateDeliveryDays(Parcel parcel) => Nordic.Contains(parcel.DestinationCountry) ? 5 : 10;
}
