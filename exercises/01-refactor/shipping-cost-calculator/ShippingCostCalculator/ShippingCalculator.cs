namespace ShippingCostCalculator;

// This class works, and the tests prove it. But every time marketing invents a new
// shipping method, we have to change three switch statements in this file...
public class ShippingCalculator
{
    public decimal CalculateCost(Parcel parcel, ShippingMethod method)
    {
        if (parcel.WeightKg <= 0)
            throw new ArgumentException("Weight must be positive.", nameof(parcel));

        decimal cost;
        switch (method)
        {
            case ShippingMethod.Standard:
                if (parcel.WeightKg <= 1)
                    cost = 49m;
                else if (parcel.WeightKg <= 5)
                    cost = 79m;
                else
                    cost = 79m + (parcel.WeightKg - 5) * 10m;
                break;

            case ShippingMethod.Express:
                if (parcel.WeightKg > 20)
                    throw new InvalidOperationException("Express only accepts parcels up to 20 kg.");
                cost = 149m + parcel.WeightKg * 15m;
                break;

            case ShippingMethod.Pickup:
                cost = 0m;
                break;

            case ShippingMethod.International:
                if (parcel.DestinationCountry == "SE")
                    throw new InvalidOperationException("Use a domestic method for Swedish addresses.");
                if (parcel.DestinationCountry == "NO" || parcel.DestinationCountry == "DK" || parcel.DestinationCountry == "FI")
                    cost = 150m + parcel.WeightKg * 20m;
                else
                    cost = 250m + parcel.WeightKg * 35m;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown shipping method.");
        }

        return Math.Round(cost, 2);
    }

    public int EstimateDeliveryDays(Parcel parcel, ShippingMethod method)
    {
        switch (method)
        {
            case ShippingMethod.Standard:
                return parcel.WeightKg > 5 ? 4 : 3;
            case ShippingMethod.Express:
                return 1;
            case ShippingMethod.Pickup:
                return 0;
            case ShippingMethod.International:
                if (parcel.DestinationCountry == "NO" || parcel.DestinationCountry == "DK" || parcel.DestinationCountry == "FI")
                    return 5;
                return 10;
            default:
                throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown shipping method.");
        }
    }

    public string GetDisplayName(ShippingMethod method)
    {
        switch (method)
        {
            case ShippingMethod.Standard:
                return "Standardfrakt";
            case ShippingMethod.Express:
                return "Expressfrakt";
            case ShippingMethod.Pickup:
                return "Hämtas i butik";
            case ShippingMethod.International:
                return "Internationell frakt";
            default:
                throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown shipping method.");
        }
    }
}
