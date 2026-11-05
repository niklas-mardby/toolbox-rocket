# 🔑 Facit: Multi-tenant pricing

> 🧩 En fullständig lösning finns på branchen `solutions` i `solutions/06-change-request/multi-tenant-pricing/`.

## 🤔 Del 2: Reflektionen efter del 1

- **Svårast:** de flesta fastnar på att reglerna har *parametrar* (procentsatser, trösklar, veckodagar) som skiljer sig per kund, och på att **ordningen** ska följa configen. Med `if`-satser blir det antingen en `switch` per kund (som inte skalar) eller en loop över configen med en `switch` på regeltyp, alltså halva lösningen redan.
- **"Medlemsrabatt" i koden:** i originalet *två* ställen (villkoret och strängen). Efter en naiv del 1 ofta fler, en per kund.
- **En ny regeltyp:** en ny `case` mitt i motorn, i en metod som redan gör allt. Risken är stor att något befintligt påverkas.
- **Data och beteende:** configen är data (vilka regler och vilka parametrar), och regeltyperna är beteende. En ny *kund* ska bara vara data. En ny *regeltyp* är kod.

## 🔧 Del 3: Efter refaktoreringen

```csharp
public interface ILinePriceRule { string Name { get; } bool Apply(PricedLine line); }
public interface IOrderPriceRule { string Name { get; } QuoteFee? Apply(decimal linesTotal); }

public static class PriceRuleFactory
{
    public static object Create(PriceRuleConfig config) => config switch
    {
        PercentOffForMembers c => new MemberDiscountRule(c.Percent),
        QuantityBreak c => new QuantityBreakRule(c.MinQuantity, c.Percent),
        CategoryCampaign c => new CategoryCampaignRule(c.Category, c.Percent, c.Days),
        BuyXPayForY c => new BuyXPayForYRule(c.Sku, c.X, c.Y),
        MinimumOrderFee c => new MinimumOrderFeeRule(c.Threshold, c.Fee),
        _ => throw new NotSupportedException(...)
    };
}

// PricingEngine: hämta config → bygg regler → radregler i ordning → avrunda → orderregler
```

- **Ny kund:** bara en ny config. Ingen kod ändras.
- **Ny regeltyp** (som `FreeShippingOver`): en ny config-record, en ny regelklass och en rad i fabriken. Motorn ändras inte.

## 🤔 Reflektionsfrågorna

### 1. Varför fick Verktygsbodens tester inte ändras?
De är **karaktäriseringstester** för det befintliga beteendet. Att de är gröna *utan ändringar* bevisar att den nya, konfigurerbara motorn plus Verktygsbodens config ger *exakt* samma priser som de hårdkodade reglerna, inklusive ordningen och avrundningen. Om testerna hade fått ändras hade du kunnat "fixa" testerna i stället för koden, och en riktig kund hade fått nya priser utan att någon märkte det.

### 2. Var är risken störst?
- **Ny kund:** låg risk. Det är bara data, och felaktig config kan valideras (okänd regeltyp, negativ procentsats).
- **Ny regeltyp:** högre risk, men *isolerad*. En ny klass kan inte påverka befintliga regler. Fabriken kastar `NotSupportedException` för okända typer, så en glömd fabriksrad syns direkt i ett test. Ett bra skydd är ett test som skapar en instans av *varje* `PriceRuleConfig`-subtyp och kör den genom fabriken.

### 3. "Välj den bästa av tre"
Det kräver att en regel kan **innehålla andra regler**: en `BestOfRule : ILinePriceRule` som provar varje inre regel på en *kopia* av raden och behåller den som gav lägst pris. Det är mönstret **Composite**: en grupp av regler som själv är en regel. Configen behöver en motsvarande `BestOf(IReadOnlyList<PriceRuleConfig> Rules)`, och fabriken anropar sig själv rekursivt för de inre reglerna. Motorn behöver inte ändras, och det visar att designen klarar förändringen.

### 4. Mer generellt än kraven?
Exempel på svar:
- Två interfaces (rad och order) kan vara mer än de fem regeltyperna kräver, men skillnaden är verklig: orderregler arbetar på summan och lägger till avgifter.
- Om du har gjort en plugin-arkitektur med assembly scanning för regeltyper är det troligen **överdesignat**. Det finns fem regeltyper, och de ändras sällan.

✅ Ett bra svar kan peka på *konkreta* delar av den egna lösningen och motivera varför de är lagom, eller erkänna att de inte är det. Se [05-overuse-critique](../../exercises/05-overuse-critique/).
