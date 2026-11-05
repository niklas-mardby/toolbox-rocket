# 📝 NOTES: Interface for everything

## 💡 Resonemanget

**Frågan för varje interface:** *pratar det med omvärlden, eller behöver det kunna bytas ut?*

| Gammalt interface | Beslut | Varför |
|-------------------|--------|--------|
| `ICustomer` | ❌ Borttaget | Data. En `record` räcker. |
| `ICustomerRepository` | ✅ **Behållet** | Läser från CRM-databasen. En verklig söm för tester och för att byta datakälla. |
| `IDateTimeWrapper` | 🔄 **Ersatt av `TimeProvider`** | Klockan *är* ett beroende, men .NET har redan en abstraktion. Den gamla koden hade en wrapper *runt* `TimeProvider`, alltså en abstraktion av en abstraktion. |
| `IStringHelper` | ❌ Borttaget | Rena strängfunktioner. Privata metoder. |
| `IAgeCalculator` | ❌ Borttaget | En subtraktion. |
| `IBirthdayCalculator` | 🔄 **Statisk klass** | Ren funktion med en knepig regel (29 februari). Statisk, men egen typ, så att den kan testas för sig om man vill. |
| `IReportLineFormatter` | ❌ Borttaget | Privat metod. |
| `IReportConfiguration` | ❌ Borttaget | Tre konstanter. Om de någon gång ska konfigureras används Options pattern. |
| `IListFactory` | ❌ Borttaget | `new List<T>()` bakom ett interface. Ett tydligt exempel på överanvändning. |
| `IBirthdayReportService` | ❌ Borttaget | Inget anropar tjänsten via interfacet, och inget behöver fejka den. Om en *controller* som använder tjänsten ska testas isolerat kan interfacet komma tillbaka. |

- **Konstruktorn gick från 8 till 2 parametrar.** Båda är riktiga beroenden mot omvärlden.
- **DI-containern behövs inte** i `Composition.cs` för två beroenden. I en riktig ASP.NET Core-app registrerar du exakt `ICustomerRepository` och `TimeProvider`. NuGet-paketet `Microsoft.Extensions.DependencyInjection` kan tas bort ur projektet.
- **Testbarheten är kvar:** testerna styr fortfarande klockan, och `ICustomerRepository` kan fejkas.

## 🔀 Alternativ

- **Behåll `IBirthdayReportService`** om tjänsten används från en controller som ska testas med en fejkad tjänst. Ett interface är motiverat när det finns en konsument som behöver byta implementation.
- **Bryt ut `FormatLine` till en egen klass** om formateringen växer, till exempel med översättningar. Men utan interface, tills någon behöver byta den.
