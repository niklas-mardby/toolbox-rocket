# 🔑 Facit: Interface for everything

> 🧩 Den förenklade koden finns på branchen `solutions` i `solutions/05-overuse-critique/interface-for-everything/`.

## 🗂️ Beslut per interface

| Interface | Beslut | Varför |
|-----------|--------|--------|
| `ICustomer` | ❌ Ta bort | Data, inte beteende. En `record`. |
| `ICustomerRepository` | ✅ **Behåll** | Pratar med omvärlden (CRM-databasen). En verklig söm. |
| `IDateTimeWrapper` | 🔄 **Ersätt med `TimeProvider`** | Klockan är ett beroende, men wrappern lindade bara in `TimeProvider`, som redan *är* abstraktionen. |
| `IStringHelper` | ❌ Ta bort | Rena funktioner, som privata metoder. |
| `IAgeCalculator` | ❌ Ta bort | En subtraktion. |
| `IBirthdayCalculator` | 🔄 **Statisk metod** | Ren funktion med en knepig regel (29 februari). Kan testas direkt utan fakes. |
| `IReportLineFormatter` | ❌ Ta bort | Privat metod. |
| `IReportConfiguration` | ❌ Ta bort | Tre konstanter. Om de ska konfigureras: `IOptions<T>`. |
| `IListFactory` | ❌ Ta bort | `new List<T>()`. 😅 |
| `IBirthdayReportService` | ❌ Ta bort (eller behåll) | Inget i appen behöver byta ut tjänsten. Behåll den om en controller ska testas med en fejkad tjänst. |

**Resultat:** 2 beroenden i stället för 8 (`ICustomerRepository` och `TimeProvider`), och ingen DI-container behövs i `Composition.cs`.

```csharp
public class BirthdayReportService(ICustomerRepository customers, TimeProvider time)
{
    public string BuildReport() { /* ... */ }
}
```

## 🤔 Reflektionsfrågorna

### 1. Principen
> **Abstrahera det som pratar med omvärlden eller det som faktiskt varierar. Skriv resten som vanlig kod.**

Omvärlden betyder databas, nätverk, filsystem, klocka, slump och miljö. De gör tester långsamma, opålitliga eller omöjliga, så där behövs en söm. Allt annat (beräkningar, formatering, data) testas bäst *som det är*.

### 2. "Varje klass ska ha ett interface"
Regeln kommer från en tid då **mock-bibliotek bara kunde fejka interfaces** och virtuella metoder, och från en tolkning av *dependency inversion principle* som "allt ska bero på abstraktioner". Den kan vara rimlig i *gränssnitt mellan moduler eller team*, där ett interface är ett kontrakt. Men som regel för *all* kod ger den indirektion utan nytta: varje "Gå till definition" i IDE:n hamnar i ett interface i stället för i koden.

### 3. Var den gamla koden lättare att testa?
**Nej, svårare.** Ett *isolerat* enhetstest av den gamla `BirthdayReportService` hade behövt **åtta fakes**, och testet hade mest testat att tjänsten anropar sina beroenden i rätt ordning. Det är ett skört test som går sönder vid varje refaktorering. Den nya versionen behöver två fakes: en klocka och en kundlista. Och testerna kontrollerar faktiskt *resultatet*.

> 📚 Mer abstraktion betyder inte automatiskt bättre testbarhet. Ofta blir det tvärtom: tester som är kopplade till implementationen i stället för till beteendet.

### 4. Använda kontra överanvända DI
- **Använda DI:** klasser får sina *verkliga beroenden* via konstruktorn, så att de kan bytas ut och testas. Composition root bestämmer implementationen.
- **Överanvända DI:** *allt* blir ett beroende, även rena funktioner, konstanter och `new List<T>()`. Konstruktorer växer, kopplingen mellan klasser döljs bakom interfaces, och koden blir svårare att navigera utan att bli mer flexibel.

En tumregel: **om du aldrig skulle vilja byta implementationen, varken i produktion eller i ett test, ska det inte vara ett beroende.**
