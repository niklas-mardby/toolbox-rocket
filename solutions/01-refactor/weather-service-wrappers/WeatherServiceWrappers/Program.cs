using System.Diagnostics;
using WeatherServiceWrappers;

var service = Composition.CreateWeatherService(new SlowWeatherApi(), new ConsoleActivityLog(), TimeProvider.System);

foreach (var city in new[] { "Stockholm", "Göteborg", "stockholm", "Stockholm " })
{
    var sw = Stopwatch.StartNew();
    var forecast = await service.GetForecastAsync(city);
    Console.WriteLine($"🌦️ {forecast.City}: {forecast.TemperatureC}°C, {forecast.Summary} ({sw.ElapsedMilliseconds} ms)");
}
