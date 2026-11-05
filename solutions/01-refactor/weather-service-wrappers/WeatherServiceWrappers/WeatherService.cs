namespace WeatherServiceWrappers;

/// <summary>Just the business logic: ask the API for a forecast. Everything else is a decorator.</summary>
public class WeatherService(IWeatherApi api) : IWeatherService
{
    public Task<Forecast> GetForecastAsync(string city) => api.FetchForecastAsync(city);
}
