namespace ShippingCostCalculator.Tests;

// These tests describe the current behaviour. They must stay green
// before AND after your refactoring. Don't change them.
public class ShippingCalculatorTests
{
    private readonly ShippingCalculator _calculator = new();

    [Theory]
    [InlineData(0.5, "SE", ShippingMethod.Standard, 49)]
    [InlineData(1.0, "SE", ShippingMethod.Standard, 49)]
    [InlineData(3.0, "SE", ShippingMethod.Standard, 79)]
    [InlineData(5.0, "SE", ShippingMethod.Standard, 79)]
    [InlineData(8.0, "SE", ShippingMethod.Standard, 109)]
    [InlineData(2.0, "SE", ShippingMethod.Express, 179)]
    [InlineData(20.0, "SE", ShippingMethod.Express, 449)]
    [InlineData(12.0, "SE", ShippingMethod.Pickup, 0)]
    [InlineData(2.0, "NO", ShippingMethod.International, 190)]
    [InlineData(2.0, "FI", ShippingMethod.International, 190)]
    [InlineData(2.0, "DE", ShippingMethod.International, 320)]
    public void CalculateCost_returns_expected_price(double weight, string country, ShippingMethod method, double expected)
    {
        var parcel = new Parcel((decimal)weight, country);

        var cost = _calculator.CalculateCost(parcel, method);

        Assert.Equal((decimal)expected, cost);
    }

    [Fact]
    public void CalculateCost_rounds_to_two_decimals()
    {
        var cost = _calculator.CalculateCost(new Parcel(1.333m, "SE"), ShippingMethod.Express);

        Assert.Equal(169.00m, cost); // 149 + 19.995 = 168.995 -> 169.00
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalculateCost_rejects_non_positive_weight(double weight)
    {
        Assert.Throws<ArgumentException>(() =>
            _calculator.CalculateCost(new Parcel((decimal)weight, "SE"), ShippingMethod.Standard));
    }

    [Fact]
    public void Express_rejects_parcels_over_20_kg()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _calculator.CalculateCost(new Parcel(20.5m, "SE"), ShippingMethod.Express));
    }

    [Fact]
    public void International_rejects_swedish_addresses()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _calculator.CalculateCost(new Parcel(2m, "SE"), ShippingMethod.International));
    }

    [Theory]
    [InlineData(2.0, "SE", ShippingMethod.Standard, 3)]
    [InlineData(6.0, "SE", ShippingMethod.Standard, 4)]
    [InlineData(2.0, "SE", ShippingMethod.Express, 1)]
    [InlineData(2.0, "SE", ShippingMethod.Pickup, 0)]
    [InlineData(2.0, "DK", ShippingMethod.International, 5)]
    [InlineData(2.0, "US", ShippingMethod.International, 10)]
    public void EstimateDeliveryDays_returns_expected_days(double weight, string country, ShippingMethod method, int expected)
    {
        var days = _calculator.EstimateDeliveryDays(new Parcel((decimal)weight, country), method);

        Assert.Equal(expected, days);
    }

    [Theory]
    [InlineData(ShippingMethod.Standard, "Standardfrakt")]
    [InlineData(ShippingMethod.Express, "Expressfrakt")]
    [InlineData(ShippingMethod.Pickup, "Hämtas i butik")]
    [InlineData(ShippingMethod.International, "Internationell frakt")]
    public void GetDisplayName_returns_swedish_name(ShippingMethod method, string expected)
    {
        Assert.Equal(expected, _calculator.GetDisplayName(method));
    }

    [Fact]
    public void Unknown_method_throws()
    {
        var unknown = (ShippingMethod)99;

        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.CalculateCost(new Parcel(1m, "SE"), unknown));
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.EstimateDeliveryDays(new Parcel(1m, "SE"), unknown));
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.GetDisplayName(unknown));
    }
}
