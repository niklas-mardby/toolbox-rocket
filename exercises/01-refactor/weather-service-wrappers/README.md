# 🌦️ Weather service wrappers

> 🧩 **Mönster:** Decorator
> 📏 **Omfattning:** en metod på ungefär 30 rader, men med fyra ansvarsområden hoptrasslade. Du behöver tänka på *ordningen* mellan dina decorators.
> 🧪 **Tester:** finns och är gröna. Du får ändra `Composition.cs` men inte testerna.

## 🎯 Mål

Du ska dela upp `WeatherService` så att varje ansvar, som validering, loggning och caching, ligger i en egen **Decorator** som kan staplas runt den egentliga tjänsten.

## 📖 Bakgrund

Appen visar väderprognoser från ett externt API. API:t är långsamt, så tjänsten cachar svaren i tio minuter. Den loggar också alla anrop och fel.

Öppna `WeatherService.cs`. Den egentliga affärslogiken är **en enda rad**: `await api.FetchForecastAsync(city)`. Resten handlar om annat. Nästa vecka ska teamet bygga en `TrafficService` som behöver exakt samma loggning och caching. Ska den koden kopieras dit?

## 🛠️ Uppgift

1. ▶️ **Kör testerna** och se att de är gröna. Kör gärna programmet också och se hur långsamt det är utan cache.
2. 🗂️ **Lista ansvarsområdena** i `GetForecastAsync`. Hur många hittar du?
3. 🧹 **Skala av `WeatherService`** så att den bara gör det den heter: hämtar en prognos.
4. 🎁 **Skapa en decorator per ansvar.** Varje decorator implementerar `IWeatherService` och tar emot en annan `IWeatherService` i konstruktorn.
5. 🧅 **Stapla dem i `Composition.cs`** i rätt ordning, så att alla tester blir gröna.
6. ✅ **Kör testerna.**
7. ➕ **Bonus:** Lägg till en decorator som mäter tiden för varje anrop och loggar `"Took {ms} ms"`. Var i stapeln ska den ligga för att mäta tiden *inklusive* cache-träffar? Och var ska den ligga för att bara mäta API-anropen? Skriv inga tester som beror på exakta millisekunder.

## ▶️ Kom igång

```bash
dotnet run --project exercises/01-refactor/weather-service-wrappers/WeatherServiceWrappers
dotnet test exercises/01-refactor/weather-service-wrappers/WeatherServiceWrappers.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Ansvarsområdena</summary>

Jag hittar fem: **validering** (tom stad), **normalisering** (trimma mellanslag), **loggning** (anrop och fel), **caching** och den egentliga **hämtningen**. Validering och normalisering kan få dela decorator om du vill.
</details>

<details>
<summary>💡 Hint 2: Hur ser en decorator ut?</summary>

```csharp
public class SomethingWeatherService(IWeatherService inner) : IWeatherService
{
    public async Task<Forecast> GetForecastAsync(string city)
    {
        // gör något före
        var result = await inner.GetForecastAsync(city);
        // gör något efter
        return result;
    }
}
```

En decorator *är* en `IWeatherService` och *har* en `IWeatherService`. Därför kan den som använder tjänsten inte se skillnad på en dekorerad och en odekorerad tjänst.
</details>

<details>
<summary>💡 Hint 3: Ordningen spelar roll</summary>

Titta noga på testet `Logs_every_request_and_every_api_call`. `"GetForecast(...)"` loggas vid *varje* anrop, även cache-träffar. Alltså måste den loggningen ligga *utanför* cachen. Men `"API call: ..."` loggas bara när API:t faktiskt anropas, alltså *innanför* cachen. Och tomma städer ska inte loggas alls.
</details>

<details>
<summary>💡 Hint 4: "API call" kan dekorera ett annat interface</summary>

En decorator kan lika gärna ligga runt `IWeatherApi` som runt `IWeatherService`. En `LoggingWeatherApi : IWeatherApi` som loggar `"API call: ..."` och skickar vidare är ett elegant sätt att logga precis när API:t anropas.
</details>

<details>
<summary>💡 Hint 5: En möjlig stapel</summary>

Utifrån och in: validering och trimning → loggning av anrop och fel → caching → `WeatherService` → loggning av API-anrop → det riktiga API:t. Prova att byta plats på två lager och se vilka tester som blir röda. Det är ett bra sätt att förstå varför ordningen är som den är.
</details>

### 📚 Central concepts

- **Decorator pattern** – linda in ett objekt i ett annat med samma interface för att lägga till beteende. 🔗 [Refactoring.Guru: Decorator](https://refactoring.guru/design-patterns/decorator)
- **Cross-cutting concerns** – sådant som loggning och caching som behövs i många delar av systemet. 🔗 [Wikipedia: Cross-cutting concern](https://en.wikipedia.org/wiki/Cross-cutting_concern)
- **Composition over inheritance** – bygg beteende genom att kombinera objekt i stället för att ärva. 🔗 [Wikipedia: Composition over inheritance](https://en.wikipedia.org/wiki/Composition_over_inheritance)
- **Single responsibility principle** – en klass, ett skäl att ändras. 🔗 [Wikipedia: Single-responsibility principle](https://en.wikipedia.org/wiki/Single-responsibility_principle)
- **TimeProvider** – .NET:s abstraktion av klockan, som gör tidsberoende kod testbar. 🔗 [Microsoft Learn: What is TimeProvider?](https://learn.microsoft.com/en-us/dotnet/standard/datetime/timeprovider-overview)
- **Caching** – spara resultat av dyra operationer för att återanvända dem. 🔗 [Microsoft Learn: Caching in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/caching)

## 🤔 Reflektionsfrågor

1. Vad händer om du byter plats på caching- och loggningsdecoratorn? Vilka tester blir röda, och varför?
2. Hur skulle du återanvända din caching-decorator för en `TrafficService` med ett annat interface? Vad är det som gör det svårt?
3. ASP.NET Core-middleware och `DelegatingHandler` för `HttpClient` fungerar på ett liknande sätt. Vad har de gemensamt med det du byggde?
4. Har `Composition.cs` blivit svårare att läsa? Vad skulle du göra för att stapeln ska vara lätt att förstå för nästa utvecklare?

---

🔑 **[Facit](../../../answers/01-refactor/weather-service-wrappers.md)**. Gör ett ärligt försök först!
