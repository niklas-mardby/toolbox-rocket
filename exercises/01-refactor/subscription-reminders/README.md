# ⏰ Subscription reminders

> 🧩 **Mönster:** Dependency Injection
> 📏 **Omfattning:** en tjänst på ungefär 40 rader med tre dolda beroenden. Refaktoreringen är kort. Det stora jobbet är testerna du kan skriva efteråt.
> 🧪 **Tester:** ett karaktäriseringstest finns och är grönt. Du får ändra `Composition.cs` men inte det testet.

## 🎯 Mål

Du ska göra `ReminderService` testbar genom att låta den **få sina beroenden** i stället för att **skapa dem själv**. Sedan ska du bevisa vinsten genom att skriva snabba enhetstester som inte var möjliga innan.

## 📖 Bakgrund

Varje morgon skickar `ReminderService` påminnelser till kunder vars prenumeration går ut inom sju dagar, förutsatt att de inte har automatisk förnyelse.

Tjänsten fungerar. Men titta på de tre första raderna i `SendRenewalReminders`:

```csharp
var repository = new SqlCustomerRepository("Server=prod-db;...");
var emailSender = new SmtpEmailSender("smtp.toolbox-shop.se", 587);
var today = DateOnly.FromDateTime(DateTime.Now);
```

Försök svara på frågorna:
- 🤔 Hur testar du att en prenumeration som går ut **exakt om sju dagar** får en påminnelse, men inte en som går ut om åtta?
- 🤔 Hur testar du vad som händer vid **skottdagen** den 29 februari?
- 🤔 Hur kontrollerar du **vad det står i mejlet**?

Det befintliga testet i `CharacterizationTests.cs` kan bara kontrollera mottagarlistan, och bara för att den fejkade databasen råkar generera data utifrån dagens datum. I verkligheten hade testet krävt en produktionsdatabas och skickat riktiga mejl.

## 🛠️ Uppgift

1. ▶️ **Kör testet** och se att det är grönt.
2. 🔍 **Hitta de dolda beroendena** i `ReminderService`. Det är fler än två.
3. 🧩 **Inför abstraktioner** för dem: ett interface för kundregistret, ett för mejlskickandet och .NET:s inbyggda `TimeProvider` för klockan.
4. 💉 **Injicera beroendena via konstruktorn** i `ReminderService`. Klassen ska inte längre innehålla något `new` för sina beroenden, inga connection strings och ingen `DateTime.Now`.
5. 🔌 **Bygg ihop de riktiga implementationerna** i `Composition.CreateReminderService()`. Karaktäriseringstestet ska fortfarande vara grönt.
6. 🧪 **Skriv enhetstester** i `ReminderServiceTests.cs` med fejkade beroenden. Testa minst:
   - att en prenumeration som går ut om exakt 7 dagar ger påminnelse, men inte en som går ut om 8
   - att kunder med `AutoRenew` inte får påminnelse
   - att utgångna prenumerationer inte får påminnelse
   - vad som står i mejlet när prenumerationen går ut *idag*
   - en påminnelse över skottdagen: idag är 2028-02-25 och prenumerationen går ut 2028-03-03 (vilket är om 7 dagar)
7. ➕ **Bonus:** Byt ut de manuella `new` i `Composition.cs` mot .NET:s DI-container (`ServiceCollection`), som redan finns som paket i projektet. Vilken livstid (`Singleton`, `Scoped`, `Transient`) passar för varje tjänst?

## ▶️ Kom igång

```bash
dotnet run --project exercises/01-refactor/subscription-reminders/SubscriptionReminders
dotnet test exercises/01-refactor/subscription-reminders/SubscriptionReminders.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: De dolda beroendena</summary>

Det finns tre: databasen (`SqlCustomerRepository`), mejlservern (`SmtpEmailSender`) och **klockan** (`DateTime.Now`). Klockan är lättast att missa, men den gör att koden beter sig olika beroende på när testet körs. Connection string och SMTP-adress är också beroenden, nämligen konfiguration som inte hör hemma i affärslogiken.
</details>

<details>
<summary>💡 Hint 2: Vilka interfaces?</summary>

Låt interfacen beskriva vad `ReminderService` *behöver*, inte hur det görs:

```csharp
public interface ICustomerRepository
{
    IReadOnlyList<Customer> GetAllCustomers();
}

