namespace WeatherServiceWrappers.Tests;

// These tests must stay green before AND after your refactoring. Don't change them.
public class WeatherServiceTests
{
    private readonly FakeWeatherApi _api = new();
    private readonly InMemoryActivityLog _log = new();
    private readonly FakeTimeProvider _time = new();

    private IWeatherService CreateService() => Composition.CreateWeatherService(_api, _log, _time);

    [Fact]
    public async Task Returns_the_forecast_from_the_api()
    {
        var service = CreateService();

        var forecast = await service.GetForecastAsync("Stockholm");

        Assert.Equal(new Forecast("Stockholm", 18, "Soligt"), forecast);
    }

    [Fact]
    public async Task Second_call_within_ten_minutes_uses_the_cache()
    {
        var service = CreateService();

        await service.GetForecastAsync("Stockholm");
        _time.Advance(TimeSpan.FromMinutes(9));
        await service.GetForecastAsync("Stockholm");

        Assert.Equal(1, _api.CallCount);
    }

    [Fact]
    public async Task Cache_expires_after_ten_minutes()
    {
        var service = CreateService();

        await service.GetForecastAsync("Stockholm");
        _time.Advance(TimeSpan.FromMinutes(10));
        await service.GetForecastAsync("Stockholm");

        Assert.Equal(2, _api.CallCount);
    }

    [Fact]
    public async Task Cache_ignores_case_and_surrounding_whitespace()
    {
        var service = CreateService();

        await service.GetForecastAsync("Stockholm");
        await service.GetForecastAsync("  stockholm ");

        Assert.Equal(1, _api.CallCount);
    }

    [Fact]
    public async Task Different_cities_are_cached_separately()
    {
        var service = CreateService();

        await service.GetForecastAsync("Stockholm");
        await service.GetForecastAsync("Göteborg");

        Assert.Equal(2, _api.CallCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Empty_city_is_rejected_without_calling_the_api(string city)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetForecastAsync(city));
        Assert.Equal(0, _api.CallCount);
        Assert.Empty(_log.Entries);
    }

    [Fact]
    public async Task Logs_every_request_and_every_api_call()
    {
        var service = CreateService();

        await service.GetForecastAsync("Stockholm");
        await service.GetForecastAsync("Stockholm");

        Assert.Equal(
            new[] { "GetForecast(Stockholm)", "API call: Stockholm", "GetForecast(Stockholm)" },
            _log.Entries);
    }

    [Fact]
    public async Task Failures_are_logged_rethrown_and_not_cached()
    {
        var service = CreateService();
        _api.ShouldFail = true;

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetForecastAsync("Stockholm"));

        Assert.Equal(
            new[] { "GetForecast(Stockholm)", "API call: Stockholm", "Failed: Stockholm" },
            _log.Entries);

        _api.ShouldFail = false;
        await service.GetForecastAsync("Stockholm");
        Assert.Equal(2, _api.CallCount);
    }
}
