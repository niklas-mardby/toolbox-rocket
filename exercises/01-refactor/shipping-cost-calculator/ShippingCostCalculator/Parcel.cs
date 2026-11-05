namespace ShippingCostCalculator;

public enum ShippingMethod
{
    Standard,
    Express,
    Pickup,
    International
}

/// <summary>A parcel to ship. DestinationCountry is an ISO code like "SE", "NO" or "DE".</summary>
public record Parcel(decimal WeightKg, string DestinationCountry);
