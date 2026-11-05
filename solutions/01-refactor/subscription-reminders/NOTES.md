# 📝 NOTES: Subscription reminders

## 💡 Resonemanget

- **Tre dolda beroenden blev tre konstruktorparametrar:** `ICustomerRepository`, `IEmailSender` och `TimeProvider`. Klockan är .NET:s egen abstraktion. Ingen egen `IClock` behövs.
- **Konfigurationen flyttade till composition root.** Connection string och SMTP-värd finns bara i `Composition.cs` (i en riktig app i `appsettings.json` och Options pattern).
- **De befintliga klasserna fick bara `: ICustomerRepository` och `: IEmailSender`.** Deras kod ändrades inte. Det är en liten och säker refaktorering.
- **Karaktäriseringstestet är kvar och grönt.** Det fungerar nu som ett test av att composition root kopplar ihop allt rätt.
- **Fem nya enhetstester** i `ReminderServiceTests.cs`: gränsen 7/8 dagar, AutoRenew, utgångna, "idag" och skottdagen. Inga mock-bibliotek, bara tre små fakes.
- **`FakeTimeProvider` sätter `LocalTimeZone` till UTC.** `GetLocalNow()` beror annars på datorns tidszon, och ett test som körs strax efter midnatt i en annan tidszon kunde få fel dag.

## 💉 Bonusen: livstider

| Tjänst | Livstid | Varför |
|--------|---------|--------|
| `TimeProvider` | Singleton | Tillståndslös och trådsäker |
| `IEmailSender` | Singleton | En SMTP-klient kan återanvändas. En riktig `SmtpClient` är dock *inte* trådsäker, och då är Transient eller en pool bättre. |
| `ICustomerRepository` | Scoped | I en webbapp: en per request, eftersom den skulle bero på en scoped `DbContext` |
| `ReminderService` | Scoped | Beror på något scoped |

`ValidateScopes = false` behövs för att hämta en scoped tjänst direkt från root-providern i den här lilla konsolappen. I en riktig app skapar du ett scope (`using var scope = provider.CreateScope()`) per jobb, till exempel i en `BackgroundService`.

## 🔀 Alternativ

- **Manuella `new` i `Composition.cs`** är lika bra med tre beroenden. Containern blir värd det först när grafen växer.
- **Bryt ut regeln** till en ren funktion, `ShouldRemind(customer, today)`, som kan testas utan några fakes alls. Det är bra i kombination med DI.
