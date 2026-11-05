# 🔌 Interface for everything

> 🧩 **Överanvänt mönster:** Dependency Injection, med ett interface för varje klass
> 📏 **Omfattning:** tio interfaces och en tjänst med åtta konstruktorparametrar, för en rapport på 20 rader. Uppgiften är inte att ta bort *alla* interfaces. Vissa *ska* stanna, och det svåra är att avgöra vilka.
> 🧪 **Tester:** finns och är gröna. Du får ändra `Composition.cs` men inte testerna.

## 🎯 Mål

Du ska skilja mellan beroenden som *behöver* abstraheras och kod som bara har fått ett interface av princip, och förenkla utan att förlora testbarheten.

## 📖 Bakgrund

Kundtjänst vill ha en rapport över kunder som fyller år de närmaste 14 dagarna. Rapporten byggs av `BirthdayReportService`.

Den förra utvecklaren följde regeln **"varje klass ska ha ett interface och registreras i DI"**. Öppna `Abstractions.cs`. Där finns tio interfaces. `BirthdayReportService` tar *åtta* beroenden i konstruktorn. Ett av dem är `IListFactory`, som har en metod: `CreateList<T>() => new List<T>()`.

Samtidigt *är* vissa abstraktioner viktiga: rapporten måste kunna testas med en fast klocka, och kunddatan kommer från en extern CRM-databas.

## 🛠️ Uppgift

1. ▶️ **Kör testerna** och se att de är gröna.
2. 🗂️ **Gå igenom alla tio interfaces.** Bestäm för vart och ett: ska det **stanna**, **tas bort** eller **ersättas** med något annat? Skriv ner varför.
3. ✂️ **Förenkla** i små steg. Kör testerna efter varje steg.
4. 🧪 **Kontrollera testbarheten.** Kan du fortfarande skriva ett test med en fast klocka och en fejkad kundlista?
5. 🔢 **Räkna:** hur många parametrar har `BirthdayReportService` nu? Hur många rader och typer har du tagit bort?

## ▶️ Kom igång

```bash
dotnet run --project exercises/05-overuse-critique/interface-for-everything/InterfaceForEverything
dotnet test exercises/05-overuse-critique/interface-for-everything/InterfaceForEverything.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: En enkel fråga</summary>

Fråga för varje interface: **Pratar implementationen med omvärlden** (databas, nätverk, filsystem, klocka)? **Finns det, eller kommer det inom kort att finnas, mer än en implementation?** Om svaret på båda är nej är interfacet troligen onödigt.
</details>

<details>
<summary>💡 Hint 2: Data och beteende</summary>

`ICustomer` är ett interface för *data*. Behöver data ett interface? Vad vinner du på det?
</details>

<details>
<summary>💡 Hint 3: Klockan</summary>

`IDateTimeWrapper` är en bra *idé*: klockan är ett beroende. Men titta på implementationen, `TimeProviderDateTimeWrapper`. Vad lindar den in? Behövs wrappern?
</details>

<details>
<summary>💡 Hint 4: Rena funktioner</summary>

`IAgeCalculator`, `IBirthdayCalculator` och `IStringHelper` innehåller *rena funktioner*: samma indata ger alltid samma utdata och inga sidoeffekter. Rena funktioner kan testas direkt, utan att fejkas. De kan vara privata metoder, eller statiska om de är intressanta nog att testa separat.
</details>

### 📚 Central concepts

- **Dependency Injection** – när och varför. 🔗 [Microsoft Learn: Dependency injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- **DI guidelines** – Microsofts råd, bland annat om att inte registrera allt. 🔗 [Microsoft Learn: Dependency injection guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)
- **TimeProvider** – .NET:s färdiga abstraktion av klockan. 🔗 [Microsoft Learn: What is TimeProvider?](https://learn.microsoft.com/en-us/dotnet/standard/datetime/timeprovider-overview)
- **Interface segregation och interfaces i C#** – vad ett interface är till för. 🔗 [Microsoft Learn: Interfaces](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/interfaces)
- **Lazy class** – klasser utan tillräckligt ansvar. 🔗 [Refactoring.Guru: Lazy Class](https://refactoring.guru/smells/lazy-class)
- **Speculative generality** – abstraktioner "ifall att". 🔗 [Refactoring.Guru: Speculative Generality](https://refactoring.guru/smells/speculative-generality)
- **Test doubles** – vad som faktiskt behöver fejkas. 🔗 [Martin Fowler: TestDouble](https://martinfowler.com/bliki/TestDouble.html)

## 🤔 Reflektionsfrågor

1. Vilka interfaces behöll du, och vilken princip använde du för att avgöra det?
2. Är "varje klass ska ha ett interface" någonsin en bra regel? Var kommer den ifrån?
3. Den gamla koden var "testbar". Var den *lättare* att testa än din version? Hur många fakes hade ett enhetstest av den gamla `BirthdayReportService` behövt?
4. Vad är skillnaden mellan att *använda* DI och att *överanvända* DI?

---

🔑 **[Facit](../../../answers/05-overuse-critique/interface-for-everything.md)**. Gör ett ärligt försök först!
