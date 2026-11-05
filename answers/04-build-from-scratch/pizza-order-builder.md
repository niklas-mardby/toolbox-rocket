# 🔑 Facit: Pizza order builder

> 🧩 En fullständig, testad lösning finns på branchen `solutions` i `solutions/04-build-from-scratch/pizza-order-builder/`.

## 🏗️ Kärnan i lösningen

```csharp
public Pizza Build()
{
    if (_size is null) throw new InvalidOperationException("Välj en storlek.");
    if (_toppings.Count > MaxToppings) throw new InvalidOperationException($"Högst {MaxToppings} toppings.");
    if (_crust == Crust.GlutenFree && _size == PizzaSize.Small)
        throw new InvalidOperationException("Glutenfri botten finns inte i storlek Small.");

    return new Pizza(_size.Value, _crust, _toppings.ToList(), _extraCheese);   // kopia!
}
```

## ➕ Bonusen: tidsregeln

Buildern behöver veta vad klockan är, så den behöver ett **beroende**: `new OrderBuilder(TimeProvider time)`. Testet skickar in en fejkad klocka. Det är samma lärdom som i [subscription-reminders](../../exercises/01-refactor/subscription-reminders/): tid är ett beroende, även inne i en builder.

## 🤔 Reflektionsfrågorna

### 1. Hade en konstruktor räckt?
För `Pizza` själv, **nästan**: `new Pizza(Large, Thin, ["Skinka"], true)` är läsbart med namngivna argument. Det buildern tillför är:
- ✅ **validering av kombinationer** med tydliga felmeddelanden,
- ✅ **standardvärden** (Classic, ingen extra ost) utan överlagrade konstruktorer,
- ✅ **stegvis byggande**, där menyn förbereder och kunden fyller i resten,
- ✅ **nästling** i `OrderBuilder.AddPizza(p => ...)`.

För `Order` är vinsten större: leveransavgiften *beräknas* utifrån helheten, och kombinationen leverans/avhämtning har regler.

### 2. `PizzaMenu` som director
I GoF är **director** den som vet *i vilken ordning* byggstegen ska anropas för att skapa en viss produkt. `PizzaMenu.Capricciosa()` är just det: den vet vilka toppings som ingår. Skillnaden är att GoF:s director oftast bygger hela produkten, medan `PizzaMenu` lämnar ett steg (storleken) till anroparen. Det är en vanlig, praktisk variant.

### 3. `WebApplication.CreateBuilder()` jämfört med `PizzaBuilder`
**Gemensamt:** konfigurera steg för steg, och `Build()` ger en färdig produkt som inte är samma typ som buildern.
**Skillnader:** `WebApplicationBuilder` är inte fluent i första hand. Du anropar `builder.Services.Add...()` på *properties* i stället för att kedja metoder. Den har dessutom bra standardvärden för allt, som konfiguration från `appsettings.json` och loggning. Och dess `Build()` *låser* konfigurationen: efter det kan inga tjänster registreras.

### 4. Kompilatorn som tvingar fram storleken
Två sätt:
- **Gör storleken obligatorisk från början:** `new PizzaBuilder(PizzaSize.Large)`. Enkelt och effektivt.
- **Step builder:** `PizzaBuilder.Start()` returnerar ett `INeedSize` som bara har `WithSize`, och det returnerar ett `IPizzaOptions` med resten av metoderna och `Build()`. Då *kan* du inte anropa `Build()` utan storlek.

Step builders är eleganta men kräver fler interfaces. För ett enda obligatoriskt värde är konstruktorparametern oftast bättre. ✅ Ett bra svar väger **säkerhet** mot **mängd kod**.
