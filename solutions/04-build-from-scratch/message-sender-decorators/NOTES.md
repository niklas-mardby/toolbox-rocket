# 📝 NOTES: Message sender decorators

## 💡 Resonemanget

- **Varje decorator gör en sak** och vet inget om de andra. De delar bara `IMessageSender`.
- **Fyra sorters decorators:**
  | Decorator | Vad den gör med anropet |
  |-----------|-------------------------|
  | `LoggingMessageSender` | Observerar före och efter, ändrar inget |
  | `RetryingMessageSender` | Anropar `inner` *flera gånger* |
  | `ProfanityFilterMessageSender` | *Ändrar* meddelandet på vägen (`message with { ... }`) |
  | `RateLimitingMessageSender` | Kan *stoppa* anropet helt, alltså kortslutning |
- **`throw;` i stället för `throw ex;`** bevarar stack trace och gör att testet `Assert.Same(original, thrown)` går igenom.
- **Retry med exception filter** (`when (attempt < _maxAttempts)`): på sista försöket matchar inte filtret, så exceptionen bubblar upp oförändrad. Det ger kort och tydlig kod utan en extra variabel för "senaste exceptionen".
- **Valideringen i retry-konstruktorn** ger fel direkt när stapeln byggs, inte först när ett meddelande skickas. Därför används en vanlig konstruktor i stället för en primärkonstruktor.
- **Regex med `\b`** ger hela ord: `fan` matchar inte `FANTASTISK`. `Regex.Escape` skyddar mot ord med specialtecken.
- **Rate limit med en kö** av tidsstämplar: gamla tidsstämplar tas bort först, sedan kontrolleras antalet. Ett `lock` gör den säker om flera trådar skickar samtidigt, eftersom en rate limiter oftast är en singleton.
- **Ordningen i stapeln** testas uttryckligen. Loggning *utanför* retry ser ett anrop. Loggning *innanför* ser varje försök. Båda kan vara rätt, beroende på vad loggen ska användas till.

## 🔀 Alternativa lösningar

- **Backoff i retry:** `await Task.Delay(TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)), ct)`. För att det ska gå att testa snabbt bör väntan gå via `TimeProvider` (`Task.Delay(delay, time, ct)`), eller via en injicerad delay-funktion.
- **Polly** (`Microsoft.Extensions.Resilience`) ger retry, timeout, circuit breaker och rate limiting färdigt. I produktion är det ofta bättre än egen kod. Övningen visar vad sådana bibliotek gör under huven.
- **`System.Threading.RateLimiting`** finns inbyggt i .NET och har färdiga sliding window-, token bucket- och fixed window-limiters.
- **DI-registrering av stapeln:** med biblioteket Scrutor kan du skriva `services.Decorate<IMessageSender, RetryingMessageSender>()`. Med .NET:s egen container bygger du stapeln med en fabriksmetod: `services.AddSingleton<IMessageSender>(sp => new SmtpSender(...).WithRetry(3).WithLogging(...))`.
