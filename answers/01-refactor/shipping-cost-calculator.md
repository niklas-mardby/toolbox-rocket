# 🔑 Facit: Shipping cost calculator

> 🧩 En fullständig kodlösning finns på branchen `solutions` i `solutions/01-refactor/shipping-cost-calculator/`. Här får du strukturen och svar på reflektionsfrågorna.

## 🏗️ Så här kan strukturen se ut

```csharp
public interface IShippingStrategy
{
    string DisplayName { get; }
    decimal CalculateCost(Parcel parcel);
    int EstimateDeliveryDays(Parcel parcel);
}

public class PickupShipping : IShippingStrategy
{
    public string DisplayName => "Hämtas i butik";
    public decimal CalculateCost(Parcel parcel) => 0m;
    public int EstimateDeliveryDays(Parcel parcel) => 0;
}

public class ShippingCalculator
{
    private readonly Dictionary<ShippingMethod, IShippingStrategy> _strategies = new()
    {
        [ShippingMethod.Standard] = new StandardShipping(),
        [ShippingMethod.Express] = new ExpressShipping(),
        [ShippingMethod.Pickup] = new PickupShipping(),
        [ShippingMethod.International] = new InternationalShipping(),
    };

    public decimal CalculateCost(Parcel parcel, ShippingMethod method)
    {
        if (parcel.WeightKg <= 0) throw new ArgumentException("Weight must be positive.", nameof(parcel));
        return Math.Round(GetStrategy(method).CalculateCost(parcel), 2);
    }

    private IShippingStrategy GetStrategy(ShippingMethod method) =>
        _strategies.TryGetValue(method, out var s)
            ? s
            : throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown shipping method.");
    // ...
}
```

**Varför ligger viktkontrollen och avrundningen kvar i `ShippingCalculator`?** De gäller *alla* fraktmetoder. Om de ligger i varje strategi kan någon som skriver en ny strategi glömma dem. Regler som är gemensamma hör hemma hos den som använder strategierna. Regler som är specifika, som "Express max 20 kg", hör hemma i strategin.

## 🤔 Reflektionsfrågorna

### 1. Hur många ställen ändrade du för att lägga till `Locker`?

**Före:** fyra ställen i *samma* fil (enumen plus tre `switch`-satser), och kompilatorn varnar inte om du glömmer en. Det blir ett `ArgumentOutOfRangeException` när koden körs.

**Efter:** du skapar *en ny* klass (`LockerShipping`) och lägger till enum-värdet och en rad i dictionaryn. Ingen befintlig logik behöver röras. Det är **open/closed principle** i praktiken: utökning genom ny kod, inte genom ändring av gammal.

### 2. Är enumen ett problem?

Inte nödvändigtvis. Enumen är det *publika kontraktet* som testerna och anroparna använder, och den är bra för till exempel ett API eller en databaskolumn. Den kostar en rad per ny metod.

Du skulle vilja ta bort den om fraktmetoder ska kunna läggas till **utan omkompilering**, till exempel som plugins, eller konfigureras per butik i en databas. Då kan strategierna registreras i DI-containern och hittas via ett namn eller en egenskap på interfacet (till exempel `string Code { get; }`).

### 3. Lättare eller svårare att läsa?

Båda svaren kan vara rätt. Ett bra svar ser **avvägningen**:
- ✅ **Lättare** att förstå *en* fraktmetod. Allt om Express står på ett ställe, och klasserna är små och lätta att testa var för sig.
- ⚠️ **Svårare** att få *överblick* över alla priser samtidigt. Du måste öppna fyra filer i stället för att läsa en `switch`.
- Den som underhåller koden och lägger till nya metoder vinner mest. Den som bara vill jämföra priser kan tycka att den gamla koden var tydligare.

### 4. Hade det varit värt det med bara två metoder?

Troligen **inte**. Två fall i en `switch` (eller en `if`) är lättläst, och ingen vinst kommer om inget förändras. Tumregeln *Rule of Three* säger: vänta tills du ser samma sorts förändring tredje gången innan du abstraherar. Det som gjorde refaktoreringen värd det här var:
- att samma `switch` fanns på **tre** ställen (duplicering), och
- att det **finns planer** på fler fraktmetoder (förändringstryck).

Utan förändringstryck är Strategy bara mer kod. Se även [YAGNI](https://martinfowler.com/bliki/Yagni.html).
