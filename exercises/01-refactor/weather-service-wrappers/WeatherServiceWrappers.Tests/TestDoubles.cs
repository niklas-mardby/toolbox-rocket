namespace WeatherServiceWrappers.Tests;

/// <summary>A fake API that counts calls and can be told to fail.</summary>
public class FakeWeatherApi : IWeatherApi
{
    public int CallCount { get; private set; }
    public bool ShouldFail { get; set; }

    public Task<Forecast> FetchForecastAsync(string city)
    {
        CallCount++;
        if (ShouldFail)
            throw new HttpRequestException("The weather API is down.");
        return Task.FromResult(new Forecast(city, 18, "Soligt"));
    }
}

/// <summary>A clock you control. .NET's TimeProvider makes this easy.</summary>
public class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _now = new(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now += by;
}
