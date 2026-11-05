# 📝 NOTES: Webshop scenarios

Facit med motiveringar finns på main i [`answers/03-pick-the-pattern/webshop-scenarios.md`](../../../answers/03-pick-the-pattern/webshop-scenarios.md).

## 💡 Snabbsvar

| # | Scenario | Mönster |
|---|----------|---------|
| 1 | "Aktiva produkter" | Domänspecifikt Repository, eller en extension method på `IQueryable` |
| 2 | Kassan | Unit of Work (redan i `DbContext`: ett `SaveChanges`) |
| 3 | Betalning per land | Adapter + Factory |
| 4 | Kampanjerna | Strategy (+ en kombinationsregel, eventuellt Composite) |
| 5 | Produktsidan | Decorator (caching) |
| 6 | OrderController | Mediator/CQRS, eller filters och uppdelning |
| 7 | "Meddela mig" | Observer (domain events, kanske via meddelandekö) |
| 8 | Fraktsedlarna | Adapter |

## 🔀 Andra giltiga svar

- **Scenario 1:** Specification pattern är ett bra svar för den som känner till det.
- **Scenario 4:** Chain of Responsibility om kampanjerna ska kunna "stoppa" varandra (till exempel "max en kampanj per vara").
- **Scenario 6:** "Dela upp controllern först och använd filters" är ofta det *bästa* första steget, och ett fullt giltigt svar.
