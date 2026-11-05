namespace WeatherServiceWrappers;

public static class Composition
{
    public static IWeatherService CreateWeatherService(IWeatherApi api, IActivityLog log, TimeProvider time)
    {
        // Built inside out. Read the comments bottom-up to follow a request.
        IWeatherService service = new WeatherService(new LoggingWeatherApi(api, log)); // 4. fetch (and log real API calls)
        service = new CachingWeatherService(service, time);                             // 3. cache: hits never reach the API
        service = new LoggingWeatherService(service, log);                              // 2. log EVERY request, incl. cache hits
        service = new ValidatingWeatherService(service);                                // 1. reject/trim before anything is logged
        return service;
    }
}
