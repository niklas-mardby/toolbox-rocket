# ✉️ Message sender decorators

> 🧩 **Mönster:** Decorator, fyra stycken som kan staplas i valfri ordning
> 📏 **Omfattning:** fyra små decorators och en uppsättning extension methods. Varje decorator är kort, men de gör fyra olika *sorters* saker med anropet, och ordningen mellan dem spelar roll.
> 🧪 **Tester:** 13 tester finns, avstängda med `Todo.Skip`. Slå på dem när du är redo.

## 🎯 Mål

Du ska bygga fyra **Decorators** för `IMessageSender` och förstå hur samma byggklossar i olika ordning ger olika beteende.

## 📖 Bakgrund

Appen skickar meddelanden via `IMessageSender`. Den riktiga avsändaren är enkel, men i produktion behövs mer runt den:

- 📝 **Loggning** av alla försök
- 🔁 **Retry** när mejlservern har tillfälliga problem
- 🤬 **Filtrering** av otrevliga ord, på uppdrag av kundtjänst
- 🛑 **Rate limiting**, så att en bugg inte skickar 10 000 mejl på en minut

Ingen av dessa funktioner ska ligga *i* avsändaren. De ska kunna slås på, stängas av och kombineras:

```csharp
IMessageSender sender = new SmtpSender()
    .WithProfanityFilter("fan", "jäkla")
    .WithRetry(3)
    .WithRateLimit(maxMessages: 100, window: TimeSpan.FromMinutes(1), TimeProvider.System)
    .WithLogging(log);
```

## 🛠️ Uppgift

1. 📖 Läs `Contracts.cs` (färdig) och stubbarna i `Decorators.cs`.
2. 🧪 Slå på testerna genom att sätta `Todo.Skip` till `null`. Titta på testdubblarna i `TestDoubles.cs`.
3. 📝 Implementera `LoggingMessageSender`.
4. 🔁 Implementera `RetryingMessageSender`.
5. 🤬 Implementera `ProfanityFilterMessageSender`.
6. 🛑 Implementera `RateLimitingMessageSender`.
7. 🔗 Implementera extension methods i `MessageSenderExtensions`.
8. ✅ Alla tester gröna? Kör programmet och förklara utskriften:
   ```bash
   dotnet run --project exercises/04-build-from-scratch/message-sender-decorators/MessageSenderDecorators
   ```
9. 🔀 **Experimentera:** byt ordning på decoratorerna i `Program.cs`. Vad händer om rate limiting ligger *innanför* retry?

## 💡 Hints

<details>
<summary>💡 Hint 1: Grundformen</summary>

Varje decorator implementerar `IMessageSender`, tar en `IMessageSender inner` och anropar `await inner.SendAsync(message, ct)` någonstans i sin egen `SendAsync`. Skillnaden är vad som händer *före*, *efter* eller *i stället för* det anropet.
</details>

<details>
<summary>💡 Hint 2: Kasta vidare samma exception</summary>

I en `catch` ska du skriva `throw;`, inte `throw ex;`. Det bevarar både objektet och stack trace.
</details>

<details>
<summary>💡 Hint 3: Retry</summary>

En `for`-loop med `try/catch` inuti. Ett *exception filter* gör det elegant:

```csharp
catch (TransientSendException) when (attempt < _maxAttempts) { /* försök igen */ }
```

På sista försöket är villkoret falskt, och då fångas inte exceptionen.
</details>

<details>
<summary>💡 Hint 4: Hela ord</summary>

`string.Replace` ersätter "fan" även i "fantastisk". Använd `Regex` med `\b` (ordgräns) och `RegexOptions.IgnoreCase`. Eftersom `Message` är en `record` kan du skapa en ändrad kopia med `message with { Subject = ..., Body = ... }`.
</details>

<details>
<summary>💡 Hint 5: Sliding window</summary>

Spara tidsstämplarna för skickade meddelanden i en `Queue<DateTimeOffset>`. Ta bort de som är äldre än fönstret från början av kön innan du räknar.
</details>

### 📚 Central concepts

- **Decorator pattern** – lägg till beteende genom att linda in. 🔗 [Refactoring.Guru: Decorator](https://refactoring.guru/design-patterns/decorator)
- **Retry pattern** – försök igen vid tillfälliga fel. 🔗 [Microsoft Learn: Retry pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/retry)
- **Resilience i .NET** – de färdiga byggklossarna för retry och mer. 🔗 [Microsoft Learn: Introduction to resilient app development](https://learn.microsoft.com/en-us/dotnet/core/resilience/)
- **Extension methods och fluent interface** – för att stapla läsbart. 🔗 [Martin Fowler: FluentInterface](https://martinfowler.com/bliki/FluentInterface.html)
- **Records och `with`** – ändrade kopior av immutabla objekt. 🔗 [Microsoft Learn: Records](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
- **Exceptions** – `throw;` och exception filters. 🔗 [Microsoft Learn: Best practices for exceptions](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)
- **TimeProvider** – testbar tid för rate limiting. 🔗 [Microsoft Learn: What is TimeProvider?](https://learn.microsoft.com/en-us/dotnet/standard/datetime/timeprovider-overview)

## 🤔 Reflektionsfrågor

1. Testerna `Logging_outside_retry_hides_the_retries` och `Logging_inside_retry_shows_every_attempt` använder samma byggklossar. När vill du ha vilken ordning?
2. Var i stapeln ska rate limiting ligga: utanför eller innanför retry? Vad räknas som "ett meddelande" i de två fallen?
3. Rate limiting-decoratorn kan *stoppa* anropet. Är den fortfarande en Decorator, eller har den blivit något annat?
4. Vad är riskerna med retry för operationer som att skicka mejl eller ta betalt? Vad betyder *idempotent*?

---

🔑 **[Facit](../../../answers/04-build-from-scratch/message-sender-decorators.md)**. Gör ett ärligt försök först!
