# 📝 NOTES: Weather service wrappers

## 💡 Resonemanget

- **Fem ansvar, fem klasser.** `WeatherService` är en rad. Validering, loggning av anrop, caching och loggning av API-anrop är var sin decorator.
- **`LoggingWeatherApi` dekorerar `IWeatherApi`, inte `IWeatherService`.** Det är det enklaste sättet att logga *exakt* när det riktiga API:t anropas, eftersom den ligger innanför cachen.
- **Ordningen följer av testerna:**
  1. Validering ytterst: tomma städer ska inte loggas, och trimningen måste ske före loggningen.
  2. Loggning utanför cachen: `GetForecast(...)` loggas även vid cache-träffar.
  3. Cachen: sparar bara lyckade svar.
  4. API-loggning innerst.
- **`Composition.cs` byggs stegvis** med en variabel som skrivs över, och med en kommentar per lager. Det är lättare att läsa än nästlade `new`.

## 🔀 Alternativ

- **Generisk cache:** bryt ut cachelogiken till en `ExpiringCache<TKey, TValue>` som flera decorators kan använda, eller använd `IMemoryCache`.
- **Scrutor** (`services.Decorate<IWeatherService, CachingWeatherService>()`) i en app med DI-container.
- **Validering och trimning i samma decorator** som loggningen. Det är färre klasser, men blandar två ansvar. Båda är försvarbara.
- **Tidsmätningen (bonusen):** en `TimingWeatherService` som använder `time.GetTimestamp()` och `time.GetElapsedTime(...)`. Ytterst mäter den totaltiden inklusive cache-träffar. Runt `IWeatherApi` mäter den bara API-anropen.
