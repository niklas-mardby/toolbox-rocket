# 📝 NOTES: Order notifications

## 💡 Resonemanget

- **Steg och reaktioner skildes åt.** Validering, lagerreservation och lagring är *steg*: de kan stoppa ordern och anropas direkt. Mejl, poäng, säljlarm, statistik och plockkö är *reaktioner*: de prenumererar på `OrderPlaced`.
- **`OrderService` gick från fem beroenden till ett** (`Inventory`).
- **Eventet höjs efter att ordern sparats**, så att ingen reaktion körs för en order som inte gick igenom. Testet `Out_of_stock_order_fails_and_nothing_else_happens` vaktar det.
- **Större reaktioner blev små klasser** (`ConfirmationEmailHandler` med flera), och de minsta är lambdas direkt i `Composition.cs`. Båda varianterna är rimliga.
- **Alla prenumerationer finns på ett ställe**, i composition root. Där ser en ny utvecklare allt som händer vid en order.
- **Bonusen** (Slack) krävde en rad i `Composition.cs` och noll ändringar i `OrderService`. `SlackTests.cs` är det nya testet.

> ⚠️ Med `+= (_, e) => ...` kan prenumerationerna inte avslutas, eftersom ingen referens till lambdan sparas. Det spelar ingen roll här, där allt lever lika länge, men det gör det om en observer lever kortare än subjectet.

## 🔀 Alternativ

- **`IEnumerable<IOrderPlacedHandler>` i konstruktorn** i stället för ett event. Det är fortfarande Observer, men passar DI-containern bättre: handlers registreras med `services.AddTransient<IOrderPlacedHandler, ...>()` och kan ha egna scoped beroenden.
- **Felhantering per handler:** loopa och fånga exceptions per handler, så att ett trasigt mejl inte hindrar statistiken.
- **Outbox pattern** i produktion: spara "OrderPlaced" i samma transaktion som ordern och låt en bakgrundstjänst köra reaktionerna, med retry.
