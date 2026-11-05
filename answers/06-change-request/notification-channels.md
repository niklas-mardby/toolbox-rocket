# 🔑 Facit: Notification channels

> 🧩 En fullständig lösning finns på branchen `solutions` i `solutions/06-change-request/notification-channels/`.

## 🤔 Del 2: Reflektionen efter del 1

- **Duplicering:** efter del 1 innehåller båda metoderna samma logik för att välja kanaler, hoppa över saknade kontaktuppgifter och falla tillbaka på mejl. Ungefär 15–20 rader kopierade, och bara texterna skiljer sig.
- **"Försenad":** ytterligare en kopia av samma logik, plus tre nya texter. Det blir cirka 25 rader.
- **WhatsApp:** en ny gren i *varje* metod, alltså tre ställen med tre händelser.
- **Tabellen:** 2 händelser × 3 kanaler = **6 rutor** som koden hanterar, och med försenad plus WhatsApp blir det 3 × 4 = **12**.

## 🔧 Del 3: Efter refaktoreringen

```csharp
public record Notification(string Subject, string Body, string ShortText)
{
    public static Notification Shipped(Customer c, string orderId, string url) => new(...);
    public static Notification Delivered(Customer c, string orderId) => new(...);
}

public interface INotificationChannel
{
    NotificationChannel Kind { get; }
    bool CanReach(Customer customer);
    void Send(Customer customer, Notification notification);
}

// NotificationService: välj kanaler, sedan skicka
var reachable = customer.PreferredChannels.Distinct()
    .Select(kind => _channels.GetValueOrDefault(kind)).OfType<INotificationChannel>()
    .Where(c => c.CanReach(customer)).ToList();
if (reachable.Count == 0) reachable.Add(_channels[NotificationChannel.Email]);
foreach (var channel in reachable) channel.Send(customer, notification);
```

Nu växer koden som **händelser + kanaler**: "försenad" blir en fabriksmetod och en publik metod, och WhatsApp blir en klass och en rad i `Composition.cs`.

## 🤔 Reflektionsfrågorna

### 1. Var ligger reservregeln?
I `NotificationService`, eftersom det är en **affärsregel om vilka kanaler som ska användas**, inte om *hur* en viss kanal fungerar. Om den låg i `EmailChannel` skulle mejlkanalen behöva veta om alla andra kanaler. Om varje kanal var en observer skulle ingen ensam kunna avgöra att "ingen annan nådde kunden".

### 2. Testa en ny `WhatsAppChannel` isolerat
Skapa kanalen med en fejkad gateway och anropa `Send` och `CanReach` direkt:
```csharp
var gateway = new RecordingWhatsApp();
var channel = new WhatsAppChannel(gateway);
channel.Send(customer, Notification.Shipped(customer, "1001", "url"));
Assert.Equal(..., gateway.Sent.Single());
```
Det är en av vinsterna med Strategy: **varje strategi kan testas för sig**, och tjänsten kan testas med fejkade kanaler.

### 3. Om SMS-gatewayen kastar
I lösningen ovan **avbryts loopen**, och kanaler efter SMS skickas inte. Troligen vill du i stället:
- fånga exceptions per kanal och logga dem, så att de andra kanalerna fortsätter,
- kanske falla tillbaka på mejl om *alla* valda kanaler misslyckades,
- i produktion: lägga notiserna i en **kö** (outbox) med retry, så att ett långsamt eller trasigt SMS-API inte påverkar orderflödet.

✅ Ett bra svar ser att **en kanal inte ska kunna förstöra för de andra**. Det är samma lärdom som för observers i [notification-observer](../04-build-from-scratch/notification-observer.md).
