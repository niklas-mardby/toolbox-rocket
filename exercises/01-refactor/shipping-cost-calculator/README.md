# 🚚 Shipping cost calculator

> 🧩 **Mönster:** Strategy
> 📏 **Omfattning:** en klass på ungefär 100 rader, ett mönster, tydliga steg. En bra första refaktorering.
> 🧪 **Tester:** finns och är gröna. Låt dem förbli gröna.

## 🎯 Mål

Du ska ersätta tre `switch`-satser över samma enum med **Strategy**, så att en ny fraktmetod kan läggas till utan att du behöver ändra befintlig kod på flera ställen.

## 📖 Bakgrund

Webbshoppen räknar ut fraktkostnad, uppskattad leveranstid och visningsnamn för fyra fraktmetoder. Allt ligger i `ShippingCalculator.cs`, och varje metod i klassen har en egen `switch` över `ShippingMethod`.

Det fungerar. Men nästa vecka vill marknadsavdelningen lägga till **paketskåp**. Om ett halvår kommer **hemleverans på kvällen**. Varje gång måste någon hitta och ändra *alla tre* `switch`-satserna och hoppas att den inte glömmer någon.

## 🛠️ Uppgift

1. ▶️ **Kör testerna** och se att de är gröna:
   ```bash
   dotnet test exercises/01-refactor/shipping-cost-calculator/ShippingCostCalculator.Tests
   ```
2. 👀 **Läs `ShippingCalculator.cs`.** Vad har de tre `switch`-satserna gemensamt? Vilken information hör ihop med *en* fraktmetod?
3. 🧩 **Skapa ett interface** som beskriver allt en fraktmetod kan: räkna ut pris, uppskatta leveranstid och ge ett visningsnamn.
4. 🏗️ **Skapa en klass per fraktmetod** som implementerar interfacet. Flytta logiken dit, en metod i taget, och kör testerna efter varje flytt.
5. 🔀 **Låt `ShippingCalculator` delegera** till rätt strategi i stället för att använda `switch`. Behåll klassens publika metoder så att testerna fortsätter fungera.
6. ✅ **Kör testerna igen.** De ska fortfarande vara gröna.
7. ➕ **Bonus:** Lägg till fraktmetoden `Locker` (paketskåp): 39 kr upp till 10 kg, ej tillgänglig över 10 kg, 2 dagars leverans, visningsnamnet "Paketskåp". Räkna hur många *befintliga* filer du behövde ändra. Jämför med hur många ställen du hade behövt ändra före refaktoreringen.

## ▶️ Kom igång

```bash
dotnet run --project exercises/01-refactor/shipping-cost-calculator/ShippingCostCalculator
dotnet test exercises/01-refactor/shipping-cost-calculator/ShippingCostCalculator.Tests
```

## 💡 Hints

<details>
<summary>💡 Hint 1: Vad ska interfacet innehålla?</summary>

Titta på vad de tre `switch`-satserna returnerar: ett pris, ett antal dagar och ett namn. Allt det beror på *vilken* fraktmetod det är. Ett interface med tre medlemmar som motsvarar de tre metoderna i `ShippingCalculator` är en bra start.
</details>

<details>
<summary>💡 Hint 2: Var börjar jag?</summary>

Börja med den enklaste fraktmetoden, `Pickup`. Skapa en klass för den, flytta in logiken från alla tre `switch`-satserna och låt `case ShippingMethod.Pickup:` anropa den nya klassen. Kör testerna. När det fungerar gör du likadant med nästa metod.
</details>

<details>
<summary>💡 Hint 3: Hur hittar ShippingCalculator rätt strategi?</summary>

När alla fyra klasser finns behöver du fortfarande översätta en `ShippingMethod` till rätt strategi. Det kan du göra på *ett* ställe, till exempel med en `Dictionary<ShippingMethod, IShippingStrategy>`. Glöm inte att ett okänt värde fortfarande ska ge `ArgumentOutOfRangeException`.
</details>

<details>
<summary>💡 Hint 4: Den duplicerade landslistan</summary>

`"NO"`, `"DK"` och `"FI"` förekommer på två ställen. När logiken för internationell frakt hamnar i en egen klass kan du samla dem i ett enda fält, till exempel en `HashSet<string>`. Det är en av fördelarna med att samla allt som hör till en fraktmetod på samma ställe.
</details>

<details>
<summary>💡 Hint 5: Viktvalideringen</summary>

Kontrollen att vikten är positiv gäller *alla* fraktmetoder. Ska den ligga i varje strategi eller ligga kvar i `ShippingCalculator` innan strategin anropas? Fundera på vad som händer om någon skriver en ny strategi och glömmer kontrollen.
</details>

### 📚 Central concepts

- **Strategy pattern** – kapsla in varianter av en algoritm bakom ett gemensamt interface så att de kan bytas ut. 🔗 [Refactoring.Guru: Strategy](https://refactoring.guru/design-patterns/strategy)
- **Replace Conditional with Polymorphism** – refaktoreringen som tar dig från `switch` till klasser. 🔗 [Refactoring.Guru: Replace Conditional with Polymorphism](https://refactoring.guru/replace-conditional-with-polymorphism)
- **Switch statements (code smell)** – när en `switch` är ett symptom och när den är helt okej. 🔗 [Refactoring.Guru: Switch Statements](https://refactoring.guru/smells/switch-statements)
- **Interfaces** – ett kontrakt som flera klasser kan uppfylla. 🔗 [Microsoft Learn: Interfaces](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/interfaces)
- **Polymorphism** – samma anrop, olika beteende beroende på objektets typ. 🔗 [Microsoft Learn: Polymorphism](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/polymorphism)
- **Open/closed principle** – koden ska vara öppen för utökning men stängd för ändring. 🔗 [Wikipedia: Open–closed principle](https://en.wikipedia.org/wiki/Open%E2%80%93closed_principle)

## 🤔 Reflektionsfrågor

1. Hur många ställen behövde du ändra för att lägga till `Locker` efter refaktoreringen, jämfört med före?
2. Det finns fortfarande en enum, `ShippingMethod`. Är den ett problem? När skulle du vilja ta bort den?
3. Har koden blivit *lättare* eller *svårare* att läsa? För vem?
4. Om det bara hade funnits två fraktmetoder och inga planer på fler, hade refaktoreringen varit värd det?

---

🔑 **[Facit](../../../answers/01-refactor/shipping-cost-calculator.md)**. Gör ett ärligt försök först!
