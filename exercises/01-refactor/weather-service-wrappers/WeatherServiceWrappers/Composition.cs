namespace WeatherServiceWrappers;

/// <summary>
/// The composition root. The tests create the weather service ONLY through this method,
/// so you may change how it is built (for example: wrap it in decorators) as much as you like.
/// </summary>
public static class Composition
{
    public static IWeatherService CreateWeatherService(IWeatherApi api, IActivityLog log, TimeProvider time)
    {
        return new WeatherService(api, log, time);
    }
}
