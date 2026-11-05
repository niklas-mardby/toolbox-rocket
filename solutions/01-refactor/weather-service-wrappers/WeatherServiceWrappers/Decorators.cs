namespace WeatherServiceWrappers;

/// <summary>Rejects empty cities and trims the rest before anyone else sees them.</summary>
public class ValidatingWeatherService(IWeatherService inner) : IWeatherService
{
    public Task<Forecast> GetForecastAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));

        return inner.GetForecastAsync(city.Trim());
    }
}

/// <summary>Logs every request, and failures, then rethrows.</summary>
public class LoggingWeatherService(IWeatherService inner, IActivityLog log) : IWeatherService
{
    public async Task<Forecast> GetForecastAsync(string city)
    {
        log.Write($"GetForecast({city})");
        try
        {
            return await inner.GetForecastAsync(city);
        }
        catch (Exception)
        {
            log.Write($"Failed: {city}");
            throw;
        }
    }
}

/// <summary>Caches successful forecasts for 10 minutes, ignoring case. Failures are not cached.</summary>
public class CachingWeatherService(IWeatherService inner, TimeProvider time) : IWeatherService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private readonly Dictionary<string, (Forecast Forecast, DateTimeOffset StoredAt)> _cache = [];

    public async Task<Forecast> GetForecastAsync(string city)
    {
        var key = city.ToUpperInvariant();
        if (_cache.TryGetValue(key, out var entry) && time.GetUtcNow() - entry.StoredAt < CacheDuration)
            return entry.Forecast;

        var forecast = await inner.GetForecastAsync(city);   // if this throws, nothing is cached
        _cache[key] = (forecast, time.GetUtcNow());
        return forecast;
    }
}

/// <summary>A decorator around the OTHER interface: logs exactly when the real API is called.</summary>
public class LoggingWeatherApi(IWeatherApi inner, IActivityLog log) : IWeatherApi
{
    public Task<Forecast> FetchForecastAsync(string city)
    {
        log.Write($"API call: {city}");
        return inner.FetchForecastAsync(city);
    }
}
