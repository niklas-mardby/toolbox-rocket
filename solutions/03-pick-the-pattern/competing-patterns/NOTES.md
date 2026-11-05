# 📝 NOTES: Competing patterns

Facit med motiveringar finns på main i [`answers/03-pick-the-pattern/competing-patterns.md`](../../../answers/03-pick-the-pattern/competing-patterns.md).

## 💡 Snabbsvar

| # | Scenario | Val |
|---|----------|-----|
| 1 | Retry runt HTTP | Decorator (CoR om någon länk svarar själv) |
| 2 | Moms per land | Båda: Factory väljer, Strategy räknar |
| 3 | Efter en order | Mediator-notification, eftersom teamet redan har en mediator |
| 4 | Den gamla loggern | Adapter (`ILogger` → `CorpLog`) + Decorator eller `BeginScope` (correlation id) |
| 5 | Requests till API | Builder (kanske inuti en typad klient) |
| 6 | Validering | Pipeline behavior, eftersom commands kommer även från en kö |

## 💡 De avgörande frågorna

- **Ändras interfacet?** Då är det en Adapter. Annars Decorator.
- **Skickas anropet alltid vidare?** Då är det en Decorator. Kan en länk svara själv? Då är det Chain of Responsibility.
- **Skapa eller använda?** Skapa betyder Factory, använda betyder Strategy.
- **Direkt prenumeration eller en mellanhand?** Direkt betyder Observer, mellanhand betyder Mediator.

## 🔀 Andra giltiga svar

- **Scenario 3:** C#-events är fullt giltiga om händelsen bara är intressant *inom* en modul.
- **Scenario 6:** endpoint filters *plus* validering i bakgrundsjobbet är också giltigt, men då dupliceras logiken.
