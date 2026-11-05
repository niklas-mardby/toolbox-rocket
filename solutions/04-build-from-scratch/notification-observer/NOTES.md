# 📝 NOTES: Notification observer

## 💡 Resonemanget

- **Statusen** lagras i en `Dictionary<string, PackageStatus>`. `StatusOf` returnerar `null` för okända paket, och det blir `OldStatus` i den första `StatusChange`.
- **Ingen notis utan ändring.** Kontrollen `oldStatus == newStatus` sker *innan* något sparas eller skickas.
- **Två sätt att notifiera** sker i samma metod: först C#-eventet, sedan alla `IObserver`s. Ordningen spelar ingen roll för testerna, men det är bra att den är förutsägbar.
- **Kopia av listan** (`_observers.ToList()`) gör att en observer kan avregistrera sig medan den notifieras, utan `InvalidOperationException` ("Collection was modified").
- **`Unsubscriber`** sätter sin `Action` till `null` efter första anropet, så att `Dispose` tål att anropas flera gånger. Det är praxis för alla `IDisposable`.
- **`SmsNotifier`** använder en privat *metod* som handler. Då fungerar `-=`, eftersom method group-konverteringen ger en delegate som är lika med den som gavs till `+=`.
- **Tiden** kommer från `TimeProvider`, så testerna kan kontrollera exakt `At`.

## 🔀 Alternativa lösningar

- **Bara events:** Del 2 kan lösas genom att `Subscribe` registrerar en lambda på `StatusChanged` som anropar `observer.OnNext`. Det är elegant, men då måste unsubscribern komma ihåg exakt den lambdan, och `Shutdown` behöver ändå en lista för att kunna anropa `OnCompleted`.
- **Felhantering per observer:** en `try/catch` runt varje `OnNext` (och `GetInvocationList()` för eventet) gör att en trasig observer inte stoppar de andra. Det testas inte här, men är ofta rätt i produktion.
- **Trådsäkerhet:** lösningen är inte trådsäker. I en webbapp där flera trådar uppdaterar samtidigt behövs ett `lock` runt listan och dictionaryn, eller `ConcurrentDictionary` och `ImmutableList`.
- **Rx.NET:** i ett riktigt projekt kan `Subject<StatusChange>` från `System.Reactive` ersätta hela den egna `IObservable`-implementationen.
