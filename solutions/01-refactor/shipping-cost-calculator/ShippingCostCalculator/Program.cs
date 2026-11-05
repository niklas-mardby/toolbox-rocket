using ShippingCostCalculator;

var calculator = new ShippingCalculator();
var parcels = new[]
{
    new Parcel(0.8m, "SE"),
    new Parcel(7m, "SE"),
    new Parcel(3m, "NO"),
    new Parcel(3m, "DE"),
};

foreach (var parcel in parcels)
{
    Console.WriteLine($"📦 {parcel.WeightKg} kg till {parcel.DestinationCountry}");
    foreach (var method in Enum.GetValues<ShippingMethod>())
    {
        try
        {
            var cost = calculator.CalculateCost(parcel, method);
            var days = calculator.EstimateDeliveryDays(parcel, method);
            Console.WriteLine($"   {calculator.GetDisplayName(method),-22} {cost,8:0.00} kr  {days,2} dagar");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"   {calculator.GetDisplayName(method),-22} ej tillgänglig ({ex.Message})");
        }
    }
    Console.WriteLine();
}
