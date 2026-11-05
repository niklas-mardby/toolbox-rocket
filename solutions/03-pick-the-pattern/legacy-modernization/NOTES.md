# 📝 NOTES: Legacy modernization

Exempelsvar och "vad ett bra svar tar hänsyn till" finns på main i [`answers/03-pick-the-pattern/legacy-modernization.md`](../../../answers/03-pick-the-pattern/legacy-modernization.md).

## 💡 Kärnan i ett bra svar

1. 🧪 **Tester först**, även långsamma characterization tests.
2. 🔌 **Sömmar runt det som ska ändras:** Adapter runt SOAP, och Adapter med DI runt den statiska databasklassen. Bara där det behövs.
3. 📅 **Deadlines styr ordningen.** SOAP-bytet har en hård deadline, så förbered det tidigt med `IFederationGateway`.
4. 🐾 **Små steg och leveranser varje vecka.** Familj levereras inom sex veckor.
5. 🚫 **Ingen omskrivning, inga generiska abstraktioner för hela kodbasen** och ingen blandning av struktur- och beteendeändringar.

## 🔀 Andra giltiga planer

- **Familj först, refaktorering efter:** giltigt om tiden är knapp, *förutsatt* att characterization tests finns.
- **Börja med SQL injection-risken:** helt rimligt om en säkerhetsgranskning har pekat ut den. Säkerhet kan gå före allt annat.
- **En separat "anti-corruption layer"** (en term från DDD) för förbundets REST-API: en mer genomarbetad variant av adaptern.
