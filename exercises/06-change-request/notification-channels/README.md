# 🔔 Notification channels

> 🧩 **Mönstret som hjälper:** Strategy (och kanske Observer)
> 📏 **Omfattning:** en tjänst med två metoder som skickar mejl. Det nya kravet lägger till två kanaler, kundens egna preferenser och en reservlösning. Problemet har *två* dimensioner som varierar, och det är det som gör det intressant.
> 🧪 **Tester:** 3 befintliga tester är gröna. 7 acceptanstester för det nya kravet är avstängda med `Todo.Skip`.

## 🎯 Mål

Du ska lägga till nya notiskanaler, upptäcka hur kombinationen av *händelser* och *kanaler* växer, och refaktorera så att varje ny kanal och varje ny händelse blir en liten, isolerad ändring.

## 📖 Bakgrund

När en order skickas eller levereras får kunden ett mejl från `NotificationService`. CRM-teamet har nyss lagt till `PreferredChannels` på `Customer`, och kunderna kan redan välja mejl, SMS och push i "Mitt konto". Men inget i notiskoden läser inställningen än.

SMS- och push-gatewayerna finns redan, eftersom marknadsmodulen använder dem.

### 📋 Det nya kravet

> **Som** kund **vill jag** få besked om min order via de kanaler jag har valt, **så att** jag ser det där jag faktiskt tittar.

Reglerna:
- 📨 Kunden kan välja en, flera eller inga kanaler.
- 📱 **SMS** innehåller en kort text med prefixet `Toolbox: `.
- 🔔 **Push** har rubriken som titel och den korta texten som innehåll.
- ⏭️ Kanaler som saknar kontaktuppgifter, till exempel inget telefonnummer, hoppas över.
- 📧 Om inga kanaler är valda, eller ingen vald kanal kan nå kunden, skickas **mejl** som reserv.

Exakta texter finns i `ChannelAcceptanceTests.cs`.

## 🛠️ Uppgift

### Del 1: Lägg till funktionen 🛠️

1. ▶️ Kör testerna. Sätt sedan `Todo.Skip` till `null`. 7 tester blir röda.
2. 🛠️ Gör dem gröna med den nuvarande strukturen. Du behöver skicka in SMS- och push-gatewayerna i `Composition.cs`.
3. 💾 Gör en commit eller spara en kopia.

### Del 2: Reflektera 🤔

4. Svara på frågorna:
   - Hur mycket av koden är *duplicerad* mellan `NotifyOrderShipped` och `NotifyOrderDelivered` nu?
   - Produktteamet vill nästa sprint lägga till **"Ordern är försenad"**. Hur många rader blir det?
   - Och året därpå kommer en fjärde kanal, **WhatsApp**. Hur många ställen ändras då?
   - Rita en tabell med händelser och kanaler. Hur många rutor måste koden hantera?

### Del 3: Refaktorera 🔧

5. ✂️ **Dela upp problemet** i *vad* som ska sägas (per händelse) och *hur* det levereras (per kanal).
6. 🔀 **Inför en Strategy för kanaler.** Varje kanal vet själv hur den skickar och om den kan nå kunden.
7. 🧭 **Låt `NotificationService` bara välja kanaler** (preferenser och reserv) och skicka meddelandet till dem.
8. ✅ Alla 10 tester ska vara gröna.

### ➕ Nästa förändring

9. Lägg till `NotifyOrderDelayed(customerId, orderId, newDate)` med texter du hittar på själv. Skriv tester. Hur många ställen ändrade du? Jämför med svaret i del 2.

## ▶️ Kom igång

```bash
dotnet run --project exercises/06-change-request/notification-channels/NotificationChannels
dotnet test exercises/06-change-request/notification-channels/NotificationChannels.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Två dimensioner</summary>

Händelserna (skickad, levererad, försenad) och kanalerna (mejl, SMS, push) varierar *oberoende* av varandra. Om koden har en gren för varje kombination växer den som *händelser × kanaler*. Om du delar upp dem växer den som *händelser + kanaler*.
</details>

<details>
<summary>💡 Hint 2: Vad ska sägas?</summary>

Varje händelse behöver tre texter: en rubrik, en längre text för mejl och en kort text för SMS och push. En `record Notification(string Subject, string Body, string ShortText)` med en statisk fabriksmetod per händelse samlar texterna snyggt.
</details>

<details>
<summary>💡 Hint 3: Kanalens interface</summary>

```csharp
public interface INotificationChannel
{
    NotificationChannel Kind { get; }
    bool CanReach(Customer customer);
    void Send(Customer customer, Notification notification);
}
```

Med `Kind` kan tjänsten slå upp kanalen från kundens preferenser.
</details>

<details>
<summary>💡 Hint 4: Observer?</summary>

Du kanske funderar på att göra "order skickad" till ett event som kanalerna prenumererar på. Fundera på var regeln "använd kundens preferenser, men mejl som reserv" skulle hamna då. Hur många ställen skulle behöva känna till den?
</details>

### 📚 Central concepts

- **Strategy** – en klass per sätt att leverera. 🔗 [Refactoring.Guru: Strategy](https://refactoring.guru/design-patterns/strategy)
- **Observer** – ett alternativ, med andra avvägningar. 🔗 [Refactoring.Guru: Observer](https://refactoring.guru/design-patterns/observer)
- **Duplicate code** – kodlukten efter del 1. 🔗 [Refactoring.Guru: Duplicate Code](https://refactoring.guru/smells/duplicate-code)
- **Divergent change** – när en klass ändras av många olika skäl. 🔗 [Refactoring.Guru: Divergent Change](https://refactoring.guru/smells/divergent-change)
- **Separation of concerns** – vad, hur och vilka. 🔗 [Wikipedia: Separation of concerns](https://en.wikipedia.org/wiki/Separation_of_concerns)
- **Single responsibility principle** – ett skäl att ändras per klass. 🔗 [Wikipedia: Single-responsibility principle](https://en.wikipedia.org/wiki/Single-responsibility_principle)

## 🤔 Reflektionsfrågor

1. Var placerade du reservregeln (mejl om inget annat når kunden)? Varför just där?
2. Hur skulle du testa en ny `WhatsAppChannel` utan att testa hela `NotificationService`?
3. Vad händer i din lösning om SMS-gatewayen kastar en exception? Ska kunden ändå få mejlet och pushnotisen?

---

🔑 **[Facit](../../../answers/06-change-request/notification-channels.md)**. Gör ett ärligt försök först!
