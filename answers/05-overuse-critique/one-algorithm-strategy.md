# 🔑 Facit: One algorithm strategy

> 🧩 Den förenklade koden finns på branchen `solutions` i `solutions/05-overuse-critique/one-algorithm-strategy/`.

## ✍️ Kritiken

| Typ | Problem |
|-----|---------|
| `IVatRateProvider` + `SwedishStandardVatRateProvider` | En klass som returnerar konstanten `0.25m`. Den gör en konstant till ett objekt. |
| `IRoundingStrategy` + implementation | En rad (`Math.Round`) bakom ett interface. |
| `ILineTotalCalculationStrategy` + implementation | `Quantity * UnitPrice` bakom ett interface. Det finns ingen annan rimlig variant. |
| `IVatCalculationStrategy` + implementation | `net * rate`, avrundat. |
| `VatCalculationStrategyType` + `VatCalculationStrategyFactory` | En enum med *ett* värde och en `switch` med *ett* fall plus ett `default` som aldrig kan inträffa. |
| `InvoiceCalculationContext` | ⚠️ **Farlig:** setters gör att objektet kan vara halvfärdigt. Glömmer någon en `Set...` får de ett fel när koden *körs*, inte när den kompileras. |

**Totalt 11 typer** (4 interfaces, 4 implementationer, enum, factory och context) **för tre beräkningar**.

## ✂️ Efter förenklingen

```csharp
public class InvoiceCalculator
{
    private const decimal VatRate = 0.25m;

    public InvoiceTotals CalculateTotals(IEnumerable<InvoiceLine> lines)
    {
        var net = Round(lines.Sum(LineTotal));
        var vat = Round(net * VatRate);
        return new InvoiceTotals(net, vat, net + vat);
    }

    private static decimal LineTotal(InvoiceLine line) { /* validering + Quantity * UnitPrice */ }
    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
```

**Tydligare nu:** momssatsen syns direkt, ordningen (netto avrundas *före* momsen räknas ut) syns på två rader, och det finns inget ogiltigt tillstånd.

## 🤔 Steg 5 och reflektionsfrågorna

### 1. Vad är fel med open/closed-argumentet?
Open/closed principle säger att koden ska vara **öppen för utökning men stängd för ändring** *där förändringar faktiskt sker*. Den säger inte att *all* kod ska förberedas för *alla* tänkbara ändringar. Att gissa var förändringarna kommer att ske, och bygga abstraktioner för dem, ger oftast **fel abstraktioner**. Momsen kommer aldrig att behöva en `ILineTotalCalculationStrategy`.

### 2. Norge om två år?
**Kostnad att ha abstraktionerna nu:** varje utvecklare som läser koden under två år måste förstå 11 typer i stället för en. Och abstraktionerna är troligen *fel* för Norge: norsk moms har flera satser beroende på varukategori, så `IVatRateProvider.GetVatRate()` *utan parametrar* fungerar inte ändå.

**Kostnad att lägga till dem då:** en kvart till en timme, med testerna som skyddsnät. Och då vet du *exakt* vad som behövs: troligen en momssats per land och kategori, som *data*, inte en strategi.

✅ Ett bra svar ser att **det är billigare att vänta**, eftersom du vet mer senare och testerna gör förändringen säker.

### 3. Mest skadligt?
Troligen **`InvoiceCalculationContext`**. De andra typerna är "bara" onödiga. Contexten gör koden *sämre* än alternativet, eftersom den inför en ny sorts fel (glömda setters) som inte fanns innan. Mönster som ökar antalet sätt att använda en klass fel är ett varningstecken.
