# 📝 NOTES: One algorithm strategy

## 💡 Resonemanget

- **Från 12 typer (inklusive `InvoiceCalculator`) och ungefär 100 rader till 1 klass och ungefär 20 rader.** `Strategies.cs` är borttagen. Hela beräkningen ryms i `InvoiceCalculator`.
- **Varje "strategi" hade exakt en implementation**, och inget i verksamheten tyder på fler. Abstraktionerna löste ett problem som inte finns.
- **`InvoiceCalculationContext` var sämst.** Med setters och en `InvalidOperationException` när en strategi saknas kunde objektet hamna i ett ogiltigt tillstånd, något som en vanlig metod inte kan.
- **Fabriken med en enum med ett enda värde** och ett `default`-fall som kastar exception var död kod.
- **Det som är viktigt finns kvar och syns tydligt:** momssatsen som en namngiven konstant, avrundningsregeln på *ett* ställe och valideringen av antal.

## ⚖️ Vad som *kunde* motivera en abstraktion

- Om företaget börjar sälja i flera länder kan momssatsen bli *data* (en tabell per land och varukategori), oftast inte en strategi.
- Om olika *kunder* får olika avrundningsregler eller beräkningsmodeller, då kan en Strategy för beräkningsmodellen vara rätt. Inför den *då*, med tester som skyddsnät. Det tar en kvart.

## 🔀 Alternativ

- **En statisk klass** (`static class InvoiceMath`) fungerar också, men testerna skapar `new InvoiceCalculator()`, så klassen behålls.
- **`Func<decimal, decimal>` för avrundningen** är en lättvikts-Strategy om flexibilitet faktiskt behövs någon gång.
