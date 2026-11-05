namespace WeatherServiceWrappers;

public record Forecast(string City, int TemperatureC, string Summary);

/// <summary>The external weather API (a third-party service). Slow and sometimes unreliable.</summary>
public interface IWeatherApi
{
    Task<Forecast> FetchForecastAsync(string city);
}

/// <summary>What the rest of our app uses.</summary>
public interface IWeatherService
{
    Task<Forecast> GetForecastAsync(string city);
}

public interface IActivityLog
{
    void Write(string message);
}

public class InMemoryActivityLog : IActivityLog
{
    private readonly List<string> _entries = [];
    public IReadOnlyList<string> Entries => _entries;

    public void Write(string message) => _entries.Add(message);
}

public class ConsoleActivityLog : IActivityLog
{
    public void Write(string message) => Console.WriteLine($"   📝 {message}");
}

/// <summary>Pretends to be a slow remote API.</summary>
public class SlowWeatherApi : IWeatherApi
{
    public async Task<Forecast> FetchForecastAsync(string city)
    {
        await Task.Delay(500);
        var temperature = city.Length * 3 - 10;
        return new Forecast(city, temperature, temperature > 10 ? "Soligt" : "Mulet");
    }
}
