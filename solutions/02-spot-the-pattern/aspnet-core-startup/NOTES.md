# 📝 NOTES: ASP.NET Core startup

Facit per block finns på main i [`answers/02-spot-the-pattern/aspnet-core-startup.md`](../../../answers/02-spot-the-pattern/aspnet-core-startup.md).

## 💡 Resonemanget

- **Läs `Program.cs` i två faser.** Allt före `builder.Build()` *konfigurerar* (Builder, DI, Options, Factory). Allt efter *kör* (middleware som Chain of Responsibility, Observer för livscykeln och endpoints).
- **Block E är tätast:** `AddHttpClient` (Factory), `DelegatingHandler` (Decorator och CoR), resilience (Decorator) och `WeatherClient` (troligen Adapter).
- **Service Locator är rätt i `NightlyCleanupService`** (singleton som behöver scoped `DbContext`) men onödigt i middleware-lambdan.
- **Middleware-ordningen** är en del av beteendet. Det motsäger intuitionen att flytta tidsmätningen *ytterst*, men det gör att den även ser fel som exception handlern har fångat.

## 🔀 Andra giltiga svar

- **Block D** kan beskrivas som Strategy (`UseSqlServer` mot `UseSqlite`) *eller* Adapter (EF Core-providern översätter till respektive databas). Båda är rimliga.
- **Block H** kan beskrivas som Observer: flera providers "lyssnar" på loggmeddelanden. Composite är strukturellt närmare, men båda tolkningarna kan försvaras.
- **Block L:** att `IOrderService` injiceras direkt i en lambda kallas ibland *method injection*.
