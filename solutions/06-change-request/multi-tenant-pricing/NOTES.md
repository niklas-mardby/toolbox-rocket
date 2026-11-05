# 📝 NOTES: Multi-tenant pricing

## 💡 Resonemanget

Lösningen skiljer på tre saker som var hoptrasslade i originalet:

| Del | Mönster | Ansvar |
|-----|---------|--------|
| `PriceRuleConfig` (fanns redan) | *data* | **Vilka** regler en kund har, och med vilka parametrar |
| `PriceRuleFactory` | **Factory** | Översätter config till regelobjekt. Det enda stället som känner till alla regeltyper. |
| `ILinePriceRule` / `IOrderPriceRule` | **Strategy** | **Hur** varje regel räknar |
| `PricingEngine` | *motor* | **I vilken ordning** reglerna körs, avrundning och listan över tillämpade regler |

- **Verktygsboden har inga särregler längre.** Kunden är bara en config bland andra. Testerna från originalet är gröna utan ändringar, vilket bevisar att den hårdkodade logiken och configen beskriver samma sak.
- **Regelnamnen genereras från parametrarna** (`Num.Format` utan nollor på slutet), så att "Mängdrabatt 10 % från 5 st" och "Mängdrabatt 5 % från 10 st" kommer från samma klass.
- **Ordningen i configen är ordningen i beräkningen.** `PricedLine.Total` uppdateras av varje regel. Därför ger `BuyXPayForY` efter `QuantityBreak` "gratis enheter värderade till det rabatterade priset", vilket är exakt vad produktägaren bestämde.
- **Rad- och orderregler är olika interfaces**, eftersom de arbetar på olika saker. Motorn kör alla radregler först och sedan orderreglerna.
- **Okänd tenant** ger `KeyNotFoundException`, och en okänd regeltyp ger `NotSupportedException` från fabriken. Det felet upptäcks när en offert räknas, så det är värt ett test för varje tenant-config vid uppstart.

## ⚖️ Avvägningar

- **`PriceRuleFactory.Create` returnerar `object`**, och motorn sorterar med `OfType<>`. Det är enkelt men något löst typat. Ett alternativ är ett gemensamt basinterface `IPriceRule` med en `Stage`-egenskap (Line eller Order), eller två fabriksmetoder.
- **Reglerna skapas vid varje offert.** De är små och billiga. Med många offerter per sekund kan regellistan cachas per tenant, men då måste cachen tömmas när en config ändras.

## 🔀 Alternativ

- **Composite**: en `CompositePriceRule` som innehåller andra regler och själv är en regel, så att regler kan grupperas ("välj den bästa av dessa tre").
- **Chain of Responsibility** för regler som ska *stoppa* kedjan, till exempel "max en rabatt per rad".
- **Specification pattern** för villkoren (medlem, kategori eller veckodag) om villkor och effekter ska kunna kombineras fritt i configen.
- **Registrera fabriker per typ i DI** (`IRuleFactory<TConfig>`) om nya regeltyper ska kunna läggas till som plugins, utan att ändra `PriceRuleFactory`.
