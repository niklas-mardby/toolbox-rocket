# 🔧 01 – Refactor

Här får du **rörig men fungerande kod** och **tester som är gröna**. Din uppgift är att refaktorera koden till ett mönster utan att ändra vad den gör.

> 📚 **Refaktorering** betyder att ändra kodens *struktur* utan att ändra dess *beteende*. Testerna är ditt skyddsnät: är de gröna före och gröna efter, så har du inte förstört något.

## 🧭 Så jobbar du

1. ▶️ Kör testerna först och se att de är gröna.
2. 👃 Läs koden och leta efter det som luktar: duplicering, långa `switch`-satser, klasser som vet för mycket.
3. 🐾 Refaktorera i **små steg**. Kör testerna efter varje steg.
4. 🚫 **Ändra inte testerna.** Om en övning har en `Composition.cs` får du ändra den, eftersom det är där objekten kopplas ihop.
5. ➕ Lägg gärna till *nya* tester för dina nya klasser.

## 📋 Övningarna

Ungefär ordnade från mindre till större:

| Övning | Mönster | Kort beskrivning |
|--------|---------|------------------|
| [🚚 shipping-cost-calculator](shipping-cost-calculator/) | Strategy | En klass med tre `switch`-satser över samma enum. |
| [📣 order-notifications](order-notifications/) | Observer | En ordertjänst som måste känna till alla som bryr sig om en ny order. |
| [🌦️ weather-service-wrappers](weather-service-wrappers/) | Decorator | Loggning, caching och affärslogik i samma metod. |
| [⏰ subscription-reminders](subscription-reminders/) | Dependency Injection | Hårdkodade beroenden och `DateTime.Now` gör koden otestbar. |
| [💳 legacy-payment-gateway](legacy-payment-gateway/) | Adapter | Ett gammalt betal-SDK läcker in i affärskoden. |
| [🏛️ order-api-pipeline](order-api-pipeline/) | Mediator / CQRS (+ Decorator) | Ett minimal API med en gudsklass och mycket duplicerad tvärgående logik. |
