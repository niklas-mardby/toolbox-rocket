namespace WeatherServiceWrappers;

// The actual business logic here is ONE line: ask the API for a forecast.
// Everything else is logging, validation, caching and error handling,
// and the next service we write will need exactly the same thing...
public class WeatherService(IWeatherApi api, IActivityLog log, TimeProvider time) : IWeatherService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private readonly Dictionary<string, (Forecast Forecast, DateTimeOffset StoredAt)> _cache = [];

    public async Task<Forecast> GetForecastAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));

        city = city.Trim();
        log.Write($"GetForecast({city})");

        try
        {
            var key = city.ToUpperInvariant();
            if (_cache.TryGetValue(key, out var entry) && time.GetUtcNow() - entry.StoredAt < CacheDuration)
            {
                return entry.Forecast;
            }

            log.Write($"API call: {city}");
            var forecast = await api.FetchForecastAsync(city);

            _cache[key] = (forecast, time.GetUtcNow());
            return forecast;
        }
        catch (Exception)
        {
            log.Write($"Failed: {city}");
            throw;
        }
    }
}
