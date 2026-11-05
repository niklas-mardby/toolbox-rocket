# 🏗️ 04 – Build from scratch

Här implementerar du ett mönster själv i en liten domän. Du får **kontraktet**, alltså de publika klasserna och metoderna, och **tester** som beskriver hur det ska fungera. Resten bygger du.

## 🧭 Så jobbar du

1. 📖 Läs README:n och de färdiga typerna. Klasserna du ska bygga finns redan som stubbar som kastar `NotImplementedException`.
2. 🧪 **Slå på testerna.** De är avstängda från början så att hela repot är grönt. I varje testprojekt finns filen `Todo.cs`:
   ```csharp
   public const string? Skip = "Ta bort mig när du är redo ...";
   ```
   Ändra värdet till `null` för att slå på alla tester i projektet. Du kan också ta bort `Skip = Todo.Skip` på enskilda tester, om du vill ta ett test i taget.
3. 🔴🟢 Kör testerna. De blir röda. Implementera tills de blir gröna.
4. ➕ Skriv gärna egna tester för det som testerna inte täcker.

> 💡 Du får lägga till egna klasser, privata metoder och fält. Ändra inte de publika signaturerna i stubbarna, för testerna är beroende av dem.

## 📋 Övningarna

Ungefär ordnade från mindre till större:

| Övning | Mönster | Kort beskrivning |
|--------|---------|------------------|
| [📦 notification-observer](notification-observer/) | Observer | Paketspårning med C#-events och `IObservable<T>`. |
| [🍕 pizza-order-builder](pizza-order-builder/) | Builder | En fluent builder för pizzor och beställningar, med validering. |
| [💳 payment-provider-adapter](payment-provider-adapter/) | Adapter | Två betal-SDK:er med helt olika API:er bakom ett gemensamt interface. |
| [✉️ message-sender-decorators](message-sender-decorators/) | Decorator | Retry, loggning, filtrering och begränsning som staplingsbara lager. |
| [📚 repository-and-unit-of-work](repository-and-unit-of-work/) | Repository + Unit of Work | Ett bibliotekssystem ovanpå EF Core och SQLite, och frågan om det behövs. |
| [📨 mini-mediator](mini-mediator/) | Mediator + Decorator | Din egen MediatR: handlers, pipeline behaviors och assembly scanning. |
