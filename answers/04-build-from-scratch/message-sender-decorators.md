# 🔑 Facit: Message sender decorators

> 🧩 En fullständig, testad lösning finns på branchen `solutions` i `solutions/04-build-from-scratch/message-sender-decorators/`.

## 🏗️ Kärnan i lösningen

```csharp
// Retry: exception filter släpper igenom sista försöket
for (var attempt = 1; ; attempt++)
{
    try { await _inner.SendAsync(message, ct); return; }
    catch (TransientSendException) when (attempt < _maxAttempts) { }
}

// Filter: ändrar meddelandet på vägen
inner.SendAsync(message with { Subject = Clean(message.Subject), Body = Clean(message.Body) }, ct);

// Extension: sista anropet blir yttersta lagret
public static IMessageSender WithRetry(this IMessageSender sender, int maxAttempts) =>
    new RetryingMessageSender(sender, maxAttempts);
```

## 🔀 Experimentet i steg 9

I `Program.cs` ligger rate limiting *utanför* retry: tre meddelanden, max två, så det tredje stoppas. Om rate limiting läggs *innanför* retry (`.WithRateLimit(...).WithRetry(3)`) räknas varje **försök** i stället för varje **meddelande**. Ett meddelande som behöver tre försök förbrukar då tre platser i fönstret.

## 🤔 Reflektionsfrågorna

### 1. Loggning utanför eller innanför retry?
- **Utanför:** loggen visar *affärshändelser*, alltså ett meddelande som skickades eller inte. Det passar för en audit-logg eller en logg som supporten läser.
- **Innanför:** loggen visar *tekniska försök*. Det är bra vid felsökning ("varför tog det 4 sekunder?") och för att se hur ofta mejlservern krånglar.
- I produktion vill du ofta ha **båda**, på olika loggnivåer: `Information` utanför och `Debug` eller `Warning` innanför.

### 2. Rate limiting utanför eller innanför retry?
- **Utanför:** gränsen gäller *meddelanden från appen*. Skyddar mot buggar som skickar för många meddelanden.
- **Innanför:** gränsen gäller *anrop till mejlservern*. Skyddar *servern* mot för många anrop, inklusive retries. Det passar om leverantören har en gräns för antal API-anrop.
- ⚠️ Om rate limiting ligger innanför retry och kastar `RateLimitExceededException`, retryas den inte, eftersom den inte är en `TransientSendException`. Det är förmodligen rätt.

### 3. Är rate limitern fortfarande en Decorator?
**Ja, strukturellt:** samma interface, lindar in ett objekt, lägger till beteende. Men den *kan välja att inte skicka vidare*, och det är kännetecknet för **Chain of Responsibility**. Det här visar att gränsen mellan mönstren är flytande. Många "decorators" i verkligheten (validering, behörighet, caching) kortsluter ibland. Se [competing-patterns](../03-pick-the-pattern/competing-patterns.md).

### 4. Risker med retry
Om ett försök **faktiskt lyckades** men svaret gick förlorat (timeout efter att servern skickat mejlet) så skickar retry samma mejl **två gånger**. För mejl är det irriterande. För betalningar är det allvarligt.

En operation är **idempotent** om den ger samma resultat oavsett hur många gånger den utförs. Att läsa data är idempotent, men att skicka ett mejl eller dra pengar är det inte. Lösningar:
- en **idempotensnyckel** som servern använder för att känna igen dubbletter,
- att bara retrya fel där du *vet* att inget hände (till exempel "connection refused", men inte en timeout).
