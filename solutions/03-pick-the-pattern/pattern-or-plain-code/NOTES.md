# 📝 NOTES: Pattern or plain code?

Facit med motiveringar finns på main i [`answers/03-pick-the-pattern/pattern-or-plain-code.md`](../../../answers/03-pick-the-pattern/pattern-or-plain-code.md).

## 💡 Snabbsvar

| # | Scenario | Svar |
|---|----------|------|
| 1 | Två rabatter | ❌ Nej |
| 2 | Internt verktyg | ❌ Nej |
| 3 | Datumformatet | ❌ Nej |
| 4 | Mejltjänsten | ✅ Ja, interfacet är en söm för tester |
| 5 | Kundposten | ❌ Nej, en konstruktor är säkrare |
| 6 | Loggningen | ❌ Nej |
| 7 | Databasbytet | ❌ Nej (YAGNI) |
| 8 | Fakturaexporten | ✅ Ja, Strategy |

## 💡 Principen bakom

> Inför ett mönster när det finns en **konkret smärta idag**: duplicering, buggar, otestbar kod eller kod som ändras ofta. Inte för att det *kanske* behövs.

## 🔀 Andra giltiga svar

- **Scenario 4:** "nej" är försvarbart om teamet bara har integrationstester mot en testmiljö med en fejkad SMTP-server. Det är ovanligt men möjligt.
- **Scenario 8:** "inte än" är försvarbart om de två avrundningsbuggarna först fixas med en gemensam hjälpmetod. Men med ett fjärde format på väg är Strategy rätt.
