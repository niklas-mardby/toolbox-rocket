# 🔑 Facit: Weather service wrappers

> 🧩 En fullständig kodlösning finns på branchen `solutions` i `solutions/01-refactor/weather-service-wrappers/`.

## 🧅 En stapel som får alla tester gröna

```csharp
public static IWeatherService CreateWeatherService(IWeatherApi api, IActivityLog log, TimeProvider time)
{
    // Utifrån och in:
    return new ValidatingWeatherService(          // 1. avvisa tomma städer, trimma
        new LoggingWeatherService(                 // 2. logga "GetForecast(...)" och "Failed: ..."
            new CachingWeatherService(             // 3. cacha i 10 minuter
                new WeatherService(                // 4. själva hämtningen
                    new LoggingWeatherApi(api, log)), // 5. logga "API call: ..."
                time),
            log));
}
```

`WeatherService` blir en enda rad:

```csharp
public class WeatherService(IWeatherApi api) : IWeatherService
{
    public Task<Forecast> GetForecastAsync(string city) => api.FetchForecastAsync(city);
}
```

### Varför just den här ordningen?

| Lager | Varför här? |
|-------|-------------|
| Validering ytterst | Tomma städer ska inte loggas (testet kräver att loggen är tom). Trimningen måste ske innan loggningen, annars loggas `"GetForecast(  stockholm )"`. |
| Loggning utanför cache | `"GetForecast(...)"` ska loggas vid *varje* anrop, även cache-träffar. |
| Cache | Måste ligga utanför API-loggningen, annars skulle `"API call"` loggas även vid träffar. |
| API-loggning innerst | Loggar bara när API:t *faktiskt* anropas. |

Ett misslyckat anrop kastar en exception genom cachen, som inte sparar något, upp till loggningsdecoratorn, som loggar `"Failed: ..."` och kastar vidare.

## 🤔 Reflektionsfrågorna

### 1. Om du byter plats på cache och loggning?

Om cachen ligger *utanför* loggningen når cache-träffar aldrig loggningsdecoratorn. Då loggas `"GetForecast(Stockholm)"` bara en gång i stället för två, och `Logs_every_request_and_every_api_call` blir rött. Det visar att **ordningen mellan decorators är en del av beteendet**: samma byggklossar i olika ordning ger olika system.

### 2. Återanvända cachen för `TrafficService`?

Svårigheten är att en decorator är bunden till **ett interface**. `CachingWeatherService` implementerar `IWeatherService` och kan inte lindas runt en `ITrafficService`. Möjliga vägar:
- ✅ Bryt ut själva cachelogiken till en generisk hjälpklass (till exempel `ExpiringCache<TKey, TValue>` eller .NET:s `IMemoryCache`) och låt två tunna decorators använda den.
- ✅ Gör interfacen generiska, som `IQueryHandler<TQuery, TResult>`. Då kan *en* `CachingHandler<TQuery, TResult>` dekorera alla. Det är precis vad mediator-pipelines gör (se [mini-mediator](../../exercises/04-build-from-scratch/mini-mediator/)).
- ⚠️ Dynamiska proxies (som Castle DynamicProxy) kan generera decorators för vilket interface som helst, men det är svårare att förstå och felsöka.

### 3. Vad har det gemensamt med middleware och `DelegatingHandler`?

Alla tre är **lager som lindar in nästa lager**, gör något före och efter och skickar vidare anropet. `DelegatingHandler` är en ren Decorator: den *är* en `HttpMessageHandler` och har en `InnerHandler`. Middleware är samma idé, men varje lager får `next` som en delegate i stället för ett objekt med samma interface, och den kan välja att inte anropa `next` alls (Chain of Responsibility). Din validering, som kastar innan anropet går vidare, beter sig faktiskt likadant.

### 4. Är `Composition.cs` svårare att läsa?

Ja, nästlade `new` blir snabbt svårlästa. Bra sätt att göra det tydligare:
- en kommentar per lager med *varför* det ligger där (som ovan),
- att bygga stapeln stegvis i variabler: `IWeatherService service = new WeatherService(...); service = new CachingWeatherService(service, time); ...`,
- att använda en extension method som `services.Decorate<IWeatherService, CachingWeatherService>()`. Biblioteket Scrutor erbjuder det för .NET:s DI-container.

✅ Ett bra svar ser att decorators flyttar komplexiteten från *metoden* till *sammansättningen*. Det är oftast en bra affär, men bara om sammansättningen är tydlig.
