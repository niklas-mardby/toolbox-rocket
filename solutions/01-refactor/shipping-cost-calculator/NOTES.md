# 📝 NOTES: Shipping cost calculator

## 💡 Resonemanget

- **Ett interface med tre medlemmar** (`DisplayName`, `CalculateCost`, `EstimateDeliveryDays`), eftersom det var de tre sakerna som de tre `switch`-satserna varierade.
- **`ShippingCalculator` behåller sitt publika API.** Testerna och alla anropare påverkas inte. Det gör refaktoreringen säker.
- **Det gemensamma ligger kvar i `ShippingCalculator`:** viktvalideringen, avrundningen och felet för okänd metod. En ny strategi kan inte glömma dem.
- **Det specifika ligger i strategin:** Express maxvikt och de nordiska länderna. Listan över nordiska länder fanns förut på två ställen och finns nu på ett.
- **Uppslaget** är en `Dictionary`. `switch`-satserna är borta. Det finns fortfarande *ett* ställe som kopplar enum till strategi, och det är medvetet.

## ➕ Bonusen: Locker

```csharp
public class LockerShipping : IShippingStrategy
{
    public string DisplayName => "Paketskåp";
    public decimal CalculateCost(Parcel parcel) => parcel.WeightKg <= 10
        ? 39m
        : throw new InvalidOperationException("Lockers only accept parcels up to 10 kg.");
    public int EstimateDeliveryDays(Parcel parcel) => 2;
}
```
Plus ett nytt enum-värde och en rad i dictionaryn. Ingen befintlig logik ändras.

## 🔀 Alternativ

- **`switch expression` som returnerar strategin**, i stället för en dictionary. Det är lika bra, och kompilatorn varnar till och med för saknade enum-fall.
- **Strategierna i DI** med en egenskap `ShippingMethod Method { get; }` och `IEnumerable<IShippingStrategy>` i konstruktorn. Det är bra i en webbapp där fraktmetoder kan ha egna beroenden, till exempel ett pris-API.
- **Ta bort enumen** och låta strategierna identifieras med en sträng (`Code`) om metoder ska kunna läggas till utan omkompilering.
