# 📝 NOTES: Everyday scenarios

Facit med motiveringar finns på main i [`answers/03-pick-the-pattern/everyday-scenarios.md`](../../../answers/03-pick-the-pattern/everyday-scenarios.md).

## 💡 Snabbsvar

| # | Scenario | Mönster |
|---|----------|---------|
| 1 | Rapportexporten | Strategy (+ Factory för att välja) |
| 2 | Anrop till externa API:er | Decorator (helst som `DelegatingHandler`) |
| 3 | Nya användare | Observer |
| 4 | Väderdatan | Adapter |
| 5 | E-postmeddelandet | Builder (eller object initializers med `required`) |
| 6 | Den otestbara klassen | Dependency Injection (+ `TimeProvider`) |

## 🔀 Andra giltiga svar

- **Scenario 2:** middleware är *inte* rätt, eftersom det gäller *utgående* anrop. Det är ett vanligt misstag.
- **Scenario 3:** en mediator med notifications är lika rätt om projektet redan har en mediator.
- **Scenario 5:** ett svar som säger "ingen Builder, `record` med `required` räcker" är helt korrekt, om det motiveras.