public interface IEmailSender
{
    void Send(string to, string subject, string body);
}
```

Låt de befintliga klasserna implementera dem. Du behöver inte ändra något annat i dem.
</details>

<details>
<summary>💡 Hint 3: Klockan</summary>

Sedan .NET 8 finns `TimeProvider` inbyggt. I produktion använder du `TimeProvider.System`. I testerna skriver du en egen liten subklass:

```csharp
public class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
```

I `ReminderService` får du dagens datum med `DateOnly.FromDateTime(time.GetLocalNow().DateTime)`.
</details>

<details>
<summary>💡 Hint 4: Fejkade beroenden</summary>

Du behöver inget mock-bibliotek. Två små klasser i testprojektet räcker:

```csharp
public class FakeCustomerRepository(params Customer[] customers) : ICustomerRepository
{
    public IReadOnlyList<Customer> GetAllCustomers() => customers;
}

public class FakeEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Body)> Sent { get; } = [];
    public void Send(string to, string subject, string body) => Sent.Add((to, subject, body));
}
```
</details>

<details>
<summary>💡 Hint 5: Med ServiceCollection</summary>

```csharp
var services = new ServiceCollection();
services.AddSingleton(TimeProvider.System);
services.AddSingleton<IEmailSender>(_ => new SmtpEmailSender("smtp.toolbox-shop.se", 587));
// ... resten
using var provider = services.BuildServiceProvider();
return provider.GetRequiredService<ReminderService>();
```

Fundera på om det är ett problem att `provider` kasseras med `using` innan tjänsten används. Vad händer med tjänster som implementerar `IDisposable`?
</details>

### 📚 Central concepts

- **Dependency Injection** – en klass får sina beroenden utifrån i stället för att skapa dem. 🔗 [Microsoft Learn: Dependency injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- **Dependency inversion principle** – högnivåkod ska bero på abstraktioner, inte på detaljer. 🔗 [Wikipedia: Dependency inversion principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
- **Inversion of Control** – ramverket eller anroparen bestämmer, inte klassen själv. 🔗 [Martin Fowler: InversionOfControl](https://martinfowler.com/bliki/InversionOfControl.html)
- **TimeProvider** – abstraktionen av klockan i .NET. 🔗 [Microsoft Learn: What is TimeProvider?](https://learn.microsoft.com/en-us/dotnet/standard/datetime/timeprovider-overview)
- **Test doubles** – fejkade objekt (fakes, stubs, mocks) som ersätter riktiga beroenden i tester. 🔗 [Martin Fowler: TestDouble](https://martinfowler.com/bliki/TestDouble.html)
- **Characterization tests** – tester som beskriver hur befintlig kod beter sig, innan du ändrar den. 🔗 [Wikipedia: Characterization test](https://en.wikipedia.org/wiki/Characterization_test)
- **Service lifetimes** – Singleton, Scoped och Transient i .NET:s container. 🔗 [Microsoft Learn: Dependency injection guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)

## 🤔 Reflektionsfrågor

1. Jämför karaktäriseringstestet med dina nya enhetstester. Vad kan de nya testa som det gamla inte kunde?
2. Klockan var ett beroende fast den inte såg ut som ett. Kan du komma på fler "osynliga" beroenden i kod du skrivit tidigare?
3. `Customer` och `ReminderResult` fick inga interfaces. Varför inte?
4. Ditt `IEmailSender`-interface har bara en implementation i produktionskoden. Är interfacet ändå motiverat?

---

🔑 **[Facit](../../../answers/01-refactor/subscription-reminders.md)**. Gör ett ärligt försök först!
