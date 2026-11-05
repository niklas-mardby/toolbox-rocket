# 🔑 Facit: Subscription reminders

> 🧩 En fullständig kodlösning med enhetstester finns på branchen `solutions` i `solutions/01-refactor/subscription-reminders/`.

## 🏗️ Efter refaktoreringen

```csharp
public class ReminderService(ICustomerRepository customers, IEmailSender email, TimeProvider time)
{
    public ReminderResult SendRenewalReminders()
    {
        var today = DateOnly.FromDateTime(time.GetLocalNow().DateTime);
        var customersToRemind = customers.GetAllCustomers()
            .Where(c => !c.AutoRenew)
            .Where(c => c.SubscriptionEndsOn >= today && c.SubscriptionEndsOn <= today.AddDays(7))
            // ... resten som förut, men med email.Send(...)
    }
}

public static class Composition
{
    public static ReminderService CreateReminderService() => new(
        new SqlCustomerRepository("Server=prod-db;Database=Shop;Trusted_Connection=True"),
        new SmtpEmailSender("smtp.toolbox-shop.se", 587),
        TimeProvider.System);
}
```

All konfiguration (connection string, SMTP-värd) ligger nu i composition root. `ReminderService` innehåller bara affärsregeln.

Ett exempel på ett test som inte var möjligt innan:

```csharp
[Fact]
public void Reminder_across_leap_day()
{
    var time = new FakeTimeProvider(new DateTimeOffset(2028, 2, 25, 8, 0, 0, TimeSpan.Zero));
    var email = new FakeEmailSender();
    var repo = new FakeCustomerRepository(new Customer("a@x.se", "A", new DateOnly(2028, 3, 3), false));

    var result = new ReminderService(repo, email, time).SendRenewalReminders();

    Assert.Equal(["a@x.se"], result.Recipients);
    Assert.Contains("om 7 dagar", email.Sent.Single().Body);
}
```

> ⚠️ `GetLocalNow()` använder datorns tidszon. I ett test som ska vara helt deterministiskt kan du antingen välja en tid mitt på dagen, som ovan, eller låta din `FakeTimeProvider` också överskugga `LocalTimeZone` och returnera `TimeZoneInfo.Utc`.

## 🤔 Reflektionsfrågorna

### 1. Vad kan de nya testerna som det gamla inte kunde?

- ✅ **Gränsfall:** exakt 7 dagar kontra 8, idag kontra igår och skottdagen. Det gamla testet var låst till de datum som den fejkade databasen råkade generera.
- ✅ **Innehåll:** vad som står i mejlet, eftersom du nu kan fånga det i en `FakeEmailSender`.
- ✅ **Isolering:** inga nätverksanrop, ingen databas, och samma resultat oavsett när eller var testet körs.
- ✅ **Snabbhet:** millisekunder, så du kan ha hundratals sådana tester.

Karaktäriseringstestet har fortfarande ett värde: det bekräftar att composition root kopplar ihop allt rätt. Ett sådant test kallas ofta *integration test* eller *smoke test*.

### 2. Fler osynliga beroenden

Vanliga exempel:
- 🎲 `Random` och `Guid.NewGuid()`, som ger olika resultat varje körning
- 📁 filsystemet (`File.ReadAllText`)
- 🌍 miljövariabler och `Environment.MachineName`
- 🌐 `CultureInfo.CurrentCulture`: `1.5.ToString()` ger `"1,5"` på en svensk dator
- 🧵 statiska tillstånd och singletons (`SomeCache.Instance`)
- 🌐 `HttpClient` som skapas med `new` inne i en metod

Gemensamt för dem: de gör koden **beroende av omvärlden** på ett sätt som inte syns i metodsignaturen.

### 3. Varför fick `Customer` och `ReminderResult` inga interfaces?

De är **data** (värdeobjekt eller DTO:er), inte **beteende**. De har inga sidoeffekter och inga beroenden till omvärlden, så det finns inget att byta ut eller fejka. I ett test skapar du dem bara med `new`. Ett interface för en `record` vore ren ceremoni (se övningen [interface-for-everything](../../exercises/05-overuse-critique/interface-for-everything/)).

> 📚 Tumregel: injicera **tjänster** (saker som *gör* något, särskilt mot omvärlden). Skapa **data** med `new`.

### 4. Är ett interface med en enda produktionsimplementation motiverat?

Här: **ja**. Det finns faktiskt två implementationer, den riktiga och den i testerna, och den riktiga gör något du absolut inte vill ska hända i ett test (skicka mejl). Interfacet är en **söm** (seam) där du kan byta beteende.

Det vore *inte* motiverat för en klass utan sidoeffekter som du aldrig behöver byta ut i tester, till exempel en ren beräkning. Där kan du testa den riktiga klassen direkt.
