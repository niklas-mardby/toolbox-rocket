# 🍕 Pizza order builder

> 🧩 **Mönster:** Builder, plus en "director" med färdiga recept
> 📏 **Omfattning:** två builders med tydliga regler. `PizzaBuilder` är rak och enkel. `OrderBuilder` bygger vidare med nästlade builders och regler som beror på helheten.
> 🧪 **Tester:** 22 tester finns, avstängda med `Todo.Skip`. Slå på dem när du är redo.

## 🎯 Mål

Du ska bygga en fluent **Builder** som skapar objekt steg för steg, validerar helheten i `Build()` och ger en oföränderlig produkt.

## 📖 Bakgrund

Pizzerian **Toolbox Pizza** ska få en beställningsapp. En pizza har storlek, botten, toppings och kanske extra ost. En beställning har en eller flera pizzor, leverans eller avhämtning och kanske en önskad tid. Det finns regler, som att glutenfri botten inte finns i Small och att leveransen är gratis från 300 kr.

En konstruktor med alla parametrar vore svårläst och kan inte validera kombinationer på ett tydligt sätt. En Builder kan:

```csharp
var order = new OrderBuilder()
    .AddPizza(PizzaMenu.Capricciosa().WithSize(PizzaSize.Large).Build(), quantity: 2)
    .AddPizza(p => p.WithSize(PizzaSize.Medium).AddTopping("Tonfisk").WithExtraCheese())
    .DeliverTo("Storgatan 1, Örebro")
    .Build();
```

## 🛠️ Uppgift

1. 📖 Läs `Products.cs` (färdig) och stubbarna i `PizzaBuilder.cs` och `OrderBuilder.cs`. Reglerna står i kommentarerna.
2. 🧪 Slå på testerna genom att sätta `Todo.Skip` till `null` i `PizzaOrderBuilder.Tests/Todo.cs`.
3. 🍕 Implementera `PizzaBuilder`. Kör `PizzaBuilderTests`.
4. 📋 Implementera `PizzaMenu`, de färdiga recepten.
5. 🛒 Implementera `OrderBuilder`. Kör `OrderBuilderTests`.
6. ▶️ Kör programmet:
   ```bash
   dotnet run --project exercises/04-build-from-scratch/pizza-order-builder/PizzaOrderBuilder
   ```
7. ➕ **Bonus:** Lägg till en regel: en beställning med leverans kan inte ha en önskad tid som är mindre än 45 minuter från nu. Hur får buildern veta vad klockan är, så att regeln går att testa?

## 💡 Hints

<details>
<summary>💡 Hint 1: Fluent-metoder</summary>

Varje metod ändrar ett privat fält och returnerar `this`:

```csharp
public PizzaBuilder WithSize(PizzaSize size)
{
    _size = size;
    return this;
}
```
</details>

<details>
<summary>💡 Hint 2: Hur vet jag om storleken är vald?</summary>

Om fältet är `PizzaSize _size` har det alltid ett värde, nämligen `Small`, standardvärdet för en enum. Använd `PizzaSize? _size` så att `null` betyder "inte vald".
</details>

<details>
<summary>💡 Hint 3: Var ska valideringen ligga?</summary>

Regler om *ett enskilt värde* (en tom topping) kan kontrolleras direkt. Regler som beror på *flera val* (glutenfritt och Small) kan bara kontrolleras när alla val är gjorda, alltså i `Build()`. Testerna visar vilket som gäller för varje regel.
</details>

<details>
<summary>💡 Hint 4: Testet om att ändra efter Build</summary>

Om pizzan får *samma lista* som buildern använder internt kommer den att ändras när buildern får fler toppings. Ge pizzan en kopia.
</details>

<details>
<summary>💡 Hint 5: PizzaMenu och "fresh builder"</summary>

Om `PizzaMenu.Vesuvio()` returnerar *samma* builder varje gång delar alla kunder på den. Skapa en ny varje gång.
</details>

### 📚 Central concepts

- **Builder pattern** – bygg ett komplext objekt steg för steg. 🔗 [Refactoring.Guru: Builder](https://refactoring.guru/design-patterns/builder)
- **Fluent interface** – metoder som kan kedjas och läses som en mening. 🔗 [Martin Fowler: FluentInterface](https://martinfowler.com/bliki/FluentInterface.html)
- **Immutable objects** – objekt som inte kan ändras efter att de skapats. 🔗 [Wikipedia: Immutable object](https://en.wikipedia.org/wiki/Immutable_object)
- **Records** – C#:s typ för immutabla dataobjekt. 🔗 [Microsoft Learn: Records](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/records)
- **Long parameter list** – problemet som Builder ofta löser. 🔗 [Refactoring.Guru: Long Parameter List](https://refactoring.guru/smells/long-parameter-list)
- **Lambda expressions** – för `AddPizza(p => ...)`. 🔗 [Microsoft Learn: Lambda expressions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions)

## 🤔 Reflektionsfrågor

1. `Pizza` har bara fyra fält. Hade en vanlig konstruktor varit lika bra? Vad tillför buildern egentligen här?
2. Vilken roll spelar `PizzaMenu` i Builder-mönstret? Jämför med "director" i GoF:s beskrivning.
3. `WebApplication.CreateBuilder()` och din `PizzaBuilder`: vad har de gemensamt, och vad skiljer dem åt?
4. Hur kunde du göra så att *kompilatorn* tvingar fram en storlek, i stället för att `Build()` kastar ett fel när programmet körs? Är det värt besväret?

---

🔑 **[Facit](../../../answers/04-build-from-scratch/pizza-order-builder.md)**. Gör ett ärligt försök först!
